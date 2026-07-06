////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Reflection;
using System.Reflection.Emit;

namespace System.Collections.Generic
{
	public static class ListExtension
	{
		private static class ListInternalArrayAccessor<T>
		{
			public static Func<List<T>, T[]> Getter;

			static ListInternalArrayAccessor()
			{
				// Thanks to Ondrej Petrzilka on Stack Overflow CC BY-SA 4.0:  
				// https://stackoverflow.com/a/17308019
				DynamicMethod dm = new DynamicMethod("get", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(T[]), new Type[] { typeof(List<T>) }, typeof(ListInternalArrayAccessor<T>), true);
				ILGenerator il = dm.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
				il.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)); // Replace argument by field
				il.Emit(OpCodes.Ret); // Return field
				Getter = (Func<List<T>, T[]>) dm.CreateDelegate(typeof(Func<List<T>, T[]>));
			}
		}

		public static void RemoveAtFast<T>(this List<T> list, int index)
		{
			list[index] = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
		}

		public static bool RemoveFast<T>(this List<T> list, T item)
		{
			int index = list.IndexOf(item);

			if (index < 0)
			{
				return false;
			}

			list.RemoveAtFast(index);
			return true;
		}

		// Refer to ListInternalArrayAccessor for credit.
		public static T[] GetInternalArray<T>(this List<T> list)
		{
			return ListInternalArrayAccessor<T>.Getter(list);
		}
	}
}
