////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.NetPak;
using SDG.Unturned;
using UnityEngine;

internal class TransformNetPakTests
{
	[Test]
	public void ReadWriteNull()
	{
		NetIdRegistry.Clear();
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteInt8(-57)); // prefix
		Assert.IsTrue(writer.WriteTransform(null));
		Assert.IsTrue(writer.WriteInt8(93)); // suffix
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		sbyte prefix;
		Assert.IsTrue(reader.ReadInt8(out prefix));
		Assert.AreEqual(-57, prefix);
		Transform transform;
		Assert.IsTrue(reader.ReadTransform(out transform));
		Assert.IsNull(transform);
		sbyte suffix;
		Assert.IsTrue(reader.ReadInt8(out suffix));
		Assert.AreEqual(93, suffix);
	}

	[Test]
	public void ReadWriteRoot()
	{
		NetIdRegistry.Clear();
		Transform root = new GameObject().transform;
		NetId netId = NetIdRegistry.Claim();
		NetIdRegistry.AssignTransform(netId, root);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteInt8(-57)); // prefix
		Assert.IsTrue(writer.WriteTransform(root));
		Assert.IsTrue(writer.WriteInt8(93)); // suffix
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		sbyte prefix;
		Assert.IsTrue(reader.ReadInt8(out prefix));
		Assert.AreEqual(-57, prefix);
		Transform transform;
		Assert.IsTrue(reader.ReadTransform(out transform));
		Assert.AreEqual(root, transform);
		sbyte suffix;
		Assert.IsTrue(reader.ReadInt8(out suffix));
		Assert.AreEqual(93, suffix);
	}

	[Test]
	public void ReadWriteChild()
	{
		NetIdRegistry.Clear();
		Transform parent = new GameObject().transform;
		Transform child = new GameObject().transform;
		child.parent = parent;
		NetId netId = NetIdRegistry.Claim();
		NetIdRegistry.AssignTransform(netId, parent);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteInt8(-57)); // prefix
		Assert.IsTrue(writer.WriteTransform(child));
		Assert.IsTrue(writer.WriteInt8(93)); // suffix
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		sbyte prefix;
		Assert.IsTrue(reader.ReadInt8(out prefix));
		Assert.AreEqual(-57, prefix);
		Transform transform;
		Assert.IsTrue(reader.ReadTransform(out transform));
		Assert.AreEqual(child, transform);
		sbyte suffix;
		Assert.IsTrue(reader.ReadInt8(out suffix));
		Assert.AreEqual(93, suffix);
	}

	[Test]
	public void ReadWriteGrandchild()
	{
		NetIdRegistry.Clear();
		Transform parent = new GameObject().transform;
		Transform child = new GameObject().transform;
		child.parent = parent;
		Transform grandchild = new GameObject().transform;
		grandchild.parent = child;
		NetId netId = NetIdRegistry.Claim();
		NetIdRegistry.AssignTransform(netId, parent);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteInt8(-57)); // prefix
		Assert.IsTrue(writer.WriteTransform(grandchild));
		Assert.IsTrue(writer.WriteInt8(93)); // suffix
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		sbyte prefix;
		Assert.IsTrue(reader.ReadInt8(out prefix));
		Assert.AreEqual(-57, prefix);
		Transform transform;
		Assert.IsTrue(reader.ReadTransform(out transform));
		Assert.AreEqual(grandchild, transform);
		sbyte suffix;
		Assert.IsTrue(reader.ReadInt8(out suffix));
		Assert.AreEqual(93, suffix);
	}

	[Test]
	public void ReadWriteGreatgrandchild()
	{
		NetIdRegistry.Clear();
		Transform parent = new GameObject().transform;
		Transform child = new GameObject().transform;
		child.parent = parent;
		Transform grandchild = new GameObject().transform;
		grandchild.parent = child;
		Transform greatgrandchild = new GameObject().transform;
		greatgrandchild.parent = grandchild;
		NetId netId = NetIdRegistry.Claim();
		NetIdRegistry.AssignTransform(netId, parent);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteInt8(-57)); // prefix
		Assert.IsTrue(writer.WriteTransform(greatgrandchild));
		Assert.IsTrue(writer.WriteInt8(93)); // suffix
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		sbyte prefix;
		Assert.IsTrue(reader.ReadInt8(out prefix));
		Assert.AreEqual(-57, prefix);
		Transform transform;
		Assert.IsTrue(reader.ReadTransform(out transform));
		Assert.AreEqual(greatgrandchild, transform);
		sbyte suffix;
		Assert.IsTrue(reader.ReadInt8(out suffix));
		Assert.AreEqual(93, suffix);
	}

	[Test]
	public void ReadWriteUnassignedRoot()
	{
		NetIdRegistry.Clear();
		Transform root = new GameObject().transform;

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteInt8(-57)); // prefix
		Assert.IsTrue(writer.WriteTransform(root));
		Assert.IsTrue(writer.WriteInt8(93)); // suffix
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		sbyte prefix;
		Assert.IsTrue(reader.ReadInt8(out prefix));
		Assert.AreEqual(-57, prefix);
		Transform transform;
		Assert.IsTrue(reader.ReadTransform(out transform));
		Assert.IsNull(transform);
		sbyte suffix;
		Assert.IsTrue(reader.ReadInt8(out suffix));
		Assert.AreEqual(93, suffix);
	}

	[Test]
	public void ReadWriteUnassignedChild()
	{
		NetIdRegistry.Clear();
		Transform parent = new GameObject().transform;
		Transform child = new GameObject().transform;
		child.parent = parent;

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteInt8(-57)); // prefix
		Assert.IsTrue(writer.WriteTransform(child));
		Assert.IsTrue(writer.WriteInt8(93)); // suffix
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		sbyte prefix;
		Assert.IsTrue(reader.ReadInt8(out prefix));
		Assert.AreEqual(-57, prefix);
		Transform transform;
		Assert.IsTrue(reader.ReadTransform(out transform));
		Assert.IsNull(transform);
		sbyte suffix;
		Assert.IsTrue(reader.ReadInt8(out suffix));
		Assert.AreEqual(93, suffix);
	}

	[Test]
	public void ReadWriteUnassignedGrandchild()
	{
		NetIdRegistry.Clear();
		Transform parent = new GameObject().transform;
		Transform child = new GameObject().transform;
		child.parent = parent;
		Transform grandchild = new GameObject().transform;
		grandchild.parent = child;

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteInt8(-57)); // prefix
		Assert.IsTrue(writer.WriteTransform(grandchild));
		Assert.IsTrue(writer.WriteInt8(93)); // suffix
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		sbyte prefix;
		Assert.IsTrue(reader.ReadInt8(out prefix));
		Assert.AreEqual(-57, prefix);
		Transform transform;
		Assert.IsTrue(reader.ReadTransform(out transform));
		Assert.IsNull(transform);
		sbyte suffix;
		Assert.IsTrue(reader.ReadInt8(out suffix));
		Assert.AreEqual(93, suffix);
	}

	[Test]
	public void ReadWriteUnassignedGreatgrandchild()
	{
		NetIdRegistry.Clear();
		Transform parent = new GameObject().transform;
		Transform child = new GameObject().transform;
		child.parent = parent;
		Transform grandchild = new GameObject().transform;
		grandchild.parent = child;
		Transform greatgrandchild = new GameObject().transform;
		greatgrandchild.parent = grandchild;

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteInt8(-57)); // prefix
		Assert.IsTrue(writer.WriteTransform(greatgrandchild));
		Assert.IsTrue(writer.WriteInt8(93)); // suffix
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		sbyte prefix;
		Assert.IsTrue(reader.ReadInt8(out prefix));
		Assert.AreEqual(-57, prefix);
		Transform transform;
		Assert.IsTrue(reader.ReadTransform(out transform));
		Assert.IsNull(transform);
		sbyte suffix;
		Assert.IsTrue(reader.ReadInt8(out suffix));
		Assert.AreEqual(93, suffix);
	}
}
