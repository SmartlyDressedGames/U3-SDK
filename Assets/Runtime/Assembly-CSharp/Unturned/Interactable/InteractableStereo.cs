////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using System;
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableStereo : Interactable
	{
		protected float _volume;
		public float volume
		{
			get => _volume;
			set
			{
				_volume = Mathf.Clamp01(value);

				if (audioSource != null)
				{
					audioSource.volume = _volume;
				}
			}
		}

		public byte compressedVolume
		{
			get => (byte) Mathf.RoundToInt(volume * 100);
			set => volume = Mathf.Clamp01(value / 100f);
		}

		public AssetReference<StereoSongAsset> track;
		public AudioSource audioSource;

		public void updateTrack(Guid newTrack)
		{
			track.GUID = newTrack;

			if (audioSource != null)
			{
				audioSource.clip = null;
				audioSource.loop = false;

				StereoSongAsset asset = Assets.Find_UseDefaultAssetMapping(track);
				if (asset != null)
				{
					if (asset.songMbRef.isValid)
					{
						audioSource.clip = asset.songMbRef.loadAsset();
					}
					else if (asset.songContentRef.isValid)
					{
						audioSource.clip = Assets.load(asset.songContentRef);
					}

					audioSource.loop = asset.isLoop;
				}

				if (audioSource.clip != null)
				{
					audioSource.Play();
				}
				else
				{
					audioSource.Stop();
				}
			}
		}

		public override void updateState(Asset asset, byte[] state)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				audioSource = transform.Find("Audio").GetComponent<AudioSource>();
			}

			GuidBuffer buffer = new GuidBuffer();
			buffer.Read(state, 0);
			updateTrack(buffer.GUID);

			compressedVolume = state[16];
		}

		public override void use()
		{
			PlayerUI.instance.boomboxUI.open(this);

			PlayerLifeUI.close();
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			message = EPlayerMessage.USE;

			text = "";
			color = Color.white;
			return !PlayerUI.window.showCursor;
		}

		internal static readonly ClientInstanceMethod<System.Guid> SendTrack = ClientInstanceMethod<System.Guid>.Get(typeof(InteractableStereo), nameof(ReceiveTrack));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveTrack(System.Guid newTrack)
		{
			updateTrack(newTrack);
		}

		public void ClientSetTrack(System.Guid newTrack)
		{
			SendTrackRequest.Invoke(GetNetId(), NetTransport.ENetReliability.Unreliable, newTrack);
		}

		private static readonly ServerInstanceMethod<System.Guid> SendTrackRequest = ServerInstanceMethod<System.Guid>.Get(typeof(InteractableStereo), nameof(ReceiveTrackRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
		public void ReceiveTrackRequest(in ServerInvocationContext context, System.Guid newTrack)
		{
			// Don't validate newTrack exists! This allows players to install music mods from
			// the workshop and play them even if the server doesn't have them installed.

			Player player = context.GetPlayer();

			if (player == null)
			{
				context.LogWarning("null player");
				return;
			}

			if (player.life.isDead)
			{
				context.LogWarning("player is dead");
				return;
			}

			if ((transform.position - player.transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("too far away");
				return;
			}

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!BarricadeManager.tryGetRegion(transform, out x, out y, out plant, out region))
			{
				context.LogWarning("invalid region");
				return;
			}

			BarricadeManager.ServerSetStereoTrackInternal(this, x, y, plant, region, newTrack);
		}

		private static readonly ClientInstanceMethod<byte> SendChangeVolume = ClientInstanceMethod<byte>.Get(typeof(InteractableStereo), nameof(ReceiveChangeVolume));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveChangeVolume(byte newVolume)
		{
			compressedVolume = newVolume;
		}

		public void ClientSetVolume(byte newVolume)
		{
			SendChangeVolumeRequest.Invoke(GetNetId(), NetTransport.ENetReliability.Unreliable, newVolume);
		}

		private static readonly ServerInstanceMethod<byte> SendChangeVolumeRequest = ServerInstanceMethod<byte>.Get(typeof(InteractableStereo), nameof(ReceiveChangeVolumeRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 8)]
		public void ReceiveChangeVolumeRequest(in ServerInvocationContext context, byte newVolume)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			if (player.life.isDead)
			{
				return;
			}

			if ((transform.position - player.transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("too far away");
				return;
			}

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!BarricadeManager.tryGetRegion(transform, out x, out y, out plant, out region))
			{
				context.LogWarning("invalid region");
				return;
			}

			newVolume = MathfEx.Min(newVolume, 100); // Clamp [0, 100]

			SendChangeVolume.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, BarricadeManager.GatherRemoteClientConnections(x, y, plant), newVolume);

			BarricadeDrop barricade = region.FindBarricadeByRootFast(transform);
			byte[] state = barricade.serversideData.barricade.state;
			state[16] = newVolume;
		}
	}
}
