////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCTimeOfDayCondition : NPCLogicCondition
	{
		public int second
		{
			get;
			protected set;
		}

		public override bool isConditionMet(Player player)
		{
			float time;
			if (LightingManager.day < LevelLighting.bias)
			{
				// 0 = dawn
				// 0.5 = noon
				// 1 = dusk
				time = LightingManager.day / LevelLighting.bias;
				time /= 2; // Map into [0, 0.5]
			}
			else
			{
				// 0 = dusk
				// 0.5 = midnight
				// 1 = dawn
				time = (LightingManager.day - LevelLighting.bias) / (1 - LevelLighting.bias);
				time = 0.5f + (time / 2); // Map into [0.5, 1]
			}

			// this was actually annoying because it meant seconds started at 0 at dawn rather than at midnight
			// add 0.25 so that time is [0.25, 1.25]
			time += 0.25f;
			// there's probably a better mathy way to do this (modulo?), but we want to turn 1.1 into 0.1
			if (time >= 1.0f)
			{
				time -= 1.0f;
			}

			int currentSecond = (int) (time * 86400);
			return doesLogicPass(currentSecond, second);
		}

		public override string formatCondition(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			// e.g. second = 3672
			// displayHour = 3672 / 3600 = 1
			// displayMinute = 3672 / 60 = 61 - 1 * 60 = 1
			// displayMinute = 3672 - 1 * 3600 - 1 * 60 = 12
			// displayString = 01:01:12

			int displayHour = second / 3600;
			int displayMinute = (second / 60) - (displayHour * 60);
			int displaySecond = second - (displayHour * 3600) - (displayMinute * 60);
			string displayString = string.Format("{0:D2}:{1:D2}:{2:D2}", displayHour, displayMinute, displaySecond);

			return string.Format(text, displayString);
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseInt32("Second", out int _value))
			{
				second = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Second");
			}
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseInt32(p.legacyPrefix + "_Second", out int _value))
			{
				second = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Second");
			}
		}

		public NPCTimeOfDayCondition() { }

		[System.Obsolete]
		public NPCTimeOfDayCondition(int newSecond, ENPCLogicType newLogicType, string newText, bool newShouldReset) : base(newLogicType, newText, newShouldReset)
		{
			second = newSecond;
		}
	}
}
