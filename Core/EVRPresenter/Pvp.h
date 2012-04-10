#pragma once


MIDL_INTERFACE("8F911837-FF4A-4C38-87F8-02EC6B05785A")
IPvpPresenter : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE GetBackBufferNoRef(IDirect3DSurface9 **ppSurface) = 0;
};

MIDL_INTERFACE("DACEB68E-8716-41F5-85DC-7F5F5D97CC65")
IMediaWindowManager : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE GetMediaWindow(HWND *phwnd) = 0;
};

