////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define LOG_CAMERASHAKE_SETTINGS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

namespace SDG.Unturned
{
	public class EffectAsset : Asset
	{
		protected GameObject _effect;
		/// <summary>
		/// Note: as of 2025-04-23 this *can* be null. (E.g., audio-only effects.)
		/// </summary>
		public GameObject effect => _effect;

#if !DEDICATED_SERVER
		/// <summary>
		/// If set, use OneShotAudioParameters to play this audio.
		/// </summary>
		public AudioReference OneShotAudio;
#endif // !DEDICATED_SERVER

		protected GameObject[] _splatters;
		public GameObject[] splatters => _splatters;

		private bool _gore;
		public bool gore => _gore;

		private byte _splatter;
		public byte splatter => _splatter;

		private float _splatterLifetime;
		public float splatterLifetime => _splatterLifetime;

		private float _splatterLifetimeSpread;
		public float splatterLifetimeSpread => _splatterLifetimeSpread;

		private bool _splatterLiquid;
		public bool splatterLiquid => _splatterLiquid;

		private EPlayerTemperature _splatterTemperature;
		public EPlayerTemperature splatterTemperature => _splatterTemperature;

		private byte _splatterPreload;
		public byte splatterPreload => _splatterPreload;

		private float _lifetime;
		public float lifetime => _lifetime;

		private float _lifetimeSpread;
		public float lifetimeSpread => _lifetimeSpread;

		private bool _isStatic;
		public bool isStatic => _isStatic;

		/// <summary>
		/// If true the music option is respected when this effect is used by ambiance volume.
		/// </summary>
		public bool isMusic
		{
			get;
			private set;
		}

		private byte _preload;
		public byte preload => _preload;

		public System.Guid blastmarkEffectGuid;
		private ushort _blast;
		public ushort blast
		{
			[System.Obsolete]
			get => _blast;
		}

		public EffectAsset FindBlastmarkEffectAsset()
		{
#pragma warning disable
			return Assets.FindEffectAssetByGuidOrLegacyId(blastmarkEffectGuid, blast);
#pragma warning restore
		}

		/// <summary>
		/// In multiplayer the effect will be spawned for players within this radius.
		/// </summary>
		public float relevantDistance
		{
			get;
			protected set;
		}

		public bool spawnOnDedicatedServer
		{
			get;
			protected set;
		}

		public bool randomizeRotation
		{
			get;
			protected set;
		}

		public float cameraShakeRadius;
		public float cameraShakeMagnitudeDegrees;

		public override EAssetType assetCategory => EAssetType.EFFECT;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (id < 200 && !OriginAllowsVanillaLegacyId && !p.data.ContainsKey("Bypass_ID_Limit"))
			{
				throw new System.NotSupportedException("ID < 200");
			}

			_effect = p.bundle.load<GameObject>("Effect");

#if !DEDICATED_SERVER
			OneShotAudio = p.data.ReadAudioReference("OneShotAudio", p.bundle);
#endif // !DEDICATED_SERVER

			_gore = p.data.ContainsKey("Gore");

			_splatters = new GameObject[p.data.ParseUInt8("Splatter")];
			for (int index = 0; index < splatters.Length; index++)
			{
				splatters[index] = p.bundle.load<GameObject>("Splatter_" + index);

				if (splatters[index] == null)
				{
					Assets.ReportError(this, string.Format("missing 'Splatter_{0}' gameobject", index));
				}
			}

			_splatter = p.data.ParseUInt8("Splatters");
			_splatterLiquid = p.data.ContainsKey("Splatter_Liquid");
			if (p.data.ContainsKey("Splatter_Temperature"))
			{
				_splatterTemperature = (EPlayerTemperature) System.Enum.Parse(typeof(EPlayerTemperature), p.data.GetString("Splatter_Temperature"), true);
			}
			else
			{
				_splatterTemperature = EPlayerTemperature.NONE;
			}
			_splatterLifetime = p.data.ParseFloat("Splatter_Lifetime");
			if (p.data.ContainsKey("Splatter_Lifetime_Spread"))
			{
				_splatterLifetimeSpread = p.data.ParseFloat("Splatter_Lifetime_Spread");
			}
			else
			{
				_splatterLifetimeSpread = 1.0f;
			}

			_lifetime = p.data.ParseFloat("Lifetime");
			if (p.data.ContainsKey("Lifetime_Spread"))
			{
				_lifetimeSpread = p.data.ParseFloat("Lifetime_Spread");
			}
			else
			{
				_lifetimeSpread = 4.0f;
			}

			_isStatic = p.data.ContainsKey("Static");
			isMusic = p.data.ParseBool("Is_Music");

			if (p.data.ContainsKey("Preload"))
			{
				_preload = p.data.ParseUInt8("Preload");
			}
			else
			{
				_preload = 1;
			}

			if (p.data.ContainsKey("Splatter_Preload"))
			{
				_splatterPreload = p.data.ParseUInt8("Splatter_Preload");
			}
			else
			{
				_splatterPreload = (byte) (Mathf.CeilToInt(splatter / (float) splatters.Length) * preload);
			}

			_blast = p.data.ParseGuidOrLegacyId("Blast", out blastmarkEffectGuid);

			relevantDistance = p.data.ParseFloat("Relevant_Distance", -1.0f);
			spawnOnDedicatedServer = p.data.ContainsKey("Spawn_On_Dedicated_Server");
			if (p.data.ContainsKey("Randomize_Rotation"))
			{
				randomizeRotation = p.data.ParseBool("Randomize_Rotation");
			}
			else
			{
				randomizeRotation = true;
			}

			cameraShakeRadius = p.data.ParseFloat("CameraShake_Radius");
			cameraShakeMagnitudeDegrees = p.data.ParseFloat("CameraShake_MagnitudeDegrees");
#if LOG_CAMERASHAKE_SETTINGS
			if (cameraShakeRadius > 0.001f && cameraShakeMagnitudeDegrees > 0.1f)
			{
				UnturnedLog.info($"{name} Radius: {cameraShakeRadius} Magnitude: {cameraShakeMagnitudeDegrees}");
			}
#endif // LOG_CAMERASHAKE_SETTINGS
		}
	}
}
