////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public static class EditorUtils
	{
		public static string GetStatusDataVersion()
		{
			StatusData statusData = Provider.LoadStatusData();
			return statusData?.Game?.FormatApplicationVersion();
		}
	}
}
