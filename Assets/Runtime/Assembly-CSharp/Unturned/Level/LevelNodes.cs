////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class LevelNodes
	{
		private const byte SAVEDATA_VERSION_CONVERTED_NODE_VOLUMES = 8;
		private const byte SAVEDATA_VERSION_FINISHED_CONVERTING_ALL_NODES = 9;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_FINISHED_CONVERTING_ALL_NODES;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_NEWEST;

		private static Transform _models;
		[System.Obsolete("Was the parent of all editor nodes in the past, but now empty for TransformHierarchy performance.")]
		public static Transform models
		{
			get
			{
				if (_models == null)
				{
					_models = new GameObject().transform;
					_models.name = "Nodes";
					_models.parent = Level.level;
					_models.tag = "Logic";
					_models.gameObject.layer = LayerMasks.LOGIC;

					CommandWindow.LogWarningFormat("Plugin referencing LevelNodes.models which has been deprecated.");
				}

				return _models;
			}
		}

		private static List<Node> _nodes;
		[System.Obsolete("All legacy node types have been converted to subclasses of IDevkitHierarchyItem")]
		public static List<Node> nodes => _nodes;

		/// <summary>
		/// If true then level should convert old node types to volumes.
		/// </summary>
		internal static bool hasLegacyVolumesForConversion;

		/// <summary>
		/// If true then level should convert old non-volumes types to devkit objects.
		/// </summary>
		internal static bool hasLegacyNodesForConversion;

		/// <summary>
		/// Hash of nodes file.
		/// Prevents using the level editor to make noLight nodes visible.
		/// </summary>
		public static byte[] hash
		{
			get;
			private set;
		}

		internal static void AutoConvertLegacyVolumes()
		{
			UnturnedLog.info("Auto converting legacy volumes");

			foreach (Node node in _nodes)
			{
				if (node is ArenaNode arenaNode)
				{
					GameObject volumeGameObject = new GameObject(); // Renames itself according to type.
					Transform volumeTransform = volumeGameObject.transform;
					volumeTransform.position = arenaNode.point;
					volumeTransform.rotation = Quaternion.identity;
					float radius = ArenaNode.CalculateRadiusFromNormalizedRadius(arenaNode._normalizedRadius);
					ArenaCompactorVolume volume = volumeGameObject.AddComponent<ArenaCompactorVolume>();
					volume.Shape = ELevelVolumeShape.Sphere;
					volume.SetSphereRadius(radius);
					SDG.Framework.Devkit.LevelHierarchy.AssignInstanceIdAndMarkDirty(volume);
				}
				else if (node is DeadzoneNode deadzoneNode)
				{
					GameObject volumeGameObject = new GameObject(); // Renames itself according to type.
					Transform volumeTransform = volumeGameObject.transform;
					volumeTransform.position = deadzoneNode.point;
					volumeTransform.rotation = Quaternion.identity;
					float radius = DeadzoneNode.CalculateRadiusFromNormalizedRadius(deadzoneNode._normalizedRadius);
					SDG.Framework.Devkit.DeadzoneVolume volume = volumeGameObject.AddComponent<SDG.Framework.Devkit.DeadzoneVolume>();
					volume.DeadzoneType = deadzoneNode.DeadzoneType;
					volume.Shape = ELevelVolumeShape.Sphere;
					volume.SetSphereRadius(radius);
					SDG.Framework.Devkit.LevelHierarchy.AssignInstanceIdAndMarkDirty(volume);
				}
				else if (node is EffectNode effectNode)
				{
					GameObject volumeGameObject = new GameObject(); // Renames itself according to type.
					Transform volumeTransform = volumeGameObject.transform;
					volumeTransform.position = effectNode.point;
					volumeTransform.rotation = Quaternion.identity;

					SDG.Framework.Devkit.AmbianceVolume volume = volumeGameObject.AddComponent<SDG.Framework.Devkit.AmbianceVolume>();
					volume.id = effectNode.id;
					volume.noLighting = effectNode.noLighting;
					volume.noWater = effectNode.noWater;
					SDG.Framework.Devkit.LevelHierarchy.AssignInstanceIdAndMarkDirty(volume);

					if (effectNode.shape == ENodeShape.BOX)
					{
						// effectNode has a 2x2x2 box collider and sets localScale equal to bounds
						volumeTransform.localScale = effectNode.bounds * 2.0f;
					}
					else
					{
						float radius = EffectNode.CalculateRadiusFromNormalizedRadius(effectNode._normalizedRadius);
						volume.SetSphereRadius(radius);
						volume.Shape = ELevelVolumeShape.Sphere;
					}
				}
				else if (node is PurchaseNode purchaseNode)
				{
					GameObject volumeGameObject = new GameObject(); // Renames itself according to type.
					Transform volumeTransform = volumeGameObject.transform;
					volumeTransform.position = purchaseNode.point;
					volumeTransform.rotation = Quaternion.identity;
					float radius = PurchaseNode.CalculateRadiusFromNormalizedRadius(purchaseNode._normalizedRadius);
					HordePurchaseVolume volume = volumeGameObject.AddComponent<HordePurchaseVolume>();
					volume.Shape = ELevelVolumeShape.Sphere;
					volume.SetSphereRadius(radius);
					SDG.Framework.Devkit.LevelHierarchy.AssignInstanceIdAndMarkDirty(volume);
				}
				else if (node is SafezoneNode safezoneNode)
				{
					GameObject volumeGameObject = new GameObject(); // Renames itself according to type.
					Transform volumeTransform = volumeGameObject.transform;
					volumeTransform.rotation = Quaternion.identity;

					SafezoneVolume volume = volumeGameObject.AddComponent<SafezoneVolume>();
					volume.noWeapons = safezoneNode.noWeapons;
					volume.noBuildables = safezoneNode.noBuildables;
					volume.noIncomingDamage = safezoneNode.noWeapons;
					SDG.Framework.Devkit.LevelHierarchy.AssignInstanceIdAndMarkDirty(volume);

					if (safezoneNode.isHeight)
					{
						// This type was hacked-in for the paintball arena event. It was an infinite plane above
						// the selected point, so we approximate that with a giant box.
						volumeTransform.position = node.point + new Vector3(0.0f, 1000.0f, 0.0f);
						volumeTransform.localScale = new Vector3(10000.0f, 2000.0f, 10000.0f);
					}
					else
					{
						volumeTransform.position = node.point;
						float radius = SafezoneNode.CalculateRadiusFromNormalizedRadius(safezoneNode._normalizedRadius);
						volume.SetSphereRadius(radius);
						volume.Shape = ELevelVolumeShape.Sphere;
					}
				}
			}
		}

		internal static void AutoConvertLegacyNodes()
		{
			UnturnedLog.info("Auto converting legacy nodes");

			foreach (Node node in _nodes)
			{
				if (node is AirdropNode airdropNode)
				{
					GameObject nodeGameObject = new GameObject(); // Renames itself according to type.
					Transform nodeTransform = nodeGameObject.transform;
					nodeTransform.position = airdropNode.point;
					nodeTransform.rotation = Quaternion.identity;
					AirdropDevkitNode component = nodeGameObject.AddComponent<AirdropDevkitNode>();
#pragma warning disable
					component.id = airdropNode.id;
#pragma warning restore
					SDG.Framework.Devkit.LevelHierarchy.AssignInstanceIdAndMarkDirty(component);
				}
				else if (node is LocationNode locationNode)
				{
					GameObject nodeGameObject = new GameObject(); // Renames itself according to type.
					Transform nodeTransform = nodeGameObject.transform;
					nodeTransform.position = locationNode.point;
					nodeTransform.rotation = Quaternion.identity;
					LocationDevkitNode component = nodeGameObject.AddComponent<LocationDevkitNode>();
					component.locationName = locationNode.name;
					component.isVisibleOnMap = true;
					SDG.Framework.Devkit.LevelHierarchy.AssignInstanceIdAndMarkDirty(component);
				}
			}
		}

		internal static Node FindLocationNode(string id)
		{
			foreach (Node node in _nodes)
			{
				if (node.type == ENodeType.LOCATION && string.Equals(((LocationNode) node).name, id, System.StringComparison.InvariantCultureIgnoreCase))
				{
					return node;
				}
			}

			return null;
		}

		public static Transform addNode(Vector3 point, ENodeType type)
		{
			if (type == ENodeType.LOCATION)
			{
				_nodes.Add(new LocationNode(point));
			}
			else if (type == ENodeType.SAFEZONE)
			{
				_nodes.Add(new SafezoneNode(point));
			}
			else if (type == ENodeType.PURCHASE)
			{
				_nodes.Add(new PurchaseNode(point));
			}
			else if (type == ENodeType.ARENA)
			{
				_nodes.Add(new ArenaNode(point));
			}
			else if (type == ENodeType.DEADZONE)
			{
				_nodes.Add(new DeadzoneNode(point));
			}
			else if (type == ENodeType.AIRDROP)
			{
				_nodes.Add(new AirdropNode(point));
			}
			else if (type == ENodeType.EFFECT)
			{
				_nodes.Add(new EffectNode(point));
			}

			return _nodes[_nodes.Count - 1].model;
		}

		public static bool isPointInsideSafezone(Vector3 point, out SafezoneNode outSafezoneNode)
		{
			SafezoneVolume volume = SafezoneVolumeManager.Get().GetFirstOverlappingVolume(point);
			outSafezoneNode = volume?.backwardsCompatibilityNode;
			return volume != null;
		}

		public static void removeNode(Transform select)
		{
			for (int index = 0; index < _nodes.Count; index++)
			{
				if (_nodes[index].model == select)
				{
					_nodes[index].remove();
					_nodes.RemoveAt(index);

					return;
				}
			}
		}

		public static Node getNode(Transform select)
		{
			for (int index = 0; index < _nodes.Count; index++)
			{
				if (_nodes[index].model == select)
				{
					return _nodes[index];
				}
			}

			return null;
		}

		public static void load()
		{
			_nodes = new List<Node>();
			hasLegacyVolumesForConversion = false;
			hasLegacyNodesForConversion = false;

			if (ReadWrite.fileExists(Level.info.path + "/Environment/Nodes.dat", false, false))
			{
				River river = new River(Level.info.path + "/Environment/Nodes.dat", false);
				byte version = river.readByte();

				if (version > 0)
				{
					bool hasLegacyVolumes = false;
					bool hasLegacyNodes = false;

					ushort nodesCount = river.readByte();
					for (ushort nodeIndex = 0; nodeIndex < nodesCount; nodeIndex++)
					{
						Vector3 point = river.readSingleVector3();
						ENodeType type = (ENodeType) river.readByte();

						if (type == ENodeType.LOCATION)
						{
							hasLegacyNodes = true;

							string name = river.readString();

							_nodes.Add(new LocationNode(point, name));
						}
						else if (type == ENodeType.SAFEZONE)
						{
							hasLegacyVolumes = true;

							float radius = river.readSingle();
							bool isHeight = false;
							if (version > 1)
							{
								isHeight = river.readBoolean();
							}

							bool noWeapons = true;
							if (version > 4)
							{
								noWeapons = river.readBoolean();
							}

							bool noBuildables = true;
							if (version > 4)
							{
								noBuildables = river.readBoolean();
							}

							_nodes.Add(new SafezoneNode(point, radius, isHeight, noWeapons, noBuildables));
						}
						else if (type == ENodeType.PURCHASE)
						{
							hasLegacyVolumes = true;

							float radius = river.readSingle();
							ushort id = river.readUInt16();
							uint cost = river.readUInt32();

							_nodes.Add(new PurchaseNode(point, radius, id, cost));
						}
						else if (type == ENodeType.ARENA)
						{
							hasLegacyVolumes = true;

							float radius = river.readSingle();
							if (version < 6)
							{
								// Max diameter was doubled from 4096 to 8192 in v6.
								radius *= 0.5f;
							}

							_nodes.Add(new ArenaNode(point, radius));
						}
						else if (type == ENodeType.DEADZONE)
						{
							hasLegacyVolumes = true;

							float radius = river.readSingle();

							EDeadzoneType deadzoneType = EDeadzoneType.DefaultRadiation;
							if (version > 6)
							{
								deadzoneType = (EDeadzoneType) river.readByte();
							}

							_nodes.Add(new DeadzoneNode(point, radius, deadzoneType));
						}
						else if (type == ENodeType.AIRDROP)
						{
							hasLegacyNodes = true;

							ushort id = river.readUInt16();

							// Nelson 2025-03-10: Previously, this logged a warning if the returned ID was zero. This is
							// problematic now when (for example) referencing redirector asset that points to an asset
							// without a legacy ID.
							Asset test = SpawnTableTool.Resolve(id, EAssetType.ITEM, OnGetTestAirdropSpawnTableErrorContext);
							if (test == null && Assets.shouldLoadAnyAssets)
							{
								byte x;
								byte y;

								if (Regions.tryGetCoordinate(point, out x, out y))
								{
									Assets.reportError(Level.info.name + " airdrop references invalid spawn table " + id + " at (" + x + ", " + y + ")!");
								}
							}

							_nodes.Add(new AirdropNode(point, id));
						}
						else if (type == ENodeType.EFFECT)
						{
							hasLegacyVolumes = true;

							byte shape = 0;
							if (version > 2)
							{
								shape = river.readByte();
							}

							float radius = river.readSingle();

							Vector3 bounds = Vector3.one;
							if (version > 2)
							{
								bounds = river.readSingleVector3();
							}

							ushort id = river.readUInt16();
							bool noWater = river.readBoolean();
							bool noLighting = false;
							if (version > 3)
							{
								noLighting = river.readBoolean();
							}

							_nodes.Add(new EffectNode(point, (ENodeShape) shape, radius, bounds, id, noWater, noLighting));
						}
					}

					hasLegacyVolumesForConversion = hasLegacyVolumes && version < SAVEDATA_VERSION_CONVERTED_NODE_VOLUMES;
					hasLegacyNodesForConversion = hasLegacyNodes && version < SAVEDATA_VERSION_FINISHED_CONVERTING_ALL_NODES;
				}

				hash = river.getHash();
				river.closeRiver();
			}
			else
			{
				hash = new byte[20];
			}
		}

		public static void save()
		{
			River river = new River(Level.info.path + "/Environment/Nodes.dat", false);
			river.writeByte(SAVEDATA_VERSION_NEWEST);

			byte count = 0;
			for (ushort index = 0; index < _nodes.Count; index++)
			{
				if (_nodes[index].type != ENodeType.LOCATION || ((LocationNode) _nodes[index]).name.Length > 0)
				{
					count++;
				}
			}

			river.writeByte(count);
			for (byte nodeIndex = 0; nodeIndex < _nodes.Count; nodeIndex++)
			{
				if (_nodes[nodeIndex].type != ENodeType.LOCATION || ((LocationNode) _nodes[nodeIndex]).name.Length > 0)
				{
					river.writeSingleVector3(_nodes[nodeIndex].point);
					river.writeByte((byte) _nodes[nodeIndex].type);

					if (_nodes[nodeIndex].type == ENodeType.LOCATION)
					{
						river.writeString(((LocationNode) _nodes[nodeIndex]).name);
					}
					else if (_nodes[nodeIndex].type == ENodeType.SAFEZONE)
					{
						river.writeSingle(((SafezoneNode) _nodes[nodeIndex]).radius);
						river.writeBoolean(((SafezoneNode) _nodes[nodeIndex]).isHeight);
						river.writeBoolean(((SafezoneNode) _nodes[nodeIndex]).noWeapons);
						river.writeBoolean(((SafezoneNode) _nodes[nodeIndex]).noBuildables);
					}
					else if (_nodes[nodeIndex].type == ENodeType.PURCHASE)
					{
						river.writeSingle(((PurchaseNode) _nodes[nodeIndex]).radius);
						river.writeUInt16(((PurchaseNode) _nodes[nodeIndex]).id);
						river.writeUInt32(((PurchaseNode) _nodes[nodeIndex]).cost);
					}
					else if (_nodes[nodeIndex].type == ENodeType.ARENA)
					{
						river.writeSingle(((ArenaNode) _nodes[nodeIndex]).radius);
					}
					else if (_nodes[nodeIndex].type == ENodeType.DEADZONE)
					{
						river.writeSingle(((DeadzoneNode) _nodes[nodeIndex]).radius);
						river.writeByte((byte) ((DeadzoneNode) _nodes[nodeIndex]).DeadzoneType);
					}
					else if (_nodes[nodeIndex].type == ENodeType.AIRDROP)
					{
						river.writeUInt16(((AirdropNode) _nodes[nodeIndex]).id);
					}
					else if (_nodes[nodeIndex].type == ENodeType.EFFECT)
					{
						river.writeByte((byte) ((EffectNode) _nodes[nodeIndex]).shape);
						river.writeSingle(((EffectNode) _nodes[nodeIndex]).radius);
						river.writeSingleVector3(((EffectNode) _nodes[nodeIndex]).bounds);
						river.writeUInt16(((EffectNode) _nodes[nodeIndex]).id);
						river.writeBoolean(((EffectNode) _nodes[nodeIndex]).noWater);
						river.writeBoolean(((EffectNode) _nodes[nodeIndex]).noLighting);
					}
				}
			}

			river.closeRiver();
		}

		private static string OnGetTestAirdropSpawnTableErrorContext()
		{
			return "level nodes airdrop test";
		}
	}
}
