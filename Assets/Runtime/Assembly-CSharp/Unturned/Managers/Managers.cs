////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Managers : MonoBehaviour
	{
		private static bool _isInitialized;
		public static bool isInitialized => _isInitialized;

#if REGIONDEBUG
		private void OnDrawGizmos()
		{
			if(LevelGround.terrain != null)
			{
				Gizmos.color = new Color(1, 1, 1, 0.5f);

				Vector3 point = new Vector3();
				Vector3 next;

				for(byte x = 0; x < Regions.WORLD_SIZE; x ++)
				{
					for(byte y = 0; y < Regions.WORLD_SIZE; y ++)
					{
						point.x = -4096 + x*Regions.REGION_SIZE;
						point.z = -4096 + y*Regions.REGION_SIZE;
						point.y = LevelGround.getHeight(point);

						next = point + Vector3.right*Regions.REGION_SIZE;
						next.y = LevelGround.getHeight(next);

						Gizmos.DrawLine(point, next);

						next = point + Vector3.forward*Regions.REGION_SIZE;
						next.y = LevelGround.getHeight(next);
						
						Gizmos.DrawLine(point, next);
					}
				}
			}
		}
#endif

		private void Awake()
		{
			if (isInitialized)
			{
				Destroy(gameObject);
				return;
			}

			_isInitialized = true;
			DontDestroyOnLoad(gameObject);

			GetComponent<SteamChannel>().setup();
		}
	}
}