////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class UnturnedPathfinding_Empty : IUnturnedPathfindingInterface
	{
		public void OnGameLevelInstantiated() { }
		public IUnturnedNavmeshInterface CreateNavmesh() => new UnturnedNavmesh_Empty();
		public IUnturnedPerNavmeshEditorInterface CreateFlag(Flag owner) => new UnturnedNavmeshFlag_Empty();
		public IUnturnedNavmeshCutInterface CreateCutForIOBS(InteractableObjectBinaryState iobs)
		{
			return null;
		}

		public System.Type GetCutComponentType()
		{
			return null;
		}

		public IUnturnedPathfindingMovementComponentInterface CreateMovementComponentForZombie(Zombie zombie)
		{
			return zombie.gameObject.AddComponent<NonPathfindingZombieMovementComponent>();
		}
	}

	public class UnturnedNavmesh_Empty : IUnturnedNavmeshInterface
	{
		public bool ContainsAnyBakedData => false;

		private Vector3 boundsCenter;
		private Vector3 boundsSize;
		private int tileXCount;
		private int tileZCount;

		private int[][] triangleArrays;
		private Vector3Int[][] vertexArrays;

		public void Deserialize(River river)
		{
			boundsCenter = river.readSingleVector3();
			boundsSize = river.readSingleVector3();

			tileXCount = river.readByte();
			tileZCount = river.readByte();

			triangleArrays = new int[tileXCount * tileZCount][];
			vertexArrays = new Vector3Int[tileXCount * tileZCount][];

			int arrayIndex = 0;
			for (int tileZ = 0; tileZ < tileZCount; tileZ++)
			{
				for (int tileX = 0; tileX < tileXCount; tileX++)
				{
					int[] triangles = new int[river.readUInt16()];
					for (int triIndex = 0; triIndex < triangles.Length; triIndex++)
					{
						triangles[triIndex] = river.readUInt16();
					}

					Vector3Int[] vertices = new Vector3Int[river.readUInt16()];
					for (int vertIndex = 0; vertIndex < vertices.Length; vertIndex++)
					{
						vertices[vertIndex] = new Vector3Int(river.readInt32(), river.readInt32(), river.readInt32());
					}

					triangleArrays[arrayIndex] = triangles;
					vertexArrays[arrayIndex] = vertices;
					++arrayIndex;
				}
			}
		}

		public void Serialize(River river)
		{
			river.writeSingleVector3(boundsCenter);
			river.writeSingleVector3(boundsSize);

			river.writeByte((byte) tileXCount);
			river.writeByte((byte) tileZCount);

			int arrayIndex = 0;
			for (int z = 0; z < tileZCount; z++)
			{
				for (int x = 0; x < tileXCount; x++)
				{
					int[] triangles = triangleArrays[arrayIndex];
					Vector3Int[] vertices = vertexArrays[arrayIndex];

					river.writeUInt16((ushort) triangles.Length);
					for (int triIndex = 0; triIndex < triangles.Length; triIndex++)
					{
						river.writeUInt16((ushort) triangles[triIndex]);
					}

					river.writeUInt16((ushort) vertices.Length);
					for (int vertIndex = 0; vertIndex < vertices.Length; vertIndex++)
					{
						Vector3Int vert = vertices[vertIndex];

						river.writeInt32(vert.x);
						river.writeInt32(vert.y);
						river.writeInt32(vert.z);
					}

					++arrayIndex;
				}
			}
		}
	}

	public class UnturnedNavmeshFlag_Empty : IUnturnedPerNavmeshEditorInterface
	{
		public int GraphIndexForUI => -1;
		public void OnDestroy() { }
		public void Bake() { }
	}
}
