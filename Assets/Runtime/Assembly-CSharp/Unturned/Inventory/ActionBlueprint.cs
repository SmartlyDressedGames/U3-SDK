////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ActionBlueprint
	{
		internal int index;
		internal string blueprintName;

		/// <summary>
		/// Index into Blueprints list. -1 means blueprint name is used instead.
		/// </summary>
		public int Index => index;

		/// <summary>
		/// Name to look for in Blueprints list.
		/// </summary>
		public string BlueprintName => blueprintName;
		
		internal bool _isLink;
		public bool isLink => _isLink;

		public Blueprint FindBlueprint(IBlueprintOwner blueprintOwner)
		{
			if (index >= 0)
			{
				return blueprintOwner.GetBlueprintByIndex(index);
			}
			else if (!string.IsNullOrEmpty(blueprintName))
			{
				return blueprintOwner.FindBlueprintByName(blueprintName);
			}
			else
			{
				return null;
			}
		}

		public override string ToString()
		{
			return $"(Index: {index} Name: {blueprintName} Link: {_isLink})";
		}

		public ActionBlueprint(int newIndex, bool newLink)
		{
			index = newIndex;
			_isLink = newLink;
		}

		[System.Obsolete("Renamed to Index")]
		public byte id => (byte) index;
	}
}
