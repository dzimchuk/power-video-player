#include "EVRPresenter.h"
#include <d3dx9tex.h>

PvpPresentEngine2::PvpPresentEngine2(HRESULT& hr) : D3DPresentEngine(hr)
{
    m_pRecentSurface = NULL;
    m_pReturnSurface = NULL;
    m_pCallback = NULL;
}

PvpPresentEngine2::~PvpPresentEngine2(void)
{
    OnReleaseResources();
}

void PvpPresentEngine2::OnReleaseResources()
{
    SafeRelease(&m_pReturnSurface);
    SafeRelease(&m_pRecentSurface);
}

HRESULT PvpPresentEngine2::PresentSwapChain(IDirect3DSwapChain9* pSwapChain, IDirect3DSurface9* pSurface)
{
    HRESULT hr = S_OK;

    if (m_pCallback != NULL && m_pRecentSurface != pSurface) // 'borrow' the latest surface
    {
        CopyComPointer(m_pRecentSurface, pSurface);

        hr = m_pCallback->OnNewSurfaceArrived();
    }

    return hr;
}

void PvpPresentEngine2::PaintFrameWithGDI()
{
}

HRESULT PvpPresentEngine2::OnCreateVideoSamples(D3DPRESENT_PARAMETERS& pp, int bufferCount)
{
    HRESULT hr = S_OK;

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

HRESULT PvpPresentEngine2::RegisterCallback(IPvpPresenterCallback *pCallback)
{
    m_pCallback = pCallback;
    return S_OK;
}

HRESULT PvpPresentEngine2::GetBackBufferNoRef(IDirect3DSurface9 **ppSurface)
{
    EnterCriticalSection(&m_ObjectLock);

    HRESULT hr = S_OK;
    *ppSurface = NULL;

    if (m_pRecentSurface != NULL)
    {
        hr = D3DXLoadSurfaceFromSurface(m_pReturnSurface,
                                        NULL,
                                        NULL,
                                        m_pRecentSurface,
                                        NULL,
                                        NULL,
                                        D3DX_FILTER_NONE,
                                        0);

        if (SUCCEEDED(hr))
        {
            *ppSurface = m_pReturnSurface;
        }
    }

    LeaveCriticalSection(&m_ObjectLock);
    return hr;
}
