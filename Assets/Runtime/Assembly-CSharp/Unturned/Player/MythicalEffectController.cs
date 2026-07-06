////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Manages lifetime and attachment of a mythical effect. Added by <see cref="ItemTool.ApplyMythicalEffect"/>.
	/// Was called `MythicLocker` with a paired `MythicLockee` prior to 2024-06-11.
	/// </summary>
	public class MythicalEffectController : MonoBehaviour
	{
		public GameObject systemPrefab;
		public Transform systemTransform;

		private bool _isMythicalEffectEnabled = true;
		public bool IsMythicalEffectEnabled
		{
			get => _isMythicalEffectEnabled;

			set
			{
				_isMythicalEffectEnabled = value;
				MaybeInstantiateOrDestroySystem();
			}
		}

		private void Update()
		{
			if (systemTransform != null)
			{
				transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
				systemTransform.SetPositionAndRotation(position, rotation);
			}
		}

		private void LateUpdate()
		{
			if (systemTransform != null)
			{
				transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
				systemTransform.SetPositionAndRotation(position, rotation);
			}
		}

		private void OnEnable()
		{
			MaybeInstantiateOrDestroySystem();
		}

		private void OnDisable()
		{
			if (systemTransform != null)
			{
				Destroy(systemTransform.gameObject);
				systemTransform = null;
			}
		}

		private void OnDestroy()
		{
			if (systemTransform != null)
			{
				Destroy(systemTransform.gameObject);
				systemTransform = null;
			}
		}

		private void Start()
		{
			MaybeInstantiateOrDestroySystem();
		}

		private void MaybeInstantiateOrDestroySystem()
		{
			if (_isMythicalEffectEnabled && gameObject.activeInHierarchy)
			{
				if (systemTransform == null && systemPrefab != null)
				{
					transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
					systemTransform = Instantiate(systemPrefab, position, rotation).transform;
					systemTransform.name = "System";

					// Nelson 2024-09-18: Molt's Scarf Effect transform is 0.25x scale to keep fire near the end of
					// the scarf. I checked that no other item currently uses a different scale.
					systemTransform.localScale = transform.localScale;
				}
			}
			else
			{
				if (systemTransform != null)
				{
					Destroy(systemTransform.gameObject);
					systemTransform = null;
				}
			}
		}
	}
}
