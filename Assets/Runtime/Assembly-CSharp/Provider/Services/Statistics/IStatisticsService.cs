////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services.Statistics.Global;
using SDG.Provider.Services.Statistics.User;

namespace SDG.Provider.Services.Statistics
{
	public interface IStatisticsService : IService
	{
		/// <summary>
		/// Current user statistics implementation.
		/// </summary>
		IUserStatisticsService userStatisticsService
		{
			get;
		}

		/// <summary>
		/// Current global statistics implementation.
		/// </summary>
		IGlobalStatisticsService globalStatisticsService
		{
			get;
		}
	}
}