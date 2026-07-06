////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class EditorObjectSearchFilter
	{
		public static EditorObjectSearchFilter parse(string filter)
		{
			if (string.IsNullOrEmpty(filter))
				return null;

			return new EditorObjectSearchFilter(filter);
		}

		public bool matches(ObjectAsset objectAsset)
		{
			if (mbFilter.HasValue)
			{
				if (mbFilter.Value.ignores(objectAsset))
					return false;
			}

			if (tokenFilter.HasValue)
			{
				if (tokenFilter.Value.ignores(objectAsset.objectName))
					return false;
			}

			if (fvFilter.HasValue)
			{
				foreach (EditorObjectSearchFilter subFilter in fvFilter.Value.subFilters)
				{
					if (subFilter.matches(objectAsset))
						return true;
				}

				return false;
			}
			else
			{
				return true;
			}
		}

		public bool ignores(ObjectAsset objectAsset)
		{
			return matches(objectAsset) == false;
		}

		public bool matches(ItemAsset itemAsset)
		{
			if (mbFilter.HasValue)
			{
				if (mbFilter.Value.ignores(itemAsset))
					return false;
			}

			if (tokenFilter.HasValue)
			{
				if (tokenFilter.Value.ignores(itemAsset.itemName))
					return false;
			}

			if (fvFilter.HasValue)
			{
				foreach (EditorObjectSearchFilter subFilter in fvFilter.Value.subFilters)
				{
					if (subFilter.matches(itemAsset))
						return true;
				}

				return false;
			}
			else
			{
				return true;
			}
		}

		public bool ignores(ItemAsset itemAsset)
		{
			return matches(itemAsset) == false;
		}

		private EditorObjectSearchFilter(string filter)
		{
			tokenFilter = TokenSearchFilter.parse(filter);
			mbFilter = MasterBundleSearchFilter.parse(filter);
			fvFilter = FavoriteSearchFilter<EditorObjectSearchFilter>.parse(filter, parseFavoriteSubFilter);
		}

		private bool parseFavoriteSubFilter(string filter, out EditorObjectSearchFilter subFilter)
		{
			subFilter = parse(filter);
			return subFilter != null;
		}

		private TokenSearchFilter? tokenFilter;
		private MasterBundleSearchFilter? mbFilter;
		private FavoriteSearchFilter<EditorObjectSearchFilter>? fvFilter;
	}
}
