////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class CharacterAnimator : MonoBehaviour
	{
		public static readonly float BLEND = 0.25f;

		protected Animation anim;
		protected Transform spine;
		protected Transform skull;
		protected Transform leftShoulder;
		protected Transform rightShoulder;
		protected Transform spineHook;

		protected string clip;

		public void sample()
		{
			anim.Sample();
		}

		public void mixAnimation(string name)
		{
			AnimationState animState = anim[name];
			if (animState != null)
			{
				animState.layer = 1;
			}
		}

		public void mixAnimation(string name, bool mixLeftShoulder, bool mixRightShoulder)
		{
			mixAnimation(name, mixLeftShoulder, mixRightShoulder, false);
		}

		public void mixAnimation(string name, bool mixLeftShoulder, bool mixRightShoulder, bool mixSkull)
		{
			AnimationState animState = anim[name];
			if (animState == null)
				return;

			if (mixLeftShoulder)
			{
				animState.AddMixingTransform(leftShoulder, true);
			}

			if (mixRightShoulder)
			{
				animState.AddMixingTransform(rightShoulder, true);
			}

			if (mixSkull)
			{
				animState.AddMixingTransform(skull, true);
			}

			animState.layer = 1;
		}

		public void AddEquippedItemAnimation(AnimationClip clip, Transform itemModelTransform)
		{
			if (clip == null)
				return;

			anim.AddClip(clip, clip.name);

			mixAnimation(clip.name, true, true);

			// Nelson 2024-06-26: Typically this is already taken care of by mixing the shoulder transforms, but with
			// the addition of Spine as an item parent option we need to ensure item itself can be animated, too.
			if (itemModelTransform != null)
			{
				AnimationState animState = anim[clip.name];
				if (animState != null)
				{
					animState.AddMixingTransform(spineHook, true);
					animState.AddMixingTransform(itemModelTransform, true);
				}
			}
		}

		public void removeAnimation(AnimationClip clip)
		{
			if (clip == null)
				return;

			if (anim[clip.name] != null)
			{
				anim.RemoveClip(clip);
			}
		}

		public void setAnimationSpeed(string name, float speed)
		{
			AnimationState animState = anim[name];
			if (animState != null)
			{
				animState.speed = speed;
			}
		}

		public float getAnimationLength(string name)
		{
			return GetAnimationLength(name, scaled: true);
		}

		/// <param name="scaled">If true, include current animation speed modifier.</param>
		public float GetAnimationLength(string name, bool scaled = true)
		{
			AnimationState animState = anim[name];
			if (animState != null)
			{
				if (scaled)
				{
					if (animState.speed != 0.0f)
					{
						return animState.clip.length / animState.speed;
					}
					else
					{
						return 0.0f;
					}
				}
				else
				{
					return animState.clip.length;
				}
			}
			else
			{
				return 0f;
			}
		}

		public bool getAnimationPlaying()
		{
			return !string.IsNullOrEmpty(clip) && anim.IsPlaying(clip);
		}

		public void state(string name)
		{
			if (anim[name] == null)
			{
				return;
			}

			anim.CrossFade(name, BLEND);
		}

		public bool checkExists(string name)
		{
			return anim[name] != null;
		}

		/// <returns>True if an animation was found and started playing.</returns>
		public bool play(string name, bool smooth)
		{
			if (anim[name] == null)
			{
				return false;
			}

			if (clip != "")
			{
				anim.Stop(clip);
			}

			clip = name;

			if (smooth)
			{
				anim.CrossFade(name, BLEND);
			}
			else
			{
				anim.Play(name);
			}

			return true;
		}

		public void stop(string name)
		{
			if (anim[name] == null)
			{
				return;
			}

			if (name == clip)
			{
				anim.Stop(name);

				clip = "";
			}
		}

		protected void init()
		{
			clip = "";

			anim = GetComponent<Animation>();
			spine = transform.Find("Skeleton").Find("Spine");
			skull = spine.Find("Skull");
			leftShoulder = spine.Find("Left_Shoulder");
			rightShoulder = spine.Find("Right_Shoulder");
			spineHook = spine.Find("Spine_Hook");
			Debug.Assert(spineHook != null, $"Missing Spine_Hook transform under {spine.GetSceneHierarchyPath()}", gameObject);
		}

		private void Awake()
		{
			init();
		}
	}
}
