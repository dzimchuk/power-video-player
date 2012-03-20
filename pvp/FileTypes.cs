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
using System.Text;

namespace Pvp
{
    internal class FileType
    {
        public string Extension;
        public string Description;
        public FileType(string description, string extension)
        {
            Extension = extension;
            Description = description;
        }
    }
    
    internal static class FileTypes
    {
        public static IList<FileType> GetFileTypes()
        {
            IList<FileType> types = new List<FileType>();
            types.Add(new FileType(Resources.Resources.file_type_asf, ".asf"));
            types.Add(new FileType(Resources.Resources.file_type_avi, ".avi"));
            types.Add(new FileType(Resources.Resources.file_type_dat, ".dat"));
            types.Add(new FileType(Resources.Resources.file_type_divx, ".divx"));
            types.Add(new FileType(Resources.Resources.file_type_flv, ".flv"));
            types.Add(new FileType(Resources.Resources.file_type_ifo, ".ifo"));
            types.Add(new FileType(Resources.Resources.file_type_m1v, ".m1v"));
            types.Add(new FileType(Resources.Resources.file_type_m2v, ".m2v"));
            types.Add(new FileType(Resources.Resources.file_type_mkv, ".mkv"));
            types.Add(new FileType(Resources.Resources.file_type_mov, ".mov"));
            types.Add(new FileType(Resources.Resources.file_type_mp4, ".mp4"));
            types.Add(new FileType(Resources.Resources.file_type_mpe, ".mpe"));
            types.Add(new FileType(Resources.Resources.file_type_mpeg, ".mpeg"));
            types.Add(new FileType(Resources.Resources.file_type_mpg, ".mpg"));
            types.Add(new FileType(Resources.Resources.file_type_qt, ".qt"));
            types.Add(new FileType(Resources.Resources.file_type_vob, ".vob"));
            types.Add(new FileType(Resources.Resources.file_type_wmv, ".wmv"));
            types.Add(new FileType(Resources.Resources.file_type_3gp, ".3gp"));
            types.Add(new FileType(Resources.Resources.file_type_3g2, ".3g2"));
            return types;
        }
    }
}
