////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.NetPak
{
	/// <summary>
	/// Indicates net reader/writer implementation should be generated.
	/// </summary>
	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	[System.AttributeUsage(System.AttributeTargets.Enum)]
	public class NetEnumAttribute : System.Attribute
	{
	}
}
