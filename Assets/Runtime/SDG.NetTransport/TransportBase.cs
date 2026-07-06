////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.NetTransport
{
	public abstract class TransportBase
	{
		public delegate string GetMessageCallback(string key, params object[] args);
		/// <summary>
		/// SDG.Unturned.MenuDashboardUI binds this for translation.
		/// Should be cleaned up when reworking translation system.
		/// </summary>
		public static GetMessageCallback OnGetMessage;

		/// <summary>
		/// Utility to localize an error message so the translations can easily be moved if needed.
		/// </summary>
		public string GetMessageText(string key)
		{
			if (OnGetMessage != null)
			{
				return OnGetMessage(key);
			}
			else
			{
				return key;
			}
		}

		public string GetMessageText(string key, params object[] args)
		{
			if (OnGetMessage != null)
			{
				return OnGetMessage(key, args);
			}
			else
			{
				return key;
			}
		}
	}
}
