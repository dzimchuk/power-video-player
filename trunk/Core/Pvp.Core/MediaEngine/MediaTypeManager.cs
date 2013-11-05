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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.IO;
using Pvp.Core.DirectShow;
using System.Collections.ObjectModel;
using Pvp.Core.MediaEngine.Renderers;

namespace Pvp.Core.MediaEngine
{
    /// <summary>
    /// Summary description for MediaTypeManager.
    /// </summary>
    public class MediaTypeManager : IDisposable
    {
        public delegate void SaveAction<T1, T2>(T1 name, T2 value); // Action<T1, T2> is supported since .NET 3.5
        public delegate TResult LoadAction<T1, TResult>(T1 name, TResult defaultValue); 

        private class MediaTypesHolder
        {
            public Hashtable Types { get; set; }
            public Hashtable AggrTypes { get; set; }

            private static MediaTypesHolder _instance;

            static MediaTypesHolder()
            {
                _instance = new MediaTypesHolder();
            }

            public static MediaTypesHolder Instance
            {
                get { return _instance; }
            }

            private MediaTypesHolder()
            {
                CreateHashtables();
            }

            private void CreateHashtables()
            {
                Types = new Hashtable();
                AggrTypes = new Hashtable();

                MediaTypeInfo[] types = new MediaTypeInfo[2];
                types[0] = new MediaTypeInfo(MediaType.Video, GetGuid("div3"));
                types[1] = new MediaTypeInfo(MediaType.Video, GetGuid("DIV3"));
                AggrTypes.Add("DivX 3 Video", types);

                types = new MediaTypeInfo[2];
                types[0] = new MediaTypeInfo(MediaType.Video, GetGuid("div4"));
                types[1] = new MediaTypeInfo(MediaType.Video, GetGuid("DIV4"));
                AggrTypes.Add("DivX 4 Video", types);

                types = new MediaTypeInfo[10];
                types[0] = new MediaTypeInfo(MediaType.Video, GetGuid("div5"));
                types[1] = new MediaTypeInfo(MediaType.Video, GetGuid("DIV5"));
                types[2] = new MediaTypeInfo(MediaType.Video, GetGuid("div6"));
                types[3] = new MediaTypeInfo(MediaType.Video, GetGuid("DIV6"));
                types[4] = new MediaTypeInfo(MediaType.Video, GetGuid("divx"));
                types[5] = new MediaTypeInfo(MediaType.Video, GetGuid("DIVX"));
                types[6] = new MediaTypeInfo(MediaType.Video, GetGuid("dx50"));
                types[7] = new MediaTypeInfo(MediaType.Video, GetGuid("DX50"));
                types[8] = new MediaTypeInfo(MediaType.Video, GetGuid("dvx1"));
                types[9] = new MediaTypeInfo(MediaType.Video, GetGuid("DVX1"));
                AggrTypes.Add("DivX 5 Video", types);

                types = new MediaTypeInfo[2];
                types[0] = new MediaTypeInfo(MediaType.Video, GetGuid("xvid"));
                types[1] = new MediaTypeInfo(MediaType.Video, GetGuid("XVID"));
                AggrTypes.Add("XviD Video", types);

                types = new MediaTypeInfo[2];
                types[0] = new MediaTypeInfo(MediaType.Video, MediaSubType.MPEG1Payload);
                types[1] = new MediaTypeInfo(MediaType.Video, MediaSubType.MPEG1Packet);
                AggrTypes.Add("MPEG 1 Video", types);

                types = new MediaTypeInfo[4];
                types[0] = new MediaTypeInfo(MediaType.Video, MediaSubType.MPEG2_VIDEO);
                types[1] = new MediaTypeInfo(MediaType.MPEG2_PES, MediaSubType.MPEG2_VIDEO);
                types[2] = new MediaTypeInfo(MediaType.DVD_ENCRYPTED_PACK, MediaSubType.MPEG2_VIDEO);
                types[3] = new MediaTypeInfo(MediaType.MPEG2_PACK, MediaSubType.MPEG2_VIDEO);
                AggrTypes.Add("MPEG 2 Video", types);

                types = new MediaTypeInfo[3];
                types[0] = new MediaTypeInfo(MediaType.Audio, MediaSubType.DOLBY_AC3);
                types[1] = new MediaTypeInfo(MediaType.MPEG2_PES, MediaSubType.DOLBY_AC3);
                types[2] = new MediaTypeInfo(MediaType.DVD_ENCRYPTED_PACK, MediaSubType.DOLBY_AC3);
                AggrTypes.Add("Dolby AC3 Audio", types);

                types = new MediaTypeInfo[3];
                types[0] = new MediaTypeInfo(MediaType.Video, MediaSubType.DVD_SUBPICTURE);
                types[1] = new MediaTypeInfo(MediaType.MPEG2_PES, MediaSubType.DVD_SUBPICTURE);
                types[2] = new MediaTypeInfo(MediaType.DVD_ENCRYPTED_PACK, MediaSubType.DVD_SUBPICTURE);
                AggrTypes.Add("DVD Subpicture", types);

                types = new MediaTypeInfo[1];
                types[0] = new MediaTypeInfo(MediaType.Audio, GetGuid(DsHlp.WAVE_FORMAT_MPEGLAYER3));
                AggrTypes.Add("MPEG Layer3 Audio", types);

                types = new MediaTypeInfo[1];
                types[0] = new MediaTypeInfo(MediaType.Stream, MediaSubType.Avi);
                AggrTypes.Add("Avi", types);

                types = new MediaTypeInfo[3];
                types[0] = new MediaTypeInfo(MediaType.Stream, MediaSubType.MPEG1System);
                types[1] = new MediaTypeInfo(MediaType.Stream, MediaSubType.MPEG1Video);
                types[2] = new MediaTypeInfo(MediaType.Stream, MediaSubType.MPEG1VideoCD);
                AggrTypes.Add("MPEG 1", types);

                types = new MediaTypeInfo[2];
                types[0] = new MediaTypeInfo(MediaType.Stream, MediaSubType.MPEG2_PROGRAM);
                types[1] = new MediaTypeInfo(MediaType.Stream, MediaSubType.MPEG2_TRANSPORT);
                AggrTypes.Add("MPEG 2", types);

                IDictionaryEnumerator ide = AggrTypes.GetEnumerator();
                while (ide.MoveNext())
                {
                    string name = (string)ide.Key;
                    MediaTypeInfo[] atypes = (MediaTypeInfo[])ide.Value;
                    IEnumerator ie = atypes.GetEnumerator();
                    while (ie.MoveNext())
                    {
                        MediaTypeInfo type = (MediaTypeInfo)ie.Current;
                        if (!Types.ContainsKey(type.majortype))
                            Types.Add(type.majortype, new Hashtable());
                        Hashtable htSubtype = (Hashtable)Types[type.majortype];
                        if (!htSubtype.ContainsKey(type.subtype))
                            htSubtype.Add(type.subtype, new SubTypeInfo());
                        SubTypeInfo info = (SubTypeInfo)htSubtype[type.subtype];
                        info.Clsid = Guid.Empty;
                        info.typeName = name;
                    }
                }
            }

            private Guid GetGuid(string strFourCC)
            {
                char[] achar = strFourCC.ToCharArray();
                if (achar.Length >= 4)
                {
                    byte[] abyte = { (byte)achar[0], (byte)achar[1], (byte)achar[2], (byte)achar[3], 0x00, 0x00, 0x10, 0x00, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71 };
                    return new Guid(abyte);
                }
                else
                    return Guid.Empty;
            }

            public Guid GetGuid(int data1)
            {
                return new Guid(data1, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
            }
        }
        
        private readonly MediaTypesHolder _typesHolder;
        private IFilterMapper2 pMapper;
                
        public MediaTypeManager()
        {
            _typesHolder = MediaTypesHolder.Instance;

            object comobj = null;
            try
            {
                Type type = Type.GetTypeFromCLSID(Clsid.FilterMapper2, true);
                comobj = Activator.CreateInstance(type);
                pMapper = (IFilterMapper2)comobj;
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
                if (pMapper != null)
                {
                    Marshal.FinalReleaseComObject(pMapper);
                    pMapper = null;
                }
            }
        }

        public class Filter
        {
            public string filterName; // filter's name
            public Guid Clsid;		  // filter's CLSID
            public override string ToString()
            {
                return filterName != null ? filterName : base.ToString();
            }
        }

        #region Private classes
        [Serializable()]
        private class MediaTypeInfo
        {
            public Guid majortype;
            public Guid subtype;
            public MediaTypeInfo(Guid major, Guid sub)
            {
                majortype = major;
                subtype = sub;
            }
        }

        [Serializable()]
        private class SubTypeInfo
        {
            public Guid Clsid;		// CLSID of the associated filter
            public string typeName; // name of the type
        }
        #endregion

        #region Private methods
        
        private MediaTypeInfo[] GetMediaTypeInfoArray(string strType)
        {
            if (_typesHolder.AggrTypes.ContainsKey(strType))
                return (MediaTypeInfo[])_typesHolder.AggrTypes[strType];
            else
                return null;
        }

        #endregion

        public void Load(LoadAction<string, Hashtable> load)
        {
            Hashtable ht1 = load("mtm_media_types", new Hashtable());
            Hashtable ht2 = load("mtm_aggr_types", new Hashtable());
            _typesHolder.Types = ht1;
            _typesHolder.AggrTypes = ht2;
        }

        public void Save(SaveAction<string, Hashtable> save)
        {
            save("mtm_media_types", _typesHolder.Types);
            save("mtm_aggr_types", _typesHolder.AggrTypes);
        }
        
        public string GetFourCC(Guid guid)
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
                Hashtable htSubtype = (Hashtable)_typesHolder.Types[majortype];
                if (htSubtype.ContainsKey(subtype))
                    return ((SubTypeInfo)htSubtype[subtype]).typeName;
            }
            return null;
        }

        // Get Clsid of the filter associated with a media type
        public Guid GetTypeClsid(Guid majortype, Guid subtype)
        {
            if (_typesHolder.Types != null && _typesHolder.Types.ContainsKey(majortype))
            {
                Hashtable htSubtype = (Hashtable)_typesHolder.Types[majortype];
                if (htSubtype.ContainsKey(subtype))
                    return ((SubTypeInfo)htSubtype[subtype]).Clsid;
            }
            return Guid.Empty;
        }

        // Get Clsid of the filter associated with a media type (by type's name)
        public Guid GetTypeClsid(string strType)
        {
            MediaTypeInfo[] atypes = GetMediaTypeInfoArray(strType);
            return atypes != null ? GetTypeClsid(atypes[0].majortype, atypes[0].subtype) 
                : Guid.Empty;
        }

        // Associate a filter with a media type
        public void SetTypesClsid(string strType, Guid filterClsid)
        {
            MediaTypeInfo[] atypes = GetMediaTypeInfoArray(strType);
            if (atypes != null)
            {
                foreach(MediaTypeInfo type in atypes)
                {
                    if (_typesHolder.Types.ContainsKey(type.majortype))
                    {
                        Hashtable htSubtype = (Hashtable)_typesHolder.Types[type.majortype];
                        if (htSubtype.ContainsKey(type.subtype))
                            ((SubTypeInfo)htSubtype[type.subtype]).Clsid = filterClsid;
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
        
        public Filter[] GetFilters(string strType)
        {
            if (pMapper != null)
            {
                MediaTypeInfo[] atypes = GetMediaTypeInfoArray(strType);
                if (atypes != null)
                {
                    ArrayList listFilters = new ArrayList();
                    ArrayList listNames = new ArrayList();
                    IEnumMoniker pEnum;
                    IMoniker[] pMonikers = new IMoniker[1];
                    IPropertyBag pPropBag;
                    Guid IID_IPropertyBag = typeof(IPropertyBag).GUID;
                    Guid IID_IBaseFilter = typeof(IBaseFilter).GUID;
                    int count = atypes.Length;
                    Guid[] arrayInTypes = new Guid[count*2];
                    for (int i=0; i<count; i++)
                    {
                        arrayInTypes[i*2] = atypes[i].majortype;
                        arrayInTypes[i*2+1] = atypes[i].subtype;
                    }
                    int hr = pMapper.EnumMatchingFilters(out pEnum, 
                                                        0,				// Reserved
                                                        true,			// Use exact match?
                                                        (int)MERIT.MERIT_DO_NOT_USE+1,
                                                        true,			// At least one input pin?
                                                        count,			// Number of major type/subtype pairs for input
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
                        while(pEnum.Next(1, pMonikers, cFetched) == DsHlp.S_OK)
                        {
                            o = null;
                            pMonikers[0].BindToStorage(null, null, 
                                ref IID_IPropertyBag, out o);
                            if (o != null && o is IPropertyBag)
                            {
                                pPropBag = (IPropertyBag)o;
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
                                            IBaseFilter pBaseFilter = (IBaseFilter)o;
                                            o = null;

                                            listNames.Add(name);
                                            Filter filter = new Filter();
                                            filter.filterName = name;
                                            pBaseFilter.GetClassID(out filter.Clsid);
                                            listFilters.Add(filter);

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
                    if (listFilters.Count != 0)
                        return (Filter[])listFilters.ToArray(typeof(Filter));
                }

            }
            return null;
        }

        /// <summary>
        /// Get a read-only list of all renderers that are supported on the current system.
        /// </summary>
        public ReadOnlyCollection<Renderer> GetPresentVideoRenderers()
        {
            IList<Renderer> renderers = new List<Renderer>();
            if (pMapper != null)
            {
                IEnumMoniker pEnum;
                IMoniker[] pMonikers = new IMoniker[1];
                Guid IID_IBaseFilter = typeof(IBaseFilter).GUID;
                                
                Guid[] inTypes = new Guid[] { MediaType.Video, Guid.Empty };
                int hr = pMapper.EnumMatchingFilters(out pEnum,
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
