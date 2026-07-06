////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	internal class GlazierConstraintFrame_uGUI : GlazierElementBase_uGUI, ISleekConstraintFrame
	{
		private ESleekConstraint _constraint;
		public ESleekConstraint Constraint
		{
			get
			{
				ValidateNotDestroyed();
				return _constraint;
			}

			set
			{
				ValidateNotDestroyed();

				if (_constraint != ESleekConstraint.NONE)
					throw new System.NotSupportedException();

				_constraint = value;

				if (Constraint == ESleekConstraint.FitInParent)
				{
					aspectRatioFitter = contentTransform.gameObject.AddComponent<AspectRatioFitter>();
					aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
					aspectRatioFitter.aspectRatio = _aspectRatio;
				}
			}
		}

		private float _aspectRatio = 1.0f;
		public float AspectRatio
		{
			get
			{
				ValidateNotDestroyed();
				return _aspectRatio;
			}

			set
			{
				ValidateNotDestroyed();
				_aspectRatio = value;

				if (aspectRatioFitter != null)
				{
					aspectRatioFitter.aspectRatio = value;
				}
			}
		}

		public GlazierConstraintFrame_uGUI(Glazier_uGUI glazier) : base(glazier)
		{ }

		public override void ConstructNew()
		{
			base.ConstructNew();

			GameObject contentGameObject = new GameObject("Content", typeof(RectTransform));
			contentTransform = contentGameObject.GetRectTransform();
			contentTransform.SetParent(transform, false);
			contentTransform.anchorMin = new Vector2(0.0f, 0.0f);
			contentTransform.anchorMax = new Vector2(1.0f, 1.0f);
			contentTransform.anchoredPosition = Vector2.zero;
			contentTransform.sizeDelta = Vector2.zero;
		}

		public override RectTransform AttachmentTransform => contentTransform;

		private RectTransform contentTransform;
		private AspectRatioFitter aspectRatioFitter;
	}
}
