////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.IO;

namespace Unturned.SystemEx
{
	public static class DirectoryInfoEx
	{
		/// <summary>
		/// Does this contain any file system infos?
		/// Useful when not iterating over the contents.
		/// </summary>
		public static bool ContainsAnything(this DirectoryInfo directory)
		{
			IEnumerable<FileSystemInfo> contents = directory.EnumerateFileSystemInfos();
			using (IEnumerator<FileSystemInfo> enumerator = contents.GetEnumerator())
			{
				return enumerator.MoveNext();
			}
		}

		/// <summary>
		/// Is this empty of any file system infos?
		/// Useful when not iterating over the contents.
		/// </summary>
		public static bool IsEmpty(this DirectoryInfo directory)
		{
			return !directory.ContainsAnything();
		}

		public static DirectoryInfo Join(DirectoryInfo path1, string path2)
		{
			return new DirectoryInfo(PathEx.Join(path1, path2));
		}

		public static DirectoryInfo Join(DirectoryInfo path1, string path2, string path3)
		{
			return new DirectoryInfo(PathEx.Join(path1, path2, path3));
		}

		public static DirectoryInfo Join(DirectoryInfo path1, string path2, string path3, string path4)
		{
			return new DirectoryInfo(PathEx.Join(path1, path2, path3, path4));
		}
	}
}
