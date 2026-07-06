////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class TypeRegistryList
	{
		private Type baseType;
		private List<Type> typesList = new List<Type>();

		public List<Type> getTypes()
		{
			return typesList;
		}

		public void addType(Type type)
		{
			if (baseType.IsAssignableFrom(type))
			{
				typesList.Add(type);
			}
			else
			{
				throw new ArgumentException(baseType + " is not assignable from " + type, "type");
			}
		}

		public void removeType(Type type)
		{
			typesList.RemoveFast(type);
		}

		public TypeRegistryList(Type newBaseType)
		{
			baseType = newBaseType;
		}
	}
}