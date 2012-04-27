#pragma once

class PvpPresentEngine2 : public D3DPresentEngine
{
public:
    PvpPresentEngine2(HRESULT& hr);
    virtual ~PvpPresentEngine2(void);

    void OnReleaseResources();
    HRESULT OnCreateVideoSamples(D3DPRESENT_PARAMETERS& pp, int bufferCount);
    HRESULT PresentSwapChain(IDirect3DSwapChain9* pSwapChain, IDirect3DSurface9* pSurface);
    void    PaintFrameWithGDI();

    HRESULT STDMETHODCALLTYPE RegisterCallback(IPvpPresenterCallback *pCallback);
    HRESULT STDMETHODCALLTYPE GetBackBufferNoRef(IDirect3DSurface9 **ppSurface);

private:
    IDirect3DSurface9 *m_pRecentSurface;
    IDirect3DSurface9 *m_pReturnSurface;

    IPvpPresenterCallback *m_pCallback;
};

