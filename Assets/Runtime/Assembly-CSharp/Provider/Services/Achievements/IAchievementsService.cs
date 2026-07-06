////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Provider.Services.Achievements
{
	public interface IAchievementsService : IService
	{
		/// <summary>
		/// Checks whether the current user has an achievement with this name.
		/// </summary>
		/// <param name="name">The name of the achievement.</param>
		/// <param name="has">Whether the user has this achievement.</param>
		/// <returns>Whether the check succesfully executed.</returns>
		bool getAchievement(string name, out bool has);

		/// <summary>
		/// Assigns the current user an achievement with this name.
		/// </summary>
		/// <param name="name">The name of the achievement.</param>
		/// <returns>Whether the assignment succesfully executed.</returns>
		bool setAchievement(string name);
	}
}