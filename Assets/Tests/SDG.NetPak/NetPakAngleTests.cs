////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.NetPak;
using UnityEngine;

internal class NetPakAngleTests
{
	[Test]
	public void ReadWriteNegativeRadians()
	{
		const float TAU = Mathf.PI * 2.0f;
		const float HALF_PI = Mathf.PI * 0.5f;

		float[] actualValues = new float[]
		{
			-0.1f,
			-HALF_PI,
			-Mathf.PI,
			-TAU - Mathf.PI,
			-TAU,
			-TAU * 2.0f,
		};

		float[] expectedValues = new float[]
		{
			TAU - 0.1f,
			TAU - HALF_PI,
			Mathf.PI,
			Mathf.PI,
			0.0f,
			0.0f,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (float value in actualValues)
		{
			Assert.IsTrue(writer.WriteRadians(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (float expectedValue in expectedValues)
		{
			float actualValue;
			Assert.IsTrue(reader.ReadRadians(out actualValue));
			Assert.That(actualValue, Is.EqualTo(expectedValue).Within(0.1f));
		}
	}

	[Test]
	public void ReadWriteNegativeDegrees()
	{
		float[] actualValues = new float[]
		{
			-15,
			-90,
			-180,
			-540,
			-360,
			-720,
		};

		float[] expectedValues = new float[]
		{
			345,
			270,
			180,
			180, // -540
			0, // -360
			0, // -720
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (float value in actualValues)
		{
			Assert.IsTrue(writer.WriteDegrees(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (float expectedValue in expectedValues)
		{
			float actualValue;
			Assert.IsTrue(reader.ReadDegrees(out actualValue));
			Assert.That(actualValue, Is.EqualTo(expectedValue).Within(5.0f));
		}
	}
}
