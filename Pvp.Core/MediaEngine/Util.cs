using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Pvp.Core.MediaEngine
{
    internal static class Util
    {
        public static void ThrowExceptionForHR(this int hr, GraphBuilderError error = GraphBuilderError.Unknown)
        {
            try
            {
                Marshal.ThrowExceptionForHR(hr);
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(error, e);
            }
        }

        public static string GetErrorText(this GraphBuilderError error)
        {
            string text;

            switch (error)
            {
                case GraphBuilderError.Unknown:
                    text = Resources.Resources.error;
                    break;
                case GraphBuilderError.FilterGraphManager:
                    text = Resources.Resources.error_cant_create_fgm;
                    break;
                case GraphBuilderError.SourceFilter:
                    text = Resources.Resources.error_no_source_filter;
                    break;
                case GraphBuilderError.NecessaryInterfaces:
                    text = Resources.Resources.error_cant_retrieve_all_interfaces;
                    break;
                case GraphBuilderError.VideoRenderer:
                    text = Resources.Resources.error_cant_create_vr;
                    break;
                case GraphBuilderError.AddVideoRenderer:
                    text = Resources.Resources.error_cant_add_vr;
                    break;
                case GraphBuilderError.AddVMR9:
                    text = Resources.Resources.error_cant_add_vmr9;
                    break;
                case GraphBuilderError.ConfigureVMR9:
                    text = Resources.Resources.error_cant_configure_vmr9;
                    break;
                case GraphBuilderError.AddVMR:
                    text = Resources.Resources.error_cant_add_vmr;
                    break;
                case GraphBuilderError.ConfigureVMR:
                    text = Resources.Resources.error_cant_configure_vmr;
                    break;
                case GraphBuilderError.CantPlayFile:
                    text = Resources.Resources.error_cant_play_file;
                    break;
                case GraphBuilderError.CantRenderFile:
                    text = Resources.Resources.error_cant_render_file;
                    break;
                case GraphBuilderError.DirectSoundFilter:
                    text = Resources.Resources.error_cant_create_ds;
                    break;
                case GraphBuilderError.AddDirectSoundFilter:
                    text = Resources.Resources.error_cant_add_ds;
                    break;
                case GraphBuilderError.DvdGraphBuilder:
                    text = Resources.Resources.error_cant_create_dvd_builder;
                    break;
                case GraphBuilderError.CantPlayDisc:
                    text = Resources.Resources.error_cant_play_disc;
                    break;
                case GraphBuilderError.NoVideoDimension:
                    text = Resources.Resources.error_cant_get_video_size; 
                    break;
                case GraphBuilderError.AddEVR:
                    text = Resources.Resources.error_cant_add_evr;
                    break;
                case GraphBuilderError.ConfigureEVR:
                    text = Resources.Resources.error_cant_configure_evr;
                    break;
                case GraphBuilderError.CantRenderSubpicture:
                    text = Resources.Resources.error_cant_render_subpicture;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("error");
            }

            return text;
        }

        public static void Raise<TEventArgs>(this TEventArgs e, Object sender, ref EventHandler<TEventArgs> eventDelegate) where TEventArgs : EventArgs
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            var temp = Interlocked.CompareExchange(ref eventDelegate, null, null);
            // If any methods registered interest with our event, notify them
            if (temp != null)
                temp(sender, e);
        }

        public static void Raise<TEventArgs>(this TEventArgs e, Object sender, ref EventHandler eventDelegate) where TEventArgs : EventArgs
        {
            var temp = Interlocked.CompareExchange(ref eventDelegate, null, null);
            if (temp != null)
                temp(sender, e);
        }

        public static void Raise(this string message, Object sender, ref EventHandler<string> eventDelegate)
        {
            var temp = Interlocked.CompareExchange(ref eventDelegate, null, null);
            if (temp != null)
                temp(sender, message);
        }
    }
}