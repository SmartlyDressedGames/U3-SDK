////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public static class DecalSystem
	{
		private static bool _isVisible;
		public static bool IsVisible
		{
			get => _isVisible;
			set
			{
				if (_isVisible != value)
				{
					_isVisible = value;
					ConvenientSavedata.get().write("Visibility_Decals", value);

					if (Level.isEditor)
					{
						foreach (Decal decal in decalsDiffuse)
						{
							decal.UpdateEditorVisibility();
						}
					}
				}
			}
		}

		private static HashSet<Decal> _decalsDiffuse = new HashSet<Decal>();
		public static HashSet<Decal> decalsDiffuse => _decalsDiffuse;

		public static void add(Decal decal)
		{
			if (decal == null)
			{
				return;
			}

			if (decal.material == null)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.LogWarningFormat(decal, "Decal {0} missing material", decal.transform.GetSceneHierarchyPath());
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
				return;
			}

			remove(decal);

			switch (decal.type)
			{
				case EDecalType.DIFFUSE:
					decalsDiffuse.Add(decal);
					break;
			}
		}

		public static void remove(Decal decal)
		{
			if (decal == null)
			{
				return;
			}

			switch (decal.type)
			{
				case EDecalType.DIFFUSE:
					decalsDiffuse.Remove(decal);
					break;
			}
		}

		static DecalSystem()
		{
			bool savedVisibility;
			if (ConvenientSavedata.get().read("Visibility_Decals", out savedVisibility))
			{
				_isVisible = savedVisibility;
			}
			else
			{
				_isVisible = true;
			}

			SDG.Framework.Utilities.TimeUtility.updated += OnUpdateGizmos;
		}

		private static void OnUpdateGizmos()
		{
			if (!_isVisible || !Level.isEditor)
				return;

			Camera cam = MainCamera.instance;
			if (cam == null)
				return;

			RuntimeGizmos runtimeGizmos = RuntimeGizmos.Get();

			float baseDecalDistance = 128 + (GraphicsSettings.normalizedDrawDistance * 128);

			foreach (Decal decal in decalsDiffuse)
			{
				if (decal.material == null)
				{
					continue;
				}

				float decalDistance = baseDecalDistance * decal.lodBias;
				float sqrDecalDistance = decalDistance * decalDistance;

				if ((decal.transform.position - cam.transform.position).sqrMagnitude > sqrDecalDistance)
				{
					continue;
				}

				Color decalColor = decal.isSelected ? Color.yellow : Color.red;
				runtimeGizmos.Box(decal.transform.localToWorldMatrix, Vector3.one, decalColor);
			}
		}
	}
}
