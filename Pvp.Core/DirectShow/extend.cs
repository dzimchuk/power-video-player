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

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

/* 
 * REGPINMEDIUM
 * MERIT
 * IGraphBuilder
 * IFilterGraph2
 * IFilterMapper2
 * IPropertyBag
 * IErrorLog
*/
namespace Pvp.Core.DirectShow
{
    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct REGPINMEDIUM
    {
        public Guid clsMedium;
        public int dw1;
        public int dw2;
    }

    [ComVisible(false)]
    public enum MERIT
    {
        MERIT_PREFERRED     = 0x800000,
        MERIT_NORMAL        = 0x600000,
        MERIT_UNLIKELY      = 0x400000,
        MERIT_DO_NOT_USE    = 0x200000,
        MERIT_SW_COMPRESSOR = 0x100000,
        MERIT_HW_COMPRESSOR = 0x100050
    }

    [Flags]
    public enum AMStreamSelectInfoFlags
    {
        Disabled = 0x0,
        Enabled = 0x01,
        Exclusive = 0x02
    }

    [Flags]
    public enum AMStreamSelectEnableFlags
    {
        DisableAll = 0x0,
        Enable = 0x01,
        EnableAll = 0x02
    }
    
    [ComVisible(true), ComImport,
    GuidAttribute("56a868a9-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IGraphBuilder
    {
        #region IFilterGraph methods
        [PreserveSig]
        int AddFilter([In] IBaseFilter pFilter, [In, MarshalAs(UnmanagedType.LPWStr)] string pName);

        [PreserveSig]
        int RemoveFilter([In] IBaseFilter pFilter);

        [PreserveSig]
        int EnumFilters([Out] out IEnumFilters ppEnum);

        [PreserveSig]
        int FindFilterByName([In, MarshalAs(UnmanagedType.LPWStr)] string pName,
            [Out] out IBaseFilter ppFilter);

        [PreserveSig]
        int ConnectDirect([In] IPin ppinOut, [In] IPin ppinIn,
            [In] ref AMMediaType pmt);

        [PreserveSig]
        int Reconnect([In] IPin ppin);

        [PreserveSig]
        int Disconnect([In] IPin ppin);

        [PreserveSig]
        int SetDefaultSyncSource();
        #endregion

        [PreserveSig]
        int Connect([In] IPin ppinOut, [In] IPin ppinIn);

        [PreserveSig]
        int Render([In] IPin ppinOut);

        [PreserveSig]
        int RenderFile([In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFile,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrPlayList);

        [PreserveSig]
        int AddSourceFilter([In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFileName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFilterName,
            [Out] out IBaseFilter ppFilter);

        [PreserveSig]
        int SetLogFile(IntPtr hFile);

        [PreserveSig]
        int Abort();

        [PreserveSig]
        int ShouldOperationContinue();
    }

    [ComVisible(true), ComImport,
    GuidAttribute("36b73882-c2c8-11cf-8b46-00805f6cef60"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFilterGraph2
    {
        #region IFilterGraph methods
        [PreserveSig]
        int AddFilter([In] IBaseFilter pFilter, [In, MarshalAs(UnmanagedType.LPWStr)] string pName);

        [PreserveSig]
        int RemoveFilter([In] IBaseFilter pFilter);

        [PreserveSig]
        int EnumFilters([Out] out IEnumFilters ppEnum);

        [PreserveSig]
        int FindFilterByName([In, MarshalAs(UnmanagedType.LPWStr)] string pName,
            [Out] out IBaseFilter ppFilter);

        [PreserveSig]
        int ConnectDirect([In] IPin ppinOut, [In] IPin ppinIn,
            [In] ref AMMediaType pmt);

        [PreserveSig]
        int Reconnect([In] IPin ppin);

        [PreserveSig]
        int Disconnect([In] IPin ppin);

        [PreserveSig]
        int SetDefaultSyncSource();
        #endregion

        #region IGraphBuilder methods
        [PreserveSig]
        int Connect([In] IPin ppinOut, [In] IPin ppinIn);

        [PreserveSig]
        int Render([In] IPin ppinOut);

        [PreserveSig]
        int RenderFile([In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFile,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrPlayList);

        [PreserveSig]
        int AddSourceFilter([In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFileName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFilterName,
            [Out] out IBaseFilter ppFilter);

        [PreserveSig]
        int SetLogFile(IntPtr hFile);

        [PreserveSig]
        int Abort();

        [PreserveSig]
        int ShouldOperationContinue();
        #endregion

        [PreserveSig]
        int AddSourceFilterForMoniker([In] IMoniker pMoniker, [In] IBindCtx pCtx, 
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFilterName,
            [Out] out IBaseFilter ppFilter);

        [PreserveSig]
        int ReconnectEx([In] IPin ppin, [In] ref AMMediaType pmt);
    
        [PreserveSig]
        int RenderEx([In] IPin pPinOut, int dwFlags, IntPtr pvContext); // pvContext must be NULL
    }

    [ComVisible(true), ComImport,
    GuidAttribute("b79bb0b0-33c1-11d1-abe1-00a0c905f375"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFilterMapper2
    {
        // create or rename ActiveMovie category
        [PreserveSig]
        int CreateCategory([In] ref Guid clsidCategory, int dwCategoryMerit,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Description);

        [PreserveSig]
        int UnregisterFilter([In] ref Guid pclsidCategory, 
            [In, MarshalAs(UnmanagedType.LPWStr)] string szInstance,
            [In] ref Guid Filter); // GUID of filter
                        
        // Register a filter, pins, and media types under a category.
        [PreserveSig]
        int RegisterFilter([In] ref Guid clsidFilter,			// GUID of the filter
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name,  // Descriptive name for the filter
            // ppMoniker can be null. or *ppMoniker can contain the
            // moniker where this filter data will be written;
            // *ppMoniker will be set to null on return. or *ppMoniker
            // can be null in which case the moniker will be returned
            // with refcount.
            [In, Out] ref IMoniker ppMoniker,
            // can be null
            [In] ref Guid pclsidCategory,
            // cannot be null
            [In, MarshalAs(UnmanagedType.LPWStr)] string szInstance,
            // rest of filter and pin registration
            [In] IntPtr prf2); // REGFILTER2*

        // Set *ppEnum to be an enumerator for filters matching the
        // requirements.
        [PreserveSig]
        int EnumMatchingFilters(
            [Out] out IEnumMoniker ppEnum						 // enumerator returned
            , int dwFlags                   // 0
            , [In, MarshalAs(UnmanagedType.Bool)] bool bExactMatch   // don't match wildcards
            , int dwMerit                   // at least this merit needed
            , [In, MarshalAs(UnmanagedType.Bool)] bool bInputNeeded  // need at least one input pin
            , int cInputTypes               // Number of input types to match
            // Any match is OK
            , [In, MarshalAs(UnmanagedType.LPArray)] Guid[] pInputTypes		// input major+subtype pair array, size_is(cInputTypes*2)
            , [In] IntPtr pMedIn			// input medium (ref REGPINMEDIUM)
            , [In] IntPtr pPinCategoryIn	// input pin category (ref Guid)
            , [In, MarshalAs(UnmanagedType.Bool)] bool bRender       // must the input be rendered?
            , [In, MarshalAs(UnmanagedType.Bool)] bool bOutputNeeded // need at least one output pin
            , int cOutputTypes              // Number of output types to match
            // Any match is OK
            , [In, MarshalAs(UnmanagedType.LPArray)] Guid[] pOutputTypes	// output major+subtype pair array, size_is(cOutputTypes*2)
            , [In] IntPtr pMedOut			// output medium (ref REGPINMEDIUM)
            , [In] IntPtr pPinCategoryOut	// output pin category (ref Guid)
            );
    }

    [ComVisible(true), ComImport,
    GuidAttribute("55272A00-42CB-11CE-8135-00AA004BB851"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyBag
    {
        [PreserveSig]
        int Read([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
                [In, Out, MarshalAs(UnmanagedType.Struct)] ref object pVar,
                [In] IErrorLog pErrorLog);
        
        [PreserveSig]
        int Write([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
                [In, MarshalAs(UnmanagedType.Struct)] ref object pVar);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("3127CA40-446E-11CE-8135-00AA004BB851"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IErrorLog
    {
        [PreserveSig]
        int AddError([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
                     [In] ref System.Runtime.InteropServices.ComTypes.EXCEPINFO pExcepInfo); 
    }

    [ComVisible(true), ComImport,
    GuidAttribute("c1960960-17f5-11d1-abe1-00a0c905f375"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAMStreamSelect
    {
        [PreserveSig]
        int Count([Out] out int pcStreams);

        [PreserveSig]
        int Info(
            [In] int lIndex,
            [Out] out IntPtr ppmt, // AM_MEDIA_TYPE**
            [Out] out AMStreamSelectInfoFlags pdwFlags,
            [Out] out int plcid,
            [Out] out int pdwGroup,
            [Out] out IntPtr ppszName, // MarshalAs(UnmanagedType.LPWStr)
            [Out] out IntPtr ppObject, // MarshalAs(UnmanagedType.IUnknown)
            [Out] out IntPtr ppUnk); // MarshalAs(UnmanagedType.IUnknown)

        [PreserveSig]
        int Enable([In] int lIndex, [In] AMStreamSelectEnableFlags dwFlags);
    }
}