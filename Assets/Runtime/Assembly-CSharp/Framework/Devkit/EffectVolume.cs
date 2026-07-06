////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////

using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Framework.Devkit
{
	public class EffectVolume : LevelVolume<EffectVolume, EffectVolumeManager>
	{
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		[SerializeField]
		internal System.Guid _effectGuid;
		public System.Guid EffectGuid => _effectGuid;

		/// <summary>
		/// Kept because lots of modders have been using this script in Unity,
		/// so removing legacy effect id would break their content.
		/// </summary>
		[SerializeField]
		protected ushort _id;
		public ushort id
		{
			[System.Obsolete]
			get => _id;
			[System.Obsolete]
			set
			{
				_id = value;
				SyncEffect();
			}
		}

		[SerializeField]
		protected int maxParticlesBase;
		[SerializeField]
		protected float rateOverTimeBase;
		[SerializeField]
		protected float _emissionMultiplier = 1.0f;

		public float emissionMultiplier
		{
			get => _emissionMultiplier;
			set
			{
				_emissionMultiplier = value;

				if (effect != null)
				{
					applyEmission();
				}
			}
		}

		[SerializeField]
		protected float _audioRangeMultiplier = 1.0f;

		public float audioRangeMultiplier
		{
			get => _audioRangeMultiplier;
			set
			{
				_audioRangeMultiplier = value;

				if (effect != null)
				{
					applyAudioRange();
				}
			}
		}

		protected Transform effect;

		private void SyncEffect()
		{
			if (effect != null)
			{
				Destroy(effect.gameObject);
				effect = null;
			}

			EffectAsset asset = Assets.FindEffectAssetByGuidOrLegacyId(_effectGuid, _id);
			if (asset != null && asset.effect != null)
			{
				if (!Dedicator.IsDedicatedServer || asset.spawnOnDedicatedServer)
				{
					InstantiateParameters instantiateParameters = new InstantiateParameters()
					{
						parent = transform,
						worldSpace = false,
					};
					effect = Instantiate(asset.effect, Vector3.zero, Quaternion.Euler(-90, 0, 0), instantiateParameters).transform;
					effect.name = "Effect";
					effect.transform.localScale = new Vector3(1, 1, 1);

					ParticleSystem particleSystem = effect.GetComponent<ParticleSystem>();
					if (particleSystem != null)
					{
						maxParticlesBase = particleSystem.main.maxParticles;
						rateOverTimeBase = particleSystem.emission.rateOverTimeMultiplier;
					}

					AudioSource audioSource = effect.GetComponent<AudioSource>();
					if (audioSource != null && audioSource.clip != null)
					{
						audioSource.time = Random.Range(0, audioSource.clip.length);
					}
				}
			}

			if (effect != null)
			{
				applyEmission();
				applyAudioRange();
			}
		}

		protected virtual void applyEmission()
		{
			if (effect == null)
			{
				return;
			}

			ParticleSystem particleSystem = effect.GetComponent<ParticleSystem>();

			if (particleSystem == null)
			{
				return;
			}

			ParticleSystem.MainModule mainModule = particleSystem.main;
			mainModule.maxParticles = (int) (maxParticlesBase * emissionMultiplier);

			ParticleSystem.EmissionModule emissionModule = particleSystem.emission;
			emissionModule.rateOverTimeMultiplier = rateOverTimeBase * emissionMultiplier;
		}

		protected virtual void applyAudioRange()
		{
			if (effect == null)
			{
				return;
			}

			AudioSource audioSource = effect.GetComponent<AudioSource>();

			if (audioSource == null)
			{
				return;
			}

			audioSource.maxDistance = audioRangeMultiplier;
		}

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			if (reader.containsKey("Emission"))
			{
				_emissionMultiplier = reader.readValue<float>("Emission");
			}

			if (reader.containsKey("Audio_Range"))
			{
				_audioRangeMultiplier = reader.readValue<float>("Audio_Range");
			}

			string effectIdString = reader.readValue("ID");
			if (ushort.TryParse(effectIdString, out _id))
			{
				_effectGuid = System.Guid.Empty;
				SyncEffect();
			}
			else if (System.Guid.TryParse(effectIdString, out _effectGuid))
			{
				_id = 0;
				SyncEffect();
			}
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			if (!_effectGuid.IsEmpty())
			{
				writer.writeValue("ID", _effectGuid);
			}
			else
			{
				writer.writeValue("ID", _id);
			}

			writer.writeValue("Emission", emissionMultiplier);
			writer.writeValue("Audio_Range", audioRangeMultiplier);
		}

		protected override void Awake()
		{
			supportsSphereShape = false;
			base.Awake();
		}

		protected override void Start()
		{
			base.Start();
			effect = transform.Find("Effect");
		}

		private class Menu : SleekWrapper
		{
			public Menu(EffectVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;
				SizeOffset_Y = 110;

				ISleekField idField = Glazier.Get().CreateStringField();
				idField.SizeOffset_X = 200;
				idField.SizeOffset_Y = 30;
				if (volume._effectGuid.IsEmpty())
				{
					idField.Text = volume._id.ToString();
				}
				else
				{
					idField.Text = volume._effectGuid.ToString("N");
				}
				idField.AddLabel("Effect ID", ESleekSide.RIGHT);
				idField.OnTextChanged += OnIdChanged;
				AddChild(idField);

				ISleekFloat32Field emissionField = Glazier.Get().CreateFloat32Field();
				emissionField.PositionOffset_Y = 40;
				emissionField.SizeOffset_X = 200;
				emissionField.SizeOffset_Y = 30;
				emissionField.Value = volume.emissionMultiplier;
				emissionField.AddLabel("Emission Rate", ESleekSide.RIGHT);
				emissionField.OnValueChanged += OnEmissionChanged;
				AddChild(emissionField);

				ISleekFloat32Field audioRangeField = Glazier.Get().CreateFloat32Field();
				audioRangeField.PositionOffset_Y = 80;
				audioRangeField.SizeOffset_X = 200;
				audioRangeField.SizeOffset_Y = 30;
				audioRangeField.Value = volume.audioRangeMultiplier;
				audioRangeField.AddLabel("Audio Range", ESleekSide.RIGHT);
				audioRangeField.OnValueChanged += OnAudioRangeChanged;
				AddChild(audioRangeField);
			}

			private void OnIdChanged(ISleekField field, string effectIdString)
			{
				if (ushort.TryParse(effectIdString, out volume._id))
				{
					volume._effectGuid = System.Guid.Empty;
					volume.SyncEffect();
				}
				else if (System.Guid.TryParse(effectIdString, out volume._effectGuid))
				{
					volume._id = 0;
					volume.SyncEffect();
				}
				else
				{
					volume._effectGuid = System.Guid.Empty;
					volume._id = 0;
					volume.SyncEffect();
				}
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnEmissionChanged(ISleekFloat32Field field, float value)
			{
				volume.emissionMultiplier = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnAudioRangeChanged(ISleekFloat32Field field, float value)
			{
				volume.audioRangeMultiplier = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private EffectVolume volume;
		}
	}
}
