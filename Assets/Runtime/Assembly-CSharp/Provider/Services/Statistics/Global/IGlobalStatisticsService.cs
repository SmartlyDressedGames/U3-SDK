////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Provider.Services.Statistics.Global
{
	public interface IGlobalStatisticsService : IService
	{
		/// <summary>
		/// Triggered when the global statistics are available.
		/// </summary>
		event GlobalStatisticsRequestReady onGlobalStatisticsRequestReady;

		/// <summary>
		/// Checks the global total of the statistic with this name.
		/// </summary>
		/// <param name="name">The name of the statistic.</param>
		/// <param name="data">The value of the statistic.</param>
		/// <returns>Whether the check succesfully executed.</returns>
		bool getStatistic(string name, out long data);

		/// <summary>
		/// Checks the global total of the statistic with this name.
		/// </summary>
		/// <param name="name">The name of the statistic.</param>
		/// <param name="data">The value of the statistic.</param>
		/// <returns>Whether the check succesfully executed.</returns>
		bool getStatistic(string name, out double data);

		/// <summary>
		/// Requests the global statistics.
		/// </summary>
		/// <returns>Whether the refresh succesfully executed.</returns>
		bool requestStatistics();
	}
}