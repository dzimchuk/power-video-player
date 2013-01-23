#include "EVRPresenter.h"

const static TCHAR szWindowName[] = TEXT("PVP_MediaWindow");

//-----------------------------------------------------------------------------
// CreateInstance
//
// Static method to create an instance of the object.
// Used by the class factory.
//-----------------------------------------------------------------------------
HRESULT MediaWindowManager::CreateInstance(REFIID iid, void **ppv)
{
    if (ppv == NULL)
    {
        return E_POINTER;
    }

    HRESULT hr = S_OK;

    MediaWindowManager *pObject = new MediaWindowManager(hr);

    if (pObject == NULL)
    {
        hr = E_OUTOFMEMORY;
    }

    if (SUCCEEDED(hr))
    {
        hr = pObject->QueryInterface(iid, ppv);
    }

    SafeRelease(&pObject);
    return hr;
}

///////////////////////////////////////////////////////////////////////////////
//
// IUnknown methods
//
///////////////////////////////////////////////////////////////////////////////

HRESULT MediaWindowManager::QueryInterface(REFIID riid, void ** ppv)
{
    static const QITAB qit[] = 
    {
        QITABENT(MediaWindowManager, IMediaWindowManager),
        { 0 }
    };
    return QISearch(this, qit, riid, ppv);
}

ULONG MediaWindowManager::AddRef()
{
    return InterlockedIncrement(&m_nRefCount);
}

ULONG MediaWindowManager::Release()
{
    assert(m_nRefCount >= 0);
    ULONG uCount = InterlockedDecrement(&m_nRefCount);
    if (uCount == 0)
    {
        delete this;
    }
    return uCount;
}


MediaWindowManager::MediaWindowManager(HRESULT& hr) : m_nRefCount(1),
                                                      m_bClassRegistered(FALSE),
                                                      m_hMediaWindow(NULL)
{
    DllAddRef();

    hr = CreateMediaWindow();
}


MediaWindowManager::~MediaWindowManager(void)
{
    if (m_hMediaWindow)
    {
        DestroyWindow(m_hMediaWindow);
    }

    if (m_bClassRegistered)
    {
        UnregisterClass(szWindowName, NULL);
    }

    DllRelease();
}

HRESULT MediaWindowManager::CreateMediaWindow()
{
    HRESULT hr = S_OK;
  
    if (!m_hMediaWindow)
    {
        WNDCLASS wndclass;
  
        wndclass.style = CS_HREDRAW | CS_VREDRAW;
        wndclass.lpfnWndProc = DefWindowProc;
        wndclass.cbClsExtra = 0;
        wndclass.cbWndExtra = 0;
        wndclass.hInstance = NULL;
        wndclass.hIcon = LoadIcon(NULL, IDI_APPLICATION);
        wndclass.hCursor = LoadCursor(NULL, IDC_ARROW);
        wndclass.hbrBackground = (HBRUSH) GetStockObject (WHITE_BRUSH);
        wndclass.lpszMenuName = NULL;
        wndclass.lpszClassName = szWindowName;
  
        if (!RegisterClass(&wndclass))
        {
            hr = E_FAIL;
            goto Cleanup;
        }

        m_bClassRegistered = TRUE;
  
        m_hMediaWindow = CreateWindow(szWindowName,
                                      szWindowName,
                                      WS_OVERLAPPEDWINDOW, 
                                      0,                   // Initial X
                                      0,                   // Initial Y
                                      0,                   // Width
                                      0,                   // Height
                                      NULL,
                                      NULL,
                                      NULL,
                                      NULL);

        if (m_hMediaWindow == NULL)
        {
            hr = E_FAIL;
            goto Cleanup;
        }
    }
  
Cleanup:
    return hr;
}

HRESULT MediaWindowManager::GetMediaWindow(HWND *phwnd)
{
    HRESULT hr = S_OK;

    if (phwnd == NULL)
    {
        return E_POINTER;
    }

    *phwnd = m_hMediaWindow;

    return hr;
}