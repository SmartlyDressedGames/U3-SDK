////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class TypeRegistryDictionary
	{
		private Type baseType;
		private Dictionary<string, Type> typesDictionary = new Dictionary<string, Type>();

		public Type getType(string key)
		{
			Type value = null;
			typesDictionary.TryGetValue(key, out value);

			return value;
		}

		public void addType(string key, Type value)
		{
			if (baseType.IsAssignableFrom(value))
			{
				typesDictionary.Add(key, value);
			}
			else
			{
				throw new ArgumentException(baseType + " is not assignable from " + value, "value");
			}
		}

		public void removeType(string key)
		{
			typesDictionary.Remove(key);
		}

		public TypeRegistryDictionary(Type newBaseType)
		{
			baseType = newBaseType;
		}
	}
}