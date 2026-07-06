////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace Unturned.SystemEx
{
	public static class TypeEx
	{
		/// <summary>
		/// Wraps <see cref="System.Type.IsAssignableFrom"/> in a try/catch block.
		/// Useful if otherType might not be loadable. (public issue #4171)
		/// </summary>
		/// <returns>False if an exception is thrown, otherwise result of IsAssignableFrom.</returns>
		public static bool TryIsAssignableFrom(this System.Type type, System.Type otherType)
		{
			try
			{
				return type.IsAssignableFrom(otherType);
			}
			catch
			{
				return false;
			}
		}
	}
}
