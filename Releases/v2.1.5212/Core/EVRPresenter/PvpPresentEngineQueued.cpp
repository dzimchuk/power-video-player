#include "EVRPresenter.h"
#include <d3dx9tex.h>

PvpPresentEngineQueued::PvpPresentEngineQueued(HRESULT& hr) : D3DPresentEngine(hr)
{
    m_pReturnSurface = NULL;
}

PvpPresentEngineQueued::~PvpPresentEngineQueued(void)
{
    OnReleaseResources();
}

void PvpPresentEngineQueued::OnReleaseResources()
{
    m_RenderedSurfaces.Clear();
    m_AvailableSurfaces.Clear();

    SafeRelease(&m_pReturnSurface);
}

HRESULT PvpPresentEngineQueued::PresentSwapChain(IDirect3DSwapChain9* pSwapChain, IDirect3DSurface9* pSurface)
{
    HRESULT hr = S_OK;

    IDirect3DSurface9 *pRenderSurface = NULL;
    if (m_AvailableSurfaces.Dequeue(&pRenderSurface) == S_OK)
    {
        hr = D3DXLoadSurfaceFromSurface(pRenderSurface,
                                        NULL,
                                        NULL,
                                        pSurface,
                                        NULL,
                                        NULL,
                                        D3DX_FILTER_NONE,
                                        0);

        m_RenderedSurfaces.Queue(pRenderSurface);
        pRenderSurface->Release();
    }

    return hr;
}

void PvpPresentEngineQueued::PaintFrameWithGDI()
{
}

HRESULT PvpPresentEngineQueued::OnCreateVideoSamples(D3DPRESENT_PARAMETERS& pp, int bufferCount)
{
    HRESULT hr = S_OK;

    for(int i = 0; i < bufferCount; i++)
    {
        IDirect3DSurface9 *pSurface = NULL;
        int hr = this->m_pDevice->CreateRenderTarget(pp.BackBufferWidth, 
                                                     pp.BackBufferHeight, 
                                                     pp.BackBufferFormat, 
                                                     pp.MultiSampleType, 
                                                     pp.MultiSampleQuality, 
                                                     true, 
                                                     &pSurface, 
                                                     NULL);
        if(FAILED(hr))
        {
            break;
        }

        hr = m_AvailableSurfaces.Queue(pSurface);
        pSurface->Release();

        if(FAILED(hr))
        {
            break;
        }
    }

    hr = this->m_pDevice->CreateRenderTarget(pp.BackBufferWidth, 
                                             pp.BackBufferHeight, 
                                             pp.BackBufferFormat, 
                                             pp.MultiSampleType, 
                                             pp.MultiSampleQuality, 
                                             true, 
                                             &m_pReturnSurface, 
                                             NULL);

    return hr;
}

HRESULT PvpPresentEngineQueued::HasNewSurfaceArrived(BOOL *newSurfaceArrived)
{
    IDirect3DSurface9 *pSurface = NULL;
    if (m_RenderedSurfaces.Dequeue(&pSurface) == S_OK)
    {
        m_RenderedSurfaces.PutBack(pSurface);
        pSurface->Release();
        *newSurfaceArrived = TRUE;
    }
    else
    {
        *newSurfaceArrived = FALSE;
    }

    return S_OK;
}

HRESULT PvpPresentEngineQueued::GetBackBufferNoRef(IDirect3DSurface9 **ppSurface)
{
    HRESULT hr = S_OK;
    *ppSurface = NULL;

    EnterCriticalSection(&m_ObjectLock); // to safely release and possibly re-create resources

    IDirect3DSurface9 *pSurface = NULL;
    if (m_RenderedSurfaces.Dequeue(&pSurface) == S_OK)
    {
        hr = D3DXLoadSurfaceFromSurface(m_pReturnSurface,
                                        NULL,
                                        NULL,
                                        pSurface,
                                        NULL,
                                        NULL,
                                        D3DX_FILTER_NONE,
                                        0);

        m_AvailableSurfaces.Queue(pSurface);
        pSurface->Release();

        if (SUCCEEDED(hr))
        {
            *ppSurface = m_pReturnSurface;
        }
    }
        
    LeaveCriticalSection(&m_ObjectLock);

    return hr;
}