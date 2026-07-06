////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_ReplicateConfig
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			Provider._modeConfigData = new ModeConfigData(Provider.mode);

			byte repairLevelMax;
			reader.ReadUInt8(out repairLevelMax);
			Provider._modeConfigData.Gameplay.Repair_Level_Max = repairLevelMax;

			reader.ReadFloat(out Provider._modeConfigData.Players.Skill_Cost_Multiplier);
			reader.ReadBit(out Provider._modeConfigData.Players.Skillset_Reduces_Skill_Cost);
			reader.ReadBit(out Provider._modeConfigData.Players.Prevent_Level_Skill_Overrides);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Hitmarkers);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Crosshair);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Ballistics);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Chart);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Satellite);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Compass);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Group_Map);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Group_HUD);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Group_Player_List);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Allow_Static_Groups);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Allow_Dynamic_Groups);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Allow_Shoulder_Camera);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Can_Suicide);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Friendly_Fire);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Bypass_Buildable_Mobility);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Bypass_Building_In_Safezones);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Bypass_No_Building_Zones);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Allow_Freeform_Buildables);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Allow_Freeform_Buildables_On_Vehicles);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Enable_Damage_Flinch);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Enable_Explosion_Camera_Shake);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Enable_Workstation_Requirements);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Disable_Motion_Sickness_Options);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Disable_Foliage_Off);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Use_2D_Scope_Overlay);
			reader.ReadBit(out Provider._modeConfigData.Gameplay.Enable_Fishing_Catch_Challenge);

			ushort timerExit;
			reader.ReadUInt16(out timerExit);
			Provider._modeConfigData.Gameplay.Timer_Exit = MathfEx.Min(timerExit, GameplayConfigData.MAX_TIMER_EXIT);

			ushort timerRespawn;
			reader.ReadUInt16(out timerRespawn);
			Provider._modeConfigData.Gameplay.Timer_Respawn = timerRespawn;

			ushort timerHome;
			reader.ReadUInt16(out timerHome);
			Provider._modeConfigData.Gameplay.Timer_Home = timerHome;

			ushort maxGroupMembers;
			reader.ReadUInt16(out maxGroupMembers);
			Provider._modeConfigData.Gameplay.Max_Group_Members = maxGroupMembers;

			reader.ReadBit(out Provider._modeConfigData.Barricades.Allow_Item_Placement_On_Vehicle);
			reader.ReadBit(out Provider._modeConfigData.Barricades.Allow_Trap_Placement_On_Vehicle);
			reader.ReadFloat(out Provider._modeConfigData.Barricades.Max_Item_Distance_From_Hull);
			reader.ReadFloat(out Provider._modeConfigData.Barricades.Max_Trap_Distance_From_Hull);
			reader.ReadFloat(out Provider._modeConfigData.Gameplay.AirStrafing_Acceleration_Multiplier);
			reader.ReadFloat(out Provider._modeConfigData.Gameplay.AirStrafing_Deceleration_Multiplier);
			reader.ReadFloat(out Provider._modeConfigData.Gameplay.FirstPerson_RecoilMultiplier);
			reader.ReadFloat(out Provider._modeConfigData.Gameplay.FirstPerson_AimingRecoilMultiplier);
			reader.ReadFloat(out Provider._modeConfigData.Gameplay.FirstPerson_AimingZoomRecoilReduction);
			reader.ReadFloat(out Provider._modeConfigData.Gameplay.ThirdPerson_RecoilMultiplier);
			reader.ReadFloat(out Provider._modeConfigData.Gameplay.ThirdPerson_SpreadMultiplier);
			reader.ReadFloat(out Provider._modeConfigData.Gameplay.Viewmodel_AimingJumpLandMultiplier);
			reader.ReadFloat(out Provider._modeConfigData.Gameplay.Viewmodel_AimingMisalignmentMultiplier);
			reader.ReadFloat(out Provider._modeConfigData.Gameplay.Min_Fishing_Bite_Interval);
			reader.ReadFloat(out Provider._modeConfigData.Gameplay.Max_Fishing_Bite_Interval);
			reader.ReadFloat(out Provider._modeConfigData.Gameplay.Fishing_MaxStrength_Bite_Interval_Multiplier);
		}
	}
}
