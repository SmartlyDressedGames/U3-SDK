////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using TMPro;
using UnityEngine;

namespace SDG.Unturned
{
	public class StatTracker : MonoBehaviour
	{
		public TextMeshPro statTrackerText
		{
			get;
			protected set;
		}

		public Transform statTrackerHook
		{
			get;
			protected set;
		}

		public GetStatTrackerValueHandler statTrackerCallback;
		protected int oldStatValue = -1;

		private static GameObject statTrackerPrefab;

		public void updateStatTracker(bool viewmodel)
		{
			if (statTrackerPrefab == null)
			{
				statTrackerPrefab = Assets.coreMasterBundle?.LoadAsset<GameObject>("Economy/Attachments/Stat_Tracker.prefab");
				if (statTrackerPrefab == null)
				{
					enabled = false;
					return;
				}
			}

			InstantiateParameters instantiateParameters = new InstantiateParameters()
			{
				parent = statTrackerHook,
				worldSpace = false,
			};
			GameObject statTrackerModel = Instantiate(statTrackerPrefab, Vector3.zero, Quaternion.identity, instantiateParameters);
			statTrackerText = statTrackerModel.GetComponentInChildren<TextMeshPro>();

			if (viewmodel)
			{
				Layerer.relayer(statTrackerModel.transform, LayerMasks.VIEWMODEL);
			}
		}

		protected void Update()
		{
			if (statTrackerCallback == null)
			{
				return;
			}

			EStatTrackerType type;
			int newStatValue;
			if (!statTrackerCallback(out type, out newStatValue))
			{
				return;
			}
			newStatValue %= 10000000; // Wrap around to zero at 9,999,999... some players have (artificially) passed 10M+ kills!

			if (oldStatValue == newStatValue)
			{
				return;
			}
			oldStatValue = newStatValue;

			statTrackerText.color = Provider.provider.economyService.getStatTrackerColor(type);
			statTrackerText.text = newStatValue.ToString("D7");
		}

		protected void Awake()
		{
			statTrackerHook = transform.Find("Stat_Tracker");
		}
	}
}
