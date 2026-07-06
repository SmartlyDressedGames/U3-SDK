////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class PreferenceData
	{
		public bool Allow_Ctrl_Shift_Alt_Salvage;

		public AudioPreferenceData Audio;
		public GraphicsPreferenceData Graphics;
		public ViewmodelPreferenceData Viewmodel;
		public ChatPreferenceData Chat;

		public PreferenceData()
		{
			Allow_Ctrl_Shift_Alt_Salvage = false;

			Audio = new AudioPreferenceData();
			Graphics = new GraphicsPreferenceData();
			Viewmodel = new ViewmodelPreferenceData();
			Chat = new ChatPreferenceData();
		}
	}

	public class AudioPreferenceData
	{
		public float Vehicle_Engine_Volume_Multiplier;

		public AudioPreferenceData()
		{
			Vehicle_Engine_Volume_Multiplier = 1.0f;
		}
	}

	public class GraphicsPreferenceData
	{
		public bool Use_Lens_Dirt;
		public float Chromatic_Aberration_Intensity;
		public float LOD_Bias;
		//public bool Restrict_Resolution_To_Monitor_Supported;
		public int Override_Resolution_Width;
		public int Override_Resolution_Height;
		public float Override_UI_Scale;
		public int Override_Fullscreen_Mode;
		public int Override_Refresh_Rate;
		public float Override_Vertical_Field_Of_View;
		public ETextContrastPreference Default_Text_Contrast;
		public ETextContrastPreference Inconspicuous_Text_Contrast;
		public ETextContrastPreference Colorful_Text_Contrast;

		public GraphicsPreferenceData()
		{
			Use_Lens_Dirt = true;
			Chromatic_Aberration_Intensity = 0.2f;
			LOD_Bias = 0.0f;
			//Restrict_Resolution_To_Monitor_Supported = false;
			Override_Resolution_Width = -1;
			Override_Resolution_Height = -1;
			Override_Refresh_Rate = -1;
			Override_UI_Scale = -1;
			Override_Fullscreen_Mode = -1;
			Override_Vertical_Field_Of_View = -1.0f;

			Inconspicuous_Text_Contrast = ETextContrastPreference.Default;
			Colorful_Text_Contrast = ETextContrastPreference.Default;
		}
	}

	public class ViewmodelPreferenceData
	{
		public float Field_Of_View_Aim;
		public float Field_Of_View_Hip;
		public float Field_Of_View_Aim_Scope;
		public float Offset_Horizontal;
		public float Offset_Vertical;
		public float Offset_Depth;

		public void Clamp()
		{
			Field_Of_View_Aim = Field_Of_View_Aim.IsFinite() ? UnityEngine.Mathf.Clamp(Field_Of_View_Aim, 1.0f, 179.0f) : 60.0f;
			Field_Of_View_Hip = Field_Of_View_Hip.IsFinite() ? UnityEngine.Mathf.Clamp(Field_Of_View_Hip, 1.0f, 179.0f) : 60.0f;
			Field_Of_View_Aim_Scope = Field_Of_View_Aim_Scope.IsFinite() ? UnityEngine.Mathf.Clamp(Field_Of_View_Aim_Scope, 1.0f, 179.0f) : 60.0f;
			Offset_Horizontal = Offset_Horizontal.IsFinite() ? UnityEngine.Mathf.Clamp(Offset_Horizontal, -1.0f, 1.0f) : 0.0f;
			Offset_Vertical = Offset_Vertical.IsFinite() ? UnityEngine.Mathf.Clamp(Offset_Vertical, -1.0f, 1.0f) : 0.0f;
			Offset_Depth = Offset_Depth.IsFinite() ? UnityEngine.Mathf.Clamp(Offset_Depth, -0.5f, 0.5f) : 0.0f;
		}

		public ViewmodelPreferenceData()
		{
			Field_Of_View_Aim = 60.0f;
			Field_Of_View_Hip = 60.0f;
			Field_Of_View_Aim_Scope = 60.0f;
			Offset_Horizontal = 0.0f;
			Offset_Vertical = 0.0f;
			Offset_Depth = 0.0f;
		}
	}

	public class ChatPreferenceData
	{
		public const int DEFAULT_HISTORY_LENGTH = 16;

		public float Fade_Delay; // How long after being received before chat messages fade out.
		public int History_Length; // How many messages to keep before throwing old ones away.
		public int Preview_Length; // How many messages to show while not in history scroll mode.

		public ChatPreferenceData()
		{
			Fade_Delay = 10.0f;
			History_Length = DEFAULT_HISTORY_LENGTH;
			Preview_Length = 5;
		}
	}
}
