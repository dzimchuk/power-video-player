#pragma once

MIDL_INTERFACE("90D15027-388A-44AB-AADF-733D3BCDBC7B")
IPvpPresenterCallback : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE OnNewSurfaceArrived() = 0;
};

MIDL_INTERFACE("8F911837-FF4A-4C38-87F8-02EC6B05785A")
IPvpPresenter : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE HasNewSurfaceArrived(BOOL *newSurfaceArrived) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetBackBufferNoRef(IDirect3DSurface9 **ppSurface) = 0;
};

MIDL_INTERFACE("97229B96-8BB6-4666-849D-680DA312977A")
IPvpPresenter2 : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE RegisterCallback(IPvpPresenterCallback *pCallback) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetBackBufferNoRef(IDirect3DSurface9 **ppSurface) = 0;
};

MIDL_INTERFACE("B3C97321-2C16-457D-AEBC-8AFA7BC9CE4A")
IPvpPresenterConfig : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE SetBufferCount(int bufferCount) = 0;
};

MIDL_INTERFACE("DACEB68E-8716-41F5-85DC-7F5F5D97CC65")
IMediaWindowManager : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE GetMediaWindow(HWND *phwnd) = 0;
};
