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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace Pvp.Core.DirectShow
{
    [ComVisible(false)]
    public class DsUtils
    {
        private static Dictionary<Guid, string> mediaTypeNames;
        static DsUtils()
        {
            mediaTypeNames = new Dictionary<Guid, string>();
            mediaTypeNames.Add(Guid.Empty, "Unknown");
            mediaTypeNames.Add(MediaType.Audio, "Audio");
            mediaTypeNames.Add(MediaType.AUXLine21Data, "AUXLine21Data");
            mediaTypeNames.Add(MediaType.DVD_ENCRYPTED_PACK, "DVD_ENCRYPTED_PACK");
            mediaTypeNames.Add(MediaType.Interleaved, "Interleaved");
            mediaTypeNames.Add(MediaType.Midi, "Midi");
            mediaTypeNames.Add(MediaType.MPEG2_PACK, "MPEG2_PACK");
            mediaTypeNames.Add(MediaType.MPEG2_PES, "MPEG2_PES");
            mediaTypeNames.Add(MediaType.ScriptCommand, "ScriptCommand");
            mediaTypeNames.Add(MediaType.Stream, "Stream");
            mediaTypeNames.Add(MediaType.Text, "Text");
            mediaTypeNames.Add(MediaType.Timecode, "Timecode");
            mediaTypeNames.Add(MediaType.Video, "Video");
        }
        
        // If this function finds a matching pin, it returns an IPin interface pointer 
        // with an outstanding reference count. The caller is responsible for releasing the interface.
        // bConnected - whether the returned Pin should be connected to some other pin.
        // nPinsToSkip - how many Pins that match the conditions should be skipped.
        // Leaving the last two parameters at their default
        // values is like saying: "Gimme the first unconnected pin".
        public static IPin GetPin(IBaseFilter pFilter, PinDirection PinDir, bool bConnected, int nPinsToSkip)
        {
            bool bFound = false;
            IEnumPins pEnum;
            IPin pPin = null;
            int cFetched;
            int nSkipped=0;
            IPin pConnectedPin=null;

            int hr = pFilter.EnumPins(out pEnum);
            if (DsHlp.FAILED(hr) || pEnum == null)
                return null;
        
            while((pEnum.Next(1, out pPin, out cFetched) == DsHlp.S_OK) && pPin != null)
            {
                PinDirection PinDirThis;
                pPin.QueryDirection(out PinDirThis);
                if (PinDir == PinDirThis)
                {
                    hr=pPin.ConnectedTo(out pConnectedPin);
                    if (pConnectedPin != null) 
                    {
                        Marshal.ReleaseComObject(pConnectedPin);
                        pConnectedPin=null;
                    }
                    if ((hr==DsHlp.S_OK && bConnected) || ((uint)hr==DsHlp.VFW_E_NOT_CONNECTED && !bConnected))
                    {
                        if (nSkipped==nPinsToSkip)
                        {
                            bFound=true;
                            break;
                        }
                        else
                        {
                            nSkipped++;
                        }
                    }
            
                }
                Marshal.ReleaseComObject(pPin);
                pPin = null;
            }
            Marshal.ReleaseComObject(pEnum);
            return (bFound ? pPin : null);  
        }

        public static IPin GetPin(IBaseFilter pFilter, PinDirection PinDir, bool bConnected)
        {
            return GetPin(pFilter, PinDir, bConnected, 0);
        }

        public static IPin GetPin(IBaseFilter pFilter, PinDirection PinDir)
        {
            return GetPin(pFilter, PinDir, false);
        }

        public static bool AddToRot(object pUnkGraph, out int pdwRegister)
        {
            IMoniker moniker = null;
            IRunningObjectTable rot = null;
            try
            {
                Marshal.ThrowExceptionForHR(GetRunningObjectTable(0, out rot));
                IntPtr iuPtr = Marshal.GetIUnknownForObject(pUnkGraph);
                int iuInt = (int) iuPtr;
                Marshal.Release(iuPtr);
                string item = String.Format("FilterGraph {0} pid {1}", 
                    iuInt.ToString("x8"), 
                    Process.GetCurrentProcess().Id.ToString("x8"));
                Marshal.ThrowExceptionForHR(CreateItemMoniker("!", item, out moniker));
                pdwRegister = rot.Register(DsHlp.ROTFLAGS_REGISTRATIONKEEPSALIVE /* | DsHlp.ROTFLAGS_ALLOWANYCLIENT*/, pUnkGraph, 
                    moniker);
                return true;
            }
            catch
            {
                pdwRegister = 0;
                return false;
            }
            finally
            {
                if (moniker != null)
                    Marshal.ReleaseComObject(moniker);
                if (rot != null)
                    Marshal.ReleaseComObject(rot);
            }
        }

        public static bool RemoveGraphFromRot(ref int pdwRegister)
        {
            IRunningObjectTable rot = null;
            try 
            {
                Marshal.ThrowExceptionForHR(GetRunningObjectTable(0, out rot));
                rot.Revoke(pdwRegister);
                pdwRegister = 0;
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if(rot != null)
                    Marshal.ReleaseComObject(rot);
            }
        }

        [DllImport("ole32.dll", ExactSpelling=true)]
        private static extern int GetRunningObjectTable(uint r,
            out IRunningObjectTable pprot);

        [DllImport("ole32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        private static extern int CreateItemMoniker(
            [MarshalAs(UnmanagedType.LPWStr)] string delim,
            [MarshalAs(UnmanagedType.LPWStr)] string item, out IMoniker ppmk);

        public static int EnumFilters(IGraphBuilder pGraph, ArrayList aFilters) // just getting their names here
        {
            aFilters.Clear();
            IEnumFilters pEnum = null;
            IBaseFilter pFilter;
            int cFetched;

            int hr = pGraph.EnumFilters(out pEnum);
            if (DsHlp.FAILED(hr)) return hr;

            while(pEnum.Next(1, out pFilter, out cFetched) == DsHlp.S_OK)
            {
                FilterInfo fInfo = new FilterInfo();
                hr = pFilter.QueryFilterInfo(out fInfo);
                if (DsHlp.FAILED(hr))
                {
                    aFilters.Add("Could not get the filter info");
                    continue;  // Maybe the next one will work.
                }

                aFilters.Add(fInfo.achName);
                
                // The FILTER_INFO structure holds a pointer to the Filter Graph
                // Manager, with a reference count that must be released.
                if (fInfo.pGraph != null)
                    Marshal.ReleaseComObject(fInfo.pGraph);
                Marshal.ReleaseComObject(pFilter);
            }
            Marshal.ReleaseComObject(pEnum);
            return DsHlp.S_OK;
        }

        // if mediatype is not supported by the pin the return value is -1
        // if mediatype is supported the return value indicates how many other types
        // were skipped before we got this one.
        public static int IsMediaTypeSupported(IPin pPin, Guid majortype)
        {
            IEnumMediaTypes pEnumTypes;
            int cFetched;
            int nSkipped=0;
            bool bFound = false;

            int hr=pPin.EnumMediaTypes(out pEnumTypes);
            if (DsHlp.FAILED(hr))
                return -1;
    
            IntPtr ptr;
            while (pEnumTypes.Next(1, out ptr, out cFetched) == DsHlp.S_OK)
            {
                AMMediaType mt = (AMMediaType)Marshal.PtrToStructure(ptr, typeof(AMMediaType));
                if (mt.majorType == majortype)
                {
                    FreeFormatBlock(ptr);
                    Marshal.FreeCoTaskMem(ptr);
                    bFound = true;
                    break;
                }
                // free the allocated memory
                FreeFormatBlock(ptr);
                Marshal.FreeCoTaskMem(ptr);
                nSkipped++;
            }

            Marshal.ReleaseComObject(pEnumTypes);
            return (bFound ? nSkipped : -1);
        }

        public static void FreeFormatBlock(IntPtr pmt)
        {
            AMMediaType mt = (AMMediaType)Marshal.PtrToStructure(pmt, typeof(AMMediaType));
            if (mt.formatSize != 0)
                Marshal.FreeCoTaskMem(mt.formatPtr);
                
            if (mt.unkPtr != IntPtr.Zero)
            {
                // Unecessary because unkPtr should not be used, but safest.
                Marshal.Release(mt.unkPtr);
            }
        }

        [DllImport("olepro32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern int OleCreatePropertyFrame(
            IntPtr hwndOwner, 
            int x, 
            int y,
            string lpszCaption, 
            int cObjects,
            [In, MarshalAs(UnmanagedType.Interface)] ref object ppUnk,
            int cPages, 
            IntPtr pPageClsID, 
            int lcid, 
            int dwReserved, 
            IntPtr pvReserved);

        public static int TraceFilterGraph(IGraphBuilder pGraph)
        {
            IEnumFilters pEnum = null;
            IBaseFilter pFilter;
            int cFetched;

            int hr = pGraph.EnumFilters(out pEnum);
            if (DsHlp.FAILED(hr)) return hr;
            Trace.WriteLine("Tracing filter graph...");
            while (pEnum.Next(1, out pFilter, out cFetched) == DsHlp.S_OK)
            {
                string filterName;
                FilterInfo fInfo = new FilterInfo();
                hr = pFilter.QueryFilterInfo(out fInfo);
                if (DsHlp.FAILED(hr))
                    filterName = "Unknown filter";
                else
                    filterName = fInfo.achName;

                // The FILTER_INFO structure holds a pointer to the Filter Graph
                // Manager, with a reference count that must be released.
                if (fInfo.pGraph != null)
                    Marshal.ReleaseComObject(fInfo.pGraph);

                Trace.WriteLine(String.Format("Filter: {0}", filterName));
                Trace.Indent();
                TraceFilter(pFilter);
                Trace.Unindent();
                Marshal.ReleaseComObject(pFilter);
            }
            Marshal.ReleaseComObject(pEnum);
            return DsHlp.S_OK;
        }

        public static void TraceFilter(IBaseFilter pFilter)
        {
            TracePins(pFilter, PinDirection.Input);
            TracePins(pFilter, PinDirection.Output);
        }

        public static void TracePins(IBaseFilter pFilter, PinDirection direction)
        {
            IPin pPin = null;
            int nSkip = 0;
            // trace connected pins
            while ((pPin = GetPin(pFilter, direction, true, nSkip)) != null)
            {
                Trace.WriteLine(String.Format("{0} pin: type: {1}, connected: True", 
                    direction.ToString(), GetMediaTypeAsString(pPin)));
                nSkip++;
                Marshal.ReleaseComObject(pPin);
            } 

            // trace unconnected pins
            nSkip = 0;
            while ((pPin = GetPin(pFilter, direction, false, nSkip)) != null)
            {
                Trace.WriteLine(String.Format("{0} pin: type: {1}, connected: False", 
                    direction.ToString(), GetMediaTypeAsString(pPin)));
                nSkip++;
                Marshal.ReleaseComObject(pPin);
            } 
        }

        public static Guid GetMediaType(IPin pPin)
        {
            Guid mediaType = Guid.Empty;
            IEnumMediaTypes pEnumTypes;
            int cFetched;
           
            int hr = pPin.EnumMediaTypes(out pEnumTypes);
            if (DsHlp.SUCCEEDED(hr))
            {
                IntPtr ptr;
                if (pEnumTypes.Next(1, out ptr, out cFetched) == DsHlp.S_OK)
                {
                    AMMediaType mt = (AMMediaType)Marshal.PtrToStructure(ptr, typeof(AMMediaType));
                    mediaType = mt.majorType;
                    // free the allocated memory
                    FreeFormatBlock(ptr);
                    Marshal.FreeCoTaskMem(ptr);
                }

                Marshal.ReleaseComObject(pEnumTypes);
            }
            return mediaType;
        }

        public static String GetMediaTypeAsString(IPin pPin)
        {
            Guid mediaType = GetMediaType(pPin);
            return mediaTypeNames.ContainsKey(mediaType) ? mediaTypeNames[mediaType] : "Unknown";
        }

        public static void Disconnect(IGraphBuilder pGraphBuilder, IPin pPin)
        {
            IPin pInputPin = null;
            if (pPin.ConnectedTo(out pInputPin) == DsHlp.S_OK)
            {
                pGraphBuilder.Disconnect(pInputPin);
                Marshal.ReleaseComObject(pInputPin);
            }

            pGraphBuilder.Disconnect(pPin);
        }
    }
}