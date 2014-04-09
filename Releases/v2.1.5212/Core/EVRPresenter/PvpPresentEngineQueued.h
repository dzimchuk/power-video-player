#pragma once

class PvpPresentEngineQueued : public D3DPresentEngine
{
public:
    PvpPresentEngineQueued(HRESULT& hr);
    virtual ~PvpPresentEngineQueued(void);

    void OnReleaseResources();
    HRESULT OnCreateVideoSamples(D3DPRESENT_PARAMETERS& pp, int bufferCount);
    HRESULT PresentSwapChain(IDirect3DSwapChain9* pSwapChain, IDirect3DSurface9* pSurface);
    void    PaintFrameWithGDI();

    HRESULT STDMETHODCALLTYPE GetBackBufferNoRef(IDirect3DSurface9 **ppSurface);
    HRESULT STDMETHODCALLTYPE HasNewSurfaceArrived(BOOL *newSurfaceArrived);

private:
    ThreadSafeQueue<IDirect3DSurface9>  m_RenderedSurfaces;
    ThreadSafeQueue<IDirect3DSurface9>  m_AvailableSurfaces;
    
    IDirect3DSurface9 *m_pReturnSurface;
};

