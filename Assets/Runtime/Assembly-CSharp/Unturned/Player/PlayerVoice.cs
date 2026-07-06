////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define VOICE_CHAT_HEAR_SELF // Send voice data back to speaker for debugging.
// #define LOG_VOICE_CHAT // Verbosely log voice details.
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void Talked(bool isTalking);

	public class PlayerVoice : PlayerCaller
	{
		/// <summary>
		/// Speaker writes compressed audio to this buffer.
		/// Listener copies network buffer here for decompression.
		/// </summary>
		private static byte[] compressedVoiceBuffer = new byte[8000];
		/// <summary>
		/// Listener writes decompressed PCM data to this buffer.
		/// </summary>
		private static readonly byte[] DECOMPRESSED_VOICE_BUFFER = new byte[22000];

		/// <summary>
		/// Seconds interval to wait between asking recording subsystem for voice data.
		/// Rather than polling every frame we wait until data has accumulated to send.
		/// </summary>
		private static readonly float RECORDING_POLL_INTERVAL = 0.05f;

		/// <summary>
		/// Seconds to wait before playing back newly received data.
		/// Allows a few samples to buffer up so that we don't stutter as more arrive.
		/// </summary>
		private static readonly float PLAYBACK_DELAY = 0.2f;

		/// <summary>
		/// Seconds to wait after playback before stopping audio source.
		/// We zero this portion of the clip to prevent pops.
		/// </summary>
		private static readonly float SILENCE_DURATION = 0.1f;

		/// <summary>
		/// Max calls to askVoice server will allow per second before blocking their voice data.
		/// Prevents spamming many tiny requests bogging down server output.
		/// </summary>
		private static readonly uint EXPECTED_ASKVOICE_PER_SECOND = (uint) (1 / RECORDING_POLL_INTERVAL) + 3;

		/// <summary>
		/// Max compressed bytes server will allow per second before blocking their voice data.
		/// When logging compressed size they averaged 3000-5000 per second, so this affords some wiggle-room.
		/// </summary>
		private static readonly uint EXPECTED_BYTES_PER_SECOND = 7000;

		[System.Obsolete("Replaced by ServerSetPermissions which is replicated to owner.")]
		public bool allowVoiceChat = true;

		/// <summary>
		/// Internal value managed by isTalking.
		/// </summary>
		private bool _isTalking = false;

		/// <summary>
		/// Is this player broadcasting their voice?
		/// Used in the menus to show an indicator who's talking.
		/// Locally set when recording starts/stops, and remotely when voice data starts/stops being received.
		/// </summary>
		public bool isTalking
		{
			get => _isTalking;
			private set
			{
				if (_isTalking == value)
					return;
				_isTalking = value;

				onTalkingChanged?.Invoke(value);
			}
		}

		/// <summary>
		/// Broadcasts after isTalking changes.
		/// </summary>
		public event Talked onTalkingChanged;

		public event System.Action<PlayerVoice> OnCustomAllowTalkingChanged;

		/// <summary>
		/// Can this player currently hear global (radio) voice chat?
		/// </summary>
		public bool canHearRadio => hasUseableWalkieTalkie || hasEarpiece;

		/// <summary>
		/// Is the player wearing an earpiece?
		/// Allows global (radio) voice chat to be heard without equipping the walkie-talkie item.
		/// </summary>
		public bool hasEarpiece => player.clothing != null && player.clothing.maskAsset != null && player.clothing.maskAsset.isEarpiece;

		/// <summary>
		/// Is a UseableWalkieTalkie currently equipped?
		/// Set by useable's equip and dequip events.
		/// </summary>
		public bool hasUseableWalkieTalkie;

		/// <summary>
		/// Was the most recent voice data we received sent using walkie talkie?
		/// </summary>
		private bool playbackUsingWalkieTalkie;

		/// <summary>
		/// Has voice data recently been received, but we're waiting slightly to begin playback?
		/// Important to give clip a chance to buffer up so that we don't stutter as more samples arrive.
		/// </summary>
		private bool hasPendingVoiceData;

		/// <summary>
		/// AudioSource.isPlaying is not trustworthy.
		/// </summary>
		private bool isPlayingVoiceData;

		/// <summary>
		/// Timer counting down to begin playback of recently received voice data.
		/// We use a timer rather than availableSamples.Count because a very short phrase could be less than threshold.
		/// </summary>
		private float pendingPlaybackDelay;

		/// <summary>
		/// Timer counting down to end playback.
		/// </summary>
		private float availablePlaybackTime;

		/// <summary>
		/// Accumulated realtime since we last polled data from voice subsystem.
		/// </summary>
		private float pollRecordingTimer;

		#region AntiSpam
		/// <summary>
		/// Last time askVoiceChat was invoked over network.
		/// </summary>
		private float lastAskVoiceRealtime;

		/// <summary>
		/// Number of times askVoiceChat has been called recently, to prevent calling it many times
		/// with tiny durations getting server to relay many packets to clients.
		/// </summary>
		private uint recentVoiceCalls;

		/// <summary>
		/// Total of recent compressed voice payload lengths.
		/// </summary>
		private uint recentVoiceBytes;

		/// <summary>
		/// Realtime since this recent conversation began.
		/// </summary>
		private float recentVoiceDuration;
		#endregion

		public delegate void RelayVoiceHandler(PlayerVoice speaker, bool wantsToUseWalkieTalkie, ref bool shouldAllow, ref bool shouldBroadcastOverRadio, ref RelayVoiceCullingHandler cullingHandler);
		public delegate bool RelayVoiceCullingHandler(PlayerVoice speaker, PlayerVoice listener);

		/// <summary>
		/// Only used by plugins.
		/// Called on server to allow plugins to override the default area and walkie-talkie voice channels.
		/// </summary>
		public static event RelayVoiceHandler onRelayVoice;

		/// <summary>
		/// Default culling handler when speaking over walkie-talkie.
		/// </summary>
		public static bool handleRelayVoiceCulling_RadioFrequency(PlayerVoice speaker, PlayerVoice listener)
		{
			if (listener.canHearRadio && speaker.player.quests.radioFrequency == listener.player.quests.radioFrequency)
			{
#if LOG_VOICE_CHAT
				UnturnedLog.info("Sending over radio to {0}", listener.channel.owner.playerID.steamID);
#endif
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Default culling handler when speaking in proximity.
		/// </summary>
		public static bool handleRelayVoiceCulling_Proximity(PlayerVoice speaker, PlayerVoice listener)
		{
			const float sqrRadius = 128 * 128;

			float sqrDistance = (speaker.transform.position - listener.transform.position).sqrMagnitude;
			if (sqrDistance < sqrRadius)
			{
#if LOG_VOICE_CHAT
				UnturnedLog.info("Sending in radius to {0}", listener.channel.owner.playerID.steamID);
#endif
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool GetAllowTalkingWhileDead()
		{
			return allowTalkingWhileDead;
		}

		public bool GetCustomAllowTalking()
		{
			return customAllowTalking;
		}

		public void ServerSetPermissions(bool allowTalkingWhileDead, bool customAllowTalking)
		{
			if (this.allowTalkingWhileDead == allowTalkingWhileDead && this.customAllowTalking == customAllowTalking)
				return;

			this.allowTalkingWhileDead = allowTalkingWhileDead;
			this.customAllowTalking = customAllowTalking;
			SendPermissions.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), allowTalkingWhileDead, customAllowTalking);
		}

		private static readonly ClientInstanceMethod<bool, bool> SendPermissions = ClientInstanceMethod<bool, bool>.Get(typeof(PlayerVoice), nameof(ReceivePermissions));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceivePermissions(bool allowTalkingWhileDead, bool customAllowTalking)
		{
			this.allowTalkingWhileDead = allowTalkingWhileDead;
			this.customAllowTalking = customAllowTalking;
		}

		[System.Obsolete]
		public void askVoiceChat(byte[] packet)
		{ }

		private static readonly ServerInstanceMethod SendVoiceChatRelay = ServerInstanceMethod.Get(typeof(PlayerVoice), nameof(ReceiveVoiceChatRelay));
		/// <summary>
		/// Called by owner to relay voice data to clients.
		/// Not using rate limit attribute because it internally tracks bytes per second.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER)]
		public void ReceiveVoiceChatRelay(in ServerInvocationContext context)
		{
			if (player.life.isDead && !allowTalkingWhileDead)
			{
				context.LogWarning("Ignoring voice while dead");
				return;
			}

			if (!customAllowTalking)
			{
				context.LogWarning("Ignoring voice because plugin has disabled talking");
				return;
			}

#pragma warning disable
			if (!allowVoiceChat)
			{
				context.LogWarning("Ignoring voice because plugin has disabled talking (legacy)");
				return;
			}
#pragma warning restore

			NetPakReader reader = context.reader;
			ushort compressedSize;
			reader.ReadUInt16(out compressedSize);
			bool wantsToUseWalkieTalkie;
			reader.ReadBit(out wantsToUseWalkieTalkie);

			byte[] source;
			int sourceOffset;
			if (!reader.ReadBytesPtr(compressedSize, out source, out sourceOffset))
			{
				// should log
				return;
			}

#if LOG_VOICE_CHAT
			UnturnedLog.info("askVoiceChat - Compressed Size: {0} Walkie Talkie: {1}", compressedSize, wantsToUseWalkieTalkie);
#endif

			if (compressedSize < 1)
				return;

			#region AntiSpam
			float deltaRealtime = Time.realtimeSinceStartup - lastAskVoiceRealtime;
			if (deltaRealtime > 2)
			{
				recentVoiceCalls = 1;
				recentVoiceBytes = compressedSize;
				recentVoiceDuration = RECORDING_POLL_INTERVAL;
			}
			else
			{
				++recentVoiceCalls;
				recentVoiceBytes += compressedSize;
				recentVoiceDuration += deltaRealtime;
			}
			lastAskVoiceRealtime = Time.realtimeSinceStartup;

#if LOG_VOICE_CHAT
			UnturnedLog.info("Calls: {0} Bytes: {1} Duration: {2}", recentVoiceCalls, recentVoiceBytes, recentVoiceDuration);
#endif

			if (recentVoiceCalls >= EXPECTED_ASKVOICE_PER_SECOND || recentVoiceBytes > EXPECTED_BYTES_PER_SECOND || recentVoiceDuration > 1)
			{
				float averageCallsPerSecond = recentVoiceCalls / recentVoiceDuration;

				// tellVoice is sent (1 / RECORDING_POLL_INTERVAL) = 20 times per second, and when logging the compressed size
				// it was averaging between 350 - 500 bytes, so a limit of 7KBps gives a little wiggle room.
				float averageBytesPerSecond = recentVoiceBytes / recentVoiceDuration;
#if LOG_VOICE_CHAT
				UnturnedLog.info("Average Calls: {0} Average Bytes: {1}", averageCallsPerSecond, averageBytesPerSecond);
#endif
				if (averageCallsPerSecond >= EXPECTED_ASKVOICE_PER_SECOND || averageBytesPerSecond > EXPECTED_BYTES_PER_SECOND)
				{
#if LOG_VOICE_CHAT
					UnturnedLog.warn("\tIgnoring");
#endif
					// They may be spamming tellVoice packets, so don't give this data to Steam.
					return;
				}
			}
			#endregion

			bool shouldAllow;
			bool shouldBroadcastOverRadio;
			if (wantsToUseWalkieTalkie)
			{
				if (hasUseableWalkieTalkie)
				{
					shouldAllow = true;
					shouldBroadcastOverRadio = true;
				}
				else
				{
					shouldAllow = false; // They expect to talk over radio, but something is mixed up.
					shouldBroadcastOverRadio = false;
				}
			}
			else
			{
				shouldAllow = true;
				shouldBroadcastOverRadio = false;
			}
			RelayVoiceCullingHandler cullingHandler = null;

			onRelayVoice?.Invoke(this, wantsToUseWalkieTalkie, ref shouldAllow, ref shouldBroadcastOverRadio, ref cullingHandler);

			if (shouldAllow == false)
			{
				return;
			}

			if (cullingHandler == null)
			{
				// On vanilla it is null at this point, or we are falling back because plugin did not assign one.
				cullingHandler = shouldBroadcastOverRadio ? handleRelayVoiceCulling_RadioFrequency
														  : (RelayVoiceCullingHandler) handleRelayVoiceCulling_Proximity;
			}

			SendPlayVoiceChatWriteParameters sendPlayVoiceChatWriteParameters = new SendPlayVoiceChatWriteParameters()
			{
				compressedSize = compressedSize,
				shouldBroadcastOverRadio = shouldBroadcastOverRadio,
				source = source,
				sourceOffset = sourceOffset,
			};

			SendPlayVoiceChat.Invoke(GetNetId(), ENetReliability.Unreliable,
			Provider.GatherRemoteClientConnectionsMatchingPredicate((SteamPlayer potentialRecipient) =>
			{
				if (potentialRecipient == null || potentialRecipient.player == null || potentialRecipient.player.voice == null)
					return false;

#if !VOICE_CHAT_HEAR_SELF
				if (potentialRecipient == channel.owner)
					return false;
#endif // !VOICE_CHAT_HEAR_SELF

				return cullingHandler(this, potentialRecipient.player.voice);
			}), SendPlayVoiceChat_Write, sendPlayVoiceChatWriteParameters);
		}

		struct SendPlayVoiceChatWriteParameters
		{
			public ushort compressedSize;
			public bool shouldBroadcastOverRadio;
			public byte[] source;
			public int sourceOffset;
		}

		private void SendPlayVoiceChat_Write(NetPakWriter writer, SendPlayVoiceChatWriteParameters p)
		{
			writer.WriteUInt16(p.compressedSize);
			writer.WriteBit(p.shouldBroadcastOverRadio);
			writer.WriteBytes(p.source, p.sourceOffset, p.compressedSize);
		}

		private static ClientInstanceMethod SendPlayVoiceChat = ClientInstanceMethod.Get(typeof(PlayerVoice), nameof(ReceivePlayVoiceChat));
		/// <summary>
		/// Called by server to relay voice data from clients.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceivePlayVoiceChat(in ClientInvocationContext context)
		{
			if (!OptionsSettings.chatVoiceIn || channel.owner.isVoiceChatLocallyMuted)
				return;

			if (audioData == null || audioSource == null || audioSource.clip == null)
				return;

			NetPakReader reader = context.reader;
			ushort compressedSize;
			reader.ReadUInt16(out compressedSize);
			bool wantsToUseWalkieTalkie;
			reader.ReadBit(out wantsToUseWalkieTalkie);

			if (!reader.ReadBytes(compressedVoiceBuffer, compressedSize))
			{
				context.LogWarning("unable to read voice buffer");
				return;
			}

#if LOG_VOICE_CHAT
			UnturnedLog.info("tellVoiceChat - Compressed Size: {0} Walkie Talkie: {1}", compressedSize, wantsToUseWalkieTalkie);
#endif

			AppendVoiceData(compressedVoiceBuffer, compressedSize, wantsToUseWalkieTalkie);
		}

		private void AppendVoiceData(byte[] compressedBuffer, uint compressedSize, bool wantsToUseWalkieTalkie)
		{
			playbackUsingWalkieTalkie = wantsToUseWalkieTalkie;

			uint decompressedSize;
			EVoiceResult decompressVoiceResult = SteamUser.DecompressVoice(compressedBuffer, compressedSize, DECOMPRESSED_VOICE_BUFFER, (uint) DECOMPRESSED_VOICE_BUFFER.Length, out decompressedSize, steamOptimalSampleRate);

			if (decompressVoiceResult != EVoiceResult.k_EVoiceResultOK || decompressedSize < 1)
			{
#if LOG_VOICE_CHAT
				UnturnedLog.warn("DecompressVoiceResult: {0} DecompressedSize: {1}", decompressVoiceResult, decompressedSize);
#endif
				return;
			}

#if LOG_VOICE_CHAT
			UnturnedLog.info("DecompressedSize: {0}", decompressedSize);
#endif

			int autoDataNormalizationIndex = audioDataWriteIndex;

			float maxMagnitude = 0.0f;

			for (uint index = 0; index < decompressedSize; index += 2)
			{
				// PCM data is little endian.
				byte low = DECOMPRESSED_VOICE_BUFFER[index];
				byte high = DECOMPRESSED_VOICE_BUFFER[index + 1];
				short encodedSample = (short) (low | (high << 8));
				float decodedSample = encodedSample / (float) short.MaxValue;

				float magnitude = Mathf.Abs(decodedSample);
				maxMagnitude = Mathf.Max(magnitude, maxMagnitude);

				audioData[audioDataWriteIndex] = decodedSample;
				audioDataWriteIndex = (audioDataWriteIndex + 1) % audioData.Length;
			}

			if (maxMagnitude != 0.0f)
			{
				// Recording may include a short trail of silence, in which case the multiplier would be calculated as 200+.
				const float MAX_NORMALIZATION_MULTIPLIER = 8.0f;
				float volumeMultiplier = Mathf.Min(1.0f / maxMagnitude, MAX_NORMALIZATION_MULTIPLIER);
				for (uint index = 0; index < decompressedSize; index += 2)
				{
					audioData[autoDataNormalizationIndex] *= volumeMultiplier;
					autoDataNormalizationIndex = (autoDataNormalizationIndex + 1) % audioData.Length;
				}
			}

			int audioDataZeroIndex = audioDataWriteIndex;
			for (int index = 0; index < zeroSamples; ++index)
			{
				audioData[audioDataZeroIndex] = 0.0f;
				audioDataZeroIndex = (audioDataZeroIndex + 1) % audioData.Length;
			}

			bool setDataResult = audioClip.SetData(audioData, 0);
#if LOG_VOICE_CHAT
			if (!setDataResult)
			{
				UnturnedLog.warn("Voice clip SetData returned false");
			}
#endif

			float decompressedPlaybackTime = decompressedSize / 2 * secondsPerSample;

			if (!isPlayingVoiceData && !hasPendingVoiceData)
			{
				hasPendingVoiceData = true;
				pendingPlaybackDelay = PLAYBACK_DELAY;
				availablePlaybackTime = SILENCE_DURATION;
			}

			availablePlaybackTime += decompressedPlaybackTime;
		}

		/// <summary>
		/// Set to true during OnDestroy to make sure we don't start recording again.
		/// </summary>
		private bool isBeingDestroyed;

		private bool _isSteamRecording;
		/// <summary>
		/// If true, SteamUser.StartVoiceRecording has been called without a corresponding call to
		/// SteamUser.StopVoiceRecording yet.
		/// </summary>
		private bool SteamIsRecording
		{
			get => _isSteamRecording;
			set
			{
				if (_isSteamRecording != value)
				{
					_isSteamRecording = value;
					if (_isSteamRecording)
					{
						SteamUser.StartVoiceRecording();
					}
					else
					{
						SteamUser.StopVoiceRecording();
					}
				}
			}
		}

		/// <summary>
		/// If true, voice toggle is in ON mode.
		/// </summary>
		private bool inputToggleState;

		/// <summary>
		/// Internal value managed by inputWantsToRecord.
		/// </summary>
		private bool _inputWantsToRecord = false;

		/// <summary>
		/// Set by updateInput based on whether voice is enabled, key is held, is alive, etc.
		/// Reset to false during OnDestroy to stop recording.
		/// </summary>
		private bool inputWantsToRecord
		{
			get => _inputWantsToRecord;
			set
			{
				if (_inputWantsToRecord == value)
					return;
				_inputWantsToRecord = value;

				if (_inputWantsToRecord)
				{
					// Started speaking so reset timer to reduce delay.
					pollRecordingTimer = 0.0f;
				}

				SynchronizeSteamIsRecording();

				SteamFriends.SetInGameVoiceSpeaking(Provider.user, inputWantsToRecord);

				if (canEverPlayback)
				{
					// Only happens during debug, but in this case we let playback play the walkie-talkie
					// sounds and trigger isTalking changes.
				}
				else
				{
					if (hasUseableWalkieTalkie)
					{
						playWalkieTalkieSound();
					}

					// Update voice icon.
					isTalking = inputWantsToRecord;
				}
			}
		}

		/// <summary>
		/// Called during Update on owner client to start/stop recording.
		/// </summary>
		private void updateInput()
		{
			bool isRecordingEnabled = OptionsSettings.chatVoiceIn && OptionsSettings.EnableOutboundVoiceChat;
			bool isAlive = player.life.IsAlive || allowTalkingWhileDead;
			bool areNonInputRequirementsMet = isRecordingEnabled && isAlive && customAllowTalking;

			if (ControlsSettings.voiceMode == EControlMode.HOLD)
			{
				bool isKeyHeld = InputEx.GetKey(ControlsSettings.voice);
				inputWantsToRecord = areNonInputRequirementsMet && isKeyHeld;
				inputToggleState = false;
			}
			else if (ControlsSettings.voiceMode == EControlMode.TOGGLE)
			{
				if (InputEx.GetKeyDown(ControlsSettings.voice))
				{
					inputToggleState = !inputToggleState;
				}

				// e.g. Player just died so we toggle off voice.
				inputToggleState &= areNonInputRequirementsMet;

				inputWantsToRecord = inputToggleState;
			}
		}

		/// <summary>
		/// Called during Update on owner client to record voice data.
		/// </summary>
		private void updateRecording()
		{
			pollRecordingTimer += Time.unscaledDeltaTime;
			if (pollRecordingTimer < RECORDING_POLL_INTERVAL)
				return;

			pollRecordingTimer = 0.0f;

			uint availableCompressedSize;
			EVoiceResult getAvailableVoiceResult = SteamUser.GetAvailableVoice(out availableCompressedSize);

			if (getAvailableVoiceResult != EVoiceResult.k_EVoiceResultOK && getAvailableVoiceResult != EVoiceResult.k_EVoiceResultNoData && getAvailableVoiceResult != EVoiceResult.k_EVoiceResultNotRecording)
			{
				UnturnedLog.error("GetAvailableVoice result: " + getAvailableVoiceResult);
			}

			if (getAvailableVoiceResult != EVoiceResult.k_EVoiceResultOK || availableCompressedSize < 1)
			{
#if LOG_VOICE_CHAT
				UnturnedLog.warn("GetAvailableVoiceResult: {0} AvailableCompressedSize: {1}", getAvailableVoiceResult, availableCompressedSize);
#endif
				return;
			}

			if (availableCompressedSize > compressedVoiceBuffer.Length)
			{
				UnturnedLog.info($"Resizing compressed voice buffer ({compressedVoiceBuffer.Length}) to fit available size ({availableCompressedSize})");
				compressedVoiceBuffer = new byte[availableCompressedSize];
			}

			uint compressedSize;
			const bool bWantsCompressed = true;
			EVoiceResult getVoiceResult = SteamUser.GetVoice(bWantsCompressed, compressedVoiceBuffer, availableCompressedSize, out compressedSize);

			if (getVoiceResult != EVoiceResult.k_EVoiceResultOK && getVoiceResult != EVoiceResult.k_EVoiceResultNoData)
			{
				UnturnedLog.error("GetVoice result: " + getVoiceResult);
			}

			if (getVoiceResult != EVoiceResult.k_EVoiceResultOK || compressedSize < 1)
			{
#if LOG_VOICE_CHAT
				UnturnedLog.warn("GetVoiceResult: {0} CompressedSize: {1}", getVoiceResult, compressedSize);
#endif
				return;
			}

#if LOG_VOICE_CHAT
			UnturnedLog.info("Sending to server: {0}", compressedSize);
#endif

			if (!_inputWantsToRecord)
			{
				// Discard recorded data.
				return;
			}

			if (Provider.isServer)
			{
				// Listen server would need to send here.
				AppendVoiceData(compressedVoiceBuffer, compressedSize, hasUseableWalkieTalkie);
			}
			else
			{
				SendVoiceChatRelay.Invoke(GetNetId(), ENetReliability.Unreliable, SendVoiceChatRelay_Write, compressedSize);
			}
		}

		private void SendVoiceChatRelay_Write(NetPakWriter writer, uint compressedSize)
		{
			ushort sendLength = (ushort) compressedSize;
			writer.WriteUInt16(sendLength);
			writer.WriteBit(hasUseableWalkieTalkie);
			writer.WriteBytes(compressedVoiceBuffer, sendLength);
		}

		private static AudioClip radioClip;
		private static AudioClip GetOrLoadRadioClip()
		{
			if (radioClip == null)
			{
				radioClip = new AudioReference("core.masterbundle", "Sounds/Radio.ogg").LoadAudioClip();
			}

			return radioClip;
		}

		/// <summary>
		/// Play walkie-talkie squawk at our position.
		/// </summary>
		private void playWalkieTalkieSound()
		{
#if !DEDICATED_SERVER
			OneShotAudioParameters parameters = new OneShotAudioParameters(transform.position, GetOrLoadRadioClip());
			parameters.RandomizeVolume(0.74f, 0.76f);
			parameters.RandomizePitch(0.99f, 1.01f);
			parameters.SetSpatialBlend2D();
			parameters.Play();
#endif
		}

		/// <summary>
		/// Start and stop playback of received audio stream.
		/// </summary>
		private void updatePlayback()
		{
			if (audioSource == null)
				return;

			if (playbackUsingWalkieTalkie)
			{
				audioSource.spatialBlend = 0;
			}
			else
			{
				audioSource.spatialBlend = 1;
			}

			if (isPlayingVoiceData)
			{
				availablePlaybackTime -= Time.deltaTime;
				if (availablePlaybackTime <= 0.0f)
				{
					audioSource.Stop();
					audioSource.time = 0.0f; // Restart next time to help with loop flicker.
					audioDataWriteIndex = 0;

					isPlayingVoiceData = false;
					hasPendingVoiceData = false;

					if (playbackUsingWalkieTalkie)
					{
						playWalkieTalkieSound();
					}

					// Update voice icon.
					isTalking = false;
				}
			}
			else
			{
				if (hasPendingVoiceData)
				{
					pendingPlaybackDelay -= Time.deltaTime;

					if (pendingPlaybackDelay <= 0.0f)
					{
						isPlayingVoiceData = true;

						if (playbackUsingWalkieTalkie)
						{
							playWalkieTalkieSound();
						}

						audioSource.Play();

						// Update voice icon.
						isTalking = true;
					}
				}
			}
		}

		/// <summary>
		/// Will this component ever need to record voice data?
		/// </summary>
		private bool canEverRecord
		{
			get
			{
#if VOICE_CHAT_HEAR_SELF
				return channel.IsLocalPlayer;
#else
				return channel.IsLocalPlayer && !Provider.isServer;
#endif // VOICE_CHAT_HEAR_SELF
			}
		}

		/// <summary>
		/// Will this component ever need to play voice data?
		/// In release builds this is only true for remote clients, but in debug we may want to locally listen.
		/// </summary>
		private bool canEverPlayback
		{
			get
			{
#if VOICE_CHAT_HEAR_SELF
				return true;
#else
				return !channel.IsLocalPlayer;
#endif
			}
		}

		private void Update()
		{
			if (Dedicator.IsDedicatedServer)
				return;

			if (canEverRecord)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Input");
				updateInput();
				UnityEngine.Profiling.Profiler.EndSample();

				UnityEngine.Profiling.Profiler.BeginSample("Recording");
				updateRecording();
				UnityEngine.Profiling.Profiler.EndSample();
			}

			if (canEverPlayback)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Playback");
				updatePlayback();
				UnityEngine.Profiling.Profiler.EndSample();
			}
		}

		internal void InitializePlayer()
		{
			if (Dedicator.IsDedicatedServer)
				return;

			audioSource = GetComponent<AudioSource>();

			if (canEverRecord)
			{
				OptionsSettings.OnVoiceAlwaysRecordingChanged += SynchronizeSteamIsRecording;
				SynchronizeSteamIsRecording();
			}

			if (canEverPlayback)
			{
				steamOptimalSampleRate = SteamUser.GetVoiceOptimalSampleRate();
				int frequency = (int) steamOptimalSampleRate;
				secondsPerSample = 1.0f / frequency;
				int lengthSamples = frequency * 2; // 2 seconds
				audioData = new float[lengthSamples];
				zeroSamples = Mathf.CeilToInt(SILENCE_DURATION * 1.5f * frequency);

				// 2021-03-30 I did spend a serious amount of time experimenting with streaming audio PCMReaderCallback,
				// but Unity buffers too much data (even with a small <1000 bytes buffer size) for it to be viable for
				// voice. Note if we try again: lengthSamples should be a power of two to prevent extra allocations.
				const bool stream = false;
				audioClip = AudioClip.Create("Voice", lengthSamples, /*mono*/ 1, frequency, stream);
				audioSource.clip = audioClip;
			}
		}

		private void OnDestroy()
		{
			isBeingDestroyed = true;
			if (canEverRecord)
			{
				OptionsSettings.OnVoiceAlwaysRecordingChanged -= SynchronizeSteamIsRecording;

				// Setting inputWantsToRecord to false also stops recording.
				inputWantsToRecord = false;
			}
		}

		private void SynchronizeSteamIsRecording()
		{
			bool isRecordingEnabled = OptionsSettings.chatVoiceIn && OptionsSettings.EnableOutboundVoiceChat;
			SteamIsRecording = isRecordingEnabled && !isBeingDestroyed && (inputWantsToRecord || OptionsSettings.VoiceAlwaysRecording);
		}

		/// <summary>
		/// Player's voice audio source cached during Start.
		/// </summary>
		private AudioSource audioSource;

		/// <summary>
		/// Looping voice audio clip.
		/// </summary>
		private AudioClip audioClip;

		/// <summary>
		/// Playback buffer.
		/// </summary>
		private float[] audioData;

		private int audioDataWriteIndex;

		/// <summary>
		/// Steam does less work on the main thread if we request samples at the native decompresser sample rate,
		/// so the re-sampling can be done on the Unity audio thread instead.
		/// </summary>
		private uint steamOptimalSampleRate;

		/// <summary>
		/// 1 / frequency
		/// </summary>
		private float secondsPerSample;

		/// <summary>
		/// Number of samples to zero after writing new audio data.
		/// </summary>
		private int zeroSamples;

		private bool allowTalkingWhileDead = false;
		private bool _customAllowTalking = true;
		private bool customAllowTalking
		{
			get => _customAllowTalking;
			set
			{
				if (_customAllowTalking != value)
				{
					_customAllowTalking = value;
					OnCustomAllowTalkingChanged?.TryInvoke("OnCustomAllowTalkingChanged", this);
				}
			}
		}
	}
}
