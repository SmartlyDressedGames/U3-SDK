////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
	public static class TypeExtension
	{
		public static object getDefaultValue(this Type type)
		{
			if (type.IsValueType)
				return Activator.CreateInstance(type);
			else
				return null;
		}
	}
}
