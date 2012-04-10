#pragma once


class PvpPresentEngine : public D3DPresentEngine
{
public:
    PvpPresentEngine(HRESULT& hr);
    ~PvpPresentEngine(void);

    void OnReleaseResources();
    HRESULT OnCreateVideoSamples(D3DPRESENT_PARAMETERS& pp);
    HRESULT PresentSwapChain(IDirect3DSwapChain9* pSwapChain, IDirect3DSurface9* pSurface);
    void    PaintFrameWithGDI();
    HRESULT GetService(REFGUID guidService, REFIID riid, void** ppv);

    HRESULT STDMETHODCALLTYPE GetBackBufferNoRef(IDirect3DSurface9 **ppSurface);

private:
    HRESULT PrepareReturnSurface();
    
    IDirect3DSurface9 *m_pRecentSurface;
    IDirect3DSurface9 *m_pReturnSurface;

    BOOL			  m_bNewSurfaceArrived;
};

