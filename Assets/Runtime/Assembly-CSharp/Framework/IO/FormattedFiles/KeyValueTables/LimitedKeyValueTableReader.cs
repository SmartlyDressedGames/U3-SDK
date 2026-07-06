////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
//#define LOG_KVT_READER

using System.Collections.Generic;
using System.IO;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables
{
	public class LimitedKeyValueTableReader : KeyValueTableReader
	{
		/// <summary>
		/// After the key "limit" is loaded we stop reading.
		/// </summary>
		public string limit
		{
			get;
			protected set;
		}

		protected override bool canContinueReadDictionary(StreamReader input, Dictionary<string, object> scope)
		{
			if (dictionaryKey == limit)
			{
				return false;
			}

			return base.canContinueReadDictionary(input, scope);
		}

		public LimitedKeyValueTableReader() : base()
		{
			limit = null;
		}

		public LimitedKeyValueTableReader(StreamReader input) : this(null, input)
		{ }

		public LimitedKeyValueTableReader(string newLimit, StreamReader input) : base(input)
		{
			limit = newLimit;
		}
	}
}
