////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services;
using SDG.Provider.Services.Translation;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Translation
{
	public class SteamworksTranslationService : Service, ITranslationService
	{
		public string language
		{
			get;
			protected set;
		}

		public override void initialize()
		{
			language = SteamUtils.GetSteamUILanguage();
		}
	}
}