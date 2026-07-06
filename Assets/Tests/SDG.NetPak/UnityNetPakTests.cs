////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.NetPak;
using UnityEngine;

internal class UnityNetPakTests
{
	[Test]
	public void ReadWriteQuaternion()
	{
		Quaternion quat = Quaternion.Euler(15.0f, 45.0f, 20.0f);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteQuaternion(quat));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		Quaternion value;
		Assert.IsTrue(reader.ReadQuaternion(out value));
		Assert.IsTrue(IsNearlyEqual(quat, value, 0.02f));
	}

	[Test]
	public void ReadWriteQuaternionRandomEulerAngles()
	{
		// Nelson 2024-06-26: Maybe this is dumb? I'm wondering if there's a problem with quaternion replication though.
		for (int number = 1; number < 1024; ++number)
		{
			Vector3 expectedEulerAngles = new Vector3(Random.Range(-360.0f, 360.0f), Random.Range(-360.0f, 360.0f), Random.Range(-360.0f, 360.0f));
			Quaternion expectedQuaternion = Quaternion.Euler(expectedEulerAngles);

			NetPakWriter writer = new NetPakWriter();
			writer.buffer = new byte[1024];
			Assert.IsTrue(writer.WriteQuaternion(expectedQuaternion, bitsPerComponent: 14));
			Assert.IsTrue(writer.Flush());

			NetPakReader reader = new NetPakReader();
			reader.SetBuffer(writer.buffer);
			Quaternion actualQuaternion;
			Assert.IsTrue(reader.ReadQuaternion(out actualQuaternion, bitsPerComponent: 14));

			expectedEulerAngles = expectedQuaternion.eulerAngles;
			Vector3 actualEulerAngles = actualQuaternion.eulerAngles;
			Assert.IsTrue(IsEulerAngleNearlyEqual(expectedEulerAngles, actualEulerAngles, 5.0f), "Expected: {0} Actual: {1}", expectedEulerAngles, actualEulerAngles);
		}
	}

	[Test]
	public void ReadWriteSpecialYawOrQuaternionRandomEulerAngles()
	{
		for (int number = 1; number < 1024; ++number)
		{
			Vector3 expectedEulerAngles;
			if (Random.value < 0.5f)
			{
				expectedEulerAngles = new Vector3(Random.Range(-360.0f, 360.0f), Random.Range(-360.0f, 360.0f), Random.Range(-360.0f, 360.0f));
			}
			else
			{
				expectedEulerAngles = new Vector3(-90.0f, Random.Range(-360.0f, 360.0f), 0.0f);
			}
			Quaternion expectedQuaternion = Quaternion.Euler(expectedEulerAngles);

			NetPakWriter writer = new NetPakWriter();
			writer.buffer = new byte[1024];
			Assert.IsTrue(writer.WriteSpecialYawOrQuaternion(expectedQuaternion, quaternionBitsPerComponent: 14));
			Assert.IsTrue(writer.Flush());

			NetPakReader reader = new NetPakReader();
			reader.SetBuffer(writer.buffer);
			Quaternion actualQuaternion;
			Assert.IsTrue(reader.ReadSpecialYawOrQuaternion(out actualQuaternion, quaternionBitsPerComponent: 14));

			Assert.IsTrue(IsNearlyEqual(expectedQuaternion, actualQuaternion, 0.05f), "Expected: {0} Actual: {1}", expectedEulerAngles, actualQuaternion.eulerAngles);
		}
	}

	[Test]
	public void ReadWriteQuaternion_LargestX()
	{
		Quaternion quat = new Quaternion(1.0f, 0.0f, 0.0f, 0.0f);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteQuaternion(quat));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		Quaternion value;
		Assert.IsTrue(reader.ReadQuaternion(out value));
		Assert.IsTrue(IsNearlyEqual(quat, value, 0.01f));
	}

	[Test]
	public void ReadWriteQuaternion_LargestY()
	{
		Quaternion quat = new Quaternion(0.0f, 1.0f, 0.0f, 0.0f);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteQuaternion(quat));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		Quaternion value;
		Assert.IsTrue(reader.ReadQuaternion(out value));
		Assert.IsTrue(IsNearlyEqual(quat, value, 0.01f));
	}

	[Test]
	public void ReadWriteQuaternion_LargestZ()
	{
		Quaternion quat = new Quaternion(0.0f, 0.0f, 1.0f, 0.0f);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteQuaternion(quat));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		Quaternion value;
		Assert.IsTrue(reader.ReadQuaternion(out value));
		Assert.IsTrue(IsNearlyEqual(quat, value, 0.01f));
	}

	[Test]
	public void ReadWriteQuaternion_LargestW()
	{
		Quaternion quat = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteQuaternion(quat));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		Quaternion value;
		Assert.IsTrue(reader.ReadQuaternion(out value));
		Assert.IsTrue(IsNearlyEqual(quat, value, 0.01f));
	}

	[Test]
	public void ReadWriteNormalVector()
	{
		Vector3[] expectedValues = new Vector3[]
		{
			new Vector3(-0.2f, -0.5f, 0.3f).normalized,
			new Vector3(0.15f, 0.6f, 0.2f).normalized,
			new Vector3(0.8f, 0.1f, -0.6f).normalized,
			new Vector3(0.5f, -0.7f, 0.4f).normalized,
			new Vector3(-1.0f, -1.0f, -1.0f).normalized,
			new Vector3(+1.0f, -1.0f, -1.0f).normalized,
			new Vector3(-1.0f, -1.0f, -1.0f).normalized,
			new Vector3(+1.0f, +1.0f, -1.0f).normalized,
			new Vector3(-1.0f, -1.0f, +1.0f).normalized,
			new Vector3(+1.0f, -1.0f, +1.0f).normalized,
			new Vector3(-1.0f, -1.0f, +1.0f).normalized,
			new Vector3(+1.0f, +1.0f, +1.0f).normalized,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (Vector3 value in expectedValues)
		{
			Assert.IsTrue(writer.WriteNormalVector3(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (Vector3 expectedValue in expectedValues)
		{
			Vector3 actualValue;
			Assert.IsTrue(reader.ReadNormalVector3(out actualValue));
			Assert.IsTrue(IsNearlyEqual(expectedValue, actualValue, 0.01f), "Expected: {0} Actual: {1}", expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteNormalVector_NegativeX()
	{
		Vector3 expected = new Vector3(-1.0f, 0.0f, 0.0f);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteNormalVector3(expected));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		Vector3 actual;
		Assert.IsTrue(reader.ReadNormalVector3(out actual));
		Assert.IsTrue(IsNearlyEqual(expected, actual, 0.01f));
	}

	[Test]
	public void ReadWriteNormalVector_PositiveX()
	{
		Vector3 expected = new Vector3(+1.0f, 0.0f, 0.0f);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteNormalVector3(expected));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		Vector3 actual;
		Assert.IsTrue(reader.ReadNormalVector3(out actual));
		Assert.IsTrue(IsNearlyEqual(expected, actual, 0.01f));
	}

	[Test]
	public void ReadWriteNormalVector_NegativeY()
	{
		Vector3 expected = new Vector3(0.0f, -1.0f, 0.0f);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteNormalVector3(expected));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		Vector3 actual;
		Assert.IsTrue(reader.ReadNormalVector3(out actual));
		Assert.IsTrue(IsNearlyEqual(expected, actual, 0.01f));
	}

	[Test]
	public void ReadWriteNormalVector_PositiveY()
	{
		Vector3 expected = new Vector3(0.0f, +1.0f, 0.0f);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteNormalVector3(expected));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		Vector3 actual;
		Assert.IsTrue(reader.ReadNormalVector3(out actual));
		Assert.IsTrue(IsNearlyEqual(expected, actual, 0.01f));
	}

	[Test]
	public void ReadWriteNormalVector_NegativeZ()
	{
		Vector3 expected = new Vector3(0.0f, 0.0f, -1.0f);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteNormalVector3(expected));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		Vector3 actual;
		Assert.IsTrue(reader.ReadNormalVector3(out actual));
		Assert.IsTrue(IsNearlyEqual(expected, actual, 0.01f));
	}

	[Test]
	public void ReadWriteNormalVector_PositiveZ()
	{
		Vector3 expected = new Vector3(0.0f, 0.0f, +1.0f);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteNormalVector3(expected));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		Vector3 actual;
		Assert.IsTrue(reader.ReadNormalVector3(out actual));
		Assert.IsTrue(IsNearlyEqual(expected, actual, 0.01f));
	}

	[Test]
	public void ReadWriteClampedVectorsWithinRange()
	{
		Vector3[] expectedValues = new Vector3[]
		{
 			Vector3.zero,
 			Vector3.one,
 			Vector3.left,
 			Vector3.right,
 			Vector3.up,
 			Vector3.down,
 			Vector3.forward,
 			Vector3.back,
			new Vector3(-float.Epsilon, float.Epsilon, -1.0000000000001f),
			new Vector3(1.0000000000001f, -0.9999999999999f, 0.9999999999999f),
			new Vector3(0.0f, -2.060574E-13f, -1.0f), // Added from bug in the wild. (public issue #3686)
  			new Vector3(-16.0f, 23.0f, 107.0f),
 			new Vector3(-3000.7f, 278.1f, -809.6f),
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (Vector3 value in expectedValues)
		{
			Assert.IsTrue(writer.WriteClampedVector3(value), "wrote clamped vector ({0})", value);
		}
		Assert.IsTrue(writer.Flush(), "flushed writer");

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (Vector3 expectedValue in expectedValues)
		{
			Vector3 actualValue;
			Assert.IsTrue(reader.ReadClampedVector3(out actualValue), "read clamped vector");
			Assert.IsTrue(IsNearlyEqual(expectedValue, actualValue, 0.01f), "Expected: {0} Actual: {1}", expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteColor32RGB()
	{
		Color32 expected = Color.green;

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteColor32RGB(expected));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		Color32 actual;
		Assert.IsTrue(reader.ReadColor32RGB(out actual));
		Assert.AreEqual(expected, actual);
	}

	[Test]
	public void ReadWriteColor32RGBA()
	{
		Color32 expected = Color.green;
		expected.a = 127;

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteColor32RGBA(expected));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		Color32 actual;
		Assert.IsTrue(reader.ReadColor32RGBA(out actual));
		Assert.AreEqual(expected, actual);
	}

	[Test]
	public void ReadWriteNormalVector3AsYaw()
	{
		Vector3[] expectedVectors = new Vector3[]
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(0f, 0f, 1f),
			new Vector3(-1f, 0f, 0f),
			new Vector3(0f, 0f, -1f),
			new Vector3(1f, 0f, 1f).normalized,
			new Vector3(-1f, 0f, 1f).normalized,
			new Vector3(1f, 0f, -1f).normalized,
			new Vector3(-1f, 0f, -1f).normalized,
		};

		foreach (Vector3 expectedVector in expectedVectors)
		{
			NetPakWriter writer = new NetPakWriter();
			writer.buffer = new byte[1024];
			Assert.IsTrue(writer.WriteNormalVector3AsYaw(expectedVector));
			Assert.IsTrue(writer.Flush());

			NetPakReader reader = new NetPakReader();
			reader.SetBuffer(writer.buffer);
			Vector3 actualVector;
			Assert.IsTrue(reader.ReadNormalVector3AsYaw(out actualVector));

			Assert.IsTrue(IsNearlyEqual(expectedVector, actualVector, 0.01f), "Expected: {0} Actual: {1}", expectedVector, actualVector);
		}
	}

	[Test]
	public void ReadWriteVector3AsYawMagnitude()
	{
		Vector3[] expectedVectors = new Vector3[]
		{
			new Vector3(10f, 0f, 0f),
			new Vector3(0f, 0f, 10f),
			new Vector3(-10f, 0f, 0f),
			new Vector3(0f, 0f, -10f),
			new Vector3(10f, 0f, 10f),
			new Vector3(-10f, 0f, 10f),
			new Vector3(10f, 0f, -10f),
			new Vector3(-10f, 0f, -10f),
		};

		foreach (Vector3 expectedVector in expectedVectors)
		{
			NetPakWriter writer = new NetPakWriter();
			writer.buffer = new byte[1024];
			Assert.IsTrue(writer.WriteVector3AsYawMagnitude(expectedVector));
			Assert.IsTrue(writer.Flush());

			NetPakReader reader = new NetPakReader();
			reader.SetBuffer(writer.buffer);
			Vector3 actualVector;
			Assert.IsTrue(reader.ReadVector3AsYawMagnitude(out actualVector));

			Assert.IsTrue(IsNearlyEqual(expectedVector, actualVector, 0.01f), "Expected: {0} Actual: {1}", expectedVector, actualVector);
		}
	}

	private bool IsNearlyEqual(float a, float b, float tolerance = 0.01f)
	{
		return Mathf.Abs(b - a) < tolerance;
	}

	private bool IsNearlyEqual(Vector3 a, Vector3 b, float tolerance = 0.001f)
	{
		return IsNearlyEqual(a.x, b.x, tolerance: tolerance)
			&& IsNearlyEqual(a.y, b.y, tolerance: tolerance)
			&& IsNearlyEqual(a.z, b.z, tolerance: tolerance);
	}

	private bool IsEulerAngleNearlyEqual(Vector3 a, Vector3 b, float tolerance = 0.001f)
	{
		return Mathf.DeltaAngle(a.x, b.x) < tolerance
			&& Mathf.DeltaAngle(a.y, b.y) < tolerance
			&& Mathf.DeltaAngle(a.z, b.z) < tolerance;
	}

	private bool IsNearlyEqual(Quaternion a, Quaternion b, float tolerance = 0.001f)
	{
		return 1.0f - Mathf.Abs(Quaternion.Dot(a, b)) < tolerance;
	}
}
