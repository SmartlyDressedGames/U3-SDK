////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows foreach loop to iterate renderers defined in lod group.
	/// </summary>
	public struct LodGroupEnumerator : IEnumerator<Renderer>, IEnumerable<Renderer>
	{
		public LodGroupEnumerator(LODGroup lodGroup)
		{
			lods = lodGroup.GetLODs();
			lodIndex = 0;
			rendererIndex = -1;
		}

		public Renderer Current => lods[lodIndex].renderers[rendererIndex];

		object IEnumerator.Current => Current;

		public void Dispose()
		{

		}

		private bool MoveRendererIndex()
		{
			Renderer[] renderers = lods[lodIndex].renderers;
			if (renderers == null || renderers.Length < 1)
				return false;

			while (++rendererIndex < renderers.Length)
			{
				if (renderers[rendererIndex] == null)
				{
					// Skip null (bad import) renderers.
					continue;
				}

				return true;
			}

			return false;
		}

		public bool MoveNext()
		{
			if (lods == null || lods.Length < 1)
				return false;

			if (MoveRendererIndex())
			{
				return true;
			}

			while (++lodIndex < lods.Length)
			{
				rendererIndex = -1;
				if (MoveRendererIndex())
				{
					return true;
				}
				else
				{
					// Skip empty LOD.
					continue;
				}
			}

			return false;
		}

		public void Reset()
		{
			lodIndex = 0;
			rendererIndex = -1;
		}

		IEnumerator<Renderer> IEnumerable<Renderer>.GetEnumerator()
		{
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this;
		}

		private LOD[] lods;
		private int lodIndex;
		private int rendererIndex;
	}
}
