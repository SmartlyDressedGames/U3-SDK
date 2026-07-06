////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.NetPak;
using System.Collections.Generic;

internal class SystemNetPakTests
{
	[Test]
	public void ReadWriteSignedInt()
	{
		// Signed numbers representable with 7 bits.
		int[] expectedValues = new int[]
		{
			-64,
			0,
			63,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (int value in expectedValues)
		{
			Assert.IsTrue(writer.WriteSignedInt(value, 7));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (int expectedValue in expectedValues)
		{
			int actualValue;
			Assert.IsTrue(reader.ReadSignedInt(7, out actualValue));
			Assert.AreEqual(expectedValue, actualValue);
		}
	}
	[Test]
	public void ReadWriteClampedUnsignedFloatWithinRange()
	{
		float[] expectedValues = new float[]
		{
			0.0f,
			float.Epsilon,
			2.060574E-13f,
			0.00001f,
			0.2f,
			0.7f,
			0.9999999999999f,
			1.0000000000001f,
			41.9999999999999f,
			42.0000000000001f,
			63.0f,
			63.7f,
			64.0f,
			127.0f,
			127.5f,
			128.0f, // Should be read as 127.9xx (exclusive upper bound).
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (float value in expectedValues)
		{
			Assert.IsTrue(writer.WriteUnsignedClampedFloat(value, 7, 5));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (float expectedValue in expectedValues)
		{
			float actualValue;
			Assert.IsTrue(reader.ReadUnsignedClampedFloat(7, 5, out actualValue));
			Assert.That(actualValue, Is.EqualTo(expectedValue).Within(0.05f));
		}
	}

	/// <summary>
	/// Highest value should be off by one with a single fractional bit.
	/// </summary>
	[Test]
	public void ReadWriteClampedUnsignedFloatUpperBoundExlusive()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteUnsignedClampedFloat(127.9999f, 7, 1));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		float actualValue;
		Assert.IsTrue(reader.ReadUnsignedClampedFloat(7, 1, out actualValue));
		AssertAreNearlyEqual(127.5f, actualValue, tolerance: 0.0001f);
	}

	/// <summary>
	/// Values outside range should be clamped.
	/// </summary>
	[Test]
	public void ReadWriteClampedUnsignedFloatOutsideRange()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteUnsignedClampedFloat(-100.0f, 7, 1));
		Assert.IsTrue(writer.WriteUnsignedClampedFloat(+300.0f, 7, 1));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		float actualValue;
		Assert.IsTrue(reader.ReadUnsignedClampedFloat(7, 1, out actualValue));
		AssertAreNearlyEqual(0.0f, actualValue, tolerance: 0.0001f);
		Assert.IsTrue(reader.ReadUnsignedClampedFloat(7, 1, out actualValue));
		AssertAreNearlyEqual(127.5f, actualValue, tolerance: 0.0001f);
	}

	[Test]
	public void ReadWriteClampedFloatWithinRange()
	{
		float[] expectedValues = new float[]
		{
			-64.0f,
			-63.2f,
			-63.0f,
			-42.0000000000001f,
			-41.9999999999999f,
			-1.0000000000001f,
			-0.9999999999999f,
			-0.7f,
			-0.2f,
			-0.00001f,
			-2.060574E-13f,
			-float.Epsilon,
			0.0f,
			float.Epsilon,
			2.060574E-13f,
			0.00001f,
			0.2f,
			0.7f,
			0.9999999999999f,
			1.0000000000001f,
			41.9999999999999f,
			42.0000000000001f,
			63.0f,
			63.7f,
			64.0f, // Should be read as 63.9xx (exclusive upper bound).
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (float value in expectedValues)
		{
			Assert.IsTrue(writer.WriteClampedFloat(value, 7, 5));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (float expectedValue in expectedValues)
		{
			float actualValue;
			Assert.IsTrue(reader.ReadClampedFloat(7, 5, out actualValue));
			Assert.That(actualValue, Is.EqualTo(expectedValue).Within(0.05f));
		}
	}

	/// <summary>
	/// Lowest value should be encoded exactly with a single fractional bit.
	/// </summary>
	[Test]
	public void ReadWriteClampedFloatLowerBoundInclusive()
	{
		float expectedValue = -64.0f;

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteClampedFloat(expectedValue, 7, 1));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		float actualValue;
		Assert.IsTrue(reader.ReadClampedFloat(7, 1, out actualValue));
		AssertAreNearlyEqual(expectedValue, actualValue, tolerance: 0.0001f);
	}

	/// <summary>
	/// Highest value should be off by one with a single fractional bit.
	/// </summary>
	[Test]
	public void ReadWriteClampedFloatUpperBoundExlusive()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteClampedFloat(63.9999f, 7, 1));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		float actualValue;
		Assert.IsTrue(reader.ReadClampedFloat(7, 1, out actualValue));
		AssertAreNearlyEqual(63.5f, actualValue, tolerance: 0.0001f); // 63 + 0.5
	}

	/// <summary>
	/// Values outside range should be clamped.
	/// </summary>
	[Test]
	public void ReadWriteClampedFloatOutsideRange()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteClampedFloat(-100.0f, 7, 1));
		Assert.IsTrue(writer.WriteClampedFloat(+100.0f, 7, 1));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		float actualValue;
		Assert.IsTrue(reader.ReadClampedFloat(7, 1, out actualValue));
		AssertAreNearlyEqual(-64.0f, actualValue, tolerance: 0.0001f);
		Assert.IsTrue(reader.ReadClampedFloat(7, 1, out actualValue));
		AssertAreNearlyEqual(63.5f, actualValue, tolerance: 0.0001f); // 63 + 0.5
	}

	[Test]
	public void ReadWriteFloat()
	{
		float[] expectedValues = new float[]
		{
			float.NegativeInfinity,
			float.MinValue,
			-1.0000000000001f,
			-1.0f,
			-0.9999999999999f,
			-2.060574E-13f,
			-float.Epsilon,
			0.0f,
			float.Epsilon,
			2.060574E-13f,
			0.9999999999999f,
			1.0f,
			1.0000000000001f,
			float.MaxValue,
			float.PositiveInfinity,
			float.NaN,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (float value in expectedValues)
		{
			Assert.IsTrue(writer.WriteFloat(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (float expectedValue in expectedValues)
		{
			float actualValue;
			Assert.IsTrue(reader.ReadFloat(out actualValue));
			Assert.AreEqual(expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteInt8()
	{
		sbyte[] expectedValues = new sbyte[]
		{
			sbyte.MinValue,
			sbyte.MinValue + 1,
			-64,
			-63,
			-1,
			0,
			1,
			63,
			64,
			sbyte.MaxValue - 1,
			sbyte.MaxValue,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (sbyte value in expectedValues)
		{
			Assert.IsTrue(writer.WriteInt8(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (sbyte expectedValue in expectedValues)
		{
			sbyte actualValue;
			Assert.IsTrue(reader.ReadInt8(out actualValue));
			Assert.AreEqual(expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteInt16()
	{
		short[] expectedValues = new short[]
		{
			short.MinValue,
			short.MinValue + 1,
			sbyte.MinValue - 1,
			sbyte.MinValue,
			sbyte.MinValue + 1,
			-1,
			0,
			1,
			sbyte.MaxValue - 1,
			sbyte.MaxValue,
			sbyte.MaxValue + 1,
			byte.MaxValue - 1,
			byte.MaxValue,
			byte.MaxValue + 1,
			short.MaxValue - 1,
			short.MaxValue,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (short value in expectedValues)
		{
			Assert.IsTrue(writer.WriteInt16(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (short expectedValue in expectedValues)
		{
			short actualValue;
			Assert.IsTrue(reader.ReadInt16(out actualValue));
			Assert.AreEqual(expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteInt32()
	{
		int[] expectedValues = new int[]
		{
			int.MinValue,
			int.MinValue + 1,
			short.MinValue - 1,
			short.MinValue,
			short.MinValue + 1,
			sbyte.MinValue - 1,
			sbyte.MinValue,
			sbyte.MinValue + 1,
			-1,
			0,
			1,
			sbyte.MaxValue - 1,
			sbyte.MaxValue,
			sbyte.MaxValue + 1,
			byte.MaxValue - 1,
			byte.MaxValue,
			byte.MaxValue + 1,
			short.MaxValue - 1,
			short.MaxValue,
			short.MaxValue + 1,
			ushort.MaxValue - 1,
			ushort.MaxValue,
			ushort.MaxValue + 1,
			int.MaxValue - 1,
			int.MaxValue,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (int value in expectedValues)
		{
			Assert.IsTrue(writer.WriteInt32(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (int expectedValue in expectedValues)
		{
			int actualValue;
			Assert.IsTrue(reader.ReadInt32(out actualValue));
			Assert.AreEqual(expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteInt64()
	{
		long[] expectedValues = new long[]
		{
			long.MinValue,
			long.MinValue + 1L,
			int.MinValue - 1L,
			int.MinValue,
			int.MinValue + 1L,
			short.MinValue - 1L,
			short.MinValue,
			short.MinValue + 1L,
			sbyte.MinValue - 1L,
			sbyte.MinValue,
			sbyte.MinValue + 1L,
			-1,
			0,
			1,
			sbyte.MaxValue - 1L,
			sbyte.MaxValue,
			sbyte.MaxValue + 1L,
			byte.MaxValue - 1L,
			byte.MaxValue,
			byte.MaxValue + 1L,
			short.MaxValue - 1L,
			short.MaxValue,
			short.MaxValue + 1L,
			ushort.MaxValue - 1L,
			ushort.MaxValue,
			ushort.MaxValue + 1L,
			int.MaxValue - 1L,
			int.MaxValue,
			int.MaxValue + 1L,
			uint.MaxValue - 1L,
			uint.MaxValue,
			uint.MaxValue + 1L,
			long.MaxValue - 1L,
			long.MaxValue,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (long value in expectedValues)
		{
			Assert.IsTrue(writer.WriteInt64(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (long expectedValue in expectedValues)
		{
			long actualValue;
			Assert.IsTrue(reader.ReadInt64(out actualValue));
			Assert.AreEqual(expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteUInt8()
	{
		byte[] expectedValues = new byte[]
		{
			0,
			1,
			63,
			64,
			byte.MaxValue - 1,
			byte.MaxValue,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (byte value in expectedValues)
		{
			Assert.IsTrue(writer.WriteUInt8(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (byte expectedValue in expectedValues)
		{
			byte actualValue;
			Assert.IsTrue(reader.ReadUInt8(out actualValue));
			Assert.AreEqual(expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteUInt16()
	{
		ushort[] expectedValues = new ushort[]
		{
			0,
			1,
			63,
			64,
			byte.MaxValue - 1,
			byte.MaxValue,
			byte.MaxValue + 1,
			ushort.MaxValue - 1,
			ushort.MaxValue,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (ushort value in expectedValues)
		{
			Assert.IsTrue(writer.WriteUInt16(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (ushort expectedValue in expectedValues)
		{
			ushort actualValue;
			Assert.IsTrue(reader.ReadUInt16(out actualValue));
			Assert.AreEqual(expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteUInt32()
	{
		uint[] expectedValues = new uint[]
		{
			0,
			1,
			63,
			64,
			byte.MaxValue - 1,
			byte.MaxValue,
			byte.MaxValue + 1,
			ushort.MaxValue - 1,
			ushort.MaxValue,
			ushort.MaxValue + 1,
			uint.MaxValue - 1,
			uint.MaxValue,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (uint value in expectedValues)
		{
			Assert.IsTrue(writer.WriteUInt32(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (uint expectedValue in expectedValues)
		{
			uint actualValue;
			Assert.IsTrue(reader.ReadUInt32(out actualValue));
			Assert.AreEqual(expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteUInt64()
	{
		ulong[] expectedValues = new ulong[]
		{
			0,
			1,
			63,
			64,
			byte.MaxValue - 1L,
			byte.MaxValue,
			byte.MaxValue + 1L,
			ushort.MaxValue - 1L,
			ushort.MaxValue,
			ushort.MaxValue + 1L,
			uint.MaxValue - 1L,
			uint.MaxValue,
			uint.MaxValue + 1L,
			ulong.MaxValue - 1L,
			ulong.MaxValue,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (ulong value in expectedValues)
		{
			Assert.IsTrue(writer.WriteUInt64(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (ulong expectedValue in expectedValues)
		{
			ulong actualValue;
			Assert.IsTrue(reader.ReadUInt64(out actualValue));
			Assert.AreEqual(expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteUnsignedNormalizedFloat()
	{
		float[] expectedValues = new float[]
		{
			0.0f,
			float.Epsilon,
			2.060574E-13f,
			0.2500f,
			0.3333f,
			0.9512f,
			0.9999999999999f,
			1.0f,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (float value in expectedValues)
		{
			Assert.IsTrue(writer.WriteUnsignedNormalizedFloat(value, 16));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (float expectedValue in expectedValues)
		{
			float actualValue;
			Assert.IsTrue(reader.ReadUnsignedNormalizedFloat(16, out actualValue));
			Assert.That(actualValue, Is.EqualTo(expectedValue).Within(0.001f));
		}
	}

	[Test]
	public void ReadWriteUnsignedNormalizedFloatZeroExactly()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteUnsignedNormalizedFloat(0.0f, 2));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		float value;
		Assert.IsTrue(reader.ReadUnsignedNormalizedFloat(2, out value));
		Assert.That(value, Is.EqualTo(0.0f).Within(0.0001f));
	}

	[Test]
	public void ReadWriteUnsignedNormalizedFloatOneExactly()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteUnsignedNormalizedFloat(1.0f, 2));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		float value;
		Assert.IsTrue(reader.ReadUnsignedNormalizedFloat(2, out value));
		Assert.That(value, Is.EqualTo(1.0f).Within(0.0001f));
	}

	[Test]
	public void ReadWriteSignedNormalizedFloat()
	{
		float[] expectedValues = new float[]
		{
			-1.0f,
			-0.9999999999999f,
			-0.913f,
			-0.27f,
			-2.060574E-13f,
			-float.Epsilon,
			0.0f,
			float.Epsilon,
			2.060574E-13f,
			0.45f,
			0.866f,
			0.9999999999999f,
			1.0f,
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (float value in expectedValues)
		{
			Assert.IsTrue(writer.WriteSignedNormalizedFloat(value, 16));
		}

		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (float expectedValue in expectedValues)
		{
			float actualValue;
			Assert.IsTrue(reader.ReadSignedNormalizedFloat(16, out actualValue));
			AssertAreNearlyEqual(expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteSignedNormalizedFloatNegativeOneExactly()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteSignedNormalizedFloat(-1.0f, 2));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		float value;
		Assert.IsTrue(reader.ReadSignedNormalizedFloat(2, out value));
		AssertAreNearlyEqual(-1.0f, value, tolerance: 0.0001f);
	}

	[Test]
	public void ReadWriteSignedNormalizedFloatZeroExactly()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteSignedNormalizedFloat(0.0f, 2));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		float value;
		Assert.IsTrue(reader.ReadSignedNormalizedFloat(2, out value));
		AssertAreNearlyEqual(0.0f, value, tolerance: 0.0001f);
	}

	[Test]
	public void ReadWriteSignedNormalizedFloatPositiveOneExactly()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteSignedNormalizedFloat(+1.0f, 2));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		float value;
		Assert.IsTrue(reader.ReadSignedNormalizedFloat(2, out value));
		AssertAreNearlyEqual(+1.0f, value, tolerance: 0.0001f);
	}

	[Test]
	public void ReadWriteGuid()
	{
		System.Guid[] expectedValues = new System.Guid[]
		{
			System.Guid.Empty,
			new System.Guid("08dcb6795dd14ea39e5ca06a9a944d9f"),
			new System.Guid("77b7f947419f4f798c3c6f5cce9525d8"),
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (System.Guid value in expectedValues)
		{
			Assert.IsTrue(writer.WriteGuid(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (System.Guid expectedValue in expectedValues)
		{
			System.Guid actualValue;
			Assert.IsTrue(reader.ReadGuid(out actualValue));
			Assert.AreEqual(expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteDateTime()
	{
		System.DateTime[] expectedValues = new System.DateTime[]
		{
			System.DateTime.Today,
			System.DateTime.UtcNow,
			System.DateTime.Now,
			System.DateTime.MinValue,
			System.DateTime.MaxValue,
			new System.DateTime(0),
			new System.DateTime(1997, 7, 27),
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		foreach (System.DateTime value in expectedValues)
		{
			Assert.IsTrue(writer.WriteDateTime(value));
		}
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		foreach (System.DateTime expectedValue in expectedValues)
		{
			System.DateTime actualValue;
			Assert.IsTrue(reader.ReadDateTime(out actualValue));
			Assert.AreEqual(expectedValue, actualValue);
		}
	}

	[Test]
	public void ReadWriteList()
	{
		NetLength maxLength = new NetLength(3);

		List<int> expectedValues = new List<int>()
		{
			5,
			72,
			103
		};

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteList(expectedValues, writer.WriteInt32, maxLength));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		List<int> actualValues = new List<int>();
		Assert.IsTrue(reader.ReadList(actualValues, reader.ReadInt32, maxLength));

		Assert.That(actualValues, Is.EquivalentTo(expectedValues));
	}

	private void AssertAreNearlyEqual(float expected, float actual, float tolerance = 0.01f)
	{
		Assert.That(actual, Is.EqualTo(expected).Within(tolerance));
	}
}
