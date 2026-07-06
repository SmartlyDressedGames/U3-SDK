////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace SDG.Provider
{
	/// <summary>
	/// Response data from IInventoryService GetInventory web API.
	///
	/// One player's inventory became so large that the Steam client's built-in GetInventory fails,
	/// so as temporary fix we can send them a json file with their inventory.
	/// </summary>
	public class SteamGetInventoryResponse
	{
		public class Item
		{
			public ulong itemid;
			public ushort quantity;
			public int itemdefid;
		}

		public class InnerResponse
		{
			/// <summary>
			/// Json string representation of the contained items.
			/// </summary>
			public string item_json;
		}

		public InnerResponse response;

		/// <summary>
		/// Parse response from json file.
		/// </summary>
		public static List<Item> parse(string path)
		{
			using (StreamReader fileStreamReader = new StreamReader(path))
			using (JsonReader fileJsonReader = new JsonTextReader(fileStreamReader))
			{
				JsonSerializer jsonDeserializer = new JsonSerializer();
				SteamGetInventoryResponse file = jsonDeserializer.Deserialize<SteamGetInventoryResponse>(fileJsonReader);
				List<Item> items = JsonConvert.DeserializeObject<List<Item>>(file.response.item_json);
				return items;
			}
		}
	}
}
