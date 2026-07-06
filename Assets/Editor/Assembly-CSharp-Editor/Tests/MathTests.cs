////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;
using UnityEngine;

internal class MathTests
{
	[Test]
	public void AngleDegreesNearlyEqual()
	{
		Assert.IsTrue(MathfEx.IsAngleDegreesNearlyEqual(-0.0f, 0.0f), "zero nearly equal");
		Assert.IsTrue(MathfEx.IsAngleDegreesNearlyEqual(0.0f, 360.0f), "zero and 360 nearly equal");
		Assert.IsTrue(MathfEx.IsAngleDegreesNearlyEqual(-360.0f, 360.0f), "negative 360 and 360 nearly equal");
		Assert.IsTrue(MathfEx.IsAngleDegreesNearlyEqual(-360.0f, 720.0f), "negative 360 and 720 nearly equal");
		Assert.IsTrue(MathfEx.IsAngleDegreesNearlyEqual(-180, 180.0f), "negative 180 and 180 nearly equal");
		Assert.IsFalse(MathfEx.IsAngleDegreesNearlyEqual(-90.0f, 90.0f), "negative 90 and 90 not nearly equal");
		Assert.IsFalse(MathfEx.IsAngleDegreesNearlyEqual(35.0f, 40.0f), "35 and 40 not nearly equal");
	}

	[Test]
	public void RoundScale()
	{
		Assert.AreEqual(Vector3.one, Vector3.one.GetRoundedIfNearlyEqualToOne(), "one equals one");
		Assert.AreEqual(Vector3.one, new Vector3(1.0001f, 1.0f, 1.0f).GetRoundedIfNearlyEqualToOne(), "x nearly equals one");
		Assert.AreEqual(new Vector3(1.0f, -2.0f, 3.0f), new Vector3(1.0001f, -2.0f, 3.0f).GetRoundedIfNearlyEqualToOne(), "round positive x only");
		Assert.AreEqual(new Vector3(-1.0f, -2.0f, 3.0f), new Vector3(-1.0001f, -2.0f, 3.0f).GetRoundedIfNearlyEqualToOne(), "round negative x only");
		Assert.AreEqual(new Vector3(5.0f, 1.0f, 3.0f), new Vector3(5.0f, 1.0001f, 3.0f).GetRoundedIfNearlyEqualToOne(), "round positive y only");
		Assert.AreEqual(new Vector3(5.0f, -1.0f, 3.0f), new Vector3(5.0f, -1.0001f, 3.0f).GetRoundedIfNearlyEqualToOne(), "round negative y only");
		Assert.AreEqual(new Vector3(5.0f, 16.3f, 1.0f), new Vector3(5.0f, 16.3f, 1.0001f).GetRoundedIfNearlyEqualToOne(), "round positive z only");
		Assert.AreEqual(new Vector3(5.0f, 16.3f, -1.0f), new Vector3(5.0f, 16.3f, -1.0001f).GetRoundedIfNearlyEqualToOne(), "round negative z only");
	}

	[Test]
	public void RoundAxisAlignedQuaternion()
	{
		Assert.AreEqual(Quaternion.identity, Quaternion.identity.GetRoundedIfNearlyAxisAligned(), "identity equals identity");
		Assert.AreEqual(Quaternion.Euler(90.0f, 0.0f, 0.0f), Quaternion.Euler(89.99f, 0.0f, 0.0f).GetRoundedIfNearlyAxisAligned(), "round nearly 90 around x");
		Assert.AreEqual(Quaternion.Euler(0.0f, 90.0f, 0.0f), Quaternion.Euler(0.0f, 89.99f, 0.0f).GetRoundedIfNearlyAxisAligned(), "round nearly 90 around y");
		Assert.AreEqual(Quaternion.Euler(0.0f, 0.0f, 90.0f), Quaternion.Euler(0.0f, 0.0f, 89.99f).GetRoundedIfNearlyAxisAligned(), "round nearly 90 around z");
		Assert.AreNotEqual(Quaternion.Euler(0.0f, 90.0f, 0.0f), Quaternion.Euler(5.0f, 89.99f, 5.0f).GetRoundedIfNearlyAxisAligned(), "do not round y if other axes are not aligned");
		Assert.AreNotEqual(Quaternion.Euler(0.0f, 0.0f, 90.0f), Quaternion.Euler(5.0f, 5.0f, 89.99f).GetRoundedIfNearlyAxisAligned(), "do not round z if other axes are not aligned");
	}
}
