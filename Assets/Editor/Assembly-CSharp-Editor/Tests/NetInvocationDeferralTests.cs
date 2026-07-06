////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.NetTransport;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

internal class NetInvocationDeferralTests
{
	[Test]
	public void InvokeOnceWithoutDeferral()
	{
		NetIdRegistry.Clear();
		BarricadeDrop barricade = CreateBarricade();
		barricade.AssignNetId(NetIdRegistry.Claim());
		BarricadeDrop.SendHealth.InvokeAndLoopback(barricade.GetNetId(), ENetReliability.Reliable, new List<ITransportConnection>() { }, 25);
		AssertHealth(barricade, 25);
	}

	[Test]
	public void InvokeTwiceWithoutDeferral()
	{
		NetIdRegistry.Clear();
		BarricadeDrop barricade = CreateBarricade();
		barricade.AssignNetId(NetIdRegistry.Claim());
		BarricadeDrop.SendHealth.InvokeAndLoopback(barricade.GetNetId(), ENetReliability.Reliable, new List<ITransportConnection>() { }, 24);
		BarricadeDrop.SendHealth.InvokeAndLoopback(barricade.GetNetId(), ENetReliability.Reliable, new List<ITransportConnection>() { }, 33);
		AssertHealth(barricade, 33);
	}

	[Test]
	public void InvokeOnceWithDeferral()
	{
		NetIdRegistry.Clear();
		NetId netId = NetIdRegistry.Claim();
		NetInvocationDeferralRegistry.MarkDeferred(netId);
		BarricadeDrop.SendHealth.InvokeAndLoopback(netId, ENetReliability.Reliable, new List<ITransportConnection>() { }, 6);
		BarricadeDrop barricade = CreateBarricade();
		barricade.AssignNetId(netId);
		NetInvocationDeferralRegistry.Invoke(netId);
		AssertHealth(barricade, 6);
	}

	[Test]
	public void InvokeTwiceWithDeferral()
	{
		NetIdRegistry.Clear();
		NetId netId = NetIdRegistry.Claim();
		NetInvocationDeferralRegistry.MarkDeferred(netId);
		BarricadeDrop.SendHealth.InvokeAndLoopback(netId, ENetReliability.Reliable, new List<ITransportConnection>() { }, 7);
		BarricadeDrop.SendHealth.InvokeAndLoopback(netId, ENetReliability.Reliable, new List<ITransportConnection>() { }, 8);
		BarricadeDrop barricade = CreateBarricade();
		barricade.AssignNetId(netId);
		NetInvocationDeferralRegistry.Invoke(netId);
		AssertHealth(barricade, 8);
	}

	[Test]
	public void InvokeDeferredBlock()
	{
		NetIdRegistry.Clear();
		uint blockSize = 7;
		NetId baseNetId = NetIdRegistry.ClaimBlock(blockSize);
		NetId offsetNetId = baseNetId + 3;
		NetInvocationDeferralRegistry.MarkDeferred(baseNetId, blockSize);
		BarricadeDrop.SendHealth.InvokeAndLoopback(offsetNetId, ENetReliability.Reliable, new List<ITransportConnection>() { }, 11);
		BarricadeDrop.SendHealth.InvokeAndLoopback(offsetNetId, ENetReliability.Reliable, new List<ITransportConnection>() { }, 12);
		BarricadeDrop barricade = CreateBarricade();
		barricade.AssignNetId(offsetNetId);
		NetInvocationDeferralRegistry.Invoke(baseNetId, blockSize);
		AssertHealth(barricade, 12);
	}

	private BarricadeDrop CreateBarricade()
	{
		GameObject model = new GameObject("Barricade");
		model.AddComponent<Interactable2HP>(); // Required for ReceiveHealth
		return new BarricadeDrop(model.transform, null, null);
	}

	private void AssertHealth(BarricadeDrop barricade, byte expected)
	{
		Assert.AreEqual(expected, barricade.model.GetComponent<Interactable2HP>().hp, "health");
	}
}
