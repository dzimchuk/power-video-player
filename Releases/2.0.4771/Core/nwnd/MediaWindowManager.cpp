#include "stdafx.h"

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
    //assert(m_nRefCount >= 0);
    ULONG uCount = InterlockedDecrement(&m_nRefCount);
    if (uCount == 0)
    {
        delete this;
    }
    return uCount;
}


MediaWindowManager::MediaWindowManager(HRESULT& hr) : m_nRefCount(1),
                                                      m_bClassRegistered(FALSE),
                                                      m_hMediaWindow(NULL),
                                                      m_bRunning(FALSE),
                                                      m_bShowLogo(FALSE),
                                                      m_hLogo(NULL),
                                                      m_pVMRWindowlessControl(NULL),
                                                      m_pVMRWindowlessControl9(NULL),
                                                      m_pMFVideoDisplayControl(NULL),
                                                      m_pCallback(NULL)
{
    DllAddRef();
    hr = S_OK;
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

    if (m_hLogo != NULL)
    {
        DeleteObject(m_hLogo);
    }

    DllRelease();
}

LRESULT CALLBACK MediaWindowManager::InitialWndProc(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam) 
{
      if (Msg == WM_NCCREATE) 
      {
        LPCREATESTRUCT create_struct = reinterpret_cast<LPCREATESTRUCT>(lParam);
        void * lpCreateParam = create_struct->lpCreateParams;
        MediaWindowManager *this_window = reinterpret_cast<MediaWindowManager*>(lpCreateParam);
        SetWindowLongPtr(hWnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(this_window));
        SetWindowLongPtr(hWnd, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(&MediaWindowManager::StaticWndProc));
        return this_window->WndProc(hWnd, Msg, wParam, lParam);
      }
      return DefWindowProc(hWnd, Msg, wParam, lParam);
}

LRESULT CALLBACK MediaWindowManager::StaticWndProc(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam) 
{
    LONG_PTR user_data = GetWindowLongPtr(hWnd, GWLP_USERDATA);
    MediaWindowManager *this_window = reinterpret_cast<MediaWindowManager*>(user_data);
    return this_window->WndProc(hWnd, Msg, wParam, lParam);
}

HRESULT MediaWindowManager::CreateMediaWindow(HWND hParent, int x, int y, int nWidth, int nHeight, DWORD dwStyle)
{
    HRESULT hr = S_OK;
  
    if (!m_hMediaWindow)
    {
        WNDCLASSEX wcex;
        wcex.cbSize = sizeof(WNDCLASSEX); 
        wcex.style			= CS_HREDRAW | CS_VREDRAW | CS_DBLCLKS;
        wcex.lpfnWndProc	= &MediaWindowManager::InitialWndProc;
        wcex.cbClsExtra		= 0;
        wcex.cbWndExtra		= 0;
        wcex.hInstance		= GetDllInstanceHandle();
        wcex.hIcon			= NULL;
        wcex.hCursor		= LoadCursor(NULL, IDC_ARROW);
        wcex.hbrBackground	= NULL;
        wcex.lpszMenuName	= NULL;
        wcex.lpszClassName	= szWindowName;
        wcex.hIconSm		= NULL;
  
        if (!RegisterClassEx(&wcex))
        {
            hr = E_FAIL;
            goto Cleanup;
        }

        m_bClassRegistered = TRUE;
  
        m_hMediaWindow = CreateWindow(szWindowName,
                                      szWindowName,
                                      dwStyle, 
                                      x,                   // Initial X
                                      y,                   // Initial Y
                                      nWidth,              // Width
                                      nHeight,             // Height
                                      hParent,
                                      NULL,
                                      GetDllInstanceHandle(),
                                      this);

        if (m_hMediaWindow == NULL)
        {
            hr = E_FAIL;
            goto Cleanup;
        }
    }
  
Cleanup:
    return hr;
}

LRESULT CALLBACK MediaWindowManager::WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    PAINTSTRUCT ps;
    HDC hDC;
    LRESULT result = 0;

    if (m_pCallback != NULL)
    {
        m_pCallback->OnMessageReceived(hWnd, message, wParam, lParam);
    }

    switch (message) 
    {
        case WM_PAINT:
            hDC = BeginPaint(hWnd, &ps);
            OnPaint(hDC);
            EndPaint(hWnd, &ps);
            break;
        case WM_ERASEBKGND:
            result = 1;
            break;
        case WM_DISPLAYCHANGE:
            if (m_bRunning)
            {
                if (m_pVMRWindowlessControl != NULL)
                    m_pVMRWindowlessControl->DisplayModeChanged();
                else if (m_pVMRWindowlessControl9 != NULL)
                    m_pVMRWindowlessControl9->DisplayModeChanged();
                break;
            }
        default:
            result = DefWindowProc(hWnd, message, wParam, lParam);
    }

    return result;
}

void MediaWindowManager::OnPaint(HDC hDC)
{
    RECT rcClient;
    GetClientRect(m_hMediaWindow, &rcClient);
    
    if (m_bRunning) 
    {
        if (m_pMFVideoDisplayControl != NULL)
            m_pMFVideoDisplayControl->RepaintVideo();
        else if (m_pVMRWindowlessControl != NULL)
            m_pVMRWindowlessControl->RepaintVideo(m_hMediaWindow, hDC);
        else if (m_pVMRWindowlessControl9 != NULL)
            m_pVMRWindowlessControl9->RepaintVideo(m_hMediaWindow, hDC);
    }
    else if (m_bShowLogo && m_hLogo != NULL)
    {
        HDC memDC = CreateCompatibleDC(hDC);
        BITMAP bmpLogo;
        GetObject(m_hLogo, sizeof(BITMAP), &bmpLogo);
        HBITMAP hPrevBitmap = (HBITMAP)SelectObject(memDC, m_hLogo);
        
        RECT rcLogo;
        
        double w=bmpLogo.bmWidth;
        double h=bmpLogo.bmHeight;
        double aspectratio=w/h;
        w=rcClient.right;
        h=rcClient.bottom;
        double ratio=w/h;

        LONG hor, vert;
        if (ratio>=aspectratio)
        {
            vert = bmpLogo.bmHeight-rcClient.bottom;
            rcLogo.top= (vert>=0) ? 0 : -vert/2;
            rcLogo.bottom= (vert>=0) ? rcClient.bottom : rcLogo.top+bmpLogo.bmHeight;
            h=rcLogo.bottom-rcLogo.top;
            w=h*aspectratio;
            hor = rcClient.right - (LONG) w;
            rcLogo.left= (hor<=0) ? 0 : hor/2;
            rcLogo.right= rcLogo.left+(LONG) w;
        }
        else
        {
            hor  = bmpLogo.bmWidth-rcClient.right;
            // hor>=0 - client area is smaller than logo hor size
            rcLogo.left= (hor>=0) ? 0 : -hor/2;
            rcLogo.right= (hor>=0) ? rcClient.right : rcLogo.left+bmpLogo.bmWidth;
            w=rcLogo.right-rcLogo.left;
            h=w/aspectratio;
            vert=rcClient.bottom - (LONG) h;
            rcLogo.top= (vert<=0) ? 0 : vert/2;
            rcLogo.bottom= rcLogo.top+(LONG) h;
        }

        StretchBlt(hDC, rcLogo.left, rcLogo.top, rcLogo.right-rcLogo.left, 
                rcLogo.bottom-rcLogo.top, memDC, 0, 0, 
                bmpLogo.bmWidth, bmpLogo.bmHeight, SRCCOPY);
        
        SelectObject(memDC, hPrevBitmap);
        DeleteDC(memDC);

        HRGN rgnClient = CreateRectRgnIndirect(&rcClient); 
        HRGN rgnLogo  = CreateRectRgnIndirect(&rcLogo);  
        CombineRgn(rgnClient, rgnClient, rgnLogo, RGN_DIFF);  
        
        HBRUSH hbr = CreateSolidBrush(RGB(0,0,0)); 
        FillRgn(hDC, rgnClient, hbr); 

        DeleteObject(hbr); 
        DeleteObject(rgnClient); 
        DeleteObject(rgnLogo); 
    }
    else
    {
        HBRUSH hbr = CreateSolidBrush(RGB(0,0,0));
        FillRect(hDC, &rcClient, hbr);
        DeleteObject(hbr);
    }
}

HRESULT MediaWindowManager::CreateMediaWindow(HWND *phwnd, HWND hParent, int x, int y, int nWidth, int nHeight, DWORD dwStyle)
{
    if (phwnd == NULL)
    {
        return E_POINTER;
    }

    HRESULT hr = CreateMediaWindow(hParent, x, y, nWidth, nHeight, dwStyle);
    if (hr == S_OK)
    {
        *phwnd = m_hMediaWindow;
    }
    
    return hr;
}

HRESULT MediaWindowManager::SetRunning(BOOL running, IVMRWindowlessControl* pVMR, IVMRWindowlessControl9* pVMR9, IMFVideoDisplayControl* pEVR)
{
    m_pVMRWindowlessControl = pVMR;
    m_pVMRWindowlessControl9 = pVMR9;
    m_pMFVideoDisplayControl = pEVR;
    m_bRunning = running;

    return S_OK;
}

HRESULT MediaWindowManager::InvalidateMediaWindow()
{
    if (m_hMediaWindow != NULL)
    {
        RECT rcClient;
        GetClientRect(m_hMediaWindow, &rcClient);
        InvalidateRect(m_hMediaWindow, &rcClient, FALSE);
    }

    return S_OK;
}

HRESULT MediaWindowManager::SetLogo(HBITMAP logo)
{
    if (m_hLogo != NULL)
        DeleteObject(m_hLogo);
    m_hLogo = logo;
    InvalidateMediaWindow();

    return S_OK;
}

HRESULT MediaWindowManager::ShowLogo(BOOL show)
{
    m_bShowLogo = show;
    return S_OK;
}

HRESULT MediaWindowManager::RegisterCallback(IMediaWindowCallback *pCallback)
{
    m_pCallback = pCallback;
    return S_OK;
}

