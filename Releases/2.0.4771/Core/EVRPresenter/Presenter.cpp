//////////////////////////////////////////////////////////////////////////
//
// Presenter.cpp : Implements the presenter object.
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
//
//////////////////////////////////////////////////////////////////////////

#include "EVRPresenter.h"

#pragma warning( push )
#pragma warning( disable : 4355 )  // 'this' used in base member initializer list

// Default frame rate.
const MFRatio g_DefaultFrameRate = { 30, 1 };

// Default presenter buffer count
const int DEFAULT_PRESENTER_BUFFER_COUNT = 3;

// Function declarations.
RECT    CorrectAspectRatio(const RECT& src, const MFRatio& srcPAR, const MFRatio& destPAR);
BOOL    AreMediaTypesEqual(IMFMediaType *pType1, IMFMediaType *pType2);
HRESULT ValidateVideoArea(const MFVideoArea& area, UINT32 width, UINT32 height);
HRESULT SetDesiredSampleTime(IMFSample *pSample, const LONGLONG& hnsSampleTime, const LONGLONG& hnsDuration);
HRESULT ClearDesiredSampleTime(IMFSample *pSample);
BOOL    IsSampleTimePassed(IMFClock *pClock, IMFSample *pSample);
HRESULT SetMixerSourceRect(IMFTransform *pMixer, const MFVideoNormalizedRect& nrcSource);

// Convert a fixed-point to a float.
inline float MFOffsetToFloat(const MFOffset& offset)
{
    return offset.value + (float(offset.fract) / 65536);
}

inline MFOffset MakeOffset(float v)
{
    MFOffset offset;
    offset.value = short(v);
    offset.fract = WORD(65536 * (v-offset.value));
    return offset;
}

inline MFVideoArea MakeArea(float x, float y, DWORD width, DWORD height)
{
    MFVideoArea area;
    area.OffsetX = MakeOffset(x);
    area.OffsetY = MakeOffset(y);
    area.Area.cx = width;
    area.Area.cy = height;
    return area;
}

inline HRESULT GetFrameRate(IMFMediaType *pType, MFRatio *pRatio)
{
    return MFGetAttributeRatio(pType, MF_MT_FRAME_RATE, (UINT32*)&pRatio->Numerator, (UINT32*)&pRatio->Denominator);
}


HRESULT GetVideoDisplayArea(IMFMediaType *pType, MFVideoArea *pArea);
MFRatio GetPixelAspectRatio(IMFMediaType *pType);
HRESULT GetFourCC(IMFMediaType *pType, DWORD *pFourCC);


//-----------------------------------------------------------------------------
// CreateInstance
//
// Static method to create an instance of the object.
// Used by the class factory.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::CreateInstance(REFIID iid, void **ppv)
{
    if (ppv == NULL)
    {
        return E_POINTER;
    }

    HRESULT hr = S_OK;

    EVRCustomPresenter *pObject = new EVRCustomPresenter(hr);

    if (pObject == NULL)
    {
        hr = E_OUTOFMEMORY;
    }

    if (SUCCEEDED(hr))
    {
        hr = pObject->QueryInterface(iid, ppv);
    }

    SafeRelease(&pObject);
    return hr;
}

///////////////////////////////////////////////////////////////////////////////
//
// IUnknown methods
//
///////////////////////////////////////////////////////////////////////////////

HRESULT EVRCustomPresenter::QueryInterface(REFIID riid, void ** ppv)
{
    static const QITAB qit[] = 
    {
        QITABENT(EVRCustomPresenter, IMFVideoDeviceID),
        QITABENT(EVRCustomPresenter, IMFVideoPresenter),
        QITABENT(EVRCustomPresenter, IMFClockStateSink),  // Inherited from IMFVideoPresenter
        QITABENT(EVRCustomPresenter, IMFRateSupport),
        QITABENT(EVRCustomPresenter, IMFGetService),
        QITABENT(EVRCustomPresenter, IMFTopologyServiceLookupClient),
        QITABENT(EVRCustomPresenter, IMFVideoDisplayControl),
        QITABENT(EVRCustomPresenter, IPvpPresenter),
        QITABENT(EVRCustomPresenter, IPvpPresenter2),
        QITABENT(EVRCustomPresenter, IPvpPresenterConfig),
        { 0 }
    };
    return QISearch(this, qit, riid, ppv);
}

ULONG EVRCustomPresenter::AddRef()
{
    return InterlockedIncrement(&m_nRefCount);
}

ULONG EVRCustomPresenter::Release()
{
    assert(m_nRefCount >= 0);
    ULONG uCount = InterlockedDecrement(&m_nRefCount);
    if (uCount == 0)
    {
        delete this;
    }
    return uCount;
}

///////////////////////////////////////////////////////////////////////////////
//
// IMFGetService methods
//
///////////////////////////////////////////////////////////////////////////////

HRESULT EVRCustomPresenter::GetService(REFGUID guidService, REFIID riid, LPVOID *ppvObject)
{
    HRESULT hr = S_OK;

    if (ppvObject == NULL)
    {
        return E_POINTER;
    }

    if (guidService != MR_VIDEO_RENDER_SERVICE && guidService != MR_VIDEO_ACCELERATION_SERVICE)
    {
        return MF_E_UNSUPPORTED_SERVICE;
    }

    // First try to get the service interface from the D3DPresentEngine object.
    hr = m_pD3DPresentEngine->GetService(guidService, riid, ppvObject);
    if (FAILED(hr))
    {
        // Next, check if this object supports the interface.
        hr = QueryInterface(riid, ppvObject);
    }

    return hr;
}


///////////////////////////////////////////////////////////////////////////////
//
// IMFVideoDeviceID methods
//
//////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
// GetDeviceID
//
// Returns the presenter's device ID.
// The presenter and mixer must have matching device IDs.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::GetDeviceID(IID* pDeviceID)
{
    if (pDeviceID == NULL)
    {
        return E_POINTER;
    }

    *pDeviceID = __uuidof(IDirect3DDevice9);
    return S_OK;
}


///////////////////////////////////////////////////////////////////////////////
//
// IMFTopologyServiceLookupClient methods.
//
///////////////////////////////////////////////////////////////////////////////


//-----------------------------------------------------------------------------
// InitServicePointers
//
// Enables the presenter to get various interfaces from the EVR and mixer.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::InitServicePointers(
    IMFTopologyServiceLookup *pLookup
    )
{
    if (pLookup == NULL)
    {
        return E_POINTER;
    }

    HRESULT             hr = S_OK;
    DWORD               dwObjectCount = 0;

    EnterCriticalSection(&m_ObjectLock);

    // Do not allow initializing when playing or paused.
    if (IsActive())
    {
        hr = MF_E_INVALIDREQUEST;
        goto done;
    }

    SafeRelease(&m_pClock);
    SafeRelease(&m_pMixer);
    SafeRelease(&m_pMediaEventSink);

    // Ask for the clock. Optional, because the EVR might not have a clock.
    dwObjectCount = 1;

    (void)pLookup->LookupService(
        MF_SERVICE_LOOKUP_GLOBAL,   // Not used.
        0,                          // Reserved.
        MR_VIDEO_RENDER_SERVICE,    // Service to look up.
        IID_PPV_ARGS(&m_pClock),    // Interface to retrieve.
        &dwObjectCount              // Number of elements retrieved.
        );

    // Ask for the mixer. (Required.)
    dwObjectCount = 1;

    hr = pLookup->LookupService(
        MF_SERVICE_LOOKUP_GLOBAL, 0,
        MR_VIDEO_MIXER_SERVICE, IID_PPV_ARGS(&m_pMixer), &dwObjectCount
        );

    if (FAILED(hr))
    {
        goto done;
    }

    // Make sure that we can work with this mixer.
    hr = ConfigureMixer(m_pMixer);
    if (FAILED(hr))
    {
        goto done;
    }

    // Ask for the EVR's event-sink interface. (Required.)
    dwObjectCount = 1;

    hr = pLookup->LookupService(
        MF_SERVICE_LOOKUP_GLOBAL, 0,
        MR_VIDEO_RENDER_SERVICE, IID_PPV_ARGS(&m_pMediaEventSink),
        &dwObjectCount
        );

    if (FAILED(hr))
    {
        goto done;
    }

    // Successfully initialized. Set the state to "stopped."
    m_RenderState = RENDER_STATE_STOPPED;

done:
    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}

//-----------------------------------------------------------------------------
// ReleaseServicePointers
//
// Release all pointers obtained during the InitServicePointers method.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::ReleaseServicePointers()
{
    // Enter the shut-down state.
    EnterCriticalSection(&m_ObjectLock);

    m_RenderState = RENDER_STATE_SHUTDOWN;

    LeaveCriticalSection(&m_ObjectLock);

    // Flush any samples that were scheduled.
    Flush();

    // Clear the media type and release related resources.
    SetMediaType(NULL);

    // Release all services that were acquired from InitServicePointers.
    SafeRelease(&m_pClock);
    SafeRelease(&m_pMixer);
    SafeRelease(&m_pMediaEventSink);

    return S_OK;
}


///////////////////////////////////////////////////////////////////////////////
// IMFVideoPresenter methods
//////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
// ProcessMessage
//
// Handles various messages from the EVR.
// This method delegates all of the work to other class methods.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::ProcessMessage(
    MFVP_MESSAGE_TYPE eMessage, 
    ULONG_PTR ulParam
    )
{
    HRESULT hr = S_OK;

    EnterCriticalSection(&m_ObjectLock);

    hr = CheckShutdown();
    if (FAILED(hr))
    {
        goto done;
    }

    switch (eMessage)
    {
    // Flush all pending samples.
    case MFVP_MESSAGE_FLUSH:
        hr = Flush();
        break;

    // Renegotiate the media type with the mixer.
    case MFVP_MESSAGE_INVALIDATEMEDIATYPE:
        hr = RenegotiateMediaType();
        break;

    // The mixer received a new input sample.
    case MFVP_MESSAGE_PROCESSINPUTNOTIFY:
        hr = ProcessInputNotify();
        break;

    // Streaming is about to start.
    case MFVP_MESSAGE_BEGINSTREAMING:
        hr = BeginStreaming();
        break;

    // Streaming has ended. (The EVR has stopped.)
    case MFVP_MESSAGE_ENDSTREAMING:
        hr = EndStreaming();
        break;

    // All input streams have ended.
    case MFVP_MESSAGE_ENDOFSTREAM:
        // Set the EOS flag.
        m_bEndStreaming = TRUE;
        // Check if it's time to send the EC_COMPLETE event to the EVR.
        hr = CheckEndOfStream();
        break;

    // Frame-stepping is starting.
    case MFVP_MESSAGE_STEP:
        hr = PrepareFrameStep(LODWORD(ulParam));
        break;

    // Cancels frame-stepping.
    case MFVP_MESSAGE_CANCELSTEP:
        hr = CancelFrameStep();
        break;

    default:
        hr = E_INVALIDARG; // Unknown message. This case should never occur.
        break;
    }

done:
    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}


//-----------------------------------------------------------------------------
// GetCurrentMediaType
//
// Gets the current render format (the mixer's output format).
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::GetCurrentMediaType(
    IMFVideoMediaType** ppMediaType
    )
{
    HRESULT hr = S_OK;

    if (ppMediaType == NULL)
    {
        return E_POINTER;
    }

    *ppMediaType = NULL;

    EnterCriticalSection(&m_ObjectLock);

    hr = CheckShutdown();
    if (FAILED(hr))
    {
        goto done;
    }

    if (m_pMediaType == NULL)
    {
        hr = MF_E_NOT_INITIALIZED;
        goto done;
    }

    hr = m_pMediaType->QueryInterface(IID_PPV_ARGS(ppMediaType));

done:
    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}


///////////////////////////////////////////////////////////////////////////////
//
// IMFClockStateSink methods
//
//////////////////////////////////////////////////////////////////////////////

// Note: The IMFClockStateSink interface handles state changes from the EVR,
// such as stopping, starting, and pausing.

//-----------------------------------------------------------------------------
// OnClockStart
//
// Called when:
// (1) The clock starts from the stopped state, or
// (2) The clock seeks (jumps to a new position) while running or paused.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::OnClockStart(MFTIME hnsSystemTime, LONGLONG llClockStartOffset)
{
    EnterCriticalSection(&m_ObjectLock);

    // We cannot start after shutdown.
    HRESULT hr = CheckShutdown();
    if (FAILED(hr))
    {
        goto done;
    }

    // Check if the clock is already active (not stopped).
    if (IsActive())
    {
        m_RenderState = RENDER_STATE_STARTED;

        // If the clock position changes while the clock is active, it
        // is a seek request. We need to flush all pending samples.
        if (llClockStartOffset != PRESENTATION_CURRENT_POSITION)
        {
            Flush();
        }
    }
    else
    {
        m_RenderState = RENDER_STATE_STARTED;

        // The clock has started from the stopped state.

        // Possibly we are in the middle of frame-stepping OR have samples waiting
        // in the frame-step queue. Deal with these two cases first:
        hr = StartFrameStep();
        if (FAILED(hr))
        {
            goto done;
        }
    }

    // Now try to get new output samples from the mixer.
    ProcessOutputLoop();

done:
    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}


//-----------------------------------------------------------------------------
// OnClockRestart
//
// Called when the clock restarts from the current position while paused.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::OnClockRestart(MFTIME hnsSystemTime)
{
    EnterCriticalSection(&m_ObjectLock);

    HRESULT hr = CheckShutdown();
    if (FAILED(hr))
    {
        goto done;
    }

    // The EVR calls OnClockRestart only while paused.
    assert(m_RenderState == RENDER_STATE_PAUSED);

    m_RenderState = RENDER_STATE_STARTED;

    // Possibly we are in the middle of frame-stepping OR we have samples waiting
    // in the frame-step queue. Deal with these two cases first:
    hr = StartFrameStep();
    if (FAILED(hr))
    {
        goto done;
    }

    // Now resume the presentation loop.
    ProcessOutputLoop();

done:
    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}


//-----------------------------------------------------------------------------
// OnClockStop
//
// Called when the clock stops.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::OnClockStop(MFTIME hnsSystemTime)
{
    EnterCriticalSection(&m_ObjectLock);

    HRESULT hr = CheckShutdown();
    if (FAILED(hr))
    {
        goto done;
    }

    if (m_RenderState != RENDER_STATE_STOPPED)
    {
        m_RenderState = RENDER_STATE_STOPPED;
        Flush();

        // If we are in the middle of frame-stepping, cancel it now.
        if (m_FrameStep.state != FRAMESTEP_NONE)
        {
            CancelFrameStep();
        }
    }

done:
    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}

//-----------------------------------------------------------------------------
// OnClockPause
//
// Called when the clock is paused.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::OnClockPause(MFTIME hnsSystemTime)
{
    EnterCriticalSection(&m_ObjectLock);

    // We cannot pause the clock after shutdown.
    HRESULT hr = CheckShutdown();

    if (SUCCEEDED(hr))
    {
        // Set the state. (No other actions are necessary.)
        m_RenderState = RENDER_STATE_PAUSED;
    }

    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}


//-----------------------------------------------------------------------------
// OnClockSetRate
//
// Called when the clock rate changes.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::OnClockSetRate(MFTIME hnsSystemTime, float fRate)
{
    // Note:
    // The presenter reports its maximum rate through the IMFRateSupport interface.
    // Here, we assume that the EVR honors the maximum rate.

    EnterCriticalSection(&m_ObjectLock);

    HRESULT hr = CheckShutdown();
    if (FAILED(hr))
    {
        goto done;
    }

    // If the rate is changing from zero (scrubbing) to non-zero, cancel the
    // frame-step operation.
    if ((m_fRate == 0.0f) && (fRate != 0.0f))
    {
        CancelFrameStep();
        m_FrameStep.samples.Clear();
    }

    m_fRate = fRate;

    // Tell the scheduler about the new rate.
    m_scheduler.SetClockRate(fRate);

done:
    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}


///////////////////////////////////////////////////////////////////////////////
//
// IMFRateSupport methods
//
//////////////////////////////////////////////////////////////////////////////


//-----------------------------------------------------------------------------
// GetSlowestRate
//
// Returns the slowest playback rate that the presenter supports.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::GetSlowestRate(MFRATE_DIRECTION eDirection, BOOL bThin, float *pfRate)
{
    if (pfRate == NULL)
    {
        return E_POINTER;
    }

    EnterCriticalSection(&m_ObjectLock);

    HRESULT hr = CheckShutdown();

    if (SUCCEEDED(hr))
    {
        // There is no minimum playback rate, so the minimum is zero.
        *pfRate = 0;
    }

    LeaveCriticalSection(&m_ObjectLock);
    return S_OK;
}


//-----------------------------------------------------------------------------
// GetFastestRate
//
// Returns the fastest playback rate that the presenter supports.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::GetFastestRate(
    MFRATE_DIRECTION eDirection, 
    BOOL bThin, 
    float *pfRate
    )
{
    if (pfRate == NULL)
    {
        return E_POINTER;
    }

    EnterCriticalSection(&m_ObjectLock);

    float   fMaxRate = 0.0f;

    HRESULT hr = CheckShutdown();
    if (FAILED(hr))
    {
        goto done;
    }

    // Get the maximum *forward* rate.
    fMaxRate = GetMaxRate(bThin);

    // For reverse playback, it's the negative of fMaxRate.
    if (eDirection == MFRATE_REVERSE)
    {
        fMaxRate = -fMaxRate;
    }

    *pfRate = fMaxRate;

done:
    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}


//-----------------------------------------------------------------------------
// IsRateSupported
//
// Checks whether a specified playback rate is supported.
//
// bThin: If TRUE, the query is for thinned playback. Otherwise, the query
//        is for non-thinned playback.
// fRate: Playback rate. This value is negative for reverse playback.
// pfNearestSupportedRate:
//        Receives the rate closest to fRate that the presenter supports.
//        This parameter can be NULL.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::IsRateSupported(
    BOOL bThin, 
    float fRate, 
    float *pfNearestSupportedRate
    )
{
    EnterCriticalSection(&m_ObjectLock);

    float   fMaxRate = 0.0f;
    float   fNearestRate = fRate;  // If we support fRate, that is the nearest.

    HRESULT hr = CheckShutdown();
    if (FAILED(hr))
    {
        goto done;
    }

    // Find the maximum forward rate.
    // Note: We have no minimum rate (that is, we support anything down to 0).
    fMaxRate = GetMaxRate(bThin);

    if (fabsf(fRate) > fMaxRate)
    {
        // The (absolute) requested rate exceeds the maximum rate.
        hr = MF_E_UNSUPPORTED_RATE;

        // The nearest supported rate is fMaxRate.
        fNearestRate = fMaxRate;
        if (fRate < 0)
        {
            // Negative for reverse playback.
            fNearestRate = -fNearestRate;
        }
    }

    // Return the nearest supported rate.
    if (pfNearestSupportedRate != NULL)
    {
        *pfNearestSupportedRate = fNearestRate;
    }

done:
    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}


///////////////////////////////////////////////////////////////////////////////
//
// IMFVideoDisplayControl methods
//
///////////////////////////////////////////////////////////////////////////////

// Note: This sample supports a subset of the methods in IMFVideoDisplayControl.
// Therefore, several of the methods return E_NOTIMPL.

//-----------------------------------------------------------------------------
// SetVideoWindow
//
// Sets the window where the presenter will draw video frames.
// Note: Does not fail after shutdown.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::SetVideoWindow(HWND hwndVideo)
{
    if (!IsWindow(hwndVideo))
    {
        return E_INVALIDARG;
    }

    EnterCriticalSection(&m_ObjectLock);

    HRESULT hr = S_OK;
    HWND oldHwnd = m_pD3DPresentEngine->GetVideoWindow();

    // If the window has changed, notify the D3DPresentEngine object.
    // This will cause a new Direct3D device to be created.
    if (oldHwnd != hwndVideo)
    {
        hr = m_pD3DPresentEngine->SetVideoWindow(hwndVideo);

        // Tell the EVR that the device has changed.
        NotifyEvent(EC_DISPLAY_CHANGED, 0, 0);
    }

    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}

//-----------------------------------------------------------------------------
// GetVideoWindow
//
// Returns a handle to the video window.
// Note: Does not fail after shutdown.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::GetVideoWindow(HWND* phwndVideo)
{
    if (phwndVideo == NULL)
    {
        return E_POINTER;
    }

    EnterCriticalSection(&m_ObjectLock);


    // The D3DPresentEngine object stores the handle.
    *phwndVideo = m_pD3DPresentEngine->GetVideoWindow();

    LeaveCriticalSection(&m_ObjectLock);

    return S_OK;
}


//-----------------------------------------------------------------------------
// SetVideoPosition
//
// Sets the source and target rectangles for the video window.
// Note: Does not fail after shutdown.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::SetVideoPosition(const MFVideoNormalizedRect* pnrcSource, const LPRECT prcDest)
{
    // Validate parameters.

    // One parameter can be NULL, but not both.
    if (pnrcSource == NULL && prcDest == NULL)
    {
        return E_POINTER;
    }

    // Validate the rectangles.
    if (pnrcSource)
    {
        // The source rectangle cannot be flipped.
        if ((pnrcSource->left > pnrcSource->right) ||
            (pnrcSource->top > pnrcSource->bottom))
        {
            return E_INVALIDARG;
        }

        // The source rectangle has range (0..1)
        if ((pnrcSource->left < 0) || (pnrcSource->right > 1) ||
            (pnrcSource->top < 0) || (pnrcSource->bottom > 1))
        {
            return E_INVALIDARG;
        }
    }

    if (prcDest)
    {
        // The destination rectangle cannot be flipped.
        if ((prcDest->left > prcDest->right) ||
            (prcDest->top > prcDest->bottom))
        {
            return E_INVALIDARG;
        }
    }

    HRESULT hr = S_OK;

    EnterCriticalSection(&m_ObjectLock);

    // Update the source rectangle. Source clipping is performed by the mixer.
    if (pnrcSource)
    {
        m_nrcSource = *pnrcSource;

        if (m_pMixer)
        {
            hr = SetMixerSourceRect(m_pMixer, m_nrcSource);
            if (FAILED(hr))
            {
                goto done;
            }
        }
    }

    // Update the destination rectangle.
    if (prcDest)
    {
        RECT rcOldDest = m_pD3DPresentEngine->GetDestinationRect();

        // Check if the destination rectangle changed.
        if (!EqualRect(&rcOldDest, prcDest))
        {
            hr = m_pD3DPresentEngine->SetDestinationRect(*prcDest);
            if (FAILED(hr))
            {
                goto done;
            }

            // Set a new media type on the mixer.
            if (m_pMixer)
            {
                hr = RenegotiateMediaType();
                if (hr == MF_E_TRANSFORM_TYPE_NOT_SET)
                {
                    // This error means that the mixer is not ready for the media type.
                    // Not a failure case -- the EVR will notify us when we need to set
                    // the type on the mixer.
                    hr = S_OK;
                }
                else if (FAILED(hr))
                {
                    goto done;
                }
                else
                {
                    // The media type changed. Request a repaint of the current frame.
                    m_bRepaint = TRUE;
                    (void)ProcessOutput(); // Ignore errors, the mixer might not have a video frame.
                }
            }
        }
    }

done:
    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}


//-----------------------------------------------------------------------------
// GetVideoPosition
//
// Gets the current source and target rectangles.
// Note: Does not fail after shutdown.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::GetVideoPosition(MFVideoNormalizedRect* pnrcSource, LPRECT prcDest)
{
    if (pnrcSource == NULL || prcDest == NULL)
    {
        return E_POINTER;
    }

    EnterCriticalSection(&m_ObjectLock);

    *pnrcSource = m_nrcSource;
    *prcDest = m_pD3DPresentEngine->GetDestinationRect();

    LeaveCriticalSection(&m_ObjectLock);

    return S_OK;
}


//-----------------------------------------------------------------------------
// RepaintVideo
// Repaints the most recent video frame.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::RepaintVideo()
{
    EnterCriticalSection(&m_ObjectLock);

    HRESULT hr = CheckShutdown();

    if (SUCCEEDED(hr))
    {
        // Ignore the request if we have not presented any samples yet.
        if (m_bPrerolled)
        {
            m_bRepaint = TRUE;
            (void)ProcessOutput();
        }
    }

    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}


///////////////////////////////////////////////////////////////////////////////
//
// Private / Protected methods
//
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
// Constructor
//-----------------------------------------------------------------------------

EVRCustomPresenter::EVRCustomPresenter(HRESULT& hr) :
    m_nRefCount(1),
    m_RenderState(RENDER_STATE_SHUTDOWN),
    m_pD3DPresentEngine(NULL),
    m_pClock(NULL),
    m_pMixer(NULL),
    m_pMediaEventSink(NULL),
    m_pMediaType(NULL),
    m_bSampleNotify(FALSE),
    m_bRepaint(FALSE),
    m_bEndStreaming(FALSE),
    m_bPrerolled(FALSE),
    m_fRate(1.0f),
    m_TokenCounter(0),
    m_SampleFreeCB(this, &EVRCustomPresenter::OnSampleFree),
    m_BufferCount(DEFAULT_PRESENTER_BUFFER_COUNT)
{
    DllAddRef();

    InitializeCriticalSection(&m_ObjectLock);

    hr = S_OK;

    // Initial source rectangle = (0,0,1,1)
    m_nrcSource.top = 0;
    m_nrcSource.left = 0;
    m_nrcSource.bottom = 1;
    m_nrcSource.right = 1;

    m_pD3DPresentEngine = new PvpPresentEngine2(hr);
    // m_pD3DPresentEngine = new PvpPresentEngineQueued(hr);
    if (m_pD3DPresentEngine == NULL)
    {
        hr = E_OUTOFMEMORY;
    }

    if (FAILED(hr))
    {
        delete m_pD3DPresentEngine;
        m_pD3DPresentEngine = NULL;
    }
    else
    {
        m_scheduler.SetCallback(m_pD3DPresentEngine);
    }

    LeaveCriticalSection(&m_ObjectLock);
}

//-----------------------------------------------------------------------------
// Destructor
//-----------------------------------------------------------------------------

EVRCustomPresenter::~EVRCustomPresenter()
{
    // COM interfaces
    SafeRelease(&m_pClock);
    SafeRelease(&m_pMixer);
    SafeRelease(&m_pMediaEventSink);
    SafeRelease(&m_pMediaType);

    // Deletable objects
    delete m_pD3DPresentEngine;

    DeleteCriticalSection(&m_ObjectLock);

    DllRelease();
}


//-----------------------------------------------------------------------------
// ConfigureMixer
//
// Initializes the mixer. Called from InitServicePointers.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::ConfigureMixer(IMFTransform *pMixer)
{
    // Make sure that the mixer has the same device ID as ourselves.

    IID mixerID = GUID_NULL;
    IMFVideoDeviceID *pDeviceID = NULL;

    HRESULT hr = pMixer->QueryInterface(IID_PPV_ARGS(&pDeviceID));
    if (FAILED(hr))
    {
        goto done;
    }
    
    hr = pDeviceID->GetDeviceID(&mixerID);
    if (FAILED(hr))
    {
        goto done;
    }

    if (!IsEqualGUID(mixerID, __uuidof(IDirect3DDevice9)))
    {
        hr = MF_E_INVALIDREQUEST;
        goto done;
    }

    // Set the zoom rectangle (ie, the source clipping rectangle).
    SetMixerSourceRect(pMixer, m_nrcSource);

done:
    SafeRelease(&pDeviceID);
    return hr;
}



//-----------------------------------------------------------------------------
// RenegotiateMediaType
//
// Attempts to set an output type on the mixer.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::RenegotiateMediaType()
{
    HRESULT hr = S_OK;
    BOOL bFoundMediaType = FALSE;

    IMFMediaType *pMixerType = NULL;
    IMFMediaType *pOptimalType = NULL;
    IMFVideoMediaType *pVideoType = NULL;

    if (!m_pMixer)
    {
        return MF_E_INVALIDREQUEST;
    }

    // Loop through all of the mixer's proposed output types.
    DWORD iTypeIndex = 0;
    while (!bFoundMediaType && (hr != MF_E_NO_MORE_TYPES))
    {
        SafeRelease(&pMixerType);
        SafeRelease(&pOptimalType);

        // Step 1. Get the next media type supported by mixer.
        hr = m_pMixer->GetOutputAvailableType(0, iTypeIndex++, &pMixerType);
        if (FAILED(hr))
        {
            break;
        }

        // From now on, if anything in this loop fails, try the next type,
        // until we succeed or the mixer runs out of types.

        // Step 2. Check if we support this media type.
        if (SUCCEEDED(hr))
        {
            // Note: None of the modifications that we make later in CreateOptimalVideoType
            // will affect the suitability of the type, at least for us. (Possibly for the mixer.)
            hr = IsMediaTypeSupported(pMixerType);
        }

        // Step 3. Adjust the mixer's type to match our requirements.
        if (SUCCEEDED(hr))
        {
            hr = CreateOptimalVideoType(pMixerType, &pOptimalType);
        }

        // Step 4. Check if the mixer will accept this media type.
        if (SUCCEEDED(hr))
        {
            hr = m_pMixer->SetOutputType(0, pOptimalType, MFT_SET_TYPE_TEST_ONLY);
        }

        // Step 5. Try to set the media type on ourselves.
        if (SUCCEEDED(hr))
        {
            hr = SetMediaType(pOptimalType);
        }

        // Step 6. Set output media type on mixer.
        if (SUCCEEDED(hr))
        {
            hr = m_pMixer->SetOutputType(0, pOptimalType, 0);

            assert(SUCCEEDED(hr)); // This should succeed unless the MFT lied in the previous call.

            // If something went wrong, clear the media type.
            if (FAILED(hr))
            {
                SetMediaType(NULL);
            }
        }

        if (SUCCEEDED(hr))
        {
            bFoundMediaType = TRUE;
        }
    }

    SafeRelease(&pMixerType);
    SafeRelease(&pOptimalType);
    SafeRelease(&pVideoType);

    return hr;
}


//-----------------------------------------------------------------------------
// Flush
//
// Flushes any samples that are waiting to be presented.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::Flush()
{
    m_bPrerolled = FALSE;

    // The scheduler might have samples that are waiting for
    // their presentation time. Tell the scheduler to flush.

    // This call blocks until the scheduler threads discards all scheduled samples.
    m_scheduler.Flush();

    // Flush the frame-step queue.
    m_FrameStep.samples.Clear();

    if (m_RenderState == RENDER_STATE_STOPPED)
    {
        // Repaint with black.
        (void)m_pD3DPresentEngine->PresentSample(NULL, 0);
    }

    return S_OK;
}

//-----------------------------------------------------------------------------
// ProcessInputNotify
//
// Attempts to get a new output sample from the mixer.
//
// This method is called when the EVR sends an MFVP_MESSAGE_PROCESSINPUTNOTIFY
// message, which indicates that the mixer has a new input sample.
//
// Note: If there are multiple input streams, the mixer might not deliver an
// output sample for every input sample.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::ProcessInputNotify()
{
    HRESULT hr = S_OK;

    // Set the flag that says the mixer has a new sample.
    m_bSampleNotify = TRUE;

    if (m_pMediaType == NULL)
    {
        // We don't have a valid media type yet.
        hr = MF_E_TRANSFORM_TYPE_NOT_SET;
    }
    else
    {
        // Try to process an output sample.
        ProcessOutputLoop();
    }
    return hr;
}

//-----------------------------------------------------------------------------
// BeginStreaming
//
// Called when streaming begins.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::BeginStreaming()
{
    HRESULT hr = S_OK;

    // Start the scheduler thread.
    hr = m_scheduler.StartScheduler(m_pClock);

    return hr;
}

//-----------------------------------------------------------------------------
// EndStreaming
//
// Called when streaming ends.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::EndStreaming()
{
    HRESULT hr = S_OK;

    // Stop the scheduler thread.
    hr = m_scheduler.StopScheduler();

    return hr;
}


//-----------------------------------------------------------------------------
// CheckEndOfStream
// Performs end-of-stream actions if the EOS flag was set.
//
// Note: The presenter can receive the EOS notification before it has finished
// presenting all of the scheduled samples. Therefore, signaling EOS and
// handling EOS are distinct operations.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::CheckEndOfStream()
{
    if (!m_bEndStreaming)
    {
        // The EVR did not send the MFVP_MESSAGE_ENDOFSTREAM message.
        return S_OK;
    }

    if (m_bSampleNotify)
    {
        // The mixer still has input.
        return S_OK;
    }

    if (m_SamplePool.AreSamplesPending())
    {
        // Samples are still scheduled for rendering.
        return S_OK;
    }

    // Everything is complete. Now we can tell the EVR that we are done.
    NotifyEvent(EC_COMPLETE, (LONG_PTR)S_OK, 0);
    m_bEndStreaming = FALSE;
    return S_OK;
}


//-----------------------------------------------------------------------------
// PrepareFrameStep
//
// Gets ready to frame step. Called when the EVR sends the MFVP_MESSAGE_STEP
// message.
//
// Note: The EVR can send the MFVP_MESSAGE_STEP message before or after the
// presentation clock starts.
//-----------------------------------------------------------------------------
HRESULT EVRCustomPresenter::PrepareFrameStep(DWORD cSteps)
{
    HRESULT hr = S_OK;

    // Cache the step count.
    m_FrameStep.steps += cSteps;

    // Set the frame-step state.
    m_FrameStep.state = FRAMESTEP_WAITING_START;

    // If the clock is are already running, we can start frame-stepping now.
    // Otherwise, we will start when the clock starts.
    if (m_RenderState == RENDER_STATE_STARTED)
    {
        hr = StartFrameStep();
    }

    return hr;
}

//-----------------------------------------------------------------------------
// StartFrameStep
//
// If the presenter is waiting to frame-step, this method starts the frame-step
// operation. Called when the clock starts OR when the EVR sends the
// MFVP_MESSAGE_STEP message (see PrepareFrameStep).
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::StartFrameStep()
{
    assert(m_RenderState == RENDER_STATE_STARTED);

    HRESULT hr = S_OK;
    IMFSample *pSample = NULL;

    if (m_FrameStep.state == FRAMESTEP_WAITING_START)
    {

        // We have a frame-step request, and are waiting for the clock to start.
        // Set the state to "pending," which means we are waiting for samples.
        m_FrameStep.state = FRAMESTEP_PENDING;

        // If the frame-step queue already has samples, process them now.
        while (!m_FrameStep.samples.IsEmpty() && (m_FrameStep.state == FRAMESTEP_PENDING))
        {
            hr = m_FrameStep.samples.RemoveFront(&pSample);
            if (FAILED(hr))
            {
                goto done;
            }

            hr = DeliverFrameStepSample(pSample);
            if (FAILED(hr))
            {
                goto done;
            }

            SafeRelease(&pSample);

            // We break from this loop when:
            //   (a) the frame-step queue is empty, or
            //   (b) the frame-step operation is complete.
        }
    }
    else if (m_FrameStep.state == FRAMESTEP_NONE)
    {
        // We are not frame stepping. Therefore, if the frame-step queue has samples,
        // we need to process them normally.
        while (!m_FrameStep.samples.IsEmpty())
        {
            hr = m_FrameStep.samples.RemoveFront(&pSample);
            if (FAILED(hr))
            {
                goto done;
            }

            hr = DeliverSample(pSample, FALSE);
            if (FAILED(hr))
            {
                goto done;
            }

            SafeRelease(&pSample);
        }
    }

done:
    SafeRelease(&pSample);
    return hr;
}

//-----------------------------------------------------------------------------
// CompleteFrameStep
//
// Completes a frame-step operation. Called after the frame has been
// rendered.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::CompleteFrameStep(IMFSample *pSample)
{
    HRESULT hr = S_OK;
    MFTIME hnsSampleTime = 0;
    MFTIME hnsSystemTime = 0;

    // Update our state.
    m_FrameStep.state = FRAMESTEP_COMPLETE;
    m_FrameStep.pSampleNoRef = NULL;

    // Notify the EVR that the frame-step is complete.
    NotifyEvent(EC_STEP_COMPLETE, FALSE, 0); // FALSE = completed (not cancelled)

    // If we are scrubbing (rate == 0), also send the "scrub time" event.
    if (IsScrubbing())
    {
        // Get the time stamp from the sample.
        hr = pSample->GetSampleTime(&hnsSampleTime);
        if (FAILED(hr))
        {
            // No time stamp. Use the current presentation time.
            if (m_pClock)
            {
                (void)m_pClock->GetCorrelatedTime(0, &hnsSampleTime, &hnsSystemTime);
            }
            hr = S_OK; // (Not an error condition.)
        }

        NotifyEvent(EC_SCRUB_TIME, LODWORD(hnsSampleTime), HIDWORD(hnsSampleTime));
    }
    return hr;
}

//-----------------------------------------------------------------------------
// CancelFrameStep
//
// Cancels the frame-step operation.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::CancelFrameStep()
{
    FRAMESTEP_STATE oldState = m_FrameStep.state;

    m_FrameStep.state = FRAMESTEP_NONE;
    m_FrameStep.steps = 0;
    m_FrameStep.pSampleNoRef = NULL;
    // Don't clear the frame-step queue yet, because we might frame step again.

    if (oldState > FRAMESTEP_NONE && oldState < FRAMESTEP_COMPLETE)
    {
        // We were in the middle of frame-stepping when it was cancelled.
        // Notify the EVR.
        NotifyEvent(EC_STEP_COMPLETE, TRUE, 0); // TRUE = cancelled
    }
    return S_OK;
}


//-----------------------------------------------------------------------------
// CreateOptimalVideoType
//
// Converts a proposed media type from the mixer into a type that is suitable for the presenter.
//
// pProposedType: Media type that we got from the mixer.
// ppOptimalType: Receives the modfied media type.
//
// The presenter will attempt to set ppOptimalType as the mixer's output format.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::CreateOptimalVideoType(IMFMediaType* pProposedType, IMFMediaType **ppOptimalType)
{
    HRESULT hr = S_OK;

    RECT rcOutput;
    ZeroMemory(&rcOutput, sizeof(rcOutput));

    MFVideoArea displayArea;
    ZeroMemory(&displayArea, sizeof(displayArea));

    IMFMediaType *pMTOptimal = NULL;

    // Clone the proposed type.

    hr = MFCreateMediaType(&pMTOptimal);
    if (FAILED(hr))
    {
        goto done;
    }

    hr = pProposedType->CopyAllItems(pMTOptimal);
    if (FAILED(hr))
    {
        goto done;
    }

    // Modify the new type.

    // For purposes of this SDK sample, we assume
    // 1) The monitor's pixels are square.
    // 2) The presenter always preserves the pixel aspect ratio.

    // Set the pixel aspect ratio (PAR) to 1:1 (see assumption #1, above)
    hr = MFSetAttributeRatio(pMTOptimal, MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
    if (FAILED(hr))
    {
        goto done;
    }

    // Get the output rectangle.
    rcOutput = m_pD3DPresentEngine->GetDestinationRect();
    if (IsRectEmpty(&rcOutput))
    {
        // Calculate the output rectangle based on the media type.
        hr = CalculateOutputRectangle(pProposedType, &rcOutput);
        if (FAILED(hr))
        {
            goto done;
        }
    }

    // Set the extended color information: Use BT.709
    hr = pMTOptimal->SetUINT32(MF_MT_YUV_MATRIX, MFVideoTransferMatrix_BT709);
    if (FAILED(hr))
    {
        goto done;
    }

    hr = pMTOptimal->SetUINT32(MF_MT_TRANSFER_FUNCTION, MFVideoTransFunc_709);
    if (FAILED(hr))
    {
        goto done;
    }

    hr = pMTOptimal->SetUINT32(MF_MT_VIDEO_PRIMARIES, MFVideoPrimaries_BT709);
    if (FAILED(hr))
    {
        goto done;
    }

    hr = pMTOptimal->SetUINT32(MF_MT_VIDEO_NOMINAL_RANGE, MFNominalRange_16_235);
    if (FAILED(hr))
    {
        goto done;
    }

    hr = pMTOptimal->SetUINT32(MF_MT_VIDEO_LIGHTING, MFVideoLighting_dim);
    if (FAILED(hr))
    {
        goto done;
    }

    // Set the target rect dimensions.
    hr = MFSetAttributeSize(pMTOptimal, MF_MT_FRAME_SIZE, rcOutput.right, rcOutput.bottom);
    if (FAILED(hr))
    {
        goto done;
    }

    // Set the geometric aperture, and disable pan/scan.
    displayArea = MakeArea(0, 0, rcOutput.right, rcOutput.bottom);

    hr = pMTOptimal->SetUINT32(MF_MT_PAN_SCAN_ENABLED, FALSE);
    if (FAILED(hr))
    {
        goto done;
    }

    hr = pMTOptimal->SetBlob(MF_MT_GEOMETRIC_APERTURE, (UINT8*)&displayArea, sizeof(displayArea));
    if (FAILED(hr))
    {
        goto done;
    }

    // Set the pan/scan aperture and the minimum display aperture. We don't care
    // about them per se, but the mixer will reject the type if these exceed the
    // frame dimentions.
    hr = pMTOptimal->SetBlob(MF_MT_PAN_SCAN_APERTURE, (UINT8*)&displayArea, sizeof(displayArea));
    if (FAILED(hr))
    {
        goto done;
    }

    hr = pMTOptimal->SetBlob(MF_MT_MINIMUM_DISPLAY_APERTURE, (UINT8*)&displayArea, sizeof(displayArea));
    if (FAILED(hr))
    {
        goto done;
    }

    // Return the pointer to the caller.
    *ppOptimalType = pMTOptimal;
    (*ppOptimalType)->AddRef();

done:
    SafeRelease(&pMTOptimal);
    return hr;

}

//-----------------------------------------------------------------------------
// CalculateOutputRectangle
//
// Calculates the destination rectangle based on the mixer's proposed format.
// This calculation is used if the application did not specify a destination
// rectangle.
//
// Note: The application sets the destination rectangle by calling
// IMFVideoDisplayControl::SetVideoPosition.
//
// This method finds the display area of the mixer's proposed format and
// converts it to the pixel aspect ratio (PAR) of the display.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::CalculateOutputRectangle(IMFMediaType *pProposedType, RECT *prcOutput)
{
    HRESULT hr = S_OK;
    UINT32  srcWidth = 0, srcHeight = 0;

    MFRatio inputPAR = { 0, 0 };
    MFRatio outputPAR = { 0, 0 };
    RECT    rcOutput = { 0, 0, 0, 0};

    MFVideoArea displayArea;
    ZeroMemory(&displayArea, sizeof(displayArea));

    // Get the source's frame dimensions.
    hr = MFGetAttributeSize(pProposedType, MF_MT_FRAME_SIZE, &srcWidth, &srcHeight);
    if (FAILED(hr))
    {
        goto done;
    }

    // Get the source's display area.
    hr = GetVideoDisplayArea(pProposedType, &displayArea);
    if (FAILED(hr))
    {
        goto done;
    }

    // Calculate the x,y offsets of the display area.
    LONG offsetX = (LONG)MFOffsetToFloat(displayArea.OffsetX);
    LONG offsetY = (LONG)MFOffsetToFloat(displayArea.OffsetY);

    // Use the display area if valid. Otherwise, use the entire frame.
    if (displayArea.Area.cx != 0 &&
        displayArea.Area.cy != 0 &&
        offsetX + displayArea.Area.cx <= (LONG)(srcWidth) &&
        offsetY + displayArea.Area.cy <= (LONG)(srcHeight))
    {
        rcOutput.left   = offsetX;
        rcOutput.right  = offsetX + displayArea.Area.cx;
        rcOutput.top    = offsetY;
        rcOutput.bottom = offsetY + displayArea.Area.cy;
    }
    else
    {
        rcOutput.left = 0;
        rcOutput.top = 0;
        rcOutput.right = srcWidth;
        rcOutput.bottom = srcHeight;
    }

    // rcOutput is now either a sub-rectangle of the video frame, or the entire frame.

    // If the pixel aspect ratio of the proposed media type is different from the monitor's,
    // letterbox the video. We stretch the image rather than shrink it.

    inputPAR = GetPixelAspectRatio(pProposedType);    // Defaults to 1:1

    outputPAR.Denominator = outputPAR.Numerator = 1; // This is an assumption of the sample.

    // Adjust to get the correct picture aspect ratio.
    *prcOutput = CorrectAspectRatio(rcOutput, inputPAR, outputPAR);

done:
    return hr;
}


//-----------------------------------------------------------------------------
// SetMediaType
//
// Sets or clears the presenter's media type.
// The type has already been validated.
//-----------------------------------------------------------------------------
HRESULT EVRCustomPresenter::SetMediaType(IMFMediaType *pMediaType)
{
    // Note: pMediaType can be NULL (to clear the type)

    // Clearing the media type is allowed in any state (including shutdown).
    if (pMediaType == NULL)
    {
        SafeRelease(&m_pMediaType);
        ReleaseResources();
        return S_OK;
    }

    MFRatio fps = { 0, 0 };
    VideoSampleList sampleQueue;

    IMFSample *pSample = NULL;

    // Cannot set the media type after shutdown.
    HRESULT hr = CheckShutdown();
    if (FAILED(hr))
    {
        goto done;
    }

    // Check if the new type is actually different.
    // Note: This function safely handles NULL input parameters.
    if (AreMediaTypesEqual(m_pMediaType, pMediaType))
    {
        goto done; // Nothing more to do.
    }

    // We're really changing the type. First get rid of the old type.
    SafeRelease(&m_pMediaType);
    ReleaseResources();

    // Initialize the presenter engine with the new media type.
    // The presenter engine allocates the samples.

    hr = m_pD3DPresentEngine->CreateVideoSamples(pMediaType, sampleQueue, m_BufferCount);
    if (FAILED(hr))
    {
        goto done;
    }

    // Mark each sample with our token counter. If this batch of samples becomes
    // invalid, we increment the counter, so that we know they should be discarded.
    for (
         VideoSampleList::POSITION pos = sampleQueue.FrontPosition();
         pos != sampleQueue.EndPosition();
         pos = sampleQueue.Next(pos)
         )
    {
        hr = sampleQueue.GetItemByPosition(pos, &pSample);
        if (FAILED(hr))
        {
            goto done;
        }

        hr = pSample->SetUINT32(MFSamplePresenter_SampleCounter, m_TokenCounter);
        if (FAILED(hr))
        {
            goto done;
        }

        SafeRelease(&pSample);
    }

    // Add the samples to the sample pool.
    hr = m_SamplePool.Initialize(sampleQueue);
    if (FAILED(hr))
    {
        goto done;
    }

    // Set the frame rate on the scheduler.
    if (SUCCEEDED(GetFrameRate(pMediaType, &fps)) && (fps.Numerator != 0) && (fps.Denominator != 0))
    {
        m_scheduler.SetFrameRate(fps);
    }
    else
    {
        // NOTE: The mixer's proposed type might not have a frame rate, in which case
        // we'll use an arbitary default. (Although it's unlikely the video source
        // does not have a frame rate.)
        m_scheduler.SetFrameRate(g_DefaultFrameRate);
    }

    // Store the media type.
    assert(pMediaType != NULL);
    m_pMediaType = pMediaType;
    m_pMediaType->AddRef();

done:
    if (FAILED(hr))
    {
        ReleaseResources();
    }
    return hr;
}

//-----------------------------------------------------------------------------
// IsMediaTypeSupported
//
// Queries whether the presenter can use a proposed format from the mixer.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::IsMediaTypeSupported(IMFMediaType *pProposed)
{
    D3DFORMAT               d3dFormat = D3DFMT_UNKNOWN;
    BOOL                    bCompressed = FALSE;
    MFVideoInterlaceMode    InterlaceMode = MFVideoInterlace_Unknown;
    MFVideoArea             VideoCropArea;
    UINT32                  width = 0, height = 0;

    // Reject compressed media types.
    HRESULT hr = pProposed->IsCompressedFormat(&bCompressed);
    if (FAILED(hr))
    {
        goto done;
    }

    if (bCompressed)
    {
        hr = MF_E_INVALIDMEDIATYPE;
        goto done;
    }

    // Validate the format.
    hr = GetFourCC(pProposed, (DWORD*)&d3dFormat);
    if (FAILED(hr))
    {
        goto done;
    }

    // The D3DPresentEngine checks whether the format can be used as
    // the back-buffer format for the swap chains.
    hr = m_pD3DPresentEngine->CheckFormat(d3dFormat);
    if (FAILED(hr))
    {
        goto done;
    }

    // Reject interlaced formats.
    hr = pProposed->GetUINT32(MF_MT_INTERLACE_MODE, (UINT32*)&InterlaceMode);
    if (FAILED(hr))
    {
        goto done;
    }

    if (InterlaceMode != MFVideoInterlace_Progressive)
    {
        hr = MF_E_INVALIDMEDIATYPE;
        goto done;
    }

    hr = MFGetAttributeSize(pProposed, MF_MT_FRAME_SIZE, &width, &height);
    if (FAILED(hr))
    {
        goto done;
    }

    // Validate the various apertures (cropping regions) against the frame size.
    // Any of these apertures may be unspecified in the media type, in which case
    // we ignore it. We just want to reject invalid apertures.

    if (SUCCEEDED(pProposed->GetBlob(MF_MT_PAN_SCAN_APERTURE, (UINT8*)&VideoCropArea, sizeof(VideoCropArea), NULL)))
    {
        ValidateVideoArea(VideoCropArea, width, height);
    }
    if (SUCCEEDED(pProposed->GetBlob(MF_MT_GEOMETRIC_APERTURE, (UINT8*)&VideoCropArea, sizeof(VideoCropArea), NULL)))
    {
        ValidateVideoArea(VideoCropArea, width, height);
    }
    if (SUCCEEDED(pProposed->GetBlob(MF_MT_MINIMUM_DISPLAY_APERTURE, (UINT8*)&VideoCropArea, sizeof(VideoCropArea), NULL)))
    {
        ValidateVideoArea(VideoCropArea, width, height);
    }

done:
    return hr;
}


//-----------------------------------------------------------------------------
// ProcessOutputLoop
//
// Get video frames from the mixer and schedule them for presentation.
//-----------------------------------------------------------------------------

void EVRCustomPresenter::ProcessOutputLoop()
{
    HRESULT hr = S_OK;

    // Process as many samples as possible.
    while (hr == S_OK)
    {
        // If the mixer doesn't have a new input sample, break from the loop.
        if (!m_bSampleNotify)
        {
            hr = MF_E_TRANSFORM_NEED_MORE_INPUT;
            break;
        }

        // Try to process a sample.
        hr = ProcessOutput();

        // NOTE: ProcessOutput can return S_FALSE to indicate it did not 
        // process a sample. If so, break out of the loop.
    }

    if (hr == MF_E_TRANSFORM_NEED_MORE_INPUT)
    {
        // The mixer has run out of input data. Check for end-of-stream.
        CheckEndOfStream();
    }
}

//-----------------------------------------------------------------------------
// ProcessOutput
//
// Attempts to get a new output sample from the mixer.
//
// Called in two situations:
// (1) ProcessOutputLoop, if the mixer has a new input sample. 
// (2) Repainting the last frame. 
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::ProcessOutput()
{
    assert(m_bSampleNotify || m_bRepaint);  // See note above.

    HRESULT     hr = S_OK;
    DWORD       dwStatus = 0;
    LONGLONG    mixerStartTime = 0, mixerEndTime = 0;
    MFTIME      systemTime = 0;
    BOOL        bRepaint = m_bRepaint; // Temporarily store this state flag.

    MFT_OUTPUT_DATA_BUFFER dataBuffer;
    ZeroMemory(&dataBuffer, sizeof(dataBuffer));

    IMFSample *pSample = NULL;

    // If the clock is not running, we present the first sample,
    // and then don't present any more until the clock starts.

    if ((m_RenderState != RENDER_STATE_STARTED) &&  // Not running.
         !m_bRepaint &&             // Not a repaint request.
         m_bPrerolled               // At least one sample has been presented.
         )
    {
        return S_FALSE;
    }

    // Make sure we have a pointer to the mixer.
    if (m_pMixer == NULL)
    {
        return MF_E_INVALIDREQUEST;
    }

    // Try to get a free sample from the video sample pool.
    hr = m_SamplePool.GetSample(&pSample);
    if (hr == MF_E_SAMPLEALLOCATOR_EMPTY)
    {
        // No free samples. Try again when a sample is released.
        return S_FALSE; 
    }
    else if (FAILED(hr))
    {
        return hr;
    }

    // From now on, we have a valid video sample pointer, where the mixer will
    // write the video data.
    assert(pSample != NULL);

    // (If the following assertion fires, it means we are not managing the sample pool correctly.)
    assert(MFGetAttributeUINT32(pSample, MFSamplePresenter_SampleCounter, (UINT32)-1) == m_TokenCounter);

    if (m_bRepaint)
    {
        // Repaint request. Ask the mixer for the most recent sample.
        SetDesiredSampleTime(
            pSample, 
            m_scheduler.LastSampleTime(), 
            m_scheduler.FrameDuration()
            );

        m_bRepaint = FALSE; // OK to clear this flag now.
    }
    else
    {
        // Not a repaint request. Clear the desired sample time; the mixer will
        // give us the next frame in the stream.
        ClearDesiredSampleTime(pSample);

        if (m_pClock)
        {
            // Latency: Record the starting time for ProcessOutput.
            (void)m_pClock->GetCorrelatedTime(0, &mixerStartTime, &systemTime);
        }
    }

    // Now we are ready to get an output sample from the mixer.
    dataBuffer.dwStreamID = 0;
    dataBuffer.pSample = pSample;
    dataBuffer.dwStatus = 0;

    hr = m_pMixer->ProcessOutput(0, 1, &dataBuffer, &dwStatus);

    if (FAILED(hr))
    {
        // Return the sample to the pool.
        HRESULT hr2 = m_SamplePool.ReturnSample(pSample);
        if (FAILED(hr2))
        {
            hr = hr2;
            goto done;
        }
        // Handle some known error codes from ProcessOutput.
        if (hr == MF_E_TRANSFORM_TYPE_NOT_SET)
        {
            // The mixer's format is not set. Negotiate a new format.
            hr = RenegotiateMediaType();
        }
        else if (hr == MF_E_TRANSFORM_STREAM_CHANGE)
        {
            // There was a dynamic media type change. Clear our media type.
            SetMediaType(NULL);
        }
        else if (hr == MF_E_TRANSFORM_NEED_MORE_INPUT)
        {
            // The mixer needs more input.
            // We have to wait for the mixer to get more input.
            m_bSampleNotify = FALSE;
        }
    }
    else
    {
        // We got an output sample from the mixer.

        if (m_pClock && !bRepaint)
        {
            // Latency: Record the ending time for the ProcessOutput operation,
            // and notify the EVR of the latency.

            (void)m_pClock->GetCorrelatedTime(0, &mixerEndTime, &systemTime);

            LONGLONG latencyTime = mixerEndTime - mixerStartTime;
            NotifyEvent(EC_PROCESSING_LATENCY, (LONG_PTR)&latencyTime, 0);
        }

        // Set up notification for when the sample is released.
        hr = TrackSample(pSample);
        if (FAILED(hr))
        {
            goto done;
        }

        // Schedule the sample.
        if ((m_FrameStep.state == FRAMESTEP_NONE) || bRepaint)
        {
            hr = DeliverSample(pSample, bRepaint);
            if (FAILED(hr))
            {
                goto done;
            }
        }
        else
        {
            // We are frame-stepping (and this is not a repaint request).
            hr = DeliverFrameStepSample(pSample);
            if (FAILED(hr))
            {
                goto done;
            }
        }

        m_bPrerolled = TRUE; // We have presented at least one sample now.
    }

done:
    SafeRelease(&pSample);

    // Important: Release any events returned from the ProcessOutput method.
    SafeRelease(&dataBuffer.pEvents);
    return hr;
}


//-----------------------------------------------------------------------------
// DeliverSample
//
// Schedule a video sample for presentation.
//
// Called from:
// - ProcessOutput
// - DeliverFrameStepSample
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::DeliverSample(IMFSample *pSample, BOOL bRepaint)
{
    assert(pSample != NULL);

    D3DPresentEngine::DeviceState state = D3DPresentEngine::DeviceOK;

    // If we are not actively playing, OR we are scrubbing (rate = 0) OR this is a
    // repaint request, then we need to present the sample immediately. Otherwise,
    // schedule it normally.

    BOOL bPresentNow = ((m_RenderState != RENDER_STATE_STARTED) ||  IsScrubbing() || bRepaint);

    // Check the D3D device state.
    HRESULT hr = m_pD3DPresentEngine->CheckDeviceState(&state);

    if (SUCCEEDED(hr))
    {
        hr = m_scheduler.ScheduleSample(pSample, bPresentNow);
    }

    if (FAILED(hr))
    {
        // Notify the EVR that we have failed during streaming. The EVR will notify the
        // pipeline (ie, it will notify the Filter Graph Manager in DirectShow or the
        // Media Session in Media Foundation).

        NotifyEvent(EC_ERRORABORT, hr, 0);
    }
    else if (state == D3DPresentEngine::DeviceReset)
    {
        // The Direct3D device was re-set. Notify the EVR.
        NotifyEvent(EC_DISPLAY_CHANGED, S_OK, 0);
    }

    return hr;
}

//-----------------------------------------------------------------------------
// DeliverFrameStepSample
//
// Process a video sample for frame-stepping.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::DeliverFrameStepSample(IMFSample *pSample)
{
    HRESULT hr = S_OK;
    IUnknown *pUnk = NULL;

    // For rate 0, discard any sample that ends earlier than the clock time.
    if (IsScrubbing() && m_pClock && IsSampleTimePassed(m_pClock, pSample))
    {
        // Discard this sample.
    }
    else if (m_FrameStep.state >= FRAMESTEP_SCHEDULED)
    {
        // A frame was already submitted. Put this sample on the frame-step queue,
        // in case we are asked to step to the next frame. If frame-stepping is
        // cancelled, this sample will be processed normally.
        hr = m_FrameStep.samples.InsertBack(pSample);
        if (FAILED(hr))
        {
            goto done;
        }
    }
    else
    {
        // We're ready to frame-step.

        // Decrement the number of steps.
        if (m_FrameStep.steps > 0)
        {
            m_FrameStep.steps--;
        }

        if (m_FrameStep.steps > 0)
        {
            // This is not the last step. Discard this sample.
        }
        else if (m_FrameStep.state == FRAMESTEP_WAITING_START)
        {
            // This is the right frame, but the clock hasn't started yet. Put the
            // sample on the frame-step queue. When the clock starts, the sample
            // will be processed.
            hr = m_FrameStep.samples.InsertBack(pSample);
            if (FAILED(hr))
            {
                goto done;
            }
        }
        else
        {
            // This is the right frame *and* the clock has started. Deliver this sample.
            hr = DeliverSample(pSample, FALSE);
            if (FAILED(hr))
            {
                goto done;
            }

            // Query for IUnknown so that we can identify the sample later.
            // Per COM rules, an object always returns the same pointer when QI'ed for IUnknown.
            hr = pSample->QueryInterface(IID_PPV_ARGS(&pUnk));
            if (FAILED(hr))
            {
                goto done;
            }

            // Save this value.
            m_FrameStep.pSampleNoRef = (DWORD_PTR)pUnk; // No add-ref.

            // NOTE: We do not AddRef the IUnknown pointer, because that would prevent the
            // sample from invoking the OnSampleFree callback after the sample is presented.
            // We use this IUnknown pointer purely to identify the sample later; we never
            // attempt to dereference the pointer.

            // Update our state.
            m_FrameStep.state = FRAMESTEP_SCHEDULED;
        }
    }
done:
    SafeRelease(&pUnk);
    return hr;
}


//-----------------------------------------------------------------------------
// TrackSample
//
// Given a video sample, sets a callback that is invoked when the sample is no
// longer in use.
//
// Note: The callback method returns the sample to the pool of free samples; for
// more information, see EVRCustomPresenter::OnSampleFree().
//
// This method uses the IMFTrackedSample interface on the video sample.
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::TrackSample(IMFSample *pSample)
{
    IMFTrackedSample *pTracked = NULL;

    HRESULT hr = pSample->QueryInterface(IID_PPV_ARGS(&pTracked));

    if (SUCCEEDED(hr))
    {
        hr = pTracked->SetAllocator(&m_SampleFreeCB, NULL);
    }

    SafeRelease(&pTracked);
    return hr;
}


//-----------------------------------------------------------------------------
// ReleaseResources
//
// Releases resources that the presenter uses to render video.
//
// Note: This method flushes the scheduler queue and releases the video samples.
// It does not release helper objects such as the D3DPresentEngine, or free
// the presenter's media type.
//-----------------------------------------------------------------------------

void EVRCustomPresenter::ReleaseResources()
{
    // Increment the token counter to indicate that all existing video samples
    // are "stale." As these samples get released, we'll dispose of them.
    //
    // Note: The token counter is required because the samples are shared
    // between more than one thread, and they are returned to the presenter
    // through an asynchronous callback (OnSampleFree). Without the token, we
    // might accidentally re-use a stale sample after the ReleaseResources
    // method returns.

    m_TokenCounter++;

    Flush();

    m_SamplePool.Clear();

    m_pD3DPresentEngine->ReleaseResources();
}


//-----------------------------------------------------------------------------
// OnSampleFree
//
// Callback that is invoked when a sample is released. For more information,
// see EVRCustomPresenterTrackSample().
//-----------------------------------------------------------------------------

HRESULT EVRCustomPresenter::OnSampleFree(IMFAsyncResult *pResult)
{
    IUnknown *pObject = NULL;
    IMFSample *pSample = NULL;
    IUnknown *pUnk = NULL;

    // Get the sample from the async result object.
    HRESULT hr = pResult->GetObject(&pObject);
    if (FAILED(hr))
    {
        goto done;
    }

    hr = pObject->QueryInterface(IID_PPV_ARGS(&pSample));
    if (FAILED(hr))
    {
        goto done;
    }

    // If this sample was submitted for a frame-step, the frame step operation
    // is complete.

    if (m_FrameStep.state == FRAMESTEP_SCHEDULED)
    {
        // Query the sample for IUnknown and compare it to our cached value.
        hr = pSample->QueryInterface(IID_PPV_ARGS(&pUnk));
        if (FAILED(hr))
        {
            goto done;
        }

        if (m_FrameStep.pSampleNoRef == (DWORD_PTR)pUnk)
        {
            // Notify the EVR.
            hr = CompleteFrameStep(pSample);
            if (FAILED(hr))
            {
                goto done;
            }
        }

        // Note: Although pObject is also an IUnknown pointer, it is not 
        // guaranteed to be the exact pointer value returned through
        // QueryInterface. Therefore, the second QueryInterface call is 
        // required.
    }

    /*** Begin lock ***/

    EnterCriticalSection(&m_ObjectLock);

    UINT32 token = MFGetAttributeUINT32(
        pSample, MFSamplePresenter_SampleCounter, (UINT32)-1);

    if (token == m_TokenCounter)
    {
        // Return the sample to the sample pool.
        hr = m_SamplePool.ReturnSample(pSample);
        if (SUCCEEDED(hr))
        {
            // A free sample is available. Process more data if possible.
            (void)ProcessOutputLoop();
        }
    }

    LeaveCriticalSection(&m_ObjectLock);

    /*** End lock ***/

done:
    if (FAILED(hr))
    {
        NotifyEvent(EC_ERRORABORT, hr, 0);
    }
    SafeRelease(&pObject);
    SafeRelease(&pSample);
    SafeRelease(&pUnk);
    return hr;
}


//-----------------------------------------------------------------------------
// GetMaxRate
//
// Returns the maximum forward playback rate.
// Note: The maximum reverse rate is -1 * MaxRate().
//-----------------------------------------------------------------------------

float EVRCustomPresenter::GetMaxRate(BOOL bThin)
{
    // Non-thinned:
    // If we have a valid frame rate and a monitor refresh rate, the maximum
    // playback rate is equal to the refresh rate. Otherwise, the maximum rate
    // is unbounded (FLT_MAX).

    // Thinned: The maximum rate is unbounded.

    float   fMaxRate = FLT_MAX;
    MFRatio fps = { 0, 0 };
    UINT    MonitorRateHz = 0;

    if (!bThin && (m_pMediaType != NULL))
    {
        GetFrameRate(m_pMediaType, &fps);
        MonitorRateHz = m_pD3DPresentEngine->RefreshRate();

        if (fps.Denominator && fps.Numerator && MonitorRateHz)
        {
            // Max Rate = Refresh Rate / Frame Rate
            fMaxRate = (float)MulDiv(
                MonitorRateHz, fps.Denominator, fps.Numerator);
        }
    }

    return fMaxRate;
}

///////////////////////////////////////////////////////////////////////////////
//
// IPvpPresenter methods
//
///////////////////////////////////////////////////////////////////////////////
HRESULT EVRCustomPresenter::HasNewSurfaceArrived(BOOL *newSurfaceArrived)
{
    if (newSurfaceArrived == NULL)
    {
        return E_POINTER;
    }

    *newSurfaceArrived = FALSE;

    HRESULT hr = ((PvpPresentEngineQueued*)m_pD3DPresentEngine)->HasNewSurfaceArrived(newSurfaceArrived);

    return hr;
}

HRESULT EVRCustomPresenter::GetBackBufferNoRef(IDirect3DSurface9 **ppSurface)
{
    HRESULT hr = S_OK;

    if (ppSurface == NULL)
    {
        return E_POINTER;
    }

    // Make sure we at least return NULL
    *ppSurface = NULL;

    //hr = ((PvpPresentEngineQueued*)m_pD3DPresentEngine)->GetBackBufferNoRef(ppSurface);
    hr = ((PvpPresentEngine2*)m_pD3DPresentEngine)->GetBackBufferNoRef(ppSurface);

    return hr;
}

///////////////////////////////////////////////////////////////////////////////
//
// IPvpPresenter2 methods
//
///////////////////////////////////////////////////////////////////////////////

HRESULT EVRCustomPresenter::RegisterCallback(IPvpPresenterCallback *pCallback)
{
    HRESULT hr = S_OK;

    if (pCallback == NULL)
    {
        hr = E_POINTER;
    }
    else
    {
        hr = ((PvpPresentEngine2*)m_pD3DPresentEngine)->RegisterCallback(pCallback);
    }
    
    return hr;
}

///////////////////////////////////////////////////////////////////////////////
//
// IPvpPresenterConfig methods
//
///////////////////////////////////////////////////////////////////////////////

HRESULT EVRCustomPresenter::SetBufferCount(int bufferCount)
{
    HRESULT hr = S_OK;
    if (bufferCount > 0 && bufferCount < 9)
    {
        m_BufferCount = bufferCount;
    }
    else
    {
        hr = E_FAIL;
    }
    
    return hr;
}


//-----------------------------------------------------------------------------
// Static functions
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// CorrectAspectRatio
//
// Converts a rectangle from one pixel aspect ratio (PAR) to another PAR.
// Returns the corrected rectangle.
//
// For example, a 720 x 486 rect with a PAR of 9:10, when converted to 1x1 PAR, must
// be stretched to 720 x 540.
//-----------------------------------------------------------------------------

RECT CorrectAspectRatio(const RECT& src, const MFRatio& srcPAR, const MFRatio& destPAR)
{
    // Start with a rectangle the same size as src, but offset to the origin (0,0).
    RECT rc = {0, 0, src.right - src.left, src.bottom - src.top};

    // If the source and destination have the same PAR, there is nothing to do.
    // Otherwise, adjust the image size, in two steps:
    //  1. Transform from source PAR to 1:1
    //  2. Transform from 1:1 to destination PAR.

    if ((srcPAR.Numerator != destPAR.Numerator) || (srcPAR.Denominator != destPAR.Denominator))
    {
        // Correct for the source's PAR.

        if (srcPAR.Numerator > srcPAR.Denominator)
        {
            // The source has "wide" pixels, so stretch the width.
            rc.right = MulDiv(rc.right, srcPAR.Numerator, srcPAR.Denominator);
        }
        else if (srcPAR.Numerator < srcPAR.Denominator)
        {
            // The source has "tall" pixels, so stretch the height.
            rc.bottom = MulDiv(rc.bottom, srcPAR.Denominator, srcPAR.Numerator);
        }
        // else: PAR is 1:1, which is a no-op.


        // Next, correct for the target's PAR. This is the inverse operation of the previous.

        if (destPAR.Numerator > destPAR.Denominator)
        {
            // The destination has "wide" pixels, so stretch the height.
            rc.bottom = MulDiv(rc.bottom, destPAR.Numerator, destPAR.Denominator);
        }
        else if (destPAR.Numerator < destPAR.Denominator)
        {
            // The destination has "tall" pixels, so stretch the width.
            rc.right = MulDiv(rc.right, destPAR.Denominator, destPAR.Numerator);
        }
        // else: PAR is 1:1, which is a no-op.
    }

    return rc;
}


//-----------------------------------------------------------------------------
// AreMediaTypesEqual
//
// Tests whether two IMFMediaType's are equal. Either pointer can be NULL.
// (If both pointers are NULL, returns TRUE)
//-----------------------------------------------------------------------------

BOOL AreMediaTypesEqual(IMFMediaType *pType1, IMFMediaType *pType2)
{
    if ((pType1 == NULL) && (pType2 == NULL))
    {
        return TRUE; // Both are NULL.
    }
    else if ((pType1 == NULL) || (pType2 == NULL))
    {
        return FALSE; // One is NULL.
    }

    DWORD dwFlags = 0;
    HRESULT hr = pType1->IsEqual(pType2, &dwFlags);

    return (hr == S_OK);
}


//-----------------------------------------------------------------------------
// ValidateVideoArea:
//
// Returns S_OK if an area is smaller than width x height.
// Otherwise, returns MF_E_INVALIDMEDIATYPE.
//-----------------------------------------------------------------------------

HRESULT ValidateVideoArea(const MFVideoArea& area, UINT32 width, UINT32 height)
{

    float fOffsetX = MFOffsetToFloat(area.OffsetX);
    float fOffsetY = MFOffsetToFloat(area.OffsetY);

    if ( ((LONG)fOffsetX + area.Area.cx > (LONG)width) ||
         ((LONG)fOffsetY + area.Area.cy > (LONG)height) )
    {
        return MF_E_INVALIDMEDIATYPE;
    }
    else
    {
        return S_OK;
    }
}


//-----------------------------------------------------------------------------
// SetDesiredSampleTime
//
// Sets the "desired" sample time on a sample. This tells the mixer to output
// an earlier frame, not the next frame. (Used when repainting a frame.)
//
// This method uses the sample's IMFDesiredSample interface.
//
// hnsSampleTime: Time stamp of the frame that the mixer should output.
// hnsDuration: Duration of the frame.
//
// Note: Before re-using the sample, call ClearDesiredSampleTime to clear
// the desired time.
//-----------------------------------------------------------------------------

HRESULT SetDesiredSampleTime(IMFSample *pSample, const LONGLONG& hnsSampleTime, const LONGLONG& hnsDuration)
{
    if (pSample == NULL)
    {
        return E_POINTER;
    }

    HRESULT hr = S_OK;
    IMFDesiredSample *pDesired = NULL;

    hr = pSample->QueryInterface(IID_PPV_ARGS(&pDesired));
    if (SUCCEEDED(hr))
    {
        // This method has no return value.
        (void)pDesired->SetDesiredSampleTimeAndDuration(hnsSampleTime, hnsDuration);
    }

    SafeRelease(&pDesired);
    return hr;
}


//-----------------------------------------------------------------------------
// ClearDesiredSampleTime
//
// Clears the desired sample time. See SetDesiredSampleTime.
//-----------------------------------------------------------------------------

HRESULT ClearDesiredSampleTime(IMFSample *pSample)
{
    if (pSample == NULL)
    {
        return E_POINTER;
    }

    HRESULT hr = S_OK;

    IMFDesiredSample *pDesired = NULL;
    IUnknown *pUnkSwapChain = NULL;

    // We store some custom attributes on the sample, so we need to cache them
    // and reset them.
    //
    // This works around the fact that IMFDesiredSample::Clear() removes all of the
    // attributes from the sample.

    UINT32 counter = MFGetAttributeUINT32(pSample, MFSamplePresenter_SampleCounter, (UINT32)-1);

    (void)pSample->GetUnknown(MFSamplePresenter_SampleSwapChain, IID_IUnknown, (void**)&pUnkSwapChain);

    hr = pSample->QueryInterface(IID_PPV_ARGS(&pDesired));
    if (SUCCEEDED(hr))
    {
        // This method has no return value.
        (void)pDesired->Clear();

        hr = pSample->SetUINT32(MFSamplePresenter_SampleCounter, counter);
        if (FAILED(hr))
        {
            goto done;
        }

        if (pUnkSwapChain)
        {
            hr = pSample->SetUnknown(MFSamplePresenter_SampleSwapChain, pUnkSwapChain);
            if (FAILED(hr))
            {
                goto done;
            }
        }
    }

done:
    SafeRelease(&pUnkSwapChain);
    SafeRelease(&pDesired);
    return hr;
}


//-----------------------------------------------------------------------------
// IsSampleTimePassed
//
// Returns TRUE if the entire duration of pSample is in the past.
//
// Returns FALSE if all or part of the sample duration is in the future, or if
// the function cannot determined (e.g., if the sample lacks a time stamp).
//-----------------------------------------------------------------------------

BOOL IsSampleTimePassed(IMFClock *pClock, IMFSample *pSample)
{
    if (pSample == NULL || pClock == NULL)
    {
        return E_POINTER;
    }

    HRESULT hr = S_OK;
    MFTIME hnsTimeNow = 0;
    MFTIME hnsSystemTime = 0;
    MFTIME hnsSampleStart = 0;
    MFTIME hnsSampleDuration = 0;

    // The sample might lack a time-stamp or a duration, and the
    // clock might not report a time.

    hr = pClock->GetCorrelatedTime(0, &hnsTimeNow, &hnsSystemTime);

    if (SUCCEEDED(hr))
    {
        hr = pSample->GetSampleTime(&hnsSampleStart);
    }
    if (SUCCEEDED(hr))
    {
        hr = pSample->GetSampleDuration(&hnsSampleDuration);
    }

    if (SUCCEEDED(hr))
    {
        if (hnsSampleStart + hnsSampleDuration < hnsTimeNow)
        {
            return TRUE;
        }
    }

    return FALSE;
}


//-----------------------------------------------------------------------------
// SetMixerSourceRect
//
// Sets the ZOOM rectangle on the mixer.
//-----------------------------------------------------------------------------

HRESULT SetMixerSourceRect(IMFTransform *pMixer, const MFVideoNormalizedRect& nrcSource)
{
    if (pMixer == NULL)
    {
        return E_POINTER;
    }

    IMFAttributes *pAttributes = NULL;

    HRESULT hr = pMixer->GetAttributes(&pAttributes);
    if (SUCCEEDED(hr))
    {
        hr = pAttributes->SetBlob(VIDEO_ZOOM_RECT, (const UINT8*)&nrcSource, sizeof(nrcSource));
        pAttributes->Release();
    }
    return hr;
}

//-----------------------------------------------------------------------------
// Get the display area from a video media type.
//-----------------------------------------------------------------------------

HRESULT GetVideoDisplayArea(IMFMediaType *pType, MFVideoArea *pArea)
{
    HRESULT hr = S_OK;
    BOOL bPanScan = FALSE;
    UINT32 width = 0, height = 0;

    bPanScan = MFGetAttributeUINT32(pType, MF_MT_PAN_SCAN_ENABLED, FALSE);

    // In pan/scan mode, try to get the pan/scan region.
    if (bPanScan)
    {
        hr = pType->GetBlob(MF_MT_PAN_SCAN_APERTURE, (UINT8*)pArea, sizeof(*pArea), NULL);
    }

    // If not in pan/scan mode, or the pan/scan region is not set, get the minimimum display aperture.

    if (!bPanScan || hr == MF_E_ATTRIBUTENOTFOUND)
    {
        hr = pType->GetBlob(MF_MT_MINIMUM_DISPLAY_APERTURE, (UINT8*)pArea, sizeof(*pArea), NULL);

        if (hr == MF_E_ATTRIBUTENOTFOUND)
        {
            // Minimum display aperture is not set.

            // For backward compatibility with some components, check for a geometric aperture.
            hr = pType->GetBlob(MF_MT_GEOMETRIC_APERTURE, (UINT8*)pArea, sizeof(*pArea), NULL);
        }

        // Default: Use the entire video area.

        if (hr == MF_E_ATTRIBUTENOTFOUND)
        {
            hr = MFGetAttributeSize(pType, MF_MT_FRAME_SIZE, &width, &height);
            if (SUCCEEDED(hr))
            {
                *pArea = MakeArea(0.0, 0.0, width, height);
            }
        }
    }

    return hr;
}

// Get the pixel aspect ratio
// Defaults to 1:1 (square pixels)
MFRatio GetPixelAspectRatio(IMFMediaType *pType)
{
    MFRatio PAR = { 0, 0 };
    HRESULT hr = S_OK;

    hr = MFGetAttributeRatio(pType, MF_MT_PIXEL_ASPECT_RATIO, (UINT32*)&PAR.Numerator, (UINT32*)&PAR.Denominator);
    if (FAILED(hr))
    {
        PAR.Numerator = 1;
        PAR.Denominator = 1;
    }
    return PAR;
}


#pragma warning( pop )