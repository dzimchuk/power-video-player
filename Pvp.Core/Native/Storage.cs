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
using System.Text;

namespace Pvp.Core.Native
{
	/// <summary>
	/// 
	/// </summary>
	public class Storage
	{
		#region Funtions
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern uint GetDriveType([MarshalAs(UnmanagedType.LPTStr)] string lpRootPathName);
		
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern int GetVolumeInformation(
			[MarshalAs(UnmanagedType.LPTStr)] string lpRootPathName,
			StringBuilder lpVolumeNameBuffer,
			int nVolumeNameSize,
			out uint lpVolumeSerialNumber,
			out uint lpMaximumComponentLength,
			out uint lpFileSystemFlags,
			StringBuilder lpFileSystemNameBuffer,
			int nFileSystemNameSize);

		#endregion

		#region Defines
		public const int MAX_PATH = 260;

		public const uint DRIVE_UNKNOWN		= 0;
		public const uint DRIVE_NO_ROOT_DIR	= 1;
		public const uint DRIVE_REMOVABLE	= 2;
		public const uint DRIVE_FIXED		= 3;
		public const uint DRIVE_REMOTE		= 4;
		public const uint DRIVE_CDROM		= 5;
		public const uint DRIVE_RAMDISK		= 6;

		public const uint DELETE                           = 0x00010000;
		public const uint READ_CONTROL                     = 0x00020000;
		public const uint WRITE_DAC                        = 0x00040000;
		public const uint WRITE_OWNER                      = 0x00080000;
		public const uint SYNCHRONIZE                      = 0x00100000;

		public const uint STANDARD_RIGHTS_REQUIRED         = 0x000F0000;

		public const uint STANDARD_RIGHTS_READ             = READ_CONTROL;
		public const uint STANDARD_RIGHTS_WRITE            = READ_CONTROL;
		public const uint STANDARD_RIGHTS_EXECUTE          = READ_CONTROL;

		public const uint STANDARD_RIGHTS_ALL              = 0x001F0000;

		public const uint SPECIFIC_RIGHTS_ALL              = 0x0000FFFF; 
		
		public const uint FILE_READ_DATA            = 0x0001;    // file & pipe
		public const uint FILE_LIST_DIRECTORY       = 0x0001;    // directory

		public const uint FILE_WRITE_DATA           = 0x0002;    // file & pipe
		public const uint FILE_ADD_FILE             = 0x0002;    // directory

		public const uint FILE_APPEND_DATA          = 0x0004;    // file
		public const uint FILE_ADD_SUBDIRECTORY     = 0x0004;    // directory
		public const uint FILE_CREATE_PIPE_INSTANCE = 0x0004;    // named pipe

		public const uint FILE_READ_EA              = 0x0008;    // file & directory

		public const uint FILE_WRITE_EA             = 0x0010;    // file & directory

		public const uint FILE_EXECUTE              = 0x0020;    // file
		public const uint FILE_TRAVERSE             = 0x0020;    // directory

		public const uint FILE_DELETE_CHILD         = 0x0040;    // directory

		public const uint FILE_READ_ATTRIBUTES      = 0x0080;    // all

		public const uint FILE_WRITE_ATTRIBUTES     = 0x0100;    // all

		public const uint FILE_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x1FF;

		public const uint FILE_GENERIC_READ = 
			STANDARD_RIGHTS_READ | 
			FILE_READ_DATA       | 
			FILE_READ_ATTRIBUTES | 
			FILE_READ_EA         | 
			SYNCHRONIZE;

		public const uint FILE_GENERIC_WRITE = 
			STANDARD_RIGHTS_WRITE    | 
			FILE_WRITE_DATA          | 
			FILE_WRITE_ATTRIBUTES    | 
			FILE_WRITE_EA            | 
			FILE_APPEND_DATA         | 
			SYNCHRONIZE;

		public const uint FILE_GENERIC_EXECUTE = 
			STANDARD_RIGHTS_EXECUTE  | 
			FILE_READ_ATTRIBUTES     | 
			FILE_EXECUTE             | 
			SYNCHRONIZE;

		public const uint FILE_SHARE_READ                 = 0x00000001;
		public const uint FILE_SHARE_WRITE                = 0x00000002; 
		public const uint FILE_SHARE_DELETE               = 0x00000004; 
		public const uint FILE_ATTRIBUTE_READONLY             = 0x00000001;
		public const uint FILE_ATTRIBUTE_HIDDEN               = 0x00000002; 
		public const uint FILE_ATTRIBUTE_SYSTEM               = 0x00000004; 
		public const uint FILE_ATTRIBUTE_DIRECTORY            = 0x00000010; 
		public const uint FILE_ATTRIBUTE_ARCHIVE              = 0x00000020; 
		public const uint FILE_ATTRIBUTE_DEVICE               = 0x00000040; 
		public const uint FILE_ATTRIBUTE_NORMAL               = 0x00000080; 
		public const uint FILE_ATTRIBUTE_TEMPORARY            = 0x00000100; 
		public const uint FILE_ATTRIBUTE_SPARSE_FILE          = 0x00000200; 
		public const uint FILE_ATTRIBUTE_REPARSE_POINT        = 0x00000400; 
		public const uint FILE_ATTRIBUTE_COMPRESSED           = 0x00000800; 
		public const uint FILE_ATTRIBUTE_OFFLINE              = 0x00001000; 
		public const uint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED  = 0x00002000; 
		public const uint FILE_ATTRIBUTE_ENCRYPTED            = 0x00004000; 
		public const uint FILE_NOTIFY_CHANGE_FILE_NAME    = 0x00000001;
		public const uint FILE_NOTIFY_CHANGE_DIR_NAME     = 0x00000002;  
		public const uint FILE_NOTIFY_CHANGE_ATTRIBUTES   = 0x00000004;  
		public const uint FILE_NOTIFY_CHANGE_SIZE         = 0x00000008;  
		public const uint FILE_NOTIFY_CHANGE_LAST_WRITE   = 0x00000010;  
		public const uint FILE_NOTIFY_CHANGE_LAST_ACCESS  = 0x00000020;  
		public const uint FILE_NOTIFY_CHANGE_CREATION     = 0x00000040;  
		public const uint FILE_NOTIFY_CHANGE_SECURITY     = 0x00000100;  
		public const uint FILE_ACTION_ADDED                   = 0x00000001;
		public const uint FILE_ACTION_REMOVED                 = 0x00000002;  
		public const uint FILE_ACTION_MODIFIED                = 0x00000003;  
		public const uint FILE_ACTION_RENAMED_OLD_NAME        = 0x00000004;  
		public const uint FILE_ACTION_RENAMED_NEW_NAME        = 0x00000005;  
		public const int MAILSLOT_NO_MESSAGE             = -1; 
		public const int MAILSLOT_WAIT_FOREVER           = -1; 
		public const uint FILE_CASE_SENSITIVE_SEARCH      = 0x00000001;
		public const uint FILE_CASE_PRESERVED_NAMES       = 0x00000002; 
		public const uint FILE_UNICODE_ON_DISK            = 0x00000004; 
		public const uint FILE_PERSISTENT_ACLS            = 0x00000008; 
		public const uint FILE_FILE_COMPRESSION           = 0x00000010; 
		public const uint FILE_VOLUME_QUOTAS              = 0x00000020; 
		public const uint FILE_SUPPORTS_SPARSE_FILES      = 0x00000040; 
		public const uint FILE_SUPPORTS_REPARSE_POINTS    = 0x00000080; 
		public const uint FILE_SUPPORTS_REMOTE_STORAGE    = 0x00000100; 
		public const uint FILE_VOLUME_IS_COMPRESSED       = 0x00008000; 
		public const uint FILE_SUPPORTS_OBJECT_IDS        = 0x00010000; 
		public const uint FILE_SUPPORTS_ENCRYPTION        = 0x00020000; 
		public const uint FILE_NAMED_STREAMS              = 0x00040000; 
		public const uint FILE_READ_ONLY_VOLUME           = 0x00080000;
		#endregion
	}
}
