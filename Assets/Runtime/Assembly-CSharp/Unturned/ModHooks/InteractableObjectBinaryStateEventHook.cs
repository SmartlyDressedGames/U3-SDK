////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Events;
#if GAME
using Unturned.UnityEx;
#endif // GAME

namespace SDG.Unturned
{
	/// <summary>
	/// Can be added to any GameObject with an interactable binary state in its parents.
	///
	/// If players should not be allowed to interact with the object in the ordinary manner,
	/// add the Interactability_Remote flag to its asset to indicate only mod hooks should control it.
	/// </summary>
	[AddComponentMenu("Unturned/IOBS Event Hook")]
	public class InteractableObjectBinaryStateEventHook : MonoBehaviour
	{
		public enum EListenServerHostMode
		{
			/// <summary>
			/// When a state change is requested in singleplayer it should be treated as if running as a client on a server.
			/// This is the default to match behavior from before this option was added.
			/// </summary>
			RequestAsClient,

			/// <summary>
			/// When a state change is requested in singleplayer it should be treated as if running as a dedicated server.
			/// </summary>
			OverrideState,
		}

		/// <summary>
		/// Invoked when interactable object enters the Used / On / Enabled state.
		/// </summary>
		public UnityEvent OnStateEnabled;

		/// <summary>
		/// Invoked when interactable object enters the Unused / Off / Disabled state.
		/// </summary>
		public UnityEvent OnStateDisabled;

		/// <summary>
		/// Should the OnStateEnabled and OnStateDisabled events be invoked when the object is loaded, becomes relevant
		/// in multiplayer, and is reset? True is useful when visuals need to be kept in sync with the state, whereas
		/// false is useful for transient interactions.
		/// </summary>
		public bool InvokeWhenInitialized = true;

		/// <summary>
		/// If true, OnStateEnabled and OnStateDisabled are only invoked in singleplayer and on the server.
		/// </summary>
		[Tooltip("If true, OnStateEnabled and OnStateDisabled are only invoked in singleplayer and on the server.")]
		public bool InvokeEventsAuthorityOnly = false;

		/// <summary>
		/// If true, calls to GotoEnabledState, GotoDisabledState, and ToggleState are only processed in singleplayer
		/// and on the server.
		/// </summary>
		[Tooltip("If true, calls to GotoEnabledState, GotoDisabledState, and ToggleState are only processed in singleplayer and on the server.")]
		public bool ChangeStateAuthorityOnly = false;

		/// <summary>
		/// Controls how state change requests are performed when running as both client and server ("listen server").
		/// On the dedicated server, requesting a state change overrides the current state without processing NPC
		/// conditions, whereas when a client requests a state change NPC conditions apply. This option fixes the
		/// inconsistency in singleplayer of whether to treat as server or client. (public issue #4298)
		/// At the time of writing (2024-01-29) listen server only applies to singleplayer.
		/// </summary>
		[Tooltip("Controls how state change requests are performed when running as both client and server (singleplayer/listen server).")]
		public EListenServerHostMode ListenServerHostMode = EListenServerHostMode.RequestAsClient;

		/// <summary>
		/// Set state to Enabled if currently Disabled.
		/// 
		/// On dedicated server this directly changes the state,
		/// but as client this will apply the usual conditions and rewards.
		/// </summary>
		public void GotoEnabledState()
		{
#if GAME
			if (ChangeStateAuthorityOnly && !Provider.isServer)
				return;

			if (interactable != null)
			{
				interactable.SetUsedFromClientOrServer(true, ListenServerHostMode);
			}
#endif // GAME
		}

		/// <summary>
		/// Set state to Disabled if currently Enabled.
		///
		/// On dedicated server this directly changes the state,
		/// but as client this will apply the usual conditions and rewards.
		/// </summary>
		public void GotoDisabledState()
		{
#if GAME
			if (ChangeStateAuthorityOnly && !Provider.isServer)
				return;

			if (interactable != null)
			{
				interactable.SetUsedFromClientOrServer(false, ListenServerHostMode);
			}
#endif // GAME
		}

		/// <summary>
		/// Toggle between the Enabled and Disabled states.
		///
		/// On dedicated server this directly changes the state,
		/// but as client this will apply the usual conditions and rewards. 
		/// </summary>
		public void ToggleState()
		{
#if GAME
			if (ChangeStateAuthorityOnly && !Provider.isServer)
				return;

			if (interactable != null)
			{
				interactable.SetUsedFromClientOrServer(!interactable.isUsed, ListenServerHostMode);
			}
#endif // GAME
		}

#if GAME
		protected void Start()
		{
			interactable = gameObject.GetComponentInParent<InteractableObjectBinaryState>();
			if (interactable == null)
			{
				UnturnedLog.warn("IOBS {0} unable to find interactable", this.GetSceneHierarchyPath());
			}
			else
			{
				++interactable.modHookCounter;
				if (Provider.isServer || !InvokeEventsAuthorityOnly)
				{
					interactable.onStateChanged += onStateChanged;
					if (InvokeWhenInitialized)
					{
						interactable.onStateInitialized += onStateChanged;
						onStateChanged(interactable);
					}
				}
			}
		}

		protected void OnDestroy()
		{
			if (interactable != null)
			{
				if (Provider.isServer || !InvokeEventsAuthorityOnly)
				{
					if (InvokeWhenInitialized)
					{
						interactable.onStateInitialized -= onStateChanged;
					}
					interactable.onStateChanged -= onStateChanged;
				}
				--interactable.modHookCounter;
				interactable = null;
			}
		}

		protected void onStateChanged(InteractableObjectBinaryState sender)
		{
			Debug.Assert(Provider.isServer || !InvokeEventsAuthorityOnly);
			if (sender.isUsed)
			{
				OnStateEnabled.TryInvoke(this);
			}
			else
			{
				OnStateDisabled.TryInvoke(this);
			}
		}

		private InteractableObjectBinaryState interactable = null;
#endif // GAME
	}
}
