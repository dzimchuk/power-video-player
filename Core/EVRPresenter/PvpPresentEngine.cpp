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

HRESULT PvpPresentEngine::OnCreateVideoSamples(D3DPRESENT_PARAMETERS& pp)
{
    return S_OK; // we create our return surface in GetBackBufferNoRef
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

HRESULT PvpPresentEngine::GetBackBufferNoRef(IDirect3DSurface9 **ppSurface)
{
    EnterCriticalSection(&m_ObjectLock);

    HRESULT hr = S_OK;

    if (m_bNewSurfaceArrived && m_pRecentSurface != NULL)
    {
        hr = PrepareReturnSurface();
        m_bNewSurfaceArrived = FALSE;
    }

    if (m_pReturnSurface != NULL)
    {
        *ppSurface = m_pReturnSurface;
    }

    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}

HRESULT PvpPresentEngine::PrepareReturnSurface()
{
    HRESULT hr = S_OK;

    if (m_hwnd == NULL)
    {
        return MF_E_INVALIDREQUEST;
    }
    
    if(!m_pReturnSurface)
    {
        D3DSURFACE_DESC desc;
        
        // Get the surface description
        m_pRecentSurface->GetDesc(&desc);

        // Create a surface the same size as our sample
        hr = this->m_pDevice->CreateRenderTarget(desc.Width, 
                                                 desc.Height, 
                                                 desc.Format, 
                                                 desc.MultiSampleType, 
                                                 desc.MultiSampleQuality, 
                                                 true, 
                                                 &m_pReturnSurface, 
                                                 NULL);
        if(hr != S_OK)
            goto done;
    }
    
    if (m_pReturnSurface)
    {
        D3DSURFACE_DESC originalDesc;
        // Get the surface description of this sample
        m_pRecentSurface->GetDesc(&originalDesc);

        D3DSURFACE_DESC renderDesc;
        // Get the surface description of the render surface
        m_pReturnSurface->GetDesc(&renderDesc);

        // Compare the descriptions to make sure they match
        if(originalDesc.Width != renderDesc.Width || 
           originalDesc.Height != renderDesc.Height ||
           originalDesc.Format != renderDesc.Format)
        {
            // Release the old render surface
            SafeRelease(&m_pReturnSurface);
            
            // Create a new render surface that matches the size of this surface 
            hr = this->m_pDevice->CreateRenderTarget(originalDesc.Width, 
                                                     originalDesc.Height, 
                                                     originalDesc.Format, 
                                                     originalDesc.MultiSampleType, 
                                                     originalDesc.MultiSampleQuality, 
                                                     true, 
                                                     &m_pReturnSurface, 
                                                     NULL);
        if(hr != S_OK)
            goto done;
        }
    }

    if(m_pReturnSurface)
    {
        // Copy the passed surface to our rendered surface
        hr = D3DXLoadSurfaceFromSurface(m_pReturnSurface,
                                        NULL,
                                        NULL,
                                        m_pRecentSurface,
                                        NULL,
                                        NULL,
                                        D3DX_FILTER_NONE,
                                        0);
    }

done:
    return hr;
}



