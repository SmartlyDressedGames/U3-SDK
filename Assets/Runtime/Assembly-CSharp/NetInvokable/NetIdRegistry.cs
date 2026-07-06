////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
// #define LOG_NET_ID

using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public static class NetIdRegistry
	{
		public static NetId Claim()
		{
			return new NetId(++counter);
		}

		public static NetId ClaimBlock(uint size)
		{
			NetId result = new NetId(counter + 1);
			counter += size;
			return result;
		}

		public static object Get(NetId key)
		{
			object value;
			pairings.TryGetValue(key, out value);
			return value;
		}

		public static T Get<T>(NetId key) where T : class
		{
			return Get(key) as T;
		}

		public static void Assign(NetId key, object value)
		{
			object existingValue;
			if (pairings.TryGetValue(key, out existingValue))
			{
				if (ReferenceEquals(value, existingValue))
				{
					UnturnedLog.error($"Net id {key} was already assigned to {value}");
				}
				else
				{
					UnturnedLog.error($"Net id {key} was previously assigned to {existingValue}, reassigning to {value}");
				}

				pairings[key] = value;
			}
			else
			{
				pairings.Add(key, value);
				Log($"Assigned netid {key}: {value}");
			}
		}

		public static void AssignTransform(NetId key, Transform value)
		{
			NetId existingKey;
			object existingValue;
			bool hasExistingKey = pairings.TryGetValue(key, out existingValue);
			bool hasExistingValue = transformPairings.TryGetValue(value, out existingKey);
			if (hasExistingKey && hasExistingValue)
			{
				if (ReferenceEquals(value, existingValue))
				{
					if (key == existingKey)
					{
						UnturnedLog.error($"Net id {key} and transform {value} were already assigned");
					}
					else
					{
						UnturnedLog.error($"Net id {key} was previously assigned to transform {value}, but transform was previously assigned to net id {existingKey}, reassigning");
					}
				}
				else
				{
					if (key == existingKey)
					{
						UnturnedLog.error($"Transform {value} was previously assigned to net id {key}, but net id was previously assigned to transform {existingValue}, reassigning");
					}
					else
					{
						UnturnedLog.error($"Net id {key} was previously assigned to {existingValue} and transform {value} was previously assigned to {existingKey}, reassigning");
					}
				}

				pairings[key] = value;
				transformPairings[value] = key;
			}
			else if (hasExistingKey)
			{
				if (ReferenceEquals(value, existingValue))
				{
					UnturnedLog.error($"Net id {key} was already assigned to transform {value}");
				}
				else
				{
					UnturnedLog.error($"Net id {key} was previously assigned to {existingValue}, reassigning to transform {value}");
				}

				pairings[key] = value;
				transformPairings.Add(value, key);
			}
			else if (hasExistingValue)
			{
				if (key == existingKey)
				{
					UnturnedLog.error($"Transform {value} was already assigned to net id {key}");
				}
				else
				{
					UnturnedLog.error($"Transform {value} was previously assigned to net id {existingKey}, reassigning to net id {key}");
				}

				pairings.Add(key, value);
				transformPairings[value] = key;
			}
			else
			{
				pairings.Add(key, value);
				transformPairings.Add(value, key);
				Log($"Assigned netid {key} to transform {value}");
			}
		}

		public static bool Release(NetId key)
		{
			bool result = pairings.Remove(key);
			Log(result ? $"Released netid {key}" : $"Released unused netid {key}");
			return result;
		}

		public static void ReleaseTransform(NetId key, Transform value)
		{
			bool keyResult = pairings.Remove(key);
			bool valueResult = transformPairings.Remove(value);
#if LOG_NET_ID
			if (keyResult && valueResult)
			{
				UnturnedLog.info($"Released net id {key} transform {value}");
			}
			else if(keyResult)
			{
				UnturnedLog.error($"Released net id {key} and unused transform {value}");
			}
			else if(valueResult)
			{
				UnturnedLog.error($"Released unused net id {key} and transform {value}");
			}
			else
			{
				UnturnedLog.info($"Released unused net id {key} and unused transform {value}");
			}
#endif // LOG_NET_ID
		}

		public static Transform GetTransform(NetId netId, string path)
		{
			Transform transform = Get<Transform>(netId);
			if (transform != null && !string.IsNullOrEmpty(path))
			{
				transform = transform.Find(path);
			}
			return transform;
		}

		/// <summary>
		/// Get net ID only if transform is directly registered, not if transform is the child of a registered transform.
		/// </summary>
		public static NetId GetTransformNetId(Transform transform)
		{
			NetId result;
			if (transform == null || !transformPairings.TryGetValue(transform, out result))
			{
				result = NetId.INVALID;
			}
			return result;
		}

		private static List<Transform> pathTransforms = new List<Transform>(16);
		private static System.Text.StringBuilder pathStringBuilder = new System.Text.StringBuilder(256);
		public static bool GetTransformNetId(Transform transform, out NetId netId, out string path)
		{
			if (transform == null)
			{
				netId = NetId.INVALID;
				path = null;
				return false;
			}

			if (transformPairings.TryGetValue(transform, out netId))
			{
				// Simplest special case we can return null path.
				path = null;
				return true;
			}

			Transform parent = transform.parent;
			if (parent != null)
			{
				if (transformPairings.TryGetValue(parent, out netId))
				{
					// In this case we do not need to use string builder.
					path = transform.name;
					return true;
				}

				Transform searchTransform = parent.parent;
				if (searchTransform != null)
				{
					pathTransforms.Clear();

					do
					{
						if (transformPairings.TryGetValue(searchTransform, out netId))
						{
							pathStringBuilder.Length = 0;
							for (int index = pathTransforms.Count - 1; index >= 0; --index)
							{
								pathStringBuilder.Append(pathTransforms[index].name);
								pathStringBuilder.Append('/');
							}

							pathStringBuilder.Append(parent.name);
							pathStringBuilder.Append('/');
							pathStringBuilder.Append(transform.name);

							path = pathStringBuilder.ToString();
							return true;
						}

						pathTransforms.Add(searchTransform);
						searchTransform = searchTransform.parent;
					} while (searchTransform != null);
				}
			}

			netId = NetId.INVALID;
			path = null;
			return false;
		}

		/// <summary>
		/// Log every registered pairing.
		/// </summary>
		public static void Dump()
		{
			Log($"{pairings.Count} registered net ids:");
			int index = 0;
			foreach (KeyValuePair<NetId, object> pair in pairings)
			{
				Log($"{index} - {pair.Key}: {pair.Value}");
				++index;
			}
		}

		/// <summary>
		/// Called before loading level.
		/// </summary>
		public static void Clear()
		{
			counter = 0;
			pairings.Clear();
			transformPairings.Clear();
		}

		[System.Diagnostics.Conditional("LOG_NET_ID")]
		private static void Log(string message)
		{
			UnturnedLog.info(message);
		}

		private static uint counter;
		private static Dictionary<NetId, object> pairings = new Dictionary<NetId, object>();

		/// <summary>
		/// Reverse pairing specifically for building net id + relative path name.
		/// </summary>
		private static Dictionary<Transform, NetId> transformPairings = new Dictionary<Transform, NetId>();
	}
}
