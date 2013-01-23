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

    // IMediaWindowManager
    STDMETHOD(GetMediaWindow)(HWND *phwnd);

private:
    HRESULT CreateMediaWindow();

    BOOL m_bClassRegistered;
    HWND m_hMediaWindow;

    long m_nRefCount;
};

inline HRESULT MediaWindowManager_CreateInstance(REFIID riid, void **ppv)
{
    return MediaWindowManager::CreateInstance(riid, ppv);
}