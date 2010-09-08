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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dzimchuk.DirectShow;
using Microsoft.Win32;

namespace Dzimchuk.MediaEngine.Core.GraphBuilders
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class SourceAnalizer
    {
        protected class SourceInfo
        {
            public SourceType type = SourceType.Unknown;
            public Guid clsId = Guid.Empty;
        }

        protected class SampleBytes
        {
            public int offset;
            public int cb;
            public string mask;
            public string value;

            public SampleBytes(int offset, int cb, string value) : this(offset, cb, String.Empty, value)
            {
            }

            public SampleBytes(int offset, int cb, string mask, string value)
            {
                this.offset = offset;
                this.cb = cb;
                this.mask = mask;
                this.value = value;
            }
        }

        const string SOURCE_FILTER = "Source Filter";
        static SourceAnalizer[] analizers;
        private static bool _sourceFilterDetectionEnabled = false;

        static SourceAnalizer()
        {
            analizers = new SourceAnalizer[4];
            analizers[0] = new SourceBasic();
            analizers[1] = new SourceAsf();
            analizers[2] = new SourceMkv();
            analizers[3] = new SourceFlv();
        }
        
        internal SourceAnalizer()
        {
        }

        public static bool SourceFilterDetectionEnabled
        {
            get { return _sourceFilterDetectionEnabled; }
            set { _sourceFilterDetectionEnabled = value; }
        }

        public static void SetSourceType(string source,
                                         FilterGraph pGraph)
        {
            int nBytesToRead = 0;
            foreach (SourceAnalizer analizer in analizers)
            {
                if (analizer.NumberOfBytes > nBytesToRead)
                    nBytesToRead = analizer.NumberOfBytes;
            }
            byte[] bytes = new byte[nBytesToRead];

            using (FileStream stream = File.OpenRead(source))
            {
                stream.Read(bytes, 0, bytes.Length);
            }

            int nAnalizer = 0;
            SourceInfo si = new SourceInfo();
            while (nAnalizer < analizers.Length)
            {
                si = analizers[nAnalizer].GetSourceType(bytes);
                if (si.type != SourceType.Unknown)
                    break;

                nAnalizer++;
            }

            if(si.type == SourceType.Unknown)
                SetSourceTypeByExtension(source, si);
            
            if (si.clsId == Guid.Empty)
                SetClsIdByExtension(source, si);

            pGraph.SourceType = si.type;
            pGraph.RecommnedSourceFilterId = si.clsId;
        }

        private static void SetSourceTypeByExtension(string source,
                                                     SourceInfo si)
        {
            string ext = Path.GetExtension(source).Trim().ToLowerInvariant();
            if (ext.EndsWith("avi") || ext.EndsWith("ivx") || ext.EndsWith("mpg")
                || ext.EndsWith("peg") || ext.EndsWith("mov") || ext.EndsWith("vob")
                || ext.EndsWith("mp4") || ext.EndsWith("3gp") || ext.EndsWith("3g2"))
                si.type = SourceType.Basic;
            else if (ext.EndsWith("asf") || ext.EndsWith("wmv"))
                si.type = SourceType.Asf;
            else if (ext.EndsWith("ifo"))
                si.type = SourceType.DVD;
            else if (ext.EndsWith("mkv"))
                si.type = SourceType.Mkv;
            else if (ext.EndsWith("flv"))
                si.type = SourceType.Flv;
            else
                si.type = SourceType.Unknown;
        }

        private static void SetClsIdByExtension(string source,
                                                SourceInfo si)
        {
            if (!SourceFilterDetectionEnabled)
                return;
            
            RegistryKey key = null;
            string ext = Path.GetExtension(source).Trim().ToLowerInvariant();
            try
            {
                key = Registry.ClassesRoot.OpenSubKey(String.Format(@"Media Type\Extensions\.{0}", ext));
                string strGuid = key.GetValue(SOURCE_FILTER) as string;
                if (strGuid != null)
                {
                    si.clsId = new Guid(strGuid);
                }
            }
            catch
            { // nothing to do here
            }
            finally
            {
                if (key != null)
                    key.Close();
            }
        }

        protected bool GetSourceType(byte[] formatBytes, byte[] streamBytes, int start)
        {
            bool bRet = true;
            for (int i = 0; i < formatBytes.Length; i++)
            {
                if (formatBytes[i] != streamBytes[start])
                {
                    bRet = false;
                    break;
                }
                start++;
            }
            return bRet;
        }

        private char[] separator = new char[] { ',' };
        private IList<SampleBytes> ParseSampleBytes(string format)
        {
            IList<SampleBytes> list = new List<SampleBytes>();
            string[] bits = format.Split(separator);
            int nSamples = bits.Length / 4;
            for (int i = 0; i < nSamples; i++)
            {
                SampleBytes sample = new SampleBytes(Int32.Parse(bits[0 + (i * 4)].Trim()),
                                                     Int32.Parse(bits[1 + (i * 4)].Trim()),
                                                     bits[2 + (i * 4)].Trim(),
                                                     bits[3 + (i * 4)].Trim());
                list.Add(sample);
            }

            return list;
        }

        private const int MIN_BYTES = 2; // we want at least 3 bytes in the same position to match
        // all of provided samples in formatBytes must match those from the registry (in registry there can be more samples (quads) but we need all ours to match)
        private bool Match(string formatReg, IList<SampleBytes> formatBytes)
        {
            try
            {
                IList<SampleBytes> samplesReg = ParseSampleBytes(formatReg);
                int nMatches = 0;

                foreach (SampleBytes sample in formatBytes)
                {
                    bool bMatch = false;
                    foreach (SampleBytes sampleReg in samplesReg)
                    {
                        if (sample.offset >= sampleReg.offset) // sample is further than sampleReg
                        {
                            int gap = sample.offset - sampleReg.offset;
                            if (sampleReg.cb - gap > MIN_BYTES && sample.cb > MIN_BYTES) // overlapping area is more than MIN_BYTES
                            {
                                string sampleRegOverlap = sampleReg.value.Substring(gap * 2, Math.Min((sampleReg.cb - gap) * 2, sample.cb * 2));
                                string sampleOverlap = sample.value.Substring(0, Math.Min((sampleReg.cb - gap) * 2, sample.cb * 2));
                                bMatch = sampleRegOverlap == sampleOverlap;
                                if (bMatch)
                                    break;
                            }
                        }
                        else // sample is prior to sampleReg
                        {
                            int gap = sampleReg.offset - sample.offset;
                            if (sample.cb - gap > MIN_BYTES && sampleReg.cb > MIN_BYTES)
                            {
                                string sampleOverlap = sample.value.Substring(gap * 2, Math.Min((sample.cb - gap) * 2, sampleReg.cb * 2));
                                string sampleRegOverlap = sampleReg.value.Substring(0, Math.Min((sample.cb - gap) * 2, sampleReg.cb * 2));
                                bMatch = sampleRegOverlap == sampleOverlap;
                                if (bMatch)
                                    break;
                            }
                        }
                    }

                    if (!bMatch) // no matches found for our quad (sample) amoung samplesReg, it makes no sense to continue
                        break;
                    else
                        nMatches++; // one more is matched
                }

                return nMatches == formatBytes.Count; // all are matched?
            }
            catch
            {
                return false;
            }
        }

        protected Guid GetClsId(IList<SampleBytes> formatBytes) // offset,cb,mask,val
        {
            if (!_sourceFilterDetectionEnabled)
                return Guid.Empty;
            
            RegistryKey keyStream = null;
            RegistryKey keySubtype = null;
            Guid clsId = Guid.Empty;
            try
            {
                keyStream = Registry.ClassesRoot.OpenSubKey(String.Format(@"Media Type\{0}", MediaType.Stream.ToString("B").ToUpperInvariant()));
                if (keyStream != null)
                {
                    string[] subtypes = keyStream.GetSubKeyNames();
                    foreach (string subtype in subtypes)
                    {
                        bool bFound = false;
                        keySubtype = keyStream.OpenSubKey(subtype);
                        if (keySubtype != null)
                        {
                            string[] names = keySubtype.GetValueNames();
                            foreach (string name in names)
                            {
                                int n;
                                if (Int32.TryParse(name, out n) && keySubtype.GetValueKind(name) == RegistryValueKind.String) // value names should be 0, 1, 2, etc, i.e. numbers
                                {
                                    string formatReg = keySubtype.GetValue(name) as string;
                                    if (formatReg != null)
                                    {
                                        if (Match(formatReg.ToUpperInvariant(), formatBytes))
                                        {
                                            // found it
                                            string strGuid = keySubtype.GetValue(SOURCE_FILTER) as string;
                                            if (strGuid != null)
                                            {
                                                clsId = new Guid(strGuid);
                                                bFound = true;
                                                break; // out of inner foreach
                                            } // if
                                        } // if
                                    } // if
                                } // if
                            } // foreach

                            keySubtype.Close();
                            keySubtype = null;

                        } // if

                        if (bFound)
                            break;

                    } // foreach
                } // if

                return clsId;
            }
            catch
            {
                return clsId;
            }
            finally
            {
                if (keyStream != null)
                    keyStream.Close();
                if (keySubtype != null)
                    keySubtype.Close();
            }
        }

        protected string BytesToString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
                builder.AppendFormat("{0:X2}", b);
            return builder.ToString().ToUpperInvariant();
        }

        protected abstract int NumberOfBytes { get; }
        protected abstract SourceInfo GetSourceType(byte[] bytes);
    }

    internal class SourceBasic : SourceAnalizer
    {
        protected override int NumberOfBytes
        {
            get { return 16; }
        }

        protected override SourceInfo GetSourceType(byte[] bytes)
        {
            SourceInfo si = new SourceInfo();
            CheckAvi(bytes, ref si);
            if(si.type == SourceType.Unknown)
                CheckMpeg(bytes, ref si);
            if (si.type == SourceType.Unknown)
                CheckMov(bytes, ref si);
            return si;
        }

        private void CheckAvi(byte[] bytes, ref SourceInfo si)
        {
            byte[] formatBytes1 = { 0x52, 0x49, 0x46, 0x46 }; //0:4 = "RIFF"
            bool bContinue = GetSourceType(formatBytes1, bytes, 0);

            if (bContinue)
            {
                byte[] formatBytes2 = new byte[] { 0x41, 0x56, 0x49, 0x20, 0x4C, 0x49, 0x53, 0x54 }; //8:8 = "AVI LIST"
                bContinue = GetSourceType(formatBytes2, bytes, 8);

                if (bContinue)
                {
                    IList<SampleBytes> list = new List<SampleBytes>();
                    list.Add(new SampleBytes(0, formatBytes1.Length, BytesToString(formatBytes1)));
                    list.Add(new SampleBytes(8, formatBytes2.Length, BytesToString(formatBytes2)));

                    si.type = SourceType.Basic;
                    si.clsId = GetClsId(list);
                }
            }
        }

        private void CheckMpeg(byte[] bytes, ref SourceInfo si)
        {
            byte[] formatBytes = { 0x00, 0x00, 0x01 }; //0:3 = 000001
            bool bContinue = GetSourceType(formatBytes, bytes, 0);

            if (bContinue && (bytes[3] == 0xBA || bytes[3] == 0xB3))
            {
                if (bContinue)
                {
                    IList<SampleBytes> list = new List<SampleBytes>();
                    list.Add(new SampleBytes(0, formatBytes.Length, BytesToString(formatBytes)));

                    si.type = SourceType.Basic;
                    si.clsId = GetClsId(list);
                }
            }
        }

        private void CheckMov(byte[] bytes, ref SourceInfo si)
        {
            byte[] formatBytes = { 0x6D, 0x6F, 0x6F, 0x76 }; //4:4 = "moov"
            bool bContinue = GetSourceType(formatBytes, bytes, 4);

            if (bContinue)
            {
                IList<SampleBytes> list = new List<SampleBytes>();
                list.Add(new SampleBytes(4, formatBytes.Length, BytesToString(formatBytes)));

                si.type = SourceType.Basic;
                si.clsId = GetClsId(list);
            }
        }
    }

    internal class SourceAsf : SourceAnalizer
    {
        protected override int NumberOfBytes
        {
            get { return 13; }
        }

        protected override SourceInfo GetSourceType(byte[] bytes)
        {
            SourceInfo si = new SourceInfo();
            byte[] formatBytes = { 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00 }; //1:12 = 26B2758E66CF11A6D900AA00
            bool bContinue = GetSourceType(formatBytes, bytes, 1);

            if (bContinue)
            {
                IList<SampleBytes> list = new List<SampleBytes>();
                list.Add(new SampleBytes(1, formatBytes.Length, BytesToString(formatBytes)));

                si.type = SourceType.Asf;
                si.clsId = GetClsId(list);
            }
            return si;
        }
    }

    internal class SourceMkv : SourceAnalizer
    {
        protected override int NumberOfBytes
        {
            get { return 4; }
        }

        protected override SourceInfo GetSourceType(byte[] bytes)
        {
            SourceInfo si = new SourceInfo();
            byte[] formatBytes = { 0x1A, 0x45, 0xDF, 0xA3 }; //0:4 = 1A 45 DF A3
            bool bContinue = GetSourceType(formatBytes, bytes, 0);

            if (bContinue)
            {
                IList<SampleBytes> list = new List<SampleBytes>();
                list.Add(new SampleBytes(0, formatBytes.Length, BytesToString(formatBytes)));

                si.type = SourceType.Mkv;
                si.clsId = GetClsId(list);
            }
            return si;
        }
    }

    internal class SourceFlv : SourceAnalizer
    {
        protected override int NumberOfBytes
        {
            get { return 3; }
        }

        protected override SourceInfo GetSourceType(byte[] bytes)
        {
            SourceInfo si = new SourceInfo();
            byte[] formatBytes = { 0x46, 0x4C, 0x56 }; //0:3 = 46 4C 56
            bool bContinue = GetSourceType(formatBytes, bytes, 0);

            if (bContinue)
            {
                IList<SampleBytes> list = new List<SampleBytes>();
                list.Add(new SampleBytes(0, formatBytes.Length, BytesToString(formatBytes)));
                
                si.type = SourceType.Flv;
                si.clsId = GetClsId(list);
            }
            return si;
        }
    }
}
