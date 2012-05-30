// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>

template <class T> void SafeRelease(T **ppT)
{
    if (*ppT)
    {
        (*ppT)->Release();
        *ppT = NULL;
    }
}

void DllAddRef();
void DllRelease();
HINSTANCE GetDllInstanceHandle();

// TODO: reference additional headers your program requires here
#include <tchar.h>

#include <comdef.h>
#include <shlwapi.h>

#include <dshow.h> 
#include <d3d9.h>
#include <vmr9.h>
#include <evr.h>

#pragma comment(lib, "shlwapi.lib")
#pragma comment(lib, "quartz.lib")

#include "Interfaces.h"
#include "MediaWindowManager.h"