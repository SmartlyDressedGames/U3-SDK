////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Provider.Services.Matchmaking
{
	public class MatchmakingFilter : IMatchmakingFilter
	{
		public string key
		{
			get;
			protected set;
		}

		public string value
		{
			get;
			protected set;
		}

		public MatchmakingFilter(string newKey, string newValue)
		{
			key = newKey;
			value = newValue;
		}
	}
}
