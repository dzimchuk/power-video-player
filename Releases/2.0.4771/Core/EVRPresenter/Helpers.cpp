//////////////////////////////////////////////////////////////////////////
//
// Helpers.cpp : Miscellaneous helpers.
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


//-----------------------------------------------------------------------------
// SamplePool class
//-----------------------------------------------------------------------------

SamplePool::SamplePool() : m_bInitialized(FALSE), m_cPending(0)
{
    InitializeCriticalSection(&m_lock);
}

SamplePool::~SamplePool()
{
    DeleteCriticalSection(&m_lock);
}


//-----------------------------------------------------------------------------
// GetSample
//
// Gets a sample from the pool. If no samples are available, the method
// returns MF_E_SAMPLEALLOCATOR_EMPTY.
//-----------------------------------------------------------------------------

HRESULT SamplePool::GetSample(IMFSample **ppSample)
{
    EnterCriticalSection(&m_lock);

    if (!m_bInitialized)
    {
        LeaveCriticalSection(&m_lock);
        return MF_E_NOT_INITIALIZED;
    }

    if (m_VideoSampleQueue.IsEmpty())
    {
        LeaveCriticalSection(&m_lock);
        return MF_E_SAMPLEALLOCATOR_EMPTY;
    }


    // Get a sample from the allocated queue.

    // It doesn't matter if we pull them from the head or tail of the list,
    // but when we get it back, we want to re-insert it onto the opposite end.
    // (see ReturnSample)

    IMFSample *pSample = NULL;

    HRESULT hr = m_VideoSampleQueue.RemoveFront(&pSample);

    if (SUCCEEDED(hr))
    {
        m_cPending++;

        // Give the sample to the caller.
        *ppSample = pSample;
        (*ppSample)->AddRef();
    }

    SafeRelease(&pSample);
    LeaveCriticalSection(&m_lock);
    return hr;
}

//-----------------------------------------------------------------------------
// ReturnSample
//
// Returns a sample to the pool.
//-----------------------------------------------------------------------------

HRESULT SamplePool::ReturnSample(IMFSample *pSample)
{
    EnterCriticalSection(&m_lock);

    if (!m_bInitialized)
    {
        LeaveCriticalSection(&m_lock);
        return MF_E_NOT_INITIALIZED;
    }

    HRESULT hr = m_VideoSampleQueue.InsertBack(pSample);

    if (SUCCEEDED(hr))
    {
        m_cPending--;
    }

    LeaveCriticalSection(&m_lock);
    return hr;
}

//-----------------------------------------------------------------------------
// AreSamplesPending
//
// Returns TRUE if any samples are in use.
//-----------------------------------------------------------------------------

BOOL SamplePool::AreSamplesPending()
{
    EnterCriticalSection(&m_lock);

    BOOL bRet = FALSE;

    if (!m_bInitialized)
    {
        bRet = FALSE;
    }
    else
    {
        bRet = (m_cPending > 0);
    }

    LeaveCriticalSection(&m_lock);
    return bRet;
}


//-----------------------------------------------------------------------------
// Initialize
//
// Initializes the pool with a list of samples.
//-----------------------------------------------------------------------------

HRESULT SamplePool::Initialize(VideoSampleList& samples)
{
    EnterCriticalSection(&m_lock);

    if (m_bInitialized)
    {
        LeaveCriticalSection(&m_lock);
        return MF_E_INVALIDREQUEST;
    }

    HRESULT hr = S_OK;

    IMFSample *pSample = NULL;

    // Move these samples into our allocated queue.
    VideoSampleList::POSITION pos = samples.FrontPosition();
    while (pos != samples.EndPosition())
    {
        hr = samples.GetItemByPosition(pos, &pSample);
        if (FAILED(hr))
        {
            goto done;
        }
        
        hr = m_VideoSampleQueue.InsertBack(pSample);
        if (FAILED(hr))
        {
            goto done;
        }

        pos = samples.Next(pos);
        SafeRelease(&pSample);
    }

    m_bInitialized = TRUE;

done:
    samples.Clear();
    SafeRelease(&pSample);
    LeaveCriticalSection(&m_lock);
    return hr;
}


//-----------------------------------------------------------------------------
// Clear
//
// Releases all samples.
//-----------------------------------------------------------------------------

HRESULT SamplePool::Clear()
{
    HRESULT hr = S_OK;

    EnterCriticalSection(&m_lock);

    m_VideoSampleQueue.Clear();
    m_bInitialized = FALSE;
    m_cPending = 0;


    LeaveCriticalSection(&m_lock);
    return S_OK;
}

