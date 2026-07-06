////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;

internal class DatListTests
{
	[Test]
	public void TryGetDictionary()
	{
		IDatList list = new DatList()
		{
			new DatValue(),
			new DatDictionary(),
			new DatValue(),
		};

		Assert.IsFalse(list.TryGetDictionary(0, out IDatDictionary dict0));
		Assert.IsNull(dict0);
		Assert.IsTrue(list.TryGetDictionary(1, out IDatDictionary dict1));
		Assert.IsNotNull(dict1);
		Assert.IsFalse(list.TryGetDictionary(2, out IDatDictionary dict2));
		Assert.IsNull(dict2);
		Assert.IsFalse(list.TryGetDictionary(3, out IDatDictionary dict3));
		Assert.IsNull(dict3);
	}

	[Test]
	public void GetDictionary()
	{
		IDatList list = new DatList()
		{
			new DatValue(),
			new DatDictionary(),
			new DatValue(),
		};

		Assert.IsNull(list.GetDictionary(0));
		Assert.IsNotNull(list.GetDictionary(1));
		Assert.IsNull(list.GetDictionary(2));
		Assert.IsNull(list.GetDictionary(3));
	}

	[Test]
	public void TryGetList()
	{
		IDatList list = new DatList()
		{
			new DatValue(),
			new DatList(),
			new DatValue(),
		};

		Assert.IsFalse(list.TryGetList(0, out IDatList list0));
		Assert.IsNull(list0);
		Assert.IsTrue(list.TryGetList(1, out IDatList list1));
		Assert.IsNotNull(list1);
		Assert.IsFalse(list.TryGetList(2, out IDatList list2));
		Assert.IsNull(list2);
		Assert.IsFalse(list.TryGetList(3, out IDatList list3));
		Assert.IsNull(list3);
	}

	[Test]
	public void GetList()
	{
		IDatList list = new DatList()
		{
			new DatValue(),
			new DatList(),
			new DatValue(),
		};

		Assert.IsNull(list.GetList(0));
		Assert.IsNotNull(list.GetList(1));
		Assert.IsNull(list.GetList(2));
		Assert.IsNull(list.GetList(3));
	}

	[Test]
	public void GetStrings()
	{
		string[] values = new string[]
		{
			"abc",
			"def",
			"ghi"
		};

		DatList list = new DatList();
		foreach (string value in values)
		{
			list.Add(new DatValue(value));
		}

		int index = 0;
		foreach (IDatNode value in list)
		{
			IDatValue literal = value as IDatValue;
			Assert.IsNotNull(literal);
			Assert.AreEqual(values[index], literal.Value);
			++index;
		}
	}

	[Test]
	public void EnumerateValues()
	{
		IDatList list = new DatList()
		{
			new DatDictionary(),
			new DatValue("abc"),
			new DatValue("def"),
			new DatList(),
			new DatDictionary(),
			null,
			new DatValue("ghi"),
		};

		string[] expectedValues = new string[]
		{
			"abc",
			"def",
			"ghi"
		};

		int index = 0;
		foreach (IDatValue value in list.GetValues())
		{
			Assert.IsNotNull(value);
			Assert.AreEqual(expectedValues[index], value.Value);
			++index;
		}
	}
}
