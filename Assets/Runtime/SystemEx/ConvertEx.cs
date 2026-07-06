////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace Unturned.SystemEx
{
	public static class ConvertEx
	{
		public static bool TryEncodeUtf8StringAsBase64(string input, out string output)
		{
			if (string.IsNullOrEmpty(input))
			{
				output = input;
				return true;
			}

			output = null;

			byte[] utf8Bytes;
			try
			{
				utf8Bytes = System.Text.Encoding.UTF8.GetBytes(input);
			}
			catch
			{
				return false;
			}

			string base64String;
			try
			{
				base64String = System.Convert.ToBase64String(utf8Bytes);
			}
			catch
			{
				return false;
			}

			output = base64String;
			return true;
		}

		public static bool TryDecodeBase64AsUtf8String(string input, out string output)
		{
			if (string.IsNullOrEmpty(input))
			{
				output = input;
				return true;
			}

			output = null;

			byte[] utf8Bytes;
			try
			{
				utf8Bytes = System.Convert.FromBase64String(input);
			}
			catch
			{
				return false;
			}

			string utf8String = null;
			try
			{
				utf8String = System.Text.Encoding.UTF8.GetString(utf8Bytes);
			}
			catch
			{
				return false;
			}

			output = utf8String;
			return true;
		}
	}
}
