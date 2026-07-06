////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace Unturned.Jenkins
{
	public static class JenkinsEnvironment
	{
		public static int BuildNumber
		{
			get;
			private set;
		}

		/// <summary>
		/// The remote branch name, if any.
		/// </summary>
		public static string BranchName
		{
			get;
			private set;
		}

		static JenkinsEnvironment()
		{
			string BuildNumberValue = Environment.GetEnvironmentVariable("BUILD_NUMBER");
			int ParsedBuildNumber;
			if (int.TryParse(BuildNumberValue, out ParsedBuildNumber))
			{
				BuildNumber = ParsedBuildNumber;
			}
			else
			{
				BuildNumber = -1;
			}

			BranchName = Environment.GetEnvironmentVariable("GIT_BRANCH");
		}
	}
}
