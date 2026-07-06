////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void MainCameraInstanceChangedHandler();
	public delegate void MainCameraAvailabilityChangedHandler();

	public class MainCamera : MonoBehaviour
	{
		protected static Camera _instance;
		public static Camera instance
		{
			get => _instance;
			protected set
			{
				if (instance != value)
				{
					_instance = value;

					triggerInstanceChanged();
				}
			}
		}
		private static Transform instanceTransform;
		/// <summary>
		/// May be out of date by one frame.
		/// </summary>
		private static Vector3 recentIshPosition;

		protected static bool _isAvailable;
		public static bool isAvailable
		{
			get => _isAvailable;
			protected set
			{
				if (isAvailable != value)
				{
					_isAvailable = value;

					triggerAvailabilityChanged();
				}
			}
		}

		public static Vector3 RenderingPosition
		{
			get
			{
				return _isPositionFrozen ? frozenPosition : instanceTransform.position;
			}
		}

		protected static bool _isPositionFrozen;
		protected static Vector3 frozenPosition;
		public static bool IsPositionFrozen
		{
			get => _isPositionFrozen;

			set
			{
				_isPositionFrozen = value;
				frozenPosition = instanceTransform.position;
			}
		}

		/// <summary>
		/// Currently used by vehicles to deactivate some rendering features when outside rendering distance.
		/// Uses "frozen" position if applicable, otherwise the camera position from the most recent Update. This means
		/// it could be out-of-date, but for LOD purposes it should be "good enough."
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static float SqrDistanceFromLodPosition(Vector3 position)
		{
			return _isPositionFrozen ? (frozenPosition - position).sqrMagnitude : (recentIshPosition - position).sqrMagnitude;
		}

		public static event MainCameraInstanceChangedHandler instanceChanged;
		public static event MainCameraAvailabilityChangedHandler availabilityChanged;

		public IEnumerator activate()
		{
			yield return new WaitForEndOfFrame();

			isAvailable = true;
		}

		protected static void triggerInstanceChanged()
		{
			instanceChanged?.Invoke();
		}

		protected static void triggerAvailabilityChanged()
		{
			availabilityChanged?.Invoke();
		}

		public void Awake()
		{
			isAvailable = false;

			instanceTransform = transform;
			instance = transform.GetComponent<Camera>();
			instance.eventMask = 0; // Responsible for OnMouseXYZ events that we do not use.
			_isPositionFrozen = false;

			StartCoroutine(activate());

			UnturnedPostProcess.instance.setBaseCamera(instance);
			gameObject.GetOrAddComponent<SDG.Framework.Rendering.GLRenderer>();
		}

		public void Update()
		{
			recentIshPosition = transform.position;
		}
	}
}
