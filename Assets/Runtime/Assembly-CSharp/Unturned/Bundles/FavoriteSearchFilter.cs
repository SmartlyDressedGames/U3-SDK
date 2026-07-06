////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.IO;

namespace SDG.Unturned
{
	/// <summary>
	/// Parses fv:X from input string and loads X.txt from game folder.
	/// </summary>
	public struct FavoriteSearchFilter<T>
	{
		public delegate bool SubFilterParser(string input, out T subFilter);

		public static FavoriteSearchFilter<T>? parse(string filter, SubFilterParser parseSubFilter)
		{
			string value;
			if (SearchFilterUtil.parseKeyValue(filter, "fv:", out value) == false)
				return null;

			string fvPath = Path.Combine(ReadWrite.PATH, value) + ".txt";
			if (File.Exists(fvPath) == false)
				return null;

			// Cannot re-use a static list because this function is recursive.
			List<T> workingSubFilters = new List<T>();

			string[] fvLines = File.ReadAllLines(fvPath);
			foreach (string line in fvLines)
			{
				if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
					continue;

				T subFilter;
				if (parseSubFilter(line, out subFilter))
				{
					workingSubFilters.Add(subFilter);
				}
			}

			if (workingSubFilters.Count < 1)
				return null;

			return new FavoriteSearchFilter<T>(workingSubFilters.ToArray());
		}

		public FavoriteSearchFilter(T[] subFilters)
		{
			this.subFilters = subFilters;
		}

		public T[] subFilters
		{
			get;
			private set;
		}
	}
}
