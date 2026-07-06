////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace Unturned.SystemEx
{
	public static class GuidExtension
	{
		public static bool IsEmpty(this Guid guid)
		{
			return guid.Equals(Guid.Empty);
		}
	}
}
