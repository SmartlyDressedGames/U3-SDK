////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public enum EBlueprintSkill
	{
		NONE,
		CRAFT,
		COOK,
		REPAIR,
	}

	public static class EBlueprintSkillEx
	{
		public static string ToStringPascalCase(this EBlueprintSkill skill)
		{
			switch(skill)
			{
				case EBlueprintSkill.NONE:
					return "None";

				case EBlueprintSkill.CRAFT:
					return "Craft";

				case EBlueprintSkill.COOK:
					return "Cook";

				case EBlueprintSkill.REPAIR:
					return "Repair";

				default:
					return skill.ToString();
			}
		}

		public static void ToSkillIndices(this EBlueprintSkill skill, out int specialityIndex, out int skillIndex)
		{
			switch (skill)
			{
				case EBlueprintSkill.CRAFT:
				{
					specialityIndex = (int) EPlayerSpeciality.SUPPORT;
					skillIndex = (int) EPlayerSupport.CRAFTING;
					return;
				}

				case EBlueprintSkill.COOK:
				{
					specialityIndex = (int) EPlayerSpeciality.SUPPORT;
					skillIndex = (int) EPlayerSupport.COOKING;
					return;
				}

				case EBlueprintSkill.REPAIR:
				{
					specialityIndex = (int) EPlayerSpeciality.SUPPORT;
					skillIndex = (int) EPlayerSupport.ENGINEER;
					return;
				}

				default:
				case EBlueprintSkill.NONE:
				{
					specialityIndex = -1;
					skillIndex = -1;
					return;
				}
			}
		}
	}
}
