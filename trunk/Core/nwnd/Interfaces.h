#pragma once

MIDL_INTERFACE("7DCDEE21-0AAF-4A2F-8928-FDC0043FC2C9")
IMediaWindowCallback : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE OnMessageReceived(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam) = 0;
};

MIDL_INTERFACE("DACEB68E-8716-41F5-85DC-7F5F5D97CC65")
IMediaWindowManager : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE CreateMediaWindow(HWND *phwnd, HWND hParent, int x, int y, int nWidth, int nHeight, DWORD dwStyle) = 0;
    virtual HRESULT STDMETHODCALLTYPE SetRunning(BOOL running, IVMRWindowlessControl* pVMR, IVMRWindowlessControl9* pVMR9, IMFVideoDisplayControl* pEVR) = 0;
    virtual HRESULT STDMETHODCALLTYPE InvalidateMediaWindow() = 0;
    virtual HRESULT STDMETHODCALLTYPE SetLogo(HBITMAP logo) = 0;
    virtual HRESULT STDMETHODCALLTYPE ShowLogo(BOOL show) = 0;
    virtual HRESULT STDMETHODCALLTYPE RegisterCallback(IMediaWindowCallback *pCallback) = 0;
};