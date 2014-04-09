using System;
using System.Collections.Generic;
using Pvp.Core.DirectShow;

namespace Pvp.Core.MediaEngine.FilterRegistry
{
    internal class MediaTypesHolder
    {
        public Dictionary<Guid, Dictionary<Guid, AssociatedNamedMediaType>> Types { get; set; }
        public Dictionary<string, MediaType[]> AggrTypes { get; set; }

        private static readonly MediaTypesHolder _instance;

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
            Types = new Dictionary<Guid, Dictionary<Guid, AssociatedNamedMediaType>>();
            AggrTypes = new Dictionary<string, MediaType[]>();

            MediaType[] types = new MediaType[2];
            types[0] = new MediaType(DirectShow.MediaType.Video, GetGuid("div3"));
            types[1] = new MediaType(DirectShow.MediaType.Video, GetGuid("DIV3"));
            AggrTypes.Add("DivX 3 Video", types);

            types = new MediaType[2];
            types[0] = new MediaType(DirectShow.MediaType.Video, GetGuid("div4"));
            types[1] = new MediaType(DirectShow.MediaType.Video, GetGuid("DIV4"));
            AggrTypes.Add("DivX 4 Video", types);

            types = new MediaType[10];
            types[0] = new MediaType(DirectShow.MediaType.Video, GetGuid("div5"));
            types[1] = new MediaType(DirectShow.MediaType.Video, GetGuid("DIV5"));
            types[2] = new MediaType(DirectShow.MediaType.Video, GetGuid("div6"));
            types[3] = new MediaType(DirectShow.MediaType.Video, GetGuid("DIV6"));
            types[4] = new MediaType(DirectShow.MediaType.Video, GetGuid("divx"));
            types[5] = new MediaType(DirectShow.MediaType.Video, GetGuid("DIVX"));
            types[6] = new MediaType(DirectShow.MediaType.Video, GetGuid("dx50"));
            types[7] = new MediaType(DirectShow.MediaType.Video, GetGuid("DX50"));
            types[8] = new MediaType(DirectShow.MediaType.Video, GetGuid("dvx1"));
            types[9] = new MediaType(DirectShow.MediaType.Video, GetGuid("DVX1"));
            AggrTypes.Add("DivX 5 Video", types);

            types = new MediaType[2];
            types[0] = new MediaType(DirectShow.MediaType.Video, GetGuid("xvid"));
            types[1] = new MediaType(DirectShow.MediaType.Video, GetGuid("XVID"));
            AggrTypes.Add("XviD Video", types);

            types = new MediaType[2];
            types[0] = new MediaType(DirectShow.MediaType.Video, MediaSubType.MPEG1Payload);
            types[1] = new MediaType(DirectShow.MediaType.Video, MediaSubType.MPEG1Packet);
            AggrTypes.Add("MPEG 1 Video", types);

            types = new MediaType[4];
            types[0] = new MediaType(DirectShow.MediaType.Video, MediaSubType.MPEG2_VIDEO);
            types[1] = new MediaType(DirectShow.MediaType.MPEG2_PES, MediaSubType.MPEG2_VIDEO);
            types[2] = new MediaType(DirectShow.MediaType.DVD_ENCRYPTED_PACK, MediaSubType.MPEG2_VIDEO);
            types[3] = new MediaType(DirectShow.MediaType.MPEG2_PACK, MediaSubType.MPEG2_VIDEO);
            AggrTypes.Add("MPEG 2 Video", types);

            types = new MediaType[3];
            types[0] = new MediaType(DirectShow.MediaType.Audio, MediaSubType.DOLBY_AC3);
            types[1] = new MediaType(DirectShow.MediaType.MPEG2_PES, MediaSubType.DOLBY_AC3);
            types[2] = new MediaType(DirectShow.MediaType.DVD_ENCRYPTED_PACK, MediaSubType.DOLBY_AC3);
            AggrTypes.Add("Dolby AC3 Audio", types);

            types = new MediaType[3];
            types[0] = new MediaType(DirectShow.MediaType.Video, MediaSubType.DVD_SUBPICTURE);
            types[1] = new MediaType(DirectShow.MediaType.MPEG2_PES, MediaSubType.DVD_SUBPICTURE);
            types[2] = new MediaType(DirectShow.MediaType.DVD_ENCRYPTED_PACK, MediaSubType.DVD_SUBPICTURE);
            AggrTypes.Add("DVD Subpicture", types);

            types = new MediaType[1];
            types[0] = new MediaType(DirectShow.MediaType.Audio, GetGuid(DsHlp.WAVE_FORMAT_MPEGLAYER3));
            AggrTypes.Add("MPEG Layer3 Audio", types);

            types = new MediaType[1];
            types[0] = new MediaType(DirectShow.MediaType.Stream, MediaSubType.Avi);
            AggrTypes.Add("Avi", types);

            types = new MediaType[3];
            types[0] = new MediaType(DirectShow.MediaType.Stream, MediaSubType.MPEG1System);
            types[1] = new MediaType(DirectShow.MediaType.Stream, MediaSubType.MPEG1Video);
            types[2] = new MediaType(DirectShow.MediaType.Stream, MediaSubType.MPEG1VideoCD);
            AggrTypes.Add("MPEG 1", types);

            types = new MediaType[2];
            types[0] = new MediaType(DirectShow.MediaType.Stream, MediaSubType.MPEG2_PROGRAM);
            types[1] = new MediaType(DirectShow.MediaType.Stream, MediaSubType.MPEG2_TRANSPORT);
            AggrTypes.Add("MPEG 2", types);

            foreach (var aggrType in AggrTypes)
            {
                var name = aggrType.Key;
                foreach (var mediaType in aggrType.Value)
                {
                    if (!Types.ContainsKey(mediaType.Majortype))
                        Types.Add(mediaType.Majortype, new Dictionary<Guid, AssociatedNamedMediaType>());
                    var htSubtype = Types[mediaType.Majortype];
                    if (!htSubtype.ContainsKey(mediaType.Subtype))
                        htSubtype.Add(mediaType.Subtype, new AssociatedNamedMediaType(name, Guid.Empty));
                }
            }
        }

        private static Guid GetGuid(string strFourCC)
        {
            char[] achar = strFourCC.ToCharArray();
            if (achar.Length >= 4)
            {
                byte[] abyte = { (byte)achar[0], (byte)achar[1], (byte)achar[2], (byte)achar[3], 0x00, 0x00, 0x10, 0x00, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71 };
                return new Guid(abyte);
            }
            
            return Guid.Empty;
        }

        private static Guid GetGuid(int data1)
        {
            return new Guid(data1, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        }
    }
}