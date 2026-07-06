////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Unturned
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class NetInvokableGeneratedClassAttribute : Attribute
	{
		/// <summary>
		/// Type the annotated class was generated for.
		/// </summary>
		public readonly Type targetType;

		public NetInvokableGeneratedClassAttribute(Type targetType)
		{
			this.targetType = targetType;
		}
	}

	public enum ENetInvokableGeneratedMethodPurpose
	{
		Read,
		Write,
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class NetInvokableGeneratedMethodAttribute : Attribute
	{
		/// <summary>
		/// Method the annotated method was generated for.
		/// </summary>
		public readonly string targetMethodName;

		public readonly ENetInvokableGeneratedMethodPurpose purpose;

		public NetInvokableGeneratedMethodAttribute(string targetMethodName, ENetInvokableGeneratedMethodPurpose purpose)
		{
			this.targetMethodName = targetMethodName;
			this.purpose = purpose;
		}
	}
}
