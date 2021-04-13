using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.Renderers;

namespace Pvp.Core.MediaEngine.FilterRegistry
{
    public class MediaTypeManager : IDisposable
    {
        private readonly MediaTypesHolder _typesHolder;
        private IFilterMapper2 _pMapper;
                
        public MediaTypeManager()
        {
            _typesHolder = MediaTypesHolder.Instance;

            object comobj = null;
            try
            {
                Type type = Type.GetTypeFromCLSID(Clsid.FilterMapper2, true);
                comobj = Activator.CreateInstance(type);
                _pMapper = (IFilterMapper2)comobj;
                comobj = null;
            }
            catch
            {
            }
            finally
            {
                if (comobj != null)
                    while(Marshal.ReleaseComObject(comobj) > 0) {}
            }
        }

        ~MediaTypeManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_pMapper != null)
                {
                    Marshal.FinalReleaseComObject(_pMapper);
                    _pMapper = null;
                }
            }
        }

        private MediaType[] GetMediaTypesByFriendlyName(string mediaTypeFriendlyName)
        {
            var result = new MediaType[] { };

            if (_typesHolder.AggrTypes.ContainsKey(mediaTypeFriendlyName))
                result = _typesHolder.AggrTypes[mediaTypeFriendlyName];

            return result;
        }

        public static string GetFourCC(Guid guid)
        {
            byte[] abyte = guid.ToByteArray();
            char[] achar = new char[4];
            achar[0] = (char)abyte[0];
            achar[1] = (char)abyte[1];
            achar[2] = (char)abyte[2];
            achar[3] = (char)abyte[3];
            
            return new string(achar);
        }
    
        // Get the type's name
        public string GetTypeName(Guid majortype, Guid subtype)
        {
            if (_typesHolder.Types != null && _typesHolder.Types.ContainsKey(majortype))
            {
                var htSubtype = _typesHolder.Types[majortype];
                if (htSubtype.ContainsKey(subtype))
                    return htSubtype[subtype].MediaTypeFriendlyName;
            }
            return null;
        }

        // Get Clsid of the filter associated with a media type
        public Guid GetAssociatedFilterClassId(Guid majortype, Guid subtype)
        {
            if (_typesHolder.Types != null && _typesHolder.Types.ContainsKey(majortype))
            {
                var htSubtype = _typesHolder.Types[majortype];
                if (htSubtype.ContainsKey(subtype))
                    return htSubtype[subtype].AssociatedFilterClassId;
            }
            return Guid.Empty;
        }

        // Get Clsid of the filter associated with a media type (by type's name)
        public Guid GetAssociatedFilterClassId(string mediaTypeFriendlyName)
        {
            var mediaTypes = GetMediaTypesByFriendlyName(mediaTypeFriendlyName);
            return mediaTypes != null
                ? GetAssociatedFilterClassId(mediaTypes[0].Majortype, mediaTypes[0].Subtype)
                : Guid.Empty;
        }

        // Associate a filter with a media type
        public void SetAssociatedFilterClassId(string mediaTypeFriendlyName, Guid filterClassId)
        {
            var mediaTypes = GetMediaTypesByFriendlyName(mediaTypeFriendlyName);
            if (mediaTypes != null)
            {
                foreach(var type in mediaTypes)
                {
                    if (_typesHolder.Types.ContainsKey(type.Majortype))
                    {
                        var htSubtype = _typesHolder.Types[type.Majortype];
                        if (htSubtype.ContainsKey(type.Subtype))
                            htSubtype[type.Subtype].AssociatedFilterClassId = filterClassId;
                    }
                }
            }
        }

        // Get an array of names of all types
        public string[] TypeNames
        {
            get
            {
                ArrayList list = new ArrayList(_typesHolder.AggrTypes.Keys);
                return (string[])list.ToArray(typeof(string));
            }
        }

        /// <summary>
        /// Get a read-only collection of descriptions of the filters that can be used to render a media type.
        /// </summary>
        /// <param name="mediaTypeFriendlyName">A media type's friendly name.</param>
        /// <returns></returns>
        public ReadOnlyCollection<FilterDescription> GetMatchingFilters(string mediaTypeFriendlyName)
        {
            var mediaTypes = GetMediaTypesByFriendlyName(mediaTypeFriendlyName);
            return GetMatchingFilters(mediaTypes);
        }

        /// <summary>
        /// Get a read-only collection of descriptions of the filters that can be used to render a specified media type.
        /// </summary>
        /// <param name="mediaType">A media type to render.</param>
        /// <returns></returns>
        public ReadOnlyCollection<FilterDescription> GetMatchingFilters(MediaType mediaType)
        {
            return GetMatchingFilters(new[] { mediaType });
        }

        /// <summary>
        /// Get a read-only collection of descriptions of the filters that can be used to render specified media types.
        /// </summary>
        /// <param name="mediaTypes">Collection of media types.</param>
        /// <returns></returns>
        public ReadOnlyCollection<FilterDescription> GetMatchingFilters(ICollection<MediaType> mediaTypes)
        {
            var listFilters = new List<FilterDescription>();

            if (mediaTypes == null || !mediaTypes.Any())
                return new ReadOnlyCollection<FilterDescription>(listFilters);

            var listNames = new HashSet<string>();

            IEnumMoniker pEnum;
            var pMonikers = new IMoniker[1];
            Guid IID_IPropertyBag = typeof(IPropertyBag).GUID;
            Guid IID_IBaseFilter = typeof(IBaseFilter).GUID;

            var arrayInTypes = mediaTypes.SelectMany(t => t.ToIdPair()).ToArray();

            var hr = _pMapper.EnumMatchingFilters(out pEnum,
                                                0,				// Reserved
                                                true,			// Use exact match?
                                                (int)MERIT.MERIT_DO_NOT_USE + 1,
                                                true,			// At least one input pin?
                                                mediaTypes.Count,   // Number of major type/subtype pairs for input
                                                arrayInTypes,	// Array of major type/subtype pairs for input
                                                IntPtr.Zero,	// Input medium
                                                IntPtr.Zero,	// Input pin category
                                                false,			// Must be a renderer?
                                                false,			// At least one output pin?
                                                0,				// Number of major type/subtype pairs for output
                                                null,			// Array of major type/subtype pairs for output
                                                IntPtr.Zero,	// Output medium
                                                IntPtr.Zero);	// Output pin category
            if (DsHlp.SUCCEEDED(hr))
            {
                object o;
                IntPtr cFetched = IntPtr.Zero;
                while (pEnum.Next(1, pMonikers, cFetched) == DsHlp.S_OK)
                {
                    o = null;
                    pMonikers[0].BindToStorage(null, null,
                        ref IID_IPropertyBag, out o);
                    if (o != null && o is IPropertyBag)
                    {
                        var pPropBag = (IPropertyBag)o;
                        o = String.Empty;
                        hr = pPropBag.Read("FriendlyName", ref o, null);
                        if (DsHlp.SUCCEEDED(hr) && o is String)
                        {
                            string name = (string)o;
                            if (!listNames.Contains(name))
                            {
                                o = null;
                                pMonikers[0].BindToObject(null, null,
                                    ref IID_IBaseFilter, out o);
                                if (o != null && o is IBaseFilter)
                                {
                                    var pBaseFilter = (IBaseFilter)o;
                                    o = null;

                                    listNames.Add(name);

                                    Guid clsid;
                                    pBaseFilter.GetClassID(out clsid);

                                    var filterDescription = new FilterDescription(name, clsid);
                                    listFilters.Add(filterDescription);

                                    Marshal.FinalReleaseComObject(pBaseFilter);
                                }
                            }
                        }
                        Marshal.FinalReleaseComObject(pPropBag);
                    }

                    Marshal.ReleaseComObject(pMonikers[0]);
                }

                Marshal.ReleaseComObject(pEnum);
            }

            return new ReadOnlyCollection<FilterDescription>(listFilters);
        }

        /// <summary>
        /// Get a read-only list of all renderers that are supported on the current system.
        /// </summary>
        public ReadOnlyCollection<Renderer> GetPresentVideoRenderers()
        {
            IList<Renderer> renderers = new List<Renderer>();
            if (_pMapper != null)
            {
                IEnumMoniker pEnum;
                IMoniker[] pMonikers = new IMoniker[1];
                Guid IID_IBaseFilter = typeof(IBaseFilter).GUID;
                                
                Guid[] inTypes = new Guid[] { DirectShow.MediaType.Video, Guid.Empty };
                int hr = _pMapper.EnumMatchingFilters(out pEnum,
                                                    0,				// Reserved
                                                    true,			// Use exact match?
                                                    (int)MERIT.MERIT_DO_NOT_USE,
                                                    true,			// At least one input pin?
                                                    inTypes.Length/2,// Number of major type/subtype pairs for input
                                                    inTypes,	    // Array of major type/subtype pairs for input
                                                    IntPtr.Zero,	// Input medium
                                                    IntPtr.Zero,	// Input pin category
                                                    true,			// Must be a renderer?
                                                    false,			// At least one output pin?
                                                    0,				// Number of major type/subtype pairs for output
                                                    null,			// Array of major type/subtype pairs for output
                                                    IntPtr.Zero,	// Output medium
                                                    IntPtr.Zero);	// Output pin category
                if (DsHlp.SUCCEEDED(hr))
                {
                    try
                    {
                        object o;
                        IntPtr cFetched = IntPtr.Zero;
                        while (pEnum.Next(1, pMonikers, cFetched) == DsHlp.S_OK)
                        {
                            o = null;
                            Guid clsId = Guid.Empty;

                            try
                            {
                                pMonikers[0].BindToObject(null, null,
                                                          ref IID_IBaseFilter, out o);
                                if (o != null && o is IBaseFilter)
                                {
                                    IBaseFilter pBaseFilter = (IBaseFilter)o;
                                    o = null;

                                    pBaseFilter.GetClassID(out clsId);

                                    Marshal.FinalReleaseComObject(pBaseFilter);
                                }
                            }
                            finally
                            {
                                Marshal.ReleaseComObject(pMonikers[0]);
                            }

                            IRenderer renderer = RendererBase.GetRenderer(clsId);
                            if (renderer != null)
                                renderers.Add(renderer.Renderer);
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(pEnum);
                    }
                }
                
            }
            return new ReadOnlyCollection<Renderer>(renderers);
        }
    }
}
