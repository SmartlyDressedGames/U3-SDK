////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public enum ENPCLogicType
	{
		[InspectorName("Invalid")]
		NONE,

		[InspectorName("< Less Than")]
		LESS_THAN,

		[InspectorName("≤ Less Than or Equal")]
		LESS_THAN_OR_EQUAL_TO,

		[InspectorName("= Equal")]
		EQUAL,

		[InspectorName("≠ Not Equal")]
		NOT_EQUAL,

		[InspectorName("≥ Greater Than or Equal")]
		GREATER_THAN_OR_EQUAL_TO,

		[InspectorName("> Greater Than")]
		GREATER_THAN
	}

	public static class NPCLogicTypeEx
	{
		public static char ToCharAbbr(this ENPCLogicType type)
		{
			switch (type)
			{
				default:
				case ENPCLogicType.NONE:
					return '-'; // N/A

				case ENPCLogicType.LESS_THAN:
					return '<';

				case ENPCLogicType.LESS_THAN_OR_EQUAL_TO:
					return '≤';

				case ENPCLogicType.EQUAL:
					return '=';

				case ENPCLogicType.NOT_EQUAL:
					return '≠';

				case ENPCLogicType.GREATER_THAN_OR_EQUAL_TO:
					return '≥';

				case ENPCLogicType.GREATER_THAN:
					return '>';
			}
		}
	}
}
