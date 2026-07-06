////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class Action
	{
		internal CachingBcAssetRef blueprintOwnerRef;

		private EActionType _type;
		public EActionType type => _type;

		private ActionBlueprint[] _blueprints;
		public ActionBlueprint[] blueprints => _blueprints;

		private string _text;
		public string text => _text;

		private string _tooltip;
		public string tooltip => _tooltip;

		private string _key;
		public string key => _key;

		public Asset FindBlueprintOwnerAsset()
		{
			return blueprintOwnerRef.Get();
		}

		public bool IsAnyBlueprintLink
		{
			get
			{
				if (_blueprints == null)
					return false;

				foreach (ActionBlueprint blueprint in _blueprints)
				{
					if (blueprint.isLink)
					{
						return true;
					}
				}

				return false;
			}
		}

		public Action(ushort newSource, EActionType newType, ActionBlueprint[] newBlueprints, string newText, string newTooltip, string newKey)
		{
			_type = newType;
			_blueprints = newBlueprints;
			_text = newText;
			_tooltip = newTooltip;
			_key = newKey;
		}

		public override string ToString()
		{
			return $"(Type: {_type} Blueprints: {_blueprints?.Length} Text: {_text} Tooltip: {_tooltip} Key: {_key})";
		}

		[System.Obsolete("Please use FindBlueprintOwnerAsset for GUID support")]
		public ushort source => blueprintOwnerRef.LegacyId;
	}
}
