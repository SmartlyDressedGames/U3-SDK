////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Diagnostics;

namespace SDG.Unturned
{
	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NetPakColor32Attribute : Attribute
	{
		public bool withAlpha;

		public NetPakColor32Attribute(bool withAlpha)
		{
			this.withAlpha = withAlpha;
		}

		private NetPakColor32Attribute()
		{ }
	}

	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NetPakVector3Attribute : Attribute
	{
		public readonly int intBitCount;
		public readonly int fracBitCount;

		public NetPakVector3Attribute(int intBitCount = 13, int fracBitCount = 9)
		{
			this.intBitCount = intBitCount;
			this.fracBitCount = fracBitCount;
		}
	}

	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NetPakNormalAttribute : Attribute
	{
		public readonly int bitsPerComponent;

		public NetPakNormalAttribute(int bitsPerComponent = 9)
		{
			this.bitsPerComponent = bitsPerComponent;
		}
	}

	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NetPakNormalAsYawAttribute : Attribute
	{
		public readonly int bitCount;

		public NetPakNormalAsYawAttribute(int bitCount = 16)
		{
			this.bitCount = bitCount;
		}
	}

	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NetPakVectorAsYawAttribute : Attribute
	{
		public readonly int yawBitCount;

		public NetPakVectorAsYawAttribute(int yawBitCount = 16)
		{
			this.yawBitCount = yawBitCount;
		}
	}

	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NetPakSpecialQuaternionAttribute : Attribute
	{
		public readonly int yawBitCount;

		public NetPakSpecialQuaternionAttribute(int yawBitCount = 9)
		{
			this.yawBitCount = yawBitCount;
		}
	}
}
