////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandCamera : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			ECameraMode cameramode;
			string mode = parameter.ToLower();

			if (mode == localization.format("CameraFirst").ToLower())
			{
				cameramode = ECameraMode.FIRST;
			}
			else if (mode == localization.format("CameraThird").ToLower())
			{
				cameramode = ECameraMode.THIRD;
			}
			else if (mode == localization.format("CameraBoth").ToLower())
			{
				cameramode = ECameraMode.BOTH;
			}
			else if (mode == localization.format("CameraVehicle").ToLower())
			{
				cameramode = ECameraMode.VEHICLE;
			}
			else
			{
				CommandWindow.LogError(localization.format("NoCameraErrorText", mode));
				return;
			}

			if (Provider.isServer)
			{
				CommandWindow.LogError(localization.format("RunningErrorText"));
				return;
			}

			Provider.cameraMode = cameramode;
			CommandWindow.Log(localization.format("CameraText", mode));
		}

		public CommandCamera(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("CameraCommandText");
			_info = localization.format("CameraInfoText");
			_help = localization.format("CameraHelpText");
		}
	}
}