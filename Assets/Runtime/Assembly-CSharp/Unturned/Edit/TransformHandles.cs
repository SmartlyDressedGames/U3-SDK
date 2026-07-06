////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Merging the devkit, legacy, and housing transform handles into one place.
	/// </summary>
	public class TransformHandles
	{
		public enum EMode
		{
			/// <summary>
			/// Position and plane handles for each axis.
			/// </summary>
			Position,

			/// <summary>
			/// Disc handles for each axis.
			/// </summary>
			Rotation,

			/// <summary>
			/// Scale handles for each axis.
			/// </summary>
			Scale,

			/// <summary>
			/// Position handles on each side of box.
			/// </summary>
			PositionBounds,

			/// <summary>
			/// Scale handles on each side of box which both move and resize the box.
			/// </summary>
			ScaleBounds,
		}

		public delegate void PreTransformEventHandler(Matrix4x4 worldToPivot);
		/// <summary>
		/// Invoked when handle is clicked so that tool can save selection transform relative to pivot.
		/// This avoids floating point precision loss of applying delta for each Transformed event.
		/// </summary>
		public event PreTransformEventHandler OnPreTransform;

		public delegate void TranslatedAndRotatedEventHandler(Vector3 worldPositionDelta, Quaternion worldRotationDelta, Vector3 pivotPosition, bool modifyRotation);
		/// <summary>
		/// Invoked when handle is dragged and value actually changes.
		/// </summary>
		public event TranslatedAndRotatedEventHandler OnTranslatedAndRotated;

		public delegate void TransformedEventHandler(Matrix4x4 pivotToWorld);
		/// <summary>
		/// Invoked when handle is dragged and value actually changes.
		/// </summary>
		public event TransformedEventHandler OnTransformed;

		public bool wantsToSnap = false;
		public float snapPositionInterval = 1.0f;
		public float snapRotationIntervalDegrees = 15.0f;

		public Vector3 GetPivotPosition() { return pivotPosition; }
		public Quaternion GetPivotRotation() { return pivotRotation; }

		/// <summary>
		/// Preferred mode only takes effect while not dragging.
		/// Bounds modes fall back to non-bounds modes if bounds are not set.
		/// </summary>
		public void SetPreferredMode(EMode preferredMode)
		{
			this.preferredMode = preferredMode;
			SyncMode();
		}

		/// <summary>
		/// Pivot only takes effect while not dragging. This is to help ensure
		/// the caller does not depend on the internal pivot values.
		/// </summary>
		public void SetPreferredPivot(Vector3 position, Quaternion rotation)
		{
			preferredPivotPosition = position;
			preferredPivotRotation = rotation;
			SyncPivot();
		}

		/// <summary>
		/// Somewhat hacky, useful to make the "copy-paste transform" feature easier to implement.
		/// Invoke tranformed callback as if pivot were manually dragged to the new position and rotation.
		/// </summary>
		public void ExternallyTransformPivot(Vector3 position, Quaternion rotation, bool modifyRotation)
		{
			if (dragComponent != EComponent.NONE)
			{
				// Should not be getting called while dragging anyway.
				return;
			}

			Matrix4x4 oldPivotToWorld = Matrix4x4.TRS(pivotPosition, pivotRotation, Vector3.one);
			Matrix4x4 worldToOldPivot = oldPivotToWorld.inverse;
			OnPreTransform?.Invoke(worldToOldPivot);

			Vector3 positionDelta = position - pivotPosition;
			Quaternion rotationDelta = rotation * Quaternion.Inverse(pivotRotation);
			OnTranslatedAndRotated?.Invoke(positionDelta, rotationDelta, pivotPosition, modifyRotation);

			SetPreferredPivot(position, rotation);
		}

		/// <summary>
		/// Called before raycasting into the regular physics scene to give transform tool priority.
		/// </summary>
		public bool Raycast(Ray mouseRay)
		{
			hoverComponent = EComponent.NONE;

			UpdateViewProperties();

			if (mode == EMode.Position)
			{
				if (RaycastPositionPlane(mouseRay, pivotRotation * Vector3.up * viewAxisFlip.y, pivotRotation * Vector3.forward * viewAxisFlip.z, pivotRotation * Vector3.right * viewAxisFlip.x))
				{
					hoverComponent = EComponent.POSITION_PLANE_X;
				}
				else if (RaycastPositionPlane(mouseRay, pivotRotation * Vector3.right * viewAxisFlip.x, pivotRotation * Vector3.forward * viewAxisFlip.z, pivotRotation * Vector3.up * viewAxisFlip.y))
				{
					hoverComponent = EComponent.POSITION_PLANE_Y;
				}
				else if (RaycastPositionPlane(mouseRay, pivotRotation * Vector3.right * viewAxisFlip.x, pivotRotation * Vector3.up * viewAxisFlip.y, pivotRotation * Vector3.forward * viewAxisFlip.z))
				{
					hoverComponent = EComponent.POSITION_PLANE_Z;
				}
				else if (RaycastPositionAxis(mouseRay, pivotRotation * Vector3.right * viewAxisFlip.x))
				{
					hoverComponent = EComponent.POSITION_AXIS_X;
				}
				else if (RaycastPositionAxis(mouseRay, pivotRotation * Vector3.up * viewAxisFlip.y))
				{
					hoverComponent = EComponent.POSITION_AXIS_Y;
				}
				else if (RaycastPositionAxis(mouseRay, pivotRotation * Vector3.forward * viewAxisFlip.z))
				{
					hoverComponent = EComponent.POSITION_AXIS_Z;
				}
			}
			else if (mode == EMode.Rotation)
			{
				float nearestHitDistance = -1.0f;
				float hitDistance;
				bool hit = RaycastRotationPlane(mouseRay, pivotRotation * Vector3.right, out hitDistance);
				if (hit)
				{
					nearestHitDistance = hitDistance;
					hoverComponent = EComponent.ROTATION_X;
				}

				if (RaycastRotationPlane(mouseRay, pivotRotation * Vector3.up, out hitDistance) && (!hit || hitDistance < nearestHitDistance))
				{
					hit = true;
					nearestHitDistance = hitDistance;
					hoverComponent = EComponent.ROTATION_Y;
				}

				if (RaycastRotationPlane(mouseRay, pivotRotation * Vector3.forward, out hitDistance) && (!hit || hitDistance < nearestHitDistance))
				{
					hit = true;
					nearestHitDistance = hitDistance;
					hoverComponent = EComponent.ROTATION_Z;
				}
			}
			else if (mode == EMode.Scale)
			{
				if (RaycastSphere(mouseRay))
				{
					hoverComponent = EComponent.SCALE_UNIFORM;
				}
				else if (RaycastPositionAxis(mouseRay, pivotRotation * Vector3.right * viewAxisFlip.x))
				{
					hoverComponent = EComponent.SCALE_AXIS_X;
				}
				else if (RaycastPositionAxis(mouseRay, pivotRotation * Vector3.up * viewAxisFlip.y))
				{
					hoverComponent = EComponent.SCALE_AXIS_Y;
				}
				else if (RaycastPositionAxis(mouseRay, pivotRotation * Vector3.forward * viewAxisFlip.z))
				{
					hoverComponent = EComponent.SCALE_AXIS_Z;
				}
			}
			else if (mode == EMode.PositionBounds)
			{
				Vector3 boundsMin = pivotBounds.min;
				Vector3 boundsMax = pivotBounds.max;
				if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * -Vector3.right, -boundsMin.x))
				{
					hoverComponent = EComponent.POSITION_BOUNDS_NEGATIVE_X;
				}
				else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * -Vector3.up, -boundsMin.y))
				{
					hoverComponent = EComponent.POSITION_BOUNDS_NEGATIVE_Y;
				}
				else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * -Vector3.forward, -boundsMin.z))
				{
					hoverComponent = EComponent.POSITION_BOUNDS_NEGATIVE_Z;
				}
				else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * Vector3.right, boundsMax.x))
				{
					hoverComponent = EComponent.POSITION_BOUNDS_POSITIVE_X;
				}
				else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * Vector3.up, boundsMax.y))
				{
					hoverComponent = EComponent.POSITION_BOUNDS_POSITIVE_Y;
				}
				else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * Vector3.forward, boundsMax.z))
				{
					hoverComponent = EComponent.POSITION_BOUNDS_POSITIVE_Z;
				}
			}
			else if (mode == EMode.ScaleBounds)
			{
				Vector3 boundsMin = pivotBounds.min;
				Vector3 boundsMax = pivotBounds.max;
				if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * -Vector3.right, -boundsMin.x))
				{
					hoverComponent = EComponent.SCALE_BOUNDS_NEGATIVE_X;
				}
				else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * -Vector3.up, -boundsMin.y))
				{
					hoverComponent = EComponent.SCALE_BOUNDS_NEGATIVE_Y;
				}
				else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * -Vector3.forward, -boundsMin.z))
				{
					hoverComponent = EComponent.SCALE_BOUNDS_NEGATIVE_Z;
				}
				else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * Vector3.right, boundsMax.x))
				{
					hoverComponent = EComponent.SCALE_BOUNDS_POSITIVE_X;
				}
				else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * Vector3.up, boundsMax.y))
				{
					hoverComponent = EComponent.SCALE_BOUNDS_POSITIVE_Y;
				}
				else if (RaycastPositionBoundsAxis(mouseRay, pivotRotation * Vector3.forward, boundsMax.z))
				{
					hoverComponent = EComponent.SCALE_BOUNDS_POSITIVE_Z;
				}
			}

			return hoverComponent != EComponent.NONE;
		}

		public void MouseDown(Ray mouseRay)
		{
			dragComponent = hoverComponent;
			dragPreviousPosition = pivotPosition;
			dragPreviousRotation = pivotRotation;
			dragPreviousAngle = 0.0f;
			dragPreviousScale = Vector3.one;

			if (dragComponent.HasFlag(EComponent.POSITION_AXIS))
			{
				dragAxisOrigin = pivotPosition;
				if (dragComponent.HasFlag(EComponent.X))
				{
					dragAxisDirection = pivotRotation * Vector3.right * viewAxisFlip.x;
				}
				else if (dragComponent.HasFlag(EComponent.Y))
				{
					dragAxisDirection = pivotRotation * Vector3.up * viewAxisFlip.y;
				}
				else
				{
					dragAxisDirection = pivotRotation * Vector3.forward * viewAxisFlip.z;
				}
				dragAxisInitialDistance = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragAxisOrigin, dragAxisDirection);
			}
			else if (dragComponent.HasFlag(EComponent.POSITION_PLANE))
			{
				dragPlaneOrigin = pivotPosition;
				if (dragComponent.HasFlag(EComponent.X))
				{
					dragPlaneAxis0 = pivotRotation * Vector3.up * viewAxisFlip.y;
					dragPlaneAxis1 = pivotRotation * Vector3.forward * viewAxisFlip.z;
					dragPlaneNormal = pivotRotation * Vector3.right * viewAxisFlip.x;
				}
				else if (dragComponent.HasFlag(EComponent.Y))
				{
					dragPlaneAxis0 = pivotRotation * Vector3.right * viewAxisFlip.x;
					dragPlaneAxis1 = pivotRotation * Vector3.forward * viewAxisFlip.z;
					dragPlaneNormal = pivotRotation * Vector3.up * viewAxisFlip.y;
				}
				else
				{
					dragPlaneAxis0 = pivotRotation * Vector3.right * viewAxisFlip.x;
					dragPlaneAxis1 = pivotRotation * Vector3.up * viewAxisFlip.y;
					dragPlaneNormal = pivotRotation * Vector3.forward * viewAxisFlip.z;
				}

				Plane plane = new Plane(dragPlaneNormal, dragPlaneOrigin);
				float hitDistance;
				if (plane.Raycast(mouseRay, out hitDistance))
				{
					Vector3 hitPosition = mouseRay.origin + (mouseRay.direction * hitDistance);
					Vector3 hitOffset = hitPosition - dragPlaneOrigin;
					dragPlaneInitialDistance0 = Vector3.Dot(dragPlaneAxis0, hitOffset);
					dragPlaneInitialDistance1 = Vector3.Dot(dragPlaneAxis1, hitOffset);
				}
			}
			else if (dragComponent.HasFlag(EComponent.ROTATION))
			{
				dragRotationOrigin = pivotRotation;
				if (dragComponent.HasFlag(EComponent.X))
				{
					dragRotationAxis = pivotRotation * Vector3.right * viewAxisFlip.x;
				}
				else if (dragComponent.HasFlag(EComponent.Y))
				{
					dragRotationAxis = pivotRotation * Vector3.up * viewAxisFlip.y;
				}
				else
				{
					dragRotationAxis = pivotRotation * Vector3.forward * viewAxisFlip.z;
				}

				Plane plane = new Plane(dragRotationAxis, pivotPosition);
				float hitDistance;
				if (plane.Raycast(mouseRay, out hitDistance))
				{
					Vector3 hitPosition = mouseRay.origin + (mouseRay.direction * hitDistance);
					Vector3 hitOffset = hitPosition - pivotPosition;
					dragRotationOutwardDirection = hitOffset.normalized;
					dragRotationEdgePoint = hitPosition;
					dragRotationTangent = Vector3.Cross(dragRotationAxis, dragRotationOutwardDirection).normalized;
				}
			}
			else if (dragComponent.HasFlag(EComponent.SCALE))
			{
				dragScaleOrigin = pivotPosition;

				if (dragComponent == EComponent.SCALE_UNIFORM)
				{
					Plane plane = new Plane(-cameraForward, dragScaleOrigin);
					float hitDistance;
					if (plane.Raycast(mouseRay, out hitDistance))
					{
						Vector3 hitPosition = mouseRay.origin + (mouseRay.direction * hitDistance);
						dragScaleLocalDirection = Vector3.one;
						dragScaleWorldDirection = (hitPosition - dragScaleOrigin).normalized;
					}
				}
				else
				{
					if (dragComponent.HasFlag(EComponent.X))
					{
						dragScaleLocalDirection = Vector3.right;
						dragScaleWorldDirection = pivotRotation * dragScaleLocalDirection * viewAxisFlip.x;
					}
					else if (dragComponent.HasFlag(EComponent.Y))
					{
						dragScaleLocalDirection = Vector3.up;
						dragScaleWorldDirection = pivotRotation * dragScaleLocalDirection * viewAxisFlip.y;
					}
					else
					{
						dragScaleLocalDirection = Vector3.forward;
						dragScaleWorldDirection = pivotRotation * dragScaleLocalDirection * viewAxisFlip.z;
					}
				}

				dragScaleInitialDistance = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragScaleOrigin, dragScaleWorldDirection);
			}
			else if (dragComponent.HasFlag(EComponent.POSITION_BOUNDS))
			{
				dragAxisOrigin = pivotPosition;
				if (dragComponent.HasFlag(EComponent.X))
				{
					dragAxisDirection = pivotRotation * Vector3.right;
				}
				else if (dragComponent.HasFlag(EComponent.Y))
				{
					dragAxisDirection = pivotRotation * Vector3.up;
				}
				else
				{
					dragAxisDirection = pivotRotation * Vector3.forward;
				}
				if (dragComponent.HasFlag(EComponent.NEGATIVE))
				{
					dragAxisDirection *= -1.0f;
				}
				dragAxisInitialDistance = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragAxisOrigin, dragAxisDirection);
			}
			else if (dragComponent.HasFlag(EComponent.SCALE_BOUNDS))
			{
				dragScaleOrigin = pivotPosition;
				if (dragComponent.HasFlag(EComponent.X))
				{
					dragScaleLocalDirection = Vector3.right;
					dragScaleWorldDirection = pivotRotation * dragScaleLocalDirection;
					dragScaleBounds = pivotBounds.size.x;
				}
				else if (dragComponent.HasFlag(EComponent.Y))
				{
					dragScaleLocalDirection = Vector3.up;
					dragScaleWorldDirection = pivotRotation * dragScaleLocalDirection;
					dragScaleBounds = pivotBounds.size.y;
				}
				else
				{
					dragScaleLocalDirection = Vector3.forward;
					dragScaleWorldDirection = pivotRotation * dragScaleLocalDirection;
					dragScaleBounds = pivotBounds.size.z;
				}

				dragScaleBoundsCenter = pivotPosition + (pivotRotation * pivotBounds.center);
				dragPreviousPosition = dragScaleBoundsCenter;
				dragScaleBoundsSize = pivotBounds.size;

				if (dragComponent.HasFlag(EComponent.NEGATIVE))
				{
					dragScaleWorldDirection *= -1.0f;
				}
				dragScaleInitialDistance = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragScaleOrigin, dragScaleWorldDirection);

				// For snap axis drawing.
				dragAxisOrigin = dragScaleOrigin;
				dragAxisDirection = dragScaleWorldDirection;
				dragAxisInitialDistance = dragScaleInitialDistance;
			}

			Matrix4x4 oldPivotToWorld = Matrix4x4.TRS(dragPreviousPosition, dragPreviousRotation, dragPreviousScale);
			Matrix4x4 worldToOldPivot = oldPivotToWorld.inverse;
			OnPreTransform?.Invoke(worldToOldPivot);
		}

		public void MouseMove(Ray mouseRay)
		{
			if (dragComponent.HasFlag(EComponent.POSITION_AXIS) || dragComponent.HasFlag(EComponent.POSITION_BOUNDS))
			{
				float dragAxisDistance = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragAxisOrigin, dragAxisDirection) - dragAxisInitialDistance;

				if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
				{
					dragAxisDistance = Mathf.RoundToInt(dragAxisDistance / snapPositionInterval) * snapPositionInterval;
				}

				Vector3 positionDelta = dragAxisDirection * dragAxisDistance;
				Vector3 newPosition = dragAxisOrigin + positionDelta;
				if ((newPosition - dragPreviousPosition).magnitude > Mathf.Epsilon)
				{
					pivotPosition = newPosition;
					OnTranslatedAndRotated?.Invoke(positionDelta, Quaternion.identity, pivotPosition, /*modifyRotation*/ false);
					dragPreviousPosition = newPosition;
				}
			}
			else if (dragComponent.HasFlag(EComponent.POSITION_PLANE))
			{
				Plane plane = new Plane(dragPlaneNormal, dragPlaneOrigin);
				float hitDistance;
				if (plane.Raycast(mouseRay, out hitDistance))
				{
					Vector3 hitPosition = mouseRay.origin + (mouseRay.direction * hitDistance);
					Vector3 hitOffset = hitPosition - dragPlaneOrigin;
					float distanceAlongAxis0 = Vector3.Dot(dragPlaneAxis0, hitOffset) - dragPlaneInitialDistance0;
					float distanceAlongAxis1 = Vector3.Dot(dragPlaneAxis1, hitOffset) - dragPlaneInitialDistance1;

					if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
					{
						distanceAlongAxis0 = Mathf.RoundToInt(distanceAlongAxis0 / snapPositionInterval) * snapPositionInterval;
						distanceAlongAxis1 = Mathf.RoundToInt(distanceAlongAxis1 / snapPositionInterval) * snapPositionInterval;
					}

					Vector3 positionDelta = (dragPlaneAxis0 * distanceAlongAxis0) + (dragPlaneAxis1 * distanceAlongAxis1);
					Vector3 newPosition = dragPlaneOrigin + positionDelta;
					if ((newPosition - dragPreviousPosition).magnitude > Mathf.Epsilon)
					{
						pivotPosition = newPosition;
						OnTranslatedAndRotated?.Invoke(positionDelta, Quaternion.identity, pivotPosition, /*modifyRotation*/ false);
						dragPreviousPosition = newPosition;
					}
				}
			}
			else if (dragComponent.HasFlag(EComponent.ROTATION))
			{
				float dragAxisDistance = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragRotationEdgePoint, dragRotationTangent);
				float newAngle = dragAxisDistance * 90.0f / viewScale;

				if (wantsToSnap && snapRotationIntervalDegrees > Mathf.Epsilon)
				{
					newAngle = Mathf.RoundToInt(newAngle / snapRotationIntervalDegrees) * snapRotationIntervalDegrees;
				}

				if (Mathf.Abs(newAngle - dragPreviousAngle) > Mathf.Epsilon)
				{
					Quaternion rotationDelta = Quaternion.AngleAxis(newAngle, dragRotationAxis);
					Quaternion newRotation = rotationDelta * dragRotationOrigin;

					pivotRotation = newRotation;
					OnTranslatedAndRotated?.Invoke(Vector3.zero, rotationDelta, pivotPosition, /*modifyRotation*/ true);
					dragPreviousAngle = newAngle;
					dragPreviousRotation = newRotation;
				}
			}
			else if (dragComponent.HasFlag(EComponent.SCALE))
			{
				float dragAxisDistance = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragScaleOrigin, dragScaleWorldDirection) - dragScaleInitialDistance;
				dragAxisDistance /= viewScale;

				if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
				{
					// Does not get its own snap setting because pivot scaling mainly needs to snap for perfectly mirroring.
					dragAxisDistance = Mathf.RoundToInt(dragAxisDistance / snapPositionInterval) * snapPositionInterval;
				}

				// Prevent setting scale along axis to zero because it will break subsequent scaling.
				if (!MathfEx.IsNearlyEqual(dragAxisDistance, -1.0f, tolerance: Mathf.Epsilon))
				{
					Vector3 newScale = Vector3.one + (dragScaleLocalDirection * dragAxisDistance);
					if ((newScale - dragPreviousScale).magnitude > Mathf.Epsilon)
					{
						Matrix4x4 newPivotToWorld = Matrix4x4.TRS(dragPreviousPosition, dragPreviousRotation, newScale);

						OnTransformed?.Invoke(newPivotToWorld);
						dragPreviousScale = newScale;
					}
				}
			}
			else if (dragComponent.HasFlag(EComponent.SCALE_BOUNDS))
			{
				float dragAxisDistance = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragScaleOrigin, dragScaleWorldDirection) - dragScaleInitialDistance;

				if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
				{
					dragAxisDistance = Mathf.RoundToInt(dragAxisDistance / snapPositionInterval) * snapPositionInterval;
				}

				Vector3 newPosition = dragScaleBoundsCenter + (dragScaleWorldDirection * dragAxisDistance * 0.5f);
				Vector3 newScale = Vector3.one + (dragScaleLocalDirection * (dragAxisDistance / dragScaleBounds));
				if ((newPosition - dragPreviousPosition).magnitude > Mathf.Epsilon && (newScale - dragPreviousScale).magnitude > Mathf.Epsilon)
				{
					pivotPosition = dragScaleOrigin + (dragScaleWorldDirection * dragAxisDistance * 0.5f);
					pivotBounds.size = dragScaleBoundsSize + (dragScaleLocalDirection * dragAxisDistance);

					Matrix4x4 newPivotToWorld = Matrix4x4.TRS(newPosition, dragPreviousRotation, newScale);

					OnTransformed?.Invoke(newPivotToWorld);
					dragPreviousPosition = newPosition;
					dragPreviousScale = newScale;
				}
			}
		}

		public void MouseUp()
		{
			dragComponent = EComponent.NONE;
			wantsToSnap = false;
			SyncMode();
			SyncPivot();
		}

		public void Render(Ray mouseRay)
		{
			UpdateViewProperties();

			if ((mode == EMode.PositionBounds || mode == EMode.ScaleBounds) && hasPivotBounds)
			{
				Color boundsColor = Color.yellow;
				boundsColor.a = 0.25f;
				RuntimeGizmos.Get().Box(Matrix4x4.TRS(pivotPosition, pivotRotation, Vector3.one), pivotBounds.center, pivotBounds.size, boundsColor, layer: EGizmoLayer.Foreground);
			}

			if (mode == EMode.Position)
			{
				if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
				{
					if (dragComponent.HasFlag(EComponent.POSITION_AXIS))
					{
						DrawPositionAxisSnap(mouseRay);
					}
					else if (dragComponent.HasFlag(EComponent.POSITION_PLANE))
					{
						DrawPositionPlaneSnap(mouseRay);
					}
				}

				DrawPositionPlane(pivotRotation * Vector3.up * viewAxisFlip.y, pivotRotation * Vector3.forward * viewAxisFlip.z, Color.red, EComponent.POSITION_PLANE_X);
				DrawPositionPlane(pivotRotation * Vector3.right * viewAxisFlip.x, pivotRotation * Vector3.forward * viewAxisFlip.z, Color.green, EComponent.POSITION_PLANE_Y);
				DrawPositionPlane(pivotRotation * Vector3.right * viewAxisFlip.x, pivotRotation * Vector3.up * viewAxisFlip.y, Color.blue, EComponent.POSITION_PLANE_Z);

				DrawPositionAxis(pivotRotation * Vector3.right * viewAxisFlip.x, Color.red, EComponent.POSITION_AXIS_X);
				DrawPositionAxis(pivotRotation * Vector3.up * viewAxisFlip.y, Color.green, EComponent.POSITION_AXIS_Y);
				DrawPositionAxis(pivotRotation * Vector3.forward * viewAxisFlip.z, Color.blue, EComponent.POSITION_AXIS_Z);
			}
			else if (mode == EMode.Rotation)
			{
				if (dragComponent == EComponent.NONE)
				{
					DrawRotationCircle(pivotRotation * Vector3.up, pivotRotation * Vector3.forward, EComponent.ROTATION_X, Color.red);
					DrawRotationCircle(pivotRotation * Vector3.right, pivotRotation * Vector3.forward, EComponent.ROTATION_Y, Color.green);
					DrawRotationCircle(pivotRotation * Vector3.right, pivotRotation * Vector3.up, EComponent.ROTATION_Z, Color.blue);
				}
				else
				{
					DrawDragCircle();
				}
			}
			else if (mode == EMode.Scale)
			{
				Color color = dragComponent == EComponent.SCALE_UNIFORM ? Color.white : (hoverComponent == EComponent.SCALE_UNIFORM ? Color.yellow : Color.gray);
				RuntimeGizmos.Get().Circle(pivotPosition, viewRight, viewUp, 0.25f * viewScale, color, resolution: 16, layer: EGizmoLayer.Foreground);

				DrawScaleAxis(pivotRotation * Vector3.right * viewAxisFlip.x, Color.red, EComponent.SCALE_AXIS_X);
				DrawScaleAxis(pivotRotation * Vector3.up * viewAxisFlip.y, Color.green, EComponent.SCALE_AXIS_Y);
				DrawScaleAxis(pivotRotation * Vector3.forward * viewAxisFlip.z, Color.blue, EComponent.SCALE_AXIS_Z);
			}
			else if (mode == EMode.PositionBounds)
			{
				if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
				{
					DrawPositionAxisSnap(mouseRay);
				}

				Vector3 boundsMin = pivotBounds.min;
				Vector3 boundsMax = pivotBounds.max;
				DrawPositionBoundsAxis(pivotRotation * -Vector3.right, -boundsMin.x, Color.red, EComponent.POSITION_BOUNDS_NEGATIVE_X);
				DrawPositionBoundsAxis(pivotRotation * -Vector3.up, -boundsMin.y, Color.green, EComponent.POSITION_BOUNDS_NEGATIVE_Y);
				DrawPositionBoundsAxis(pivotRotation * -Vector3.forward, -boundsMin.z, Color.blue, EComponent.POSITION_BOUNDS_NEGATIVE_Z);
				DrawPositionBoundsAxis(pivotRotation * Vector3.right, boundsMax.x, Color.red, EComponent.POSITION_BOUNDS_POSITIVE_X);
				DrawPositionBoundsAxis(pivotRotation * Vector3.up, boundsMax.y, Color.green, EComponent.POSITION_BOUNDS_POSITIVE_Y);
				DrawPositionBoundsAxis(pivotRotation * Vector3.forward, boundsMax.z, Color.blue, EComponent.POSITION_BOUNDS_POSITIVE_Z);
			}
			else if (mode == EMode.ScaleBounds)
			{
				if (wantsToSnap && snapPositionInterval > Mathf.Epsilon)
				{
					DrawPositionAxisSnap(mouseRay);
				}

				Vector3 boundsMin = pivotBounds.min;
				Vector3 boundsMax = pivotBounds.max;
				DrawScaleBoundsAxis(pivotRotation * -Vector3.right, -boundsMin.x, Color.red, EComponent.SCALE_BOUNDS_NEGATIVE_X);
				DrawScaleBoundsAxis(pivotRotation * -Vector3.up, -boundsMin.y, Color.green, EComponent.SCALE_BOUNDS_NEGATIVE_Y);
				DrawScaleBoundsAxis(pivotRotation * -Vector3.forward, -boundsMin.z, Color.blue, EComponent.SCALE_BOUNDS_NEGATIVE_Z);
				DrawScaleBoundsAxis(pivotRotation * Vector3.right, boundsMax.x, Color.red, EComponent.SCALE_BOUNDS_POSITIVE_X);
				DrawScaleBoundsAxis(pivotRotation * Vector3.up, boundsMax.y, Color.green, EComponent.SCALE_BOUNDS_POSITIVE_Y);
				DrawScaleBoundsAxis(pivotRotation * Vector3.forward, boundsMax.z, Color.blue, EComponent.SCALE_BOUNDS_POSITIVE_Z);
			}
		}

		private static List<Component> workingComponentList = new List<Component>();
		public void UpdateBoundsFromSelection(IEnumerable<GameObject> selection)
		{
			if (dragComponent != EComponent.NONE)
			{
				// Cannot change mode while dragging.
				return;
			}

			pivotBounds = default;
			hasPivotBounds = false;

			Matrix4x4 pivotToWorld = Matrix4x4.TRS(pivotPosition, pivotRotation, Vector3.one);
			Matrix4x4 worldToPivot = pivotToWorld.inverse;

			void EncapsuleBounds(Transform transform, Vector3 center, Vector3 extents)
			{
				pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(-extents.x, -extents.y, -extents.z))));
				pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(-extents.x, -extents.y, extents.z))));
				pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(-extents.x, extents.y, -extents.z))));
				pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(-extents.x, extents.y, extents.z))));
				pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(extents.x, -extents.y, -extents.z))));
				pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(extents.x, -extents.y, extents.z))));
				pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(extents.x, extents.y, -extents.z))));
				pivotBounds.Encapsulate(worldToPivot.MultiplyPoint3x4(transform.TransformPoint(center + new Vector3(extents.x, extents.y, extents.z))));
			}

			foreach (GameObject item in selection)
			{
				item.GetComponentsInChildren(workingComponentList);
				foreach (Component component in workingComponentList)
				{
					if (component is MeshFilter meshFilter)
					{
						if (meshFilter.sharedMesh != null)
						{
							Bounds localBounds = meshFilter.sharedMesh.bounds;
							EncapsuleBounds(component.transform, localBounds.center, localBounds.extents);
							hasPivotBounds = true;
						}
					}
					else if (component is BoxCollider boxCollider)
					{
						EncapsuleBounds(component.transform, boxCollider.center, boxCollider.size * 0.5f);
						hasPivotBounds = true;
					}
					else if (component is SphereCollider sphereCollider)
					{
						// Todo: this is technically not correct because SphereCollider radius uses max scale, not per-axis scale.
						float radius = sphereCollider.radius;
						Vector3 extents = new Vector3(radius, radius, radius);
						EncapsuleBounds(component.transform, sphereCollider.center, extents);
						hasPivotBounds = true;
					}
				}

				workingComponentList.Clear();
			}

			SyncMode();
		}

		private bool RaycastPositionAxis(Ray mouseRay, Vector3 axisDirection)
		{
			float distanceAlongAxis = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, pivotPosition, axisDirection);
			float distanceFromAxis = MathfEx.DistanceBetweenRays(mouseRay.origin, mouseRay.direction, pivotPosition, axisDirection);
			return distanceAlongAxis > 0.0f && distanceAlongAxis < viewScale && distanceFromAxis < viewScale * 0.1f;
		}

		private bool RaycastPositionPlane(Ray mouseRay, Vector3 axis0, Vector3 axis1, Vector3 planeNormal)
		{
			Plane plane = new Plane(planeNormal, pivotPosition);
			float hitDistance;
			if (!plane.Raycast(mouseRay, out hitDistance))
			{
				return false;
			}

			Vector3 hitPosition = mouseRay.origin + (mouseRay.direction * hitDistance);
			Vector3 hitOffset = hitPosition - pivotPosition;
			float distanceAlongAxis0 = Vector3.Dot(axis0, hitOffset);
			float distanceAlongAxis1 = Vector3.Dot(axis1, hitOffset);
			// Scaled down to 25% so that the plane is a small square rather than the size of the full arrows.
			return distanceAlongAxis0 > 0.0f && distanceAlongAxis0 < viewScale * 0.25f && distanceAlongAxis1 > 0.0f && distanceAlongAxis1 < viewScale * 0.25f;
		}

		private bool RaycastRotationPlane(Ray mouseRay, Vector3 planeNormal, out float hitDistance)
		{
			Plane plane = new Plane(planeNormal, pivotPosition);
			if (!plane.Raycast(mouseRay, out hitDistance))
			{
				return false;
			}

			Vector3 hitPosition = mouseRay.origin + (mouseRay.direction * hitDistance);
			Vector3 hitOffset = hitPosition - pivotPosition;
			float minSqrDistance = MathfEx.Square(viewScale * 0.9f);
			float maxSqrDistance = MathfEx.Square(viewScale * 1.1f);
			float sqrDistance = hitOffset.sqrMagnitude;
			return sqrDistance > minSqrDistance && sqrDistance < maxSqrDistance;
		}

		private bool RaycastPositionBoundsAxis(Ray mouseRay, Vector3 axisDirection, float offset)
		{
			Vector3 arrowBase = pivotPosition + (axisDirection * offset);
			float distanceAlongAxis = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, arrowBase, axisDirection);
			float distanceFromAxis = MathfEx.DistanceBetweenRays(mouseRay.origin, mouseRay.direction, arrowBase, axisDirection);
			return distanceAlongAxis > 0.0f && distanceAlongAxis < viewScale && distanceFromAxis < viewScale * 0.1f;
		}

		private bool RaycastSphere(Ray mouseRay)
		{
			Vector3 sphereCenter = pivotPosition;
			float sphereRadius = viewScale * 0.25f;
			float sqrSphereRadius = sphereRadius * sphereRadius;

			Vector3 sphereRelativeToRay = sphereCenter - mouseRay.origin;
			float sphereCenterDistanceAlongRay = Vector3.Dot(sphereRelativeToRay, mouseRay.direction);
			if (sphereCenterDistanceAlongRay < 0.0f)
			{
				// Sphere is behind ray.
				return false;
			}

			Vector3 projectedCenter = mouseRay.origin + (mouseRay.direction * sphereCenterDistanceAlongRay);
			float sqrSphereCenterDistanceFromRay = (sphereCenter - projectedCenter).sqrMagnitude;
			return sqrSphereCenterDistanceFromRay <= sqrSphereRadius;
		}

		private void DrawPositionAxisSnap(Ray mouseRay)
		{
			Color color = new Color(0, 0, 0, 0.5f);

			const int notchCount = 10;

			Vector3 cross = Vector3.Cross(dragAxisDirection, viewDirection).normalized;
			Vector3 wingtipOffset = cross * 0.1f * viewScale;

			float mouseDistance = MathfEx.ProjectRayOntoRay(mouseRay.origin, mouseRay.direction, dragAxisOrigin, dragAxisDirection) - dragAxisInitialDistance;
			float snapDistance = (Mathf.RoundToInt(mouseDistance / snapPositionInterval) * snapPositionInterval) + dragAxisInitialDistance;
			Vector3 snapCenter = dragAxisOrigin + (dragAxisDirection * snapDistance);

			for (int index = -notchCount; index <= notchCount; ++index)
			{
				Vector3 lineCenter = snapCenter + (dragAxisDirection * snapPositionInterval * index);
				RuntimeGizmos.Get().Line(lineCenter - wingtipOffset, lineCenter + wingtipOffset, color, layer: EGizmoLayer.Foreground);
			}
		}

		private void DrawPositionPlaneSnap(Ray mouseRay)
		{
			Plane plane = new Plane(dragPlaneNormal, dragPlaneOrigin);
			float hitDistance;
			if (!plane.Raycast(mouseRay, out hitDistance))
				return;

			Vector3 hitPosition = mouseRay.origin + (mouseRay.direction * hitDistance);
			Vector3 hitOffset = hitPosition - dragPlaneOrigin;
			float distanceAlongAxis0 = Vector3.Dot(dragPlaneAxis0, hitOffset) - dragPlaneInitialDistance0;
			float snapDistance0 = (Mathf.RoundToInt(distanceAlongAxis0 / snapPositionInterval) * snapPositionInterval) + dragPlaneInitialDistance0;
			float distanceAlongAxis1 = Vector3.Dot(dragPlaneAxis1, hitOffset) - dragPlaneInitialDistance1;
			float snapDistance1 = (Mathf.RoundToInt(distanceAlongAxis1 / snapPositionInterval) * snapPositionInterval) + dragPlaneInitialDistance1;
			Vector3 snapCenter = dragPlaneOrigin + (dragPlaneAxis0 * snapDistance0) + (dragPlaneAxis1 * snapDistance1);

			Color color = new Color(0, 0, 0, 0.5f);

			const int notchCount = 10;

			// Offset from center to grid edge on each axis.
			Vector3 edgeOffset0 = dragPlaneAxis0 * snapPositionInterval * notchCount;
			Vector3 edgeOffset1 = dragPlaneAxis1 * snapPositionInterval * notchCount;

			for (int index0 = -notchCount; index0 <= notchCount; ++index0)
			{
				Vector3 lineCenter = snapCenter + (dragPlaneAxis0 * snapPositionInterval * index0);
				RuntimeGizmos.Get().Line(lineCenter - edgeOffset1, lineCenter + edgeOffset1, color, layer: EGizmoLayer.Foreground);
			}
			for (int index1 = -notchCount; index1 <= notchCount; ++index1)
			{
				Vector3 lineCenter = snapCenter + (dragPlaneAxis1 * snapPositionInterval * index1);
				RuntimeGizmos.Get().Line(lineCenter - edgeOffset0, lineCenter + edgeOffset0, color, layer: EGizmoLayer.Foreground);
			}
		}

		private void DrawPositionAxis(Vector3 direction, Color color, EComponent component)
		{
			Color axisColor = dragComponent == component ? Color.white : (hoverComponent == component ? Color.yellow : color);
			RuntimeGizmos.Get().Arrow(pivotPosition, direction, viewScale, axisColor, layer: EGizmoLayer.Foreground);
		}

		private void DrawPositionPlane(Vector3 axis0, Vector3 axis1, Color color, EComponent component)
		{
			Color planeColor = dragComponent == component ? Color.white : (hoverComponent == component ? Color.yellow : color);

			// Scaled down to 25% so that the plane is a small square rather than the size of the full arrows.
			Vector3 offset0 = axis0 * 0.25f * viewScale;
			Vector3 offset1 = axis1 * 0.25f * viewScale;
			Vector3 corner = pivotPosition + offset0 + offset1;
			RuntimeGizmos.Get().Line(pivotPosition + offset0, corner, planeColor, layer: EGizmoLayer.Foreground);
			RuntimeGizmos.Get().Line(pivotPosition + offset1, corner, planeColor, layer: EGizmoLayer.Foreground);
		}

		private void DrawRotationCircle(Vector3 axis0, Vector3 axis1, EComponent component, Color color)
		{
			Color circleColor = hoverComponent == component ? Color.yellow : color;
			RuntimeGizmos.Get().Circle(pivotPosition, axis0, axis1, viewScale, circleColor, resolution: 32, layer: EGizmoLayer.Foreground);
		}

		private void DrawDragCircle()
		{
			if (wantsToSnap)
			{
				Color snapColor = new Color(0, 0, 0, 0.5f);
				float angleOffset = Mathf.Deg2Rad * dragPreviousAngle;
				float step = Mathf.Deg2Rad * snapRotationIntervalDegrees;
				int count = Mathf.Max(1, Mathf.CeilToInt(Mathf.PI / 2 / step));
				for (int index = -count; index <= count; index++)
				{
					if (index == 0)
						continue;

					float rot = angleOffset + (index * step);
					float cos = Mathf.Cos(rot);
					float sin = Mathf.Sin(rot);
					Vector3 notchDirection = (dragRotationOutwardDirection * cos) + (dragRotationTangent * sin);
					Vector3 innerPosition = pivotPosition + (notchDirection * viewScale * 0.9f);
					Vector3 outerPosition = pivotPosition + (notchDirection * viewScale * 1.1f);
					RuntimeGizmos.Get().Line(innerPosition, outerPosition, snapColor, layer: EGizmoLayer.Foreground);
				}
			}

			Color color = Color.white;

			RuntimeGizmos.Get().Circle(pivotPosition, dragRotationOutwardDirection, dragRotationTangent, viewScale, color, resolution: 32, layer: EGizmoLayer.Foreground);

			float currentAngleRadians = Mathf.Deg2Rad * dragPreviousAngle;
			float currentCos = Mathf.Cos(currentAngleRadians);
			float currentSin = Mathf.Sin(currentAngleRadians);
			Vector3 currentAngleDirection = (dragRotationOutwardDirection * currentCos) + (dragRotationTangent * currentSin);
			RuntimeGizmos.Get().Line(pivotPosition, pivotPosition + (currentAngleDirection * viewScale * 1.1f), color, layer: EGizmoLayer.Foreground);

			color.a = 0.5f;
			Vector3 edgePoint = pivotPosition + (dragRotationOutwardDirection * viewScale);
			Vector3 tangentOffset = dragRotationTangent * 0.5f * viewScale;
			RuntimeGizmos.Get().Line(pivotPosition, edgePoint, color, layer: EGizmoLayer.Foreground);
			RuntimeGizmos.Get().Line(edgePoint, edgePoint - tangentOffset, color, layer: EGizmoLayer.Foreground);
			RuntimeGizmos.Get().Line(edgePoint, edgePoint + tangentOffset, color, layer: EGizmoLayer.Foreground);
		}

		private void DrawScaleAxis(Vector3 direction, Color color, EComponent component)
		{
			Vector3 cross = Vector3.Cross(direction, viewDirection).normalized;
			Vector3 wingtipOffset = cross * 0.1f * viewScale;

			Color axisColor = dragComponent == component ? Color.white : (hoverComponent == component ? Color.yellow : color);

			Vector3 apex = pivotPosition + (direction * viewScale);
			RuntimeGizmos.Get().Line(pivotPosition, apex, axisColor, layer: EGizmoLayer.Foreground);
			RuntimeGizmos.Get().Line(apex - wingtipOffset, apex + wingtipOffset, axisColor, layer: EGizmoLayer.Foreground);
		}

		private void DrawPositionBoundsAxis(Vector3 direction, float offset, Color color, EComponent component)
		{
			Vector3 cross = Vector3.Cross(direction, viewDirection).normalized;
			Vector3 wingtipOffset = cross * 0.1f * viewScale;

			Color axisColor = dragComponent == component ? Color.white : (hoverComponent == component ? Color.yellow : color);

			Vector3 basePosition = pivotPosition + (direction * offset);
			Vector3 apex = basePosition + (direction * viewScale);
			Vector3 wingtipCenter = basePosition + (direction * 0.75f * viewScale);
			RuntimeGizmos.Get().Line(basePosition, apex, axisColor, layer: EGizmoLayer.Foreground);
			RuntimeGizmos.Get().Line(wingtipCenter - wingtipOffset, apex, axisColor, layer: EGizmoLayer.Foreground);
			RuntimeGizmos.Get().Line(wingtipCenter + wingtipOffset, apex, axisColor, layer: EGizmoLayer.Foreground);
		}

		private void DrawScaleBoundsAxis(Vector3 direction, float offset, Color color, EComponent component)
		{
			Vector3 cross = Vector3.Cross(direction, viewDirection).normalized;
			Vector3 wingtipOffset = cross * 0.1f * viewScale;

			Color axisColor = dragComponent == component ? Color.white : (hoverComponent == component ? Color.yellow : color);

			Vector3 basePosition = pivotPosition + (direction * offset);
			Vector3 apex = basePosition + (direction * viewScale);
			RuntimeGizmos.Get().Line(basePosition, apex, axisColor, layer: EGizmoLayer.Foreground);
			RuntimeGizmos.Get().Line(apex - wingtipOffset, apex + wingtipOffset, axisColor, layer: EGizmoLayer.Foreground);
		}

		private void SyncMode()
		{
			if (dragComponent != EComponent.NONE)
			{
				// Cannot change mode while dragging.
				return;
			}

			if (preferredMode == EMode.PositionBounds && !hasPivotBounds)
			{
				mode = EMode.Position;
			}
			else if (preferredMode == EMode.ScaleBounds && !hasPivotBounds)
			{
				mode = EMode.Scale;
			}
			else
			{
				mode = preferredMode;
			}
		}

		private void SyncPivot()
		{
			if (dragComponent != EComponent.NONE)
			{
				// Cannot change pivot while dragging.
				return;
			}

			pivotPosition = preferredPivotPosition;
			pivotRotation = preferredPivotRotation;
		}

		/// <summary>
		/// Update properties that depend on the transform of the camera relative to our handles.
		/// </summary>
		private void UpdateViewProperties()
		{
			if (MainCamera.instance == null)
			{
				viewDirection = Vector3.forward;
				viewScale = 1.0f;
				viewAxisFlip = Vector3.one;
				cameraForward = Vector3.forward;
				return;
			}

			cameraForward = MainCamera.instance.transform.forward;
			Vector3 viewPosition = MainCamera.instance.transform.position;
			Vector3 viewOffset = pivotPosition - viewPosition;
			float viewDistance = viewOffset.magnitude;
			if (viewDistance < 0.001f)
			{
				viewDirection = Vector3.forward;
				viewScale = 1.0f;
				viewAxisFlip = Vector3.one;
				return;
			}

			viewDirection = viewOffset / viewDistance;
			viewRight = Vector3.Cross(viewDirection, Vector3.up).normalized;
			viewUp = Vector3.Cross(viewDirection, viewRight).normalized;

			viewScale = viewDistance * 0.5f;
			viewAxisFlip.x = Vector3.Dot(viewDirection, pivotRotation * Vector3.right) < 0 ? 1 : -1;
			viewAxisFlip.y = Vector3.Dot(viewDirection, pivotRotation * Vector3.up) < 0 ? 1 : -1;
			viewAxisFlip.z = Vector3.Dot(viewDirection, pivotRotation * Vector3.forward) < 0 ? 1 : -1;
		}

		private EMode preferredMode = EMode.Position;
		private EMode mode = EMode.Position;

		/// <summary>
		/// Center of handle.
		/// </summary>
		private Vector3 pivotPosition;
		/// <summary>
		/// Rotation of handle.
		/// </summary>
		private Quaternion pivotRotation = Quaternion.identity;

		private Vector3 preferredPivotPosition;
		private Quaternion preferredPivotRotation;

		private Bounds pivotBounds;
		/// <summary>
		/// True if pivotBounds is non-zero.
		/// </summary>
		private bool hasPivotBounds;

		/// <summary>
		/// Mouse currently over this handle.
		/// </summary>
		private EComponent hoverComponent;
		/// <summary>
		/// Mouse currently dragging this handle.
		/// </summary>
		private EComponent dragComponent;

		/// <summary>
		/// Direction from camera toward pivot.
		/// </summary>
		private Vector3 viewDirection;

		private Vector3 viewRight;
		private Vector3 viewUp;
		private Vector3 cameraForward;

		/// <summary>
		/// Multiplier according to distance between camera and pivot to keep handles a constant on-screen size.
		/// </summary>
		private float viewScale = 1.0f;

		/// <summary>
		/// Multiplier to flip axis handles according to which side the camera is on.
		/// </summary>
		private Vector3 viewAxisFlip = Vector3.one;

		// Previous value to compare delta for value changed events.
		private Vector3 dragPreviousPosition;
		private Quaternion dragPreviousRotation;
		private float dragPreviousAngle;
		private Vector3 dragPreviousScale;

		private Vector3 dragAxisOrigin;
		private Vector3 dragAxisDirection;
		private float dragAxisInitialDistance;

		private Vector3 dragPlaneOrigin;
		private Vector3 dragPlaneAxis0;
		private Vector3 dragPlaneAxis1;
		private Vector3 dragPlaneNormal;
		private float dragPlaneInitialDistance0;
		private float dragPlaneInitialDistance1;

		/// <summary>
		/// Pivot rotation when rotation drag started.
		/// </summary>
		private Quaternion dragRotationOrigin;
		/// <summary>
		/// Rotating around this axis.
		/// </summary>
		private Vector3 dragRotationAxis;
		/// <summary>
		/// Direction from circle center to edge point.
		/// </summary>
		private Vector3 dragRotationOutwardDirection;
		/// <summary>
		/// Point on the edge of the circle.
		/// </summary>
		private Vector3 dragRotationEdgePoint;
		/// <summary>
		/// Drag along this tangent to the circle.
		/// </summary>
		private Vector3 dragRotationTangent;

		private Vector3 dragScaleOrigin;
		private Vector3 dragScaleLocalDirection;
		private Vector3 dragScaleWorldDirection;
		private float dragScaleInitialDistance;
		private Vector3 dragScaleBoundsCenter;
		private Vector3 dragScaleBoundsSize;
		private float dragScaleBounds;

		[System.Flags]
		private enum EComponent
		{
			NONE = 0,
			X = 1 << 0,
			Y = 1 << 1,
			Z = 1 << 2,
			POSITION_AXIS = 1 << 3,
			POSITION_PLANE = 1 << 4,
			ROTATION = 1 << 5,
			SCALE = 1 << 6,
			POSITION_BOUNDS = 1 << 7,
			NEGATIVE = 1 << 8,
			POSITIVE = 1 << 9,
			SCALE_BOUNDS = 1 << 10,

			POSITION_AXIS_X = POSITION_AXIS | X,
			POSITION_AXIS_Y = POSITION_AXIS | Y,
			POSITION_AXIS_Z = POSITION_AXIS | Z,
			POSITION_PLANE_X = POSITION_PLANE | X,
			POSITION_PLANE_Y = POSITION_PLANE | Y,
			POSITION_PLANE_Z = POSITION_PLANE | Z,
			ROTATION_X = ROTATION | X,
			ROTATION_Y = ROTATION | Y,
			ROTATION_Z = ROTATION | Z,
			SCALE_AXIS_X = SCALE | X,
			SCALE_AXIS_Y = SCALE | Y,
			SCALE_AXIS_Z = SCALE | Z,
			SCALE_UNIFORM = SCALE | X | Y | Z,
			POSITION_BOUNDS_NEGATIVE_X = POSITION_BOUNDS | NEGATIVE | X,
			POSITION_BOUNDS_POSITIVE_X = POSITION_BOUNDS | POSITIVE | X,
			POSITION_BOUNDS_NEGATIVE_Y = POSITION_BOUNDS | NEGATIVE | Y,
			POSITION_BOUNDS_POSITIVE_Y = POSITION_BOUNDS | POSITIVE | Y,
			POSITION_BOUNDS_NEGATIVE_Z = POSITION_BOUNDS | NEGATIVE | Z,
			POSITION_BOUNDS_POSITIVE_Z = POSITION_BOUNDS | POSITIVE | Z,
			SCALE_BOUNDS_NEGATIVE_X = SCALE_BOUNDS | NEGATIVE | X,
			SCALE_BOUNDS_POSITIVE_X = SCALE_BOUNDS | POSITIVE | X,
			SCALE_BOUNDS_NEGATIVE_Y = SCALE_BOUNDS | NEGATIVE | Y,
			SCALE_BOUNDS_POSITIVE_Y = SCALE_BOUNDS | POSITIVE | Y,
			SCALE_BOUNDS_NEGATIVE_Z = SCALE_BOUNDS | NEGATIVE | Z,
			SCALE_BOUNDS_POSITIVE_Z = SCALE_BOUNDS | POSITIVE | Z,
		}
	}
}
