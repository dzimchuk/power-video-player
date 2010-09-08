/* ****************************************************************************
 *
 * Copyright (c) Andrei Dzimchuk. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

#include "stdafx.h"
#include <dshow.h> 
#include <d3d9.h>
#include <vmr9.h>
#include <evr.h>

#pragma comment(lib, "quartz.lib")

HINSTANCE g_hinstThisDll = NULL;        // Our DLL's module handle
HWND g_hWnd = NULL;
HWND g_hParent = NULL;
TCHAR szWindowClass[] = _T("PVP Media Window");

BOOL bRunning = FALSE;
BOOL bShowLogo = FALSE;
HBITMAP hLogo = NULL;
IVMRWindowlessControl *pVMRWindowlessControl = NULL;
IVMRWindowlessControl9 *pVMRWindowlessControl9 = NULL;
IMFVideoDisplayControl *pMFVideoDisplayControl = NULL;

LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);
void OnPaint(HDC hDC);

BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
                     )
{
    if (ul_reason_for_call==DLL_PROCESS_ATTACH) 
    {
        g_hinstThisDll = (HINSTANCE) hModule;
        
        // Calling DisableThreadLibraryCalls() prevents DllMain() from 
        // getting called for every thread that attaches/detaches from
        // our DLL.
        DisableThreadLibraryCalls((HMODULE) hModule); 
    }
    else if (ul_reason_for_call==DLL_PROCESS_DETACH)
    {
        if (hLogo != NULL)
            DeleteObject(hLogo);
    }
    return TRUE;
}

extern "C" __declspec(dllexport) HWND __stdcall CreateMediaWindow(HWND hParent, int nWidth, int nHeight)
{
    WNDCLASSEX wcex;
    wcex.cbSize = sizeof(WNDCLASSEX); 
    wcex.style			= CS_HREDRAW | CS_VREDRAW | CS_DBLCLKS;
    wcex.lpfnWndProc	= (WNDPROC)WndProc;
    wcex.cbClsExtra		= 0;
    wcex.cbWndExtra		= 0;
    wcex.hInstance		= g_hinstThisDll;
    wcex.hIcon			= NULL;
    wcex.hCursor		= LoadCursor(NULL, IDC_ARROW);
    wcex.hbrBackground	= NULL;
    wcex.lpszMenuName	= NULL;
    wcex.lpszClassName	= szWindowClass;
    wcex.hIconSm		= NULL;

    RegisterClassEx(&wcex);

    g_hParent = hParent;
    g_hWnd = CreateWindow(szWindowClass, NULL, WS_VISIBLE | WS_CHILD | WS_CLIPSIBLINGS, 
                    0, 0, nWidth, nHeight, hParent, (HMENU)0x16161616, g_hinstThisDll, NULL);
    return g_hWnd;
}

extern "C" __declspec(dllexport) void __stdcall SetRunning(BOOL running, 
                                                           IVMRWindowlessControl* pVMR, 
                                                           IVMRWindowlessControl9* pVMR9,
                                                           IMFVideoDisplayControl* pEVR)
{
    pVMRWindowlessControl = pVMR;
    pVMRWindowlessControl9 = pVMR9;
    pMFVideoDisplayControl = pEVR;
    bRunning = running;
}

extern "C" __declspec(dllexport) void __stdcall InvalidateMediaWindow()
{
    if (g_hWnd != NULL)
    {
        RECT rcClient;
        GetClientRect(g_hWnd, &rcClient);
        InvalidateRect(g_hWnd, &rcClient, FALSE);
    }
}

extern "C" __declspec(dllexport) void __stdcall SetLogo(HBITMAP logo)
{
    if (hLogo != NULL)
        DeleteObject(hLogo);
    hLogo = logo;
    InvalidateMediaWindow();
}

extern "C" __declspec(dllexport) void __stdcall IsShowLogo(BOOL show)
{
    bShowLogo = show;
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    PAINTSTRUCT ps;
    HDC hDC; 

    switch (message) 
    {
    case WM_PAINT:
        hDC = BeginPaint(hWnd, &ps);
        OnPaint(hDC);
        EndPaint(hWnd, &ps);
        break;
    case WM_ERASEBKGND:
        return 1;
    case WM_DISPLAYCHANGE:
        if (bRunning)
        {
            if (pVMRWindowlessControl != NULL)
                pVMRWindowlessControl->DisplayModeChanged();
            else if (pVMRWindowlessControl9 != NULL)
                pVMRWindowlessControl9->DisplayModeChanged();
            break;
        }
    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
    }
    return 0;
}

void OnPaint(HDC hDC)
{
    RECT rcClient;
    GetClientRect(g_hWnd, &rcClient);
    
    if (bRunning) 
    {
        if (pMFVideoDisplayControl != NULL)
            pMFVideoDisplayControl->RepaintVideo();
        else if (pVMRWindowlessControl != NULL)
            pVMRWindowlessControl->RepaintVideo(g_hWnd, hDC);
        else if (pVMRWindowlessControl9 != NULL)
            pVMRWindowlessControl9->RepaintVideo(g_hWnd, hDC);
    }
    else if (bShowLogo && hLogo != NULL)
    {
        HDC memDC = CreateCompatibleDC(hDC);
        BITMAP bmpLogo;
        GetObject(hLogo, sizeof(BITMAP), &bmpLogo);
        HBITMAP hPrevBitmap = (HBITMAP)SelectObject(memDC, hLogo);
        
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