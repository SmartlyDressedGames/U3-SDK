////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Provider.Services.Statistics.User
{
	public interface IUserStatisticsService : IService
	{
		/// <summary>
		/// Triggered when the user's statistics are available.
		/// </summary>
		event UserStatisticsRequestReady onUserStatisticsRequestReady;

		/// <summary>
		/// Checks the current user's statistics with this name.
		/// </summary>
		/// <param name="name">The name of the statistic.</param>
		/// <param name="data">The value of the statistic.</param>
		/// <returns>Whether the check succesfully executed.</returns>
		bool getStatistic(string name, out int data);

		/// <summary>
		/// Assigns the current user's statistics with this name.
		/// </summary>
		/// <param name="name">The name of the statistic.</param>
		/// <param name="data">The value of the statistic.</param>
		/// <returns>Whether the check succesfully executed.</returns>
		bool setStatistic(string name, int data);

		/// <summary>
		/// Checks the current user's statistics with this name.
		/// </summary>
		/// <param name="name">The name of the statistic.</param>
		/// <param name="data">The value of the statistic.</param>
		/// <returns>Whether the check succesfully executed.</returns>
		bool getStatistic(string name, out float data);

		/// <summary>
		/// Assigns the current user's statistics with this name.
		/// </summary>
		/// <param name="name">The name of the statistic.</param>
		/// <param name="data">The value of the statistic.</param>
		/// <returns>Whether the check succesfully executed.</returns>
		bool setStatistic(string name, float data);

		/// <summary>
		/// Requests the user's statistics.
		/// </summary>
		/// <returns>Whether the refresh succesfully executed.</returns>
		bool requestStatistics();
	}
}