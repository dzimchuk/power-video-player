#pragma once


class MediaWindowManager : public IMediaWindowManager
{
public:
    static HRESULT CreateInstance(REFIID iid, void **ppv);

    // IUnknown methods
    STDMETHOD(QueryInterface)(REFIID riid, void** ppv);
    STDMETHOD_(ULONG, AddRef)();
    STDMETHOD_(ULONG, Release)();

    MediaWindowManager(HRESULT& hr);
    virtual ~MediaWindowManager(void);

    LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);

    // IMediaWindowManager
    STDMETHOD(CreateMediaWindow)(HWND *phwnd, HWND hParent, int x, int y, int nWidth, int nHeight, DWORD dwStyle);
    STDMETHOD(SetRunning)(BOOL running, 
                          IVMRWindowlessControl* pVMR, 
                          IVMRWindowlessControl9* pVMR9,
                          IMFVideoDisplayControl* pEVR);
    STDMETHOD(InvalidateMediaWindow)();
    STDMETHOD(SetLogo)(HBITMAP logo);
    STDMETHOD(ShowLogo)(BOOL show);
    STDMETHOD(RegisterCallback)(IMediaWindowCallback *pCallback);

private:
    static LRESULT CALLBACK InitialWndProc(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam);
    static LRESULT CALLBACK StaticWndProc(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam);

    HRESULT CreateMediaWindow(HWND hParent, int x, int y, int nWidth, int nHeight, DWORD dwStyle);
    void OnPaint(HDC hDC);

    BOOL m_bRunning;
    BOOL m_bShowLogo;
    HBITMAP m_hLogo;
    IVMRWindowlessControl *m_pVMRWindowlessControl;
    IVMRWindowlessControl9 *m_pVMRWindowlessControl9;
    IMFVideoDisplayControl *m_pMFVideoDisplayControl;

    IMediaWindowCallback *m_pCallback;

    BOOL m_bClassRegistered;
    HWND m_hMediaWindow;

    long m_nRefCount;
};

inline HRESULT MediaWindowManager_CreateInstance(REFIID riid, void **ppv)
{
    return MediaWindowManager::CreateInstance(riid, ppv);
}