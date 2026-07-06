////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCFlagMathReward : INPCReward
	{
		public ushort flag_A_ID
		{
			get;
			protected set;
		}

		public ushort flag_B_ID
		{
			get;
			protected set;
		}

		private short defaultFlag_B_Value;

		public ENPCOperationType operationType
		{
			get;
			protected set;
		}

		/// <summary>
		/// Determines how NPCFlagMathReward handles formatReward.
		/// </summary>
		public ETextFormatMode TextFormatMode
		{
			get;
			set;
		}

		public enum ETextFormatMode
		{
			/// <summary>
			/// Use text as-is without formatting. For backwards compatibility.
			/// </summary>
			None,

			/// <summary>
			/// Format flag A value into {0} and flag B value (or default) into {1}.
			/// </summary>
			FlagValues,
		}

		public override void GrantReward(Player player)
		{
			short flag_A_Value;
			player.quests.getFlag(flag_A_ID, out flag_A_Value);

			short flag_B_Value;
			if (flag_B_ID == 0 || !player.quests.getFlag(flag_B_ID, out flag_B_Value))
			{
				flag_B_Value = defaultFlag_B_Value;
			}

			switch (operationType)
			{
				case ENPCOperationType.ASSIGN:
					flag_A_Value = flag_B_Value;
					break;

				case ENPCOperationType.ADDITION:
					flag_A_Value += flag_B_Value;
					break;

				case ENPCOperationType.SUBTRACTION:
					flag_A_Value -= flag_B_Value;
					break;

				case ENPCOperationType.MULTIPLICATION:
					flag_A_Value *= flag_B_Value;
					break;

				case ENPCOperationType.DIVISION:
					flag_A_Value /= flag_B_Value;
					break;

				case ENPCOperationType.MODULO:
					flag_A_Value %= flag_B_Value;
					break;

				case ENPCOperationType.RANDOM_INCLUSIVE:
				{
					if (flag_A_Value != flag_B_Value)
					{
						int min = flag_A_Value;
						int max = flag_B_Value;
						// Manually swap min/max if necessary so we ensure +1 is added to the higher value.
						if (min > max)
						{
							int temp = max;
							max = min;
							min = temp;
						}
						// +1 because Unity's random max is exclusive.
						int randomValue = UnityEngine.Random.Range(min, max + 1);
						flag_A_Value = MathfEx.ClampToShort(randomValue);
					}
					break;
				}

				case ENPCOperationType.RANDOM_EXCLUSIVE:
				{
					// If A and B are equal we ignore the "exclusion rule", same as Unity's built-in random.
					if (flag_A_Value != flag_B_Value)
					{
						int randomValue = UnityEngine.Random.Range(flag_A_Value, flag_B_Value);
						flag_A_Value = MathfEx.ClampToShort(randomValue);
					}
					break;
				}

				default:
					return;
			}

			player.quests.sendSetFlag(flag_A_ID, flag_A_Value);
		}

		public override string formatReward(Player player)
		{
			if (TextFormatMode == ETextFormatMode.None)
			{
				return base.formatReward(player);
			}

			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			short flag_A_Value;
			player.quests.getFlag(flag_A_ID, out flag_A_Value);

			short flag_B_Value;
			if (flag_B_ID == 0 || !player.quests.getFlag(flag_B_ID, out flag_B_Value))
			{
				flag_B_Value = defaultFlag_B_Value;
			}

			return Local.FormatText(text, flag_A_Value, flag_B_Value);
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseUInt16("A_ID", out ushort _aId))
			{
				flag_A_ID = _aId;
			}
			else
			{
				p.ReportRequiredOptionInvalid("A_ID");
			}

			bool parsedBId = p.data.TryParseUInt16("B_ID", out ushort _bId);
			flag_B_ID = _bId;

			bool parsedBValue = p.data.TryParseInt16("B_Value", out short _bValue);
			defaultFlag_B_Value = _bValue;

			if (!parsedBId && !parsedBValue)
			{
				p.ReportError("requires B_ID or B_Value");
			}

			if (p.data.TryParseEnum("Operation", out ENPCOperationType _operation))
			{
				operationType = _operation;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Operation");
			}

			TextFormatMode = p.data.ParseEnum("TextFormat", ETextFormatMode.None);
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseUInt16(p.legacyPrefix + "_A_ID", out ushort _aId))
			{
				flag_A_ID = _aId;
			}
			else
			{
				p.ReportRequiredOptionInvalid("A_ID");
			}

			bool parsedBId = p.data.TryParseUInt16(p.legacyPrefix + "_B_ID", out ushort _bId);
			flag_B_ID = _bId;

			bool parsedBValue = p.data.TryParseInt16(p.legacyPrefix + "_B_Value", out short _bValue);
			defaultFlag_B_Value = _bValue;

			if (!parsedBId && !parsedBValue)
			{
				p.ReportError("requires B_ID or B_Value");
			}

			if (p.data.TryParseEnum(p.legacyPrefix + "_Operation", out ENPCOperationType _operation))
			{
				operationType = _operation;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Operation");
			}

			TextFormatMode = p.data.ParseEnum(p.legacyPrefix + "_TextFormat", ETextFormatMode.None);
		}

		public NPCFlagMathReward() { }

		[System.Obsolete]
		public NPCFlagMathReward(ushort newFlag_A_ID, ushort newFlag_B_ID, short newFlag_B_Value, ENPCOperationType newOperationType, string newText) : base(newText)
		{
			flag_A_ID = newFlag_A_ID;
			flag_B_ID = newFlag_B_ID;
			defaultFlag_B_Value = newFlag_B_Value;
			operationType = newOperationType;
		}
	}
}
