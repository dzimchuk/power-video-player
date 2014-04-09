#include "EVRPresenter.h"
#include <d3dx9tex.h>

PvpPresentEngine::PvpPresentEngine(HRESULT& hr) : D3DPresentEngine(hr)
{
    m_pRecentSurface = NULL;
    m_pReturnSurface = NULL;
    m_bNewSurfaceArrived = FALSE;
}

PvpPresentEngine::~PvpPresentEngine(void)
{
    OnReleaseResources();
}

void PvpPresentEngine::OnReleaseResources()
{
    SafeRelease(&m_pReturnSurface);
    SafeRelease(&m_pRecentSurface);
    m_bNewSurfaceArrived = FALSE;
}

HRESULT PvpPresentEngine::PresentSwapChain(IDirect3DSwapChain9* pSwapChain, IDirect3DSurface9* pSurface)
{
    EnterCriticalSection(&m_ObjectLock);

    if (m_pRecentSurface != pSurface) // 'borrow' the latest surface
    {
        CopyComPointer(m_pRecentSurface, pSurface);
        m_bNewSurfaceArrived = TRUE;
    }
    
    LeaveCriticalSection(&m_ObjectLock);
    return S_OK;
}

void PvpPresentEngine::PaintFrameWithGDI()
{
}

HRESULT PvpPresentEngine::OnCreateVideoSamples(D3DPRESENT_PARAMETERS& pp, int bufferCount)
{
    int hr = this->m_pDevice->CreateRenderTarget(pp.BackBufferWidth, 
                                                 pp.BackBufferHeight, 
                                                 pp.BackBufferFormat, 
                                                 pp.MultiSampleType, 
                                                 pp.MultiSampleQuality, 
                                                 true, 
                                                 &m_pReturnSurface, 
                                                 NULL);

    return hr;
}

HRESULT PvpPresentEngine::GetService(REFGUID guidService, REFIID riid, void** ppv)
{
    assert(ppv != NULL);

    HRESULT hr = S_OK;

    if (m_pDeviceManager == NULL)
    {
        hr = MF_E_UNSUPPORTED_SERVICE;
        goto done;
    }

    if (guidService == MR_VIDEO_RENDER_SERVICE && riid == __uuidof(IDirect3DDeviceManager9))
    {
         *ppv = m_pDeviceManager;
         m_pDeviceManager->AddRef();
    }
    /*else if (guidService == MR_VIDEO_ACCELERATION_SERVICE)
    {
        hr = m_pDeviceManager->QueryInterface(__uuidof(IDirect3DDeviceManager9), (void**) ppv);
    }*/
    else
    {
        hr = MF_E_UNSUPPORTED_SERVICE;
    }

done:
    return hr;
}

HRESULT PvpPresentEngine::HasNewSurfaceArrived(BOOL *newSurfaceArrived)
{
    EnterCriticalSection(&m_ObjectLock);

    *newSurfaceArrived = m_bNewSurfaceArrived;

    LeaveCriticalSection(&m_ObjectLock);
    return S_OK;
}

HRESULT PvpPresentEngine::GetBackBufferNoRef(IDirect3DSurface9 **ppSurface)
{
    EnterCriticalSection(&m_ObjectLock);

    HRESULT hr = S_OK;
    *ppSurface = NULL;

    if (m_bNewSurfaceArrived && m_pRecentSurface != NULL)
    {
        hr = D3DXLoadSurfaceFromSurface(m_pReturnSurface,
                                        NULL,
                                        NULL,
                                        m_pRecentSurface,
                                        NULL,
                                        NULL,
                                        D3DX_FILTER_NONE,
                                        0);

        m_bNewSurfaceArrived = FALSE;

        if (SUCCEEDED(hr))
        {
            *ppSurface = m_pReturnSurface;
        }
    }

    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}




