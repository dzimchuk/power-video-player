using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.FilterRegistry;
using MediaType = Pvp.Core.MediaEngine.FilterRegistry.MediaType;

namespace Pvp.Core.MediaEngine
{
    internal static class FilterGraphExtensions
    {
        public static Tuple<IBaseFilter, IBasicAudio> AddSoundRenderer(this IGraphBuilder pGraphBuilder)
        {
            var baseFilter = DsUtils.GetFilter(Clsid.DSoundRender, false);
            if (baseFilter == null)
            {
                TraceSink.GetTraceSink().TraceWarning("Could not instantiate DirectSound Filter.");
                return null;
            }

            // add the DirectSound filter to the graph
            var hr = pGraphBuilder.AddFilter(baseFilter, "DirectSound Filter");
            if (DsHlp.FAILED(hr))
            {
                Marshal.FinalReleaseComObject(baseFilter);

                TraceSink.GetTraceSink().TraceWarning("Could not add DirectSound Filter to the filter graph.");
                return null;
            }

            IBasicAudio basicAudio = baseFilter as IBasicAudio;
            if (basicAudio == null)
            {
                pGraphBuilder.RemoveFilter(baseFilter);
                Marshal.FinalReleaseComObject(baseFilter);

                TraceSink.GetTraceSink().TraceWarning("Could not get IBasicAudio interface.");
                return null;
            }

            return new Tuple<IBaseFilter, IBasicAudio>(baseFilter, basicAudio);
        }

        public static IBaseFilter AddFilter(this IGraphBuilder pGraphBuilder, FilterDescription filterDescription)
        {
            var baseFilter = DsUtils.GetFilter(filterDescription.ClassId, true);
            var hr = pGraphBuilder.AddFilter(baseFilter, filterDescription.Name);
            
            if (DsHlp.FAILED(hr))
            {
                Marshal.FinalReleaseComObject(baseFilter);

                hr.ThrowExceptionForHR();
            }

            return baseFilter;
        }

        public static IEnumerable<SelectableStream> GetSelectableStreams(this IAMStreamSelect pStreamSelect)
        {
            var result = new List<SelectableStream>();

            int count;
            var hr = pStreamSelect.Count(out count);
            if (DsHlp.SUCCEEDED(hr) && count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var stream = pStreamSelect.GetSelectableStream(i);
                    if (stream != null)
                    {
                        result.Add(stream);
                    }
                }
            }

            return result;
        } 

        public static bool IsStreamSelected(this IAMStreamSelect pStreamSelect, int index)
        {
            var result = false;

            int count;
            var hr = pStreamSelect.Count(out count);
            if (DsHlp.SUCCEEDED(hr) && count > index)
            {
                var stream = pStreamSelect.GetSelectableStream(index);
                if (stream != null)
                {
                    result = stream.Enabled;
                }
            }

            return result;
        }

        public static bool SelectStream(this IAMStreamSelect pStreamSelect, int index)
        {
            var result = false;

            int count;
            var hr = pStreamSelect.Count(out count);
            if (DsHlp.SUCCEEDED(hr) && count > index)
            {
                hr = pStreamSelect.Enable(index, AMStreamSelectEnableFlags.Enable);
                result = DsHlp.SUCCEEDED(hr);
            }

            return result;
        }

        public static SelectableStream GetSelectableStream(this IAMStreamSelect pStreamSelect, int index)
        {
            SelectableStream result = null;

            pStreamSelect.InspectStream(index, (mt, name, enabled) =>
                                               {
                                                   result = new SelectableStream
                                                            {
                                                                Index = index,
                                                                Name = name,
                                                                Enabled = enabled,
                                                                MajorType = mt.majorType,
                                                                SubType = mt.subType
                                                            };
                                               });

            return result;
        }

        public static void InspectStream(this IAMStreamSelect pStreamSelect, int index, Action<AMMediaType, string, bool> inspect)
        {
            IntPtr ppmt;
            AMStreamSelectInfoFlags pdwFlags;
            int plcid;
            int pdwGroup;
            IntPtr ppszName;
            IntPtr ppObject;
            IntPtr ppUnk;

            var hr = pStreamSelect.Info(index, out ppmt, out pdwFlags, out plcid, out pdwGroup, out ppszName, out ppObject, out ppUnk);
            if (DsHlp.SUCCEEDED(hr))
            {
                var mt = (AMMediaType)Marshal.PtrToStructure(ppmt, typeof(AMMediaType));
                var name = Marshal.PtrToStringAuto(ppszName);
                var enabled = (pdwFlags & AMStreamSelectInfoFlags.Enabled) != AMStreamSelectInfoFlags.Disabled ||
                              (pdwFlags & AMStreamSelectInfoFlags.Exclusive) != AMStreamSelectInfoFlags.Disabled;

                inspect(mt, name, enabled);

                DsUtils.FreeFormatBlock(ppmt);
                Marshal.FreeCoTaskMem(ppmt);

                Marshal.FreeCoTaskMem(ppszName);
                if (ppObject != IntPtr.Zero)
                    Marshal.Release(ppObject);
                if (ppUnk != IntPtr.Zero)
                    Marshal.Release(ppUnk);
            }
        }

        public static void EnumPins(this IBaseFilter filter, PinDirection direction, bool connected, Action<IPin, AMMediaType> action)
        {
            var nPinsToSkip = 0;
            IPin pPin;
            while ((pPin = DsUtils.GetPin(filter, direction, connected, nPinsToSkip)) != null)
            {
                nPinsToSkip++;

                var pin = pPin;
                pPin.EnumMediaTypes(mt => action(pin, mt));

                Marshal.ReleaseComObject(pPin);
            }
        }

        public static void EnumMediaTypes(this IPin pPin, Action<AMMediaType> action)
        {
            IEnumMediaTypes pEnumTypes;

            var hr = pPin.EnumMediaTypes(out pEnumTypes);
            if (hr == DsHlp.S_OK)
            {
                IntPtr ptr;
                int cFetched;

                if (pEnumTypes.Next(1, out ptr, out cFetched) == DsHlp.S_OK)
                {
                    AMMediaType mt = (AMMediaType)Marshal.PtrToStructure(ptr, typeof(AMMediaType));

                    action(mt);

                    DsUtils.FreeFormatBlock(ptr);
                    Marshal.FreeCoTaskMem(ptr);
                }
                Marshal.ReleaseComObject(pEnumTypes);
            }
        }

        public static MediaType[] GetMediaTypes(this IPin pPin)
        {
            var mediaTypes = new List<MediaType>();
            pPin.EnumMediaTypes(mt => mediaTypes.Add(new MediaType(mt.majorType, mt.subType)));

            return mediaTypes.ToArray();
        }
    }
}