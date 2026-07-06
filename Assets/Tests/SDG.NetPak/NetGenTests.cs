////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////

/* Todo: broke this when moving into an asmdef.
internal class NetGenTests
{
	[Test]
	public void ReadWriteEnum()
	{
		const int bitsRequired = 5;

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteEnum(EClientMessage.InvokeMethod));
		Assert.AreEqual(bitsRequired, writer.scratchBitCount);
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		EClientMessage actualValue;
		Assert.IsTrue(reader.ReadEnum(out actualValue));
		Assert.AreEqual(EClientMessage.InvokeMethod, actualValue);
		Assert.AreEqual(32 - bitsRequired, reader.scratchBitCount);
	}

	[Test]
	public void ReadEnumOutsideRange()
	{
		TestDelegate code = () =>
		{
			NetPakReader reader = new NetPakReader();
			reader.SetBuffer(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
			ESteamPacket actualValue;
			Assert.IsFalse(reader.ReadEnum(out actualValue));
		};

#if WITH_NETPAK_EXCEPTIONS
		Assert.Throws<System.IndexOutOfRangeException>(code);
#else
		code();
#endif // WITH_NETPAK_EXCEPTIONS
	}
}
*/
