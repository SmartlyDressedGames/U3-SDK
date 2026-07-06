////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
//#define ENABLE_HOUSING_GIZMOS
#endif
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	/// <summary>
	/// Initially these were structs so that they would be adjacent in memory and therefore faster to iterate lots of them,
	/// but making them classes lets them reference each other which significantly simplifies finding adjactent housing parts.
	/// </summary>
	internal class HousingEdge
	{
		// Center position between both attachments.
		public Vector3 position;

		// Direction from backwardFloors toward forwardFloors.
		// Newly created edges point away from the floor, so new floors are in the backwardFloors.
		public Vector3 direction;

		// Yaw if placing triangle along direction.
		public float rotation;

		/// <summary>
		/// Item along positive direction.
		/// Can be multiple on existing saves or if players found an exploit.
		/// </summary>
		public List<StructureDrop> forwardFloors;

		/// <summary>
		/// Item along negative direction.
		/// Can be multiple on existing saves or if players found an exploit.
		/// </summary>
		public List<StructureDrop> backwardFloors;

		/// <summary>
		/// Item between floors.
		/// Can be multiple on existing saves or if players found an exploit.
		/// </summary>
		public List<StructureDrop> walls;

		public HousingVertex vertex0;
		public HousingVertex vertex1;

		public HousingEdge upperEdge;
		public HousingEdge lowerEdge;

		public bool ShouldBeRemoved => backwardFloors.IsEmpty() && forwardFloors.IsEmpty() && walls.IsEmpty() && (lowerEdge == null || lowerEdge.walls.IsEmpty());

		/// <summary>
		/// Is there a wall in this slot, and is it full height (not rampart)?
		/// </summary>
		public bool HasFullHeightWall()
		{
			foreach (StructureDrop wall in walls)
			{
				if (wall.asset.construct == EConstruct.WALL)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// This check prevents placing roof onto the upper edge of a rampart because ramparts
		/// create an edge at full wall height even though they are short.
		/// 
		/// Ideally in the future wall height will become configurable and remove
		/// the need for this check.
		///
		/// See public issue #3590.
		/// </summary>
		public bool CanAttachRoof
		{
			get
			{
				if (forwardFloors.Count + backwardFloors.Count + walls.Count > 0)
				{
					// Snapping onto the edge of an existing floor/roof,
					// or the bottom of a wall/rampart.
					return true;
				}

				if (lowerEdge != null && lowerEdge.HasFullHeightWall())
				{
					// Can place a roof if snapping onto top of a full-height wall.
					return true;
				}

				if (vertex0 != null && vertex1 != null && vertex0.lowerVertex != null && vertex1.lowerVertex != null && vertex0.lowerVertex.HasFullHeightPillar() && vertex1.lowerVertex.HasFullHeightPillar())
				{
					// Can place a roof on rampart if supported by two full-height pillars.
					return true;
				}

				return false;
			}
		}
	}

	internal class HousingVertex
	{
		/// <summary>
		/// Position at the base of the pillar.
		/// </summary>
		public Vector3 position;

		/// <summary>
		/// Yaw if placing pillar at this vertex.
		/// </summary>
		public float rotation;

		/// <summary>
		/// Pillar or post currently occupying this slot.
		/// Can be multiple on existing saves or if players found an exploit.
		/// </summary>
		public List<StructureDrop> pillars = new List<StructureDrop>(1);

		/// <summary>
		/// Can be zero if pillar is floating, or up to six in the center of a triangular circle.
		/// </summary>
		public List<StructureDrop> floors = new List<StructureDrop>(4);

		public List<HousingEdge> edges = new List<HousingEdge>(4);

		public HousingVertex upperVertex;
		public HousingVertex lowerVertex;

		public bool ShouldBeRemoved => pillars.Count < 1 && floors.Count < 1 && edges.Count < 1 && (lowerVertex == null || lowerVertex.pillars.IsEmpty());

		/// <summary>
		/// Is there a pillar in this slot, and is it full height (not post)?
		/// </summary>
		public bool HasFullHeightPillar()
		{
			foreach (StructureDrop pillar in pillars)
			{
				if (pillar.asset.construct == EConstruct.PILLAR)
				{
					return true;
				}
			}

			return false;
		}
	}

	internal enum EHousingPlacementResult
	{
		Success,
		MissingSlot,
		Obstructed,
		MissingPillar,

		/// <summary>
		/// Floors must be placed touching the terrain, or a fake-terrain object like a grassy cliff model.
		/// </summary>
		MissingGround,

		/// <summary>
		/// Pillars can be partly underground or inside a designated allowed underground area. Otherwise,
		/// if the very top of the pillar is underground placement is blocked. (public issue #4250)
		/// </summary>
		ObstructedByGround,
	}

	internal abstract class HousingConnectionData
	{ }

	internal class HousingWallConnections : HousingConnectionData
	{
		public HousingEdge lowerEdge;
		public HousingEdge upperEdge;
	}

	internal class HousingPillarConnections : HousingConnectionData
	{
		public HousingVertex lowerVertex;
		public HousingVertex upperVertex;
	}

	internal class HousingSquareFloorConnections : HousingConnectionData
	{
		public HousingVertex vertex0;
		public HousingVertex vertex1;
		public HousingVertex vertex2;
		public HousingVertex vertex3;

		public HousingEdge edge0;
		public HousingEdge edge1;
		public HousingEdge edge2;
		public HousingEdge edge3;
	}

	internal class HousingTriangleFloorConnections : HousingConnectionData
	{
		public HousingVertex vertex0;
		public HousingVertex vertex1;
		public HousingVertex vertex2;

		public HousingEdge edge0;
		public HousingEdge edge1;
		public HousingEdge edge2;
	}

	public class HousingConnections
	{
		/// <summary>
		/// Side length of square and triangular floor/roof.
		/// Walls can be slightly less, but we treat them as if they are the full length.
		/// </summary>
		public const float EDGE_LENGTH = 6.0f;
		public const float HALF_EDGE_LENGTH = 3.0f;

		public const float WALL_HEIGHT = 4.25f;
		public const float HALF_WALL_HEIGHT = 2.125f;

		/// <summary>
		/// Vertical distance from edge center to wall pivot.
		/// </summary>
		public const float WALL_PIVOT_OFFSET = HALF_WALL_HEIGHT;

		/// <summary>
		/// Vertical distance from edge center to rampart pivot.
		/// </summary>
		public const float RAMPART_PIVOT_OFFSET = 0.9f;

		private const float FOUNDATION_HEIGHT = 10.25f;
		private const float HALF_FOUNDATION_HEIGHT = 5.125f;
		private const float FOUNDATION_CENTER_OFFSET = -4.875f;

		/// <summary>
		/// If position is nearly equal within this threshold then edges/vertices will connect.
		/// </summary>
		private const float LINK_TOLERANCE = 0.02f; // 2cm

		/// <summary>
		/// Maximum distance from player's viewpoint to allow placement.
		/// </summary>
		internal const float MAX_PLACEMENT_DISTANCE = 16.0f; // 16m
		internal const float MAX_PLACEMENT_SQR_DISTANCE = MAX_PLACEMENT_DISTANCE * MAX_PLACEMENT_DISTANCE;

		/// <summary>
		/// How far to search for empty slot best match.
		/// </summary>
		private const float MAX_FIND_EMPTY_SLOT_DISTANCE = 8.0f; // 8m
		private const float MAX_FIND_EMPTY_SLOT_SQR_DISTANCE = MAX_FIND_EMPTY_SLOT_DISTANCE * MAX_FIND_EMPTY_SLOT_DISTANCE;

		/// <summary>
		/// Cosine of the angle between ray direction and direction toward slot must be greater than this.
		/// </summary>
		private const float MIN_FIND_EMPTY_SLOT_COSINE = 0.9f;

		/// <summary>
		/// When validating item placement expand physics overlap this much.
		/// Useful to ensure slightly-touching overlaps (e.g. pillar touching the pillar above) are handled properly.
		/// </summary>
		private const float PLACEMENT_OVERLAP_PADDING = 0.02f; // 2cm

		/// <summary>
		/// Ensure players, vehicles, zombies, animals, etc are not within this distance of pending placement.
		/// </summary>
		private const float CHARACTER_OVERLAP_PADDING = 0.25f; // 25cm
		private const float HALF_CHARACTER_OVERLAP_PADDING = CHARACTER_OVERLAP_PADDING * 0.5f;

		/// <summary>
		/// Distance from triangle pivot to apex of triangle.
		/// </summary>
		private const float TRIANGLE_APEX_PIVOT_OFFSET = 2.1961524227066318805823390245176f; // 6 - (sin(60) * 6)

		/// <summary>
		/// Radius of circle within triangle edges.
		/// </summary>
		private const float TRIANGLE_INNER_RADIUS = 1.7320508075688772935274463415059f; // tan(30) * HALF_SIDE_LENGTH

		/// <summary>
		/// Distance from triangle pivot to center of triangle.
		/// </summary>
		internal const float TRIANGLE_CENTER_PIVOT_OFFSET = -HALF_EDGE_LENGTH + TRIANGLE_INNER_RADIUS;

		/// <summary>
		/// Small threshold to allow placing even with existing barricades on the floor.
		/// </summary>
		private const float FOUNDATION_TOP_MARGIN = 0.1f; // 10cm
		private const float HALF_FOUNDATION_TOP_MARGIN = FOUNDATION_TOP_MARGIN * 0.5f;

		private const float ROOF_THICKNESS = 0.5f;
		private const float HALF_ROOF_THICKNESS = 0.25f;

		/// <summary>
		/// House overlap is approximately the same size as the housing item's collider(s), and is intended to check whether
		/// any pre-existing barricades or structural items are in the way. For example whether a wall cannot be placed because
		/// there is a storage crate in the way, or if a foundation is blocked by another slightly rotated foundation.
		/// </summary>
		private const int HOUSE_OVERLAP_LAYER_MASK = RayMasks.BARRICADE | RayMasks.STRUCTURE;

		/// <summary>
		/// Character overlap is slightly larger than the house overlap, and checks whether any players, vehicles, animals, zombies, etc
		/// are nearby. This is necessary because when house and characters were combined in a single physics query it was possible to
		/// stand *just* close enough to step into the collider as it was spawned.
		/// </summary>
		private const int CHARACTER_OVERLAP_LAYER_MASK = RayMasks.VEHICLE | RayMasks.PLAYER | RayMasks.ENEMY | RayMasks.AGENT;

		public HousingConnections()
		{
			edgesGrid = new RegionList<HousingEdge>();
			verticesGrid = new RegionList<HousingVertex>();
		}

		internal void OnLogMemoryUsage(List<string> results)
		{
			int edgeCount = 0;
			int edgeWalls = 0;
			int edgeFloors = 0;
			foreach (HousingEdge edge in edgesGrid.EnumerateAllItems())
			{
				++edgeCount;
				edgeWalls += edge.walls?.Count ?? 0;
				edgeFloors += edge.forwardFloors?.Count ?? 0;
				edgeFloors += edge.backwardFloors?.Count ?? 0;
			}

			results.Add($"Housing connection edges: {edgeCount}");
			results.Add($"Housing connection edge walls: {edgeWalls}");
			results.Add($"Housing connection edge floors: {edgeFloors}");

			int vertexCount = 0;
			int vertexPillars = 0;
			int vertexFloors = 0;
			int vertexEdges = 0;
			foreach (HousingVertex vertex in verticesGrid.EnumerateAllItems())
			{
				++vertexCount;
				vertexPillars += vertex.pillars?.Count ?? 0;
				vertexFloors += vertex.floors?.Count ?? 0;
				vertexEdges += vertex.edges?.Count ?? 0;
			}

			results.Add($"Housing connection vertices: {vertexCount}");
			results.Add($"Housing connection vertex pillars: {vertexPillars}");
			results.Add($"Housing connection vertex floors: {vertexFloors}");
			results.Add($"Housing connection vertex edges: {vertexEdges}");
		}

		/// <summary>
		/// Called when a housing item is spawned or after moving an existing item.
		/// </summary>
		internal void LinkConnections(StructureDrop drop)
		{
			switch (drop.asset.construct)
			{
				case EConstruct.FLOOR:
					LinkSquareFloor(drop);
					return;

				case EConstruct.WALL:
					LinkWall(drop, -WALL_PIVOT_OFFSET);
					return;

				case EConstruct.RAMPART:
					LinkWall(drop, -RAMPART_PIVOT_OFFSET);
					return;

				case EConstruct.ROOF:
					LinkSquareFloor(drop);
					return;

				case EConstruct.PILLAR:
					LinkPillar(drop, drop.model.position + new Vector3(0.0f, -WALL_PIVOT_OFFSET, 0.0f));
					return;

				case EConstruct.POST:
					LinkPillar(drop, drop.model.position + new Vector3(0.0f, -RAMPART_PIVOT_OFFSET, 0.0f));
					return;

				case EConstruct.FLOOR_POLY:
					LinkTriangularFloor(drop);
					return;

				case EConstruct.ROOF_POLY:
					LinkTriangularFloor(drop);
					return;
			}

			UnturnedLog.error($"Link housing connection unhandled: {drop.asset.construct}");
		}

		/// <summary>
		/// Called before a housing item is destroyed or before moving a housing item.
		/// </summary>
		internal void UnlinkConnections(StructureDrop drop)
		{
			switch (drop.asset.construct)
			{
				case EConstruct.FLOOR:
					UnlinkSquareFloor(drop);
					return;

				case EConstruct.WALL:
				case EConstruct.RAMPART:
					UnlinkWall(drop);
					return;

				case EConstruct.ROOF:
					UnlinkSquareFloor(drop);
					return;

				case EConstruct.PILLAR:
				case EConstruct.POST:
					UnlinkPillar(drop);
					return;

				case EConstruct.FLOOR_POLY:
					UnlinkTriangleFloor(drop);
					return;

				case EConstruct.ROOF_POLY:
					UnlinkTriangleFloor(drop);
					return;
			}

			UnturnedLog.error($"Unlink housing connection unhandled: {drop.asset.construct}");
		}

		/// <summary>
		/// Search grid for existing vertex at approximately equal position.
		/// Considers adjacent grid cells if near cell boundary to avoid issues with floating point inaccuracy. 
		/// </summary>
		private HousingVertex FindVertex(Vector3 position)
		{
			foreach (HousingVertex vertex in verticesGrid.EnumerateItemsInSquare(position, LINK_TOLERANCE))
			{
				if (vertex.position.IsNearlyEqual(position, LINK_TOLERANCE))
				{
					return vertex;
				}
			}

			return null;
		}

		/// <summary>
		/// Search grid for existing edge at approximately equal position.
		/// Considers adjacent grid cells if near cell boundary to avoid issues with floating point inaccuracy. 
		/// </summary>
		private HousingEdge FindEdge(Vector3 position)
		{
			foreach (HousingEdge edge in edgesGrid.EnumerateItemsInSquare(position, LINK_TOLERANCE))
			{
				if (edge.position.IsNearlyEqual(position, LINK_TOLERANCE))
				{
					return edge;
				}
			}

			return null;
		}

		private void RemoveVertex(HousingVertex vertex)
		{
			foreach (HousingEdge edge in vertex.edges)
			{
				if (edge.vertex0 == vertex)
				{
					edge.vertex0 = null;
				}
				else if (edge.vertex1 == vertex)
				{
					edge.vertex1 = null;
				}
			}
			vertex.edges.Clear();

			if (vertex.upperVertex != null)
			{
				vertex.upperVertex.lowerVertex = null;
				vertex.upperVertex = null;
			}

			if (vertex.lowerVertex != null)
			{
				vertex.lowerVertex.upperVertex = null;
				vertex.lowerVertex = null;
			}

			if (!verticesGrid.RemoveFast(vertex.position, vertex, LINK_TOLERANCE))
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.LogError($"Failed to remove vertex from grid at {vertex.position}");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			}
		}

		private void RemoveEdge(HousingEdge edge)
		{
			if (edge.vertex0 != null)
			{
				if (!edge.vertex0.edges.RemoveFast(edge))
				{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					Debug.LogError($"Failed to remove edge from vertex0 at {edge.vertex0.position}");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
				}
				edge.vertex0 = null;
			}

			if (edge.vertex1 != null)
			{
				if (!edge.vertex1.edges.RemoveFast(edge))
				{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					Debug.LogError($"Failed to remove edge from vertex1 at {edge.vertex1.position}");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
				}
				edge.vertex1 = null;
			}

			if (edge.upperEdge != null)
			{
				edge.upperEdge.lowerEdge = null;
				edge.upperEdge = null;
			}

			if (edge.lowerEdge != null)
			{
				edge.lowerEdge.upperEdge = null;
				edge.lowerEdge = null;
			}

			if (!edgesGrid.RemoveFast(edge.position, edge, LINK_TOLERANCE))
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.LogError($"Failed to remove edge from grid at {edge.position}");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			}
		}

		private Vector3 GetFloorCenter(StructureDrop floor)
		{
			bool isTriangle = floor.asset.construct == EConstruct.FLOOR_POLY || floor.asset.construct == EConstruct.ROOF_POLY;
			return isTriangle ? floor.model.TransformPoint(0.0f, -TRIANGLE_CENTER_PIVOT_OFFSET, 0.0f) : floor.model.position;
		}

		private HousingEdge LinkSquareEdge(StructureDrop drop, Vector3 direction, float rotation)
		{
			Vector3 edgePosition = drop.model.position + (direction * HALF_EDGE_LENGTH);
			return LinkFloorEdge(drop, edgePosition, direction, rotation);
		}

		/// <summary>
		/// Find existing edge and add connection, or add new empty edge.
		/// </summary>
		private HousingEdge LinkFloorEdge(StructureDrop floor, Vector3 edgePosition, Vector3 direction, float rotation)
		{
			HousingEdge edge = FindEdge(edgePosition);
			if (edge != null)
			{
				if (edge.forwardFloors.IsEmpty() && edge.backwardFloors.IsEmpty())
				{
					// Floating wall? Reset direction and rotation.
					edge.direction = direction;
					edge.rotation = GetModelYaw(floor.model) + rotation;
					edge.backwardFloors.Add(floor);
				}
				else
				{
					if (Vector3.Dot(direction, edge.direction) > 0.0f)
					{
						edge.backwardFloors.Add(floor);
					}
					else
					{
						edge.forwardFloors.Add(floor);
					}
				}
			}
			else
			{
				edge = new HousingEdge();
				edge.position = edgePosition;
				edge.direction = direction;
				edge.rotation = GetModelYaw(floor.model) + rotation;
				edge.forwardFloors = new List<StructureDrop>(1);
				edge.backwardFloors = new List<StructureDrop>(1) { floor };
				edge.walls = new List<StructureDrop>(1);
				edgesGrid.Add(edgePosition, edge);
			}

			return edge;
		}

		/// <summary>
		/// Find existing vertex and add connection, or add new empty vertex.
		/// </summary>
		private HousingVertex LinkFloorVertex(StructureDrop floor, Vector3 vertexPosition)
		{
			HousingVertex vertex = FindVertex(vertexPosition);
			if (vertex != null)
			{
				if (vertex.floors.Count < 1)
				{
					// Floating pillar? Reset vertex rotation.
					vertex.rotation = GetModelYaw(floor.model);
				}

				vertex.floors.Add(floor);
			}
			else
			{
				vertex = new HousingVertex();
				vertex.position = vertexPosition;
				vertex.rotation = GetModelYaw(floor.model);
				vertex.floors.Add(floor);
				verticesGrid.Add(vertexPosition, vertex);
			}

			return vertex;
		}

		private void UnlinkFloorEdge(StructureDrop floor, HousingEdge edge)
		{
			if (!edge.forwardFloors.RemoveFast(floor) && !edge.backwardFloors.RemoveFast(floor))
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.LogError($"Failed to remove floor from edge at {edge.position}");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			}

			if (edge.ShouldBeRemoved)
			{
				RemoveEdge(edge);
			}
		}

		private void UnlinkFloorVertex(StructureDrop floor, HousingVertex vertex)
		{
			if (!vertex.floors.RemoveFast(floor))
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.LogError($"Failed to remove floor from vertex at {vertex.position}");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			}

			if (vertex.ShouldBeRemoved)
			{
				RemoveVertex(vertex);
			}
		}

		private void UnlinkSquareFloor(StructureDrop floor)
		{
			HousingSquareFloorConnections connections = (HousingSquareFloorConnections) floor.housingConnectionData;
			floor.housingConnectionData = null;

			UnlinkFloorEdge(floor, connections.edge0);
			UnlinkFloorEdge(floor, connections.edge1);
			UnlinkFloorEdge(floor, connections.edge2);
			UnlinkFloorEdge(floor, connections.edge3);

			// Unlink vertices AFTER edges because vertex remains if any edges reference it.
			UnlinkFloorVertex(floor, connections.vertex0);
			UnlinkFloorVertex(floor, connections.vertex1);
			UnlinkFloorVertex(floor, connections.vertex2);
			UnlinkFloorVertex(floor, connections.vertex3);
		}

		private void UnlinkTriangleFloor(StructureDrop floor)
		{
			HousingTriangleFloorConnections connections = (HousingTriangleFloorConnections) floor.housingConnectionData;
			floor.housingConnectionData = null;

			UnlinkFloorEdge(floor, connections.edge0);
			UnlinkFloorEdge(floor, connections.edge1);
			UnlinkFloorEdge(floor, connections.edge2);

			// Unlink vertices AFTER edges because vertex remains if any edges reference it.
			UnlinkFloorVertex(floor, connections.vertex0);
			UnlinkFloorVertex(floor, connections.vertex1);
			UnlinkFloorVertex(floor, connections.vertex2);
		}

		private void LinkEdgeWithVertices(HousingEdge edge, HousingVertex vertex0, HousingVertex vertex1)
		{
			if (edge.vertex0 != vertex0 && edge.vertex1 != vertex0)
			{
				if (edge.vertex0 == null)
				{
					edge.vertex0 = vertex0;
				}
				else
				{
					edge.vertex1 = vertex0;
				}
				vertex0.edges.Add(edge);
			}

			if (edge.vertex0 != vertex1 && edge.vertex1 != vertex1)
			{
				if (edge.vertex0 == null)
				{
					edge.vertex0 = vertex1;
				}
				else
				{
					edge.vertex1 = vertex1;
				}
				vertex1.edges.Add(edge);
			}
		}

		private void LinkSquareFloor(StructureDrop drop)
		{
			HousingSquareFloorConnections connections = new HousingSquareFloorConnections();
			drop.housingConnectionData = connections;

			connections.edge0 = LinkSquareEdge(drop, drop.model.TransformDirection(new Vector3(1.0f, 0.0f, 0.0f)), 270.0f);
			connections.edge1 = LinkSquareEdge(drop, drop.model.TransformDirection(new Vector3(-1.0f, 0.0f, 0.0f)), 90.0f);
			connections.edge2 = LinkSquareEdge(drop, drop.model.TransformDirection(new Vector3(0.0f, 1.0f, 0.0f)), 0.0f);
			connections.edge3 = LinkSquareEdge(drop, drop.model.TransformDirection(new Vector3(0.0f, -1.0f, 0.0f)), 180.0f);

			connections.vertex0 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(HALF_EDGE_LENGTH, HALF_EDGE_LENGTH, 0.0f)));
			connections.vertex1 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(-HALF_EDGE_LENGTH, HALF_EDGE_LENGTH, 0.0f)));
			connections.vertex2 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(HALF_EDGE_LENGTH, -HALF_EDGE_LENGTH, 0.0f)));
			connections.vertex3 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(-HALF_EDGE_LENGTH, -HALF_EDGE_LENGTH, 0.0f)));

			LinkEdgeWithVertices(connections.edge0, connections.vertex0, connections.vertex2);
			LinkEdgeWithVertices(connections.edge1, connections.vertex1, connections.vertex3);
			LinkEdgeWithVertices(connections.edge2, connections.vertex0, connections.vertex1);
			LinkEdgeWithVertices(connections.edge3, connections.vertex2, connections.vertex3);
		}

		private readonly Vector3 leftLocalDirection = new Vector3(0.86602540378443864676372317075294f, -0.5f, 0.0f);
		private readonly Vector3 rightLocalDirection = new Vector3(-0.86602540378443864676372317075294f, -0.5f, 0.0f);

		private void LinkTriangularFloor(StructureDrop drop)
		{
			HousingTriangleFloorConnections connections = new HousingTriangleFloorConnections();
			drop.housingConnectionData = connections;

			Vector3 forward = drop.model.TransformDirection(new Vector3(0.0f, 1.0f, 0.0f));
			connections.edge0 = LinkFloorEdge(drop, drop.model.position + (forward * HALF_EDGE_LENGTH), forward, 0.0f);

			const float angleOffsetY = 0.40192378864668405970883048774119f; // 3 - (sin(60) * 3)

			Vector3 leftPosition = drop.model.TransformPoint(new Vector3(1.5f, angleOffsetY, 0.0f));
			Vector3 leftDirection = drop.model.TransformDirection(leftLocalDirection);
			connections.edge1 = LinkFloorEdge(drop, leftPosition, leftDirection, 240.0f);

			Vector3 rightPosition = drop.model.TransformPoint(new Vector3(-1.5f, angleOffsetY, 0.0f));
			Vector3 rightDirection = drop.model.TransformDirection(rightLocalDirection);
			connections.edge2 = LinkFloorEdge(drop, rightPosition, rightDirection, 120.0f);

			// Flat edge vertices
			connections.vertex0 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(HALF_EDGE_LENGTH, HALF_EDGE_LENGTH, 0.0f)));
			connections.vertex1 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(-HALF_EDGE_LENGTH, HALF_EDGE_LENGTH, 0.0f)));

			// Apex
			connections.vertex2 = LinkFloorVertex(drop, drop.model.TransformPoint(new Vector3(0.0f, -TRIANGLE_APEX_PIVOT_OFFSET, 0.0f)));

			LinkEdgeWithVertices(connections.edge0, connections.vertex0, connections.vertex1);
			LinkEdgeWithVertices(connections.edge1, connections.vertex0, connections.vertex2);
			LinkEdgeWithVertices(connections.edge2, connections.vertex1, connections.vertex2);
		}

		/// <summary>
		/// Find existing edge and set associated wall, or add an empty edge at wall's location.
		/// </summary>
		private void LinkWall(StructureDrop wall, float pivotOffset)
		{
			Vector3 lowerEdgePosition = wall.model.position + new Vector3(0.0f, pivotOffset, 0.0f);
			Vector3 upperEdgePosition = lowerEdgePosition + new Vector3(0.0f, WALL_HEIGHT, 0.0f);

			HousingEdge existingLowerEdge = FindEdge(lowerEdgePosition);
			HousingEdge existingUpperEdge = FindEdge(upperEdgePosition);

			HousingEdge lowerEdge = existingLowerEdge;
			if (lowerEdge == null)
			{
				Vector3 lowerVertexPosition0 = wall.model.TransformPoint(HALF_EDGE_LENGTH, 0.0f, 0.0f) + new Vector3(0.0f, pivotOffset, 0.0f);
				Vector3 lowerVertexPosition1 = wall.model.TransformPoint(-HALF_EDGE_LENGTH, 0.0f, 0.0f) + new Vector3(0.0f, pivotOffset, 0.0f);

				HousingVertex lowerVertex0 = FindVertex(lowerVertexPosition0);
				if (lowerVertex0 == null)
				{
					lowerVertex0 = new HousingVertex();
					lowerVertex0.position = lowerVertexPosition0;
					lowerVertex0.rotation = GetModelYaw(wall.model); // Will be fixed when floor is placed.
					verticesGrid.Add(lowerVertexPosition0, lowerVertex0);
				}

				HousingVertex lowerVertex1 = FindVertex(lowerVertexPosition1);
				if (lowerVertex1 == null)
				{
					lowerVertex1 = new HousingVertex();
					lowerVertex1.position = lowerVertexPosition1;
					lowerVertex1.rotation = GetModelYaw(wall.model); // Will be fixed when floor is placed.
					verticesGrid.Add(lowerVertexPosition1, lowerVertex1);
				}

				lowerEdge = new HousingEdge();
				lowerEdge.position = lowerEdgePosition;
				lowerEdge.direction = wall.model.TransformDirection(0.0f, 1.0f, 0.0f);
				lowerEdge.rotation = GetModelYaw(wall.model);
				lowerEdge.walls = new List<StructureDrop>(1);
				lowerEdge.forwardFloors = new List<StructureDrop>(1);
				lowerEdge.backwardFloors = new List<StructureDrop>(1);

				lowerEdge.vertex0 = lowerVertex0;
				lowerVertex0.edges.Add(lowerEdge);

				lowerEdge.vertex1 = lowerVertex1;
				lowerVertex1.edges.Add(lowerEdge);

				edgesGrid.Add(lowerEdgePosition, lowerEdge);
			}

			HousingEdge upperEdge = existingUpperEdge;
			if (upperEdge == null)
			{
				Vector3 upperVertexPosition0 = wall.model.TransformPoint(HALF_EDGE_LENGTH, 0.0f, 0.0f) + new Vector3(0.0f, pivotOffset + WALL_HEIGHT, 0.0f);
				Vector3 upperVertexPosition1 = wall.model.TransformPoint(-HALF_EDGE_LENGTH, 0.0f, 0.0f) + new Vector3(0.0f, pivotOffset + WALL_HEIGHT, 0.0f);

				HousingVertex upperVertex0 = FindVertex(upperVertexPosition0);
				if (upperVertex0 == null)
				{
					upperVertex0 = new HousingVertex();
					upperVertex0.position = upperVertexPosition0;
					upperVertex0.rotation = GetModelYaw(wall.model); // Will be fixed when floor is placed.
					verticesGrid.Add(upperVertexPosition0, upperVertex0);
				}

				HousingVertex upperVertex1 = FindVertex(upperVertexPosition1);
				if (upperVertex1 == null)
				{
					upperVertex1 = new HousingVertex();
					upperVertex1.position = upperVertexPosition1;
					upperVertex1.rotation = GetModelYaw(wall.model); // Will be fixed when floor is placed.
					verticesGrid.Add(upperVertexPosition1, upperVertex1);
				}

				upperEdge = new HousingEdge();
				upperEdge.position = upperEdgePosition;
				upperEdge.direction = wall.model.TransformDirection(0.0f, 1.0f, 0.0f);
				upperEdge.rotation = GetModelYaw(wall.model);
				upperEdge.walls = new List<StructureDrop>(1);
				upperEdge.forwardFloors = new List<StructureDrop>(1);
				upperEdge.backwardFloors = new List<StructureDrop>(1);

				upperEdge.vertex0 = upperVertex0;
				upperVertex0.edges.Add(upperEdge);

				upperEdge.vertex1 = upperVertex1;
				upperVertex1.edges.Add(upperEdge);

				edgesGrid.Add(upperEdgePosition, upperEdge);
			}

			upperEdge.lowerEdge = lowerEdge;
			lowerEdge.upperEdge = upperEdge;

			HousingWallConnections connections = new HousingWallConnections();
			wall.housingConnectionData = connections;
			connections.lowerEdge = lowerEdge;
			connections.upperEdge = upperEdge;
			lowerEdge.walls.Add(wall);
		}

		/// <summary>
		/// Find slot occupied by wall and remove if no longer attached to anything.
		/// </summary>
		private void UnlinkWall(StructureDrop wall)
		{
			HousingWallConnections connections = (HousingWallConnections) wall.housingConnectionData;
			wall.housingConnectionData = null;

			if (!connections.lowerEdge.walls.RemoveFast(wall))
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.LogError($"Failed to remove wall from edge at {connections.lowerEdge.position}");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			}

			if (connections.lowerEdge.ShouldBeRemoved)
			{
				// Removing edge also clears reference vertices.
				HousingVertex lowerVertex0 = connections.lowerEdge.vertex0;
				HousingVertex lowerVertex1 = connections.lowerEdge.vertex1;

				RemoveEdge(connections.lowerEdge);

				if (lowerVertex0.ShouldBeRemoved)
				{
					RemoveVertex(lowerVertex0);
				}
				if (lowerVertex1.ShouldBeRemoved)
				{
					RemoveVertex(lowerVertex1);
				}
			}

			// Upper edge ShouldBeRemoved returns false if still connected to a lower edge with a wall.
			if (connections.upperEdge.ShouldBeRemoved)
			{
				HousingVertex upperVertex0 = connections.upperEdge.vertex0;
				HousingVertex upperVertex1 = connections.upperEdge.vertex1;

				RemoveEdge(connections.upperEdge);

				if (upperVertex0.ShouldBeRemoved)
				{
					RemoveVertex(upperVertex0);
				}
				if (upperVertex1.ShouldBeRemoved)
				{
					RemoveVertex(upperVertex1);
				}
			}
		}

		/// <summary>
		/// Find existing vertex and set associated pillar, or add an empty vertex at pillar's location.
		/// </summary>
		private void LinkPillar(StructureDrop pillar, Vector3 lowerVertexPosition)
		{
			Vector3 upperVertexPosition = lowerVertexPosition + new Vector3(0.0f, WALL_HEIGHT, 0.0f);

			HousingVertex lowerVertex = FindVertex(lowerVertexPosition);
			if (lowerVertex == null)
			{
				// Either nearby floor/wall has not loaded yet, or maybe this pillar is floating.
				// Add a vertex that will get its rotation fixed when floor/wall is loaded.
				lowerVertex = new HousingVertex();
				lowerVertex.position = lowerVertexPosition;
				lowerVertex.rotation = GetModelYaw(pillar.model); // Will be fixed when floor is placed.
				verticesGrid.Add(lowerVertexPosition, lowerVertex);
			}

			HousingVertex upperVertex = FindVertex(upperVertexPosition);
			if (upperVertex == null)
			{
				upperVertex = new HousingVertex();
				upperVertex.position = upperVertexPosition;
				upperVertex.rotation = GetModelYaw(pillar.model); // Will be fixed when floor is placed.
				verticesGrid.Add(upperVertexPosition, upperVertex);
			}

			lowerVertex.upperVertex = upperVertex;
			upperVertex.lowerVertex = lowerVertex;

			HousingPillarConnections connections = new HousingPillarConnections();
			pillar.housingConnectionData = connections;
			connections.lowerVertex = lowerVertex;
			connections.upperVertex = upperVertex;
			lowerVertex.pillars.Add(pillar);
		}

		/// <summary>
		/// Find slot occupied by pillar and remove if no longer attached to anything.
		/// </summary>
		private void UnlinkPillar(StructureDrop pillar)
		{
			HousingPillarConnections connections = (HousingPillarConnections) pillar.housingConnectionData;
			pillar.housingConnectionData = null;

			if (!connections.lowerVertex.pillars.RemoveFast(pillar))
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.LogError($"Failed to remove pillar from vertex at {connections.lowerVertex.position}");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			}

			if (connections.lowerVertex.ShouldBeRemoved)
			{
				RemoveVertex(connections.lowerVertex);
			}

			// Upper vertex ShouldBeRemoved returns false if still connected to a lower vertex with a pillar.
			if (connections.upperVertex.ShouldBeRemoved)
			{
				RemoveVertex(connections.upperVertex);
			}
		}

		internal bool DoesHitCountAsTerrain(RaycastHit hit)
		{
			if (hit.transform == null)
			{
				return false;
			}

			if (hit.transform.CompareTag("Ground"))
			{
				// Literally hit the Terrain game object.
				return true;
			}
			else
			{
				ObjectAsset objAsset = LevelObjects.getAsset(hit.transform);
				return objAsset != null && objAsset.allowStructures;
			}
		}

		private float ScorePosition(Ray ray, Vector3 testPosition)
		{
			Vector3 delta = testPosition - ray.origin;
			float sqrDelta = delta.sqrMagnitude;
			if (sqrDelta > MAX_FIND_EMPTY_SLOT_SQR_DISTANCE)
			{
				// Out of range.
				return MAX_FIND_EMPTY_SLOT_SQR_DISTANCE + 1.0f;
			}

			Vector3 testDirection = (testPosition - ray.origin).normalized;
			float cosine = Vector3.Dot(testDirection, ray.direction);
			if (cosine < MIN_FIND_EMPTY_SLOT_COSINE)
			{
				// Too far away from ray.
				return MAX_FIND_EMPTY_SLOT_SQR_DISTANCE + 1.0f;
			}

			float score = (1.0f - cosine) * sqrDelta;
#if ENABLE_HOUSING_GIZMOS
			RuntimeGizmos.Get().Label(testPosition, score.ToString(), Color.magenta);
#endif // ENABLE_HOUSING_GIZMOS
			return score;
		}

		internal bool FindEmptyFloorSlot(Ray ray, bool isRoof, out Vector3 position, out float rotation)
		{
			position = default;
			rotation = default;

			float bestMatch = MAX_FIND_EMPTY_SLOT_SQR_DISTANCE;
			bool hasAnyMatch = false;

			foreach (HousingEdge edge in edgesGrid.EnumerateItemsInSquare(ray.origin, MAX_FIND_EMPTY_SLOT_DISTANCE))
			{
				if (isRoof && !edge.CanAttachRoof)
					continue;

				if (edge.forwardFloors.IsEmpty())
				{
					Vector3 testPosition = edge.position + (edge.direction * HALF_EDGE_LENGTH * 0.5f);
					float score = ScorePosition(ray, testPosition);
					if (score < bestMatch)
					{
						bestMatch = score;
						position = edge.position + (edge.direction * HALF_EDGE_LENGTH);
						rotation = edge.rotation + 180.0f;
						hasAnyMatch = true;
					}
				}

				if (edge.backwardFloors.IsEmpty())
				{
					Vector3 testPosition = edge.position - (edge.direction * HALF_EDGE_LENGTH * 0.5f);
					float score = ScorePosition(ray, testPosition);
					if (score < bestMatch)
					{
						bestMatch = score;
						position = edge.position - (edge.direction * HALF_EDGE_LENGTH);
						rotation = edge.rotation;
						hasAnyMatch = true;
					}
				}
			}

			return hasAnyMatch;
		}

		internal bool FindEmptyWallSlot(Ray ray, out Vector3 position, out float rotation)
		{
			position = default;
			rotation = default;

			float bestMatch = MAX_FIND_EMPTY_SLOT_SQR_DISTANCE;
			bool hasAnyMatch = false;

			foreach (HousingEdge edge in edgesGrid.EnumerateItemsInSquare(ray.origin, MAX_FIND_EMPTY_SLOT_DISTANCE))
			{
				if (edge.walls.IsEmpty())
				{
					Vector3 testPosition = edge.position + new Vector3(0.0f, HALF_WALL_HEIGHT, 0.0f);
					float score = ScorePosition(ray, testPosition);
					if (score < bestMatch)
					{
						bestMatch = score;
						position = testPosition;
						rotation = edge.rotation;
						hasAnyMatch = true;
					}
				}

				if (edge.lowerEdge == null || edge.lowerEdge.walls.IsEmpty())
				{
					Vector3 testPosition = edge.position + new Vector3(0.0f, -HALF_WALL_HEIGHT, 0.0f);
					float score = ScorePosition(ray, testPosition);
					if (score < bestMatch)
					{
						bestMatch = score;
						position = testPosition;
						rotation = edge.rotation;
						hasAnyMatch = true;
					}
				}
			}

			return hasAnyMatch;
		}

		internal bool FindEmptyPillarSlot(Ray ray, out Vector3 position, out float rotation)
		{
			position = default;
			rotation = default;

			float bestMatch = MAX_FIND_EMPTY_SLOT_SQR_DISTANCE;
			bool hasAnyMatch = false;

			foreach (HousingVertex vertex in verticesGrid.EnumerateItemsInSquare(ray.origin, MAX_FIND_EMPTY_SLOT_DISTANCE))
			{
				if (vertex.pillars.IsEmpty())
				{
					Vector3 testPosition = vertex.position + new Vector3(0.0f, HALF_WALL_HEIGHT, 0.0f);
					float score = ScorePosition(ray, testPosition);
					if (score < bestMatch)
					{
						bestMatch = score;
						position = testPosition;
						rotation = vertex.rotation;
						hasAnyMatch = true;
					}
				}

				if (vertex.lowerVertex == null || vertex.lowerVertex.pillars.IsEmpty())
				{
					Vector3 testPosition = vertex.position + new Vector3(0.0f, -HALF_WALL_HEIGHT, 0.0f);
					float score = ScorePosition(ray, testPosition);
					if (score < bestMatch)
					{
						bestMatch = score;
						position = testPosition;
						rotation = vertex.rotation;
						hasAnyMatch = true;
					}
				}
			}

			return hasAnyMatch;
		}

		private bool SnapFloorPlacementToEdge(bool isRoof, ref Vector3 placementPosition)
		{
			// Finding a floor edge is not strictly necessary because floors can be placed anymore, but fixes alignment on server when snapping.
			foreach (HousingEdge edge in edgesGrid.EnumerateItemsInSquare(placementPosition, HALF_EDGE_LENGTH + LINK_TOLERANCE))
			{
				if (!edge.backwardFloors.IsEmpty() && !edge.forwardFloors.IsEmpty())
				{
					// Both slots are already filled.
					continue;
				}

				if (isRoof && !edge.CanAttachRoof)
					continue;

				Vector3 forwardTestPosition = edge.position + (edge.direction * HALF_EDGE_LENGTH);
				if (forwardTestPosition.IsNearlyEqual(placementPosition, LINK_TOLERANCE))
				{
					// Copy slot's exact transform. On the server this prevents drift from server->client->server compression.
					placementPosition = forwardTestPosition;
					return true;
				}

				Vector3 backwardTestPosition = edge.position - (edge.direction * HALF_EDGE_LENGTH);
				if (backwardTestPosition.IsNearlyEqual(placementPosition, LINK_TOLERANCE))
				{
					// Copy slot's exact transform. On the server this prevents drift from server->client->server compression.
					placementPosition = backwardTestPosition;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Hack to prevent ignoring floor which might be overlapping pending floor placement.
		/// For example when placing a square floor on the opposite edge of a spot which has a triangular floor
		/// we do not want to ignore the triangular floor during the physics query.
		/// </summary>
		private void IgnoreVertexFloorsExceptNearPosition(HousingVertex vertex, Vector3 overlapCenter, float pendingItemRadius)
		{
			if (vertex == null)
				return;

			foreach (StructureDrop floor in vertex.floors)
			{
				bool isTriangle = floor.asset.construct == EConstruct.FLOOR_POLY || floor.asset.construct == EConstruct.ROOF_POLY;
				Vector3 floorCenter = isTriangle
					? floor.model.TransformPoint(0.0f, -TRIANGLE_CENTER_PIVOT_OFFSET, 0.0f)
					: floor.model.position;
				float floorRadius = (isTriangle ? TRIANGLE_INNER_RADIUS : HALF_EDGE_LENGTH) * 0.95f;
				float minRadius = pendingItemRadius + floorRadius;
				float sqrMinRadius = minRadius * minRadius;

				if ((floorCenter - overlapCenter).GetHorizontalSqrMagnitude() > sqrMinRadius)
				{
					ignoreDrops.Add(floor);
				}
#if ENABLE_HOUSING_GIZMOS
				else
				{
					RuntimeGizmos.Get().Sphere(floorCenter, minRadius, Color.red);
				}
#endif // ENABLE_HOUSING_GIZMOS
			}
		}

		private void IgnoreVertexPillarsFloorsAndWalls(HousingVertex vertex)
		{
			if (vertex == null)
				return;

			// Overlapping adjacent pillar is expected.
			ignoreDrops.AddAny(vertex.pillars);

			// Overlapping adjacent floor is expected.
			ignoreDrops.AddAny(vertex.floors);

			// Overlapping adjacent walls is expected.
			foreach (HousingEdge edge in vertex.edges)
			{
				ignoreDrops.AddAny(edge.walls);
			}
		}

		private void IgnoreVertexPillarsAndWalls(HousingVertex vertex)
		{
			if (vertex == null)
				return;

			// Overlapping pillar below is expected.
			ignoreDrops.AddAny(vertex.pillars);

			// Overlapping walls below is expected.
			foreach (HousingEdge edge in vertex.edges)
			{
				ignoreDrops.AddAny(edge.walls);
			}
		}

		private void IgnoreVertexFloors(HousingVertex vertex)
		{
			if (vertex == null)
				return;

			// Overlapping adjacent floor is expected.
			ignoreDrops.AddAny(vertex.floors);
		}

		private bool CanIgnoreOverlaps(int overlapCount, ref string obstructionHint)
		{
			for (int overlapIndex = 0; overlapIndex < overlapCount; ++overlapIndex)
			{
				Transform rootTransform = overlapBuffer[overlapIndex].transform.root;
				StructureDrop drop = StructureDrop.FindByTransformFastMaybeNull(rootTransform);
				if (drop == null)
				{
					// Not a housing part!
					// The only other layer type used for CanIgnoreOverlaps is Barricade.
					BarricadeDrop barricade = BarricadeDrop.FindByTransformFastMaybeNull(rootTransform);
					if (barricade != null && barricade.asset != null)
					{
						obstructionHint = barricade.asset.itemName;
					}
#if ENABLE_HOUSING_GIZMOS
					if (!string.IsNullOrEmpty(obstructionHint))
					{
						RuntimeGizmos.Get().Label(rootTransform.position, "Obstructed by: " + obstructionHint, Color.red);
					}
#endif // ENABLE_HOUSING_GIZMOS
					return false;
				}

				if (!ignoreDrops.Contains(drop))
				{
					obstructionHint = drop.asset.itemName;
#if ENABLE_HOUSING_GIZMOS
					RuntimeGizmos.Get().Label(rootTransform.position, "Obstructed by: " + drop.asset.itemName, Color.red);
#endif // ENABLE_HOUSING_GIZMOS
					return false;
				}
			}

			return true;
		}

		private bool IsFloorAboveGround(Vector3 center, float testHeight)
		{
			const int layerMask = RayMasks.GROUND | RayMasks.LARGE | RayMasks.MEDIUM;
			RaycastHit hit;
			if (Physics.Raycast(center + Vector3.up, Vector3.down, out hit, testHeight + 1.0f, layerMask))
			{
				return DoesHitCountAsTerrain(hit);
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Used by triangular floor and roof validation to test for collisions.
		/// </summary>
		private bool TestTriangleOverlapsCommon(Vector3 center, float placementRotation, float overlapPositionOffset, float overlapHalfHeight, ref string obstructionHint)
		{
			// We approximate triangle overlap with an inner and outer box along each edge. This length slightly overlaps in the center.
			const float outerSize = 0.6f;
			const float halfOuterSize = outerSize * 0.5f;
			const float innerSize = 1.15f;
			const float halfInnerSize = innerSize * 0.5f;
			const float halfInnerEdgeLength = HALF_EDGE_LENGTH * 0.65f;
			const float halfCharacterOverlapSize = 0.9f;

			Vector3 outerHalfExtents = new Vector3(HALF_EDGE_LENGTH + PLACEMENT_OVERLAP_PADDING, overlapHalfHeight, halfOuterSize + PLACEMENT_OVERLAP_PADDING);
			Vector3 innerHalfExtents = new Vector3(halfInnerEdgeLength + PLACEMENT_OVERLAP_PADDING, overlapHalfHeight, halfInnerSize + PLACEMENT_OVERLAP_PADDING);
			Vector3 characterHalfExtents = new Vector3(HALF_EDGE_LENGTH + CHARACTER_OVERLAP_PADDING, overlapHalfHeight + CHARACTER_OVERLAP_PADDING, halfCharacterOverlapSize + CHARACTER_OVERLAP_PADDING);

			bool TestOverlaps(Quaternion edgeRotation, ref string obstructionHint)
			{
				Vector3 edgeDirection = edgeRotation * Vector3.forward;

				Vector3 outerOverlapCenter = center + new Vector3(0.0f, overlapPositionOffset, 0.0f) + (edgeDirection * (-TRIANGLE_INNER_RADIUS + halfOuterSize));
				Vector3 innerOverlapCenter = center + new Vector3(0.0f, overlapPositionOffset, 0.0f) + (edgeDirection * (-TRIANGLE_INNER_RADIUS + outerSize + halfInnerSize));

				int outerOverlapCount = Physics.OverlapBoxNonAlloc(outerOverlapCenter, outerHalfExtents, overlapBuffer, edgeRotation, HOUSE_OVERLAP_LAYER_MASK, QueryTriggerInteraction.Collide);
				bool overlapTestPassed = CanIgnoreOverlaps(outerOverlapCount, ref obstructionHint);
				if (overlapTestPassed)
				{
					int innerOverlapCount = Physics.OverlapBoxNonAlloc(innerOverlapCenter, innerHalfExtents, overlapBuffer, edgeRotation, HOUSE_OVERLAP_LAYER_MASK, QueryTriggerInteraction.Collide);
					overlapTestPassed &= CanIgnoreOverlaps(innerOverlapCount, ref obstructionHint);
				}

#if ENABLE_HOUSING_GIZMOS
				Color color = overlapTestPassed ? Color.green : Color.red;
				RuntimeGizmos.Get().Box(outerOverlapCenter, edgeRotation, outerHalfExtents * 2.0f, color);
				RuntimeGizmos.Get().Box(innerOverlapCenter, edgeRotation, innerHalfExtents * 2.0f, color);
#endif // ENABLE_HOUSING_GIZMOS

				if (!overlapTestPassed)
				{
					return false;
				}

				Vector3 characterOverlapCenter = center + new Vector3(0.0f, overlapPositionOffset, 0.0f) + (edgeDirection * (-TRIANGLE_INNER_RADIUS + halfCharacterOverlapSize));
				bool characterOverlapTestPassed = !Physics.CheckBox(characterOverlapCenter, characterHalfExtents, edgeRotation, CHARACTER_OVERLAP_LAYER_MASK, QueryTriggerInteraction.Collide);

#if ENABLE_HOUSING_GIZMOS
				Color characterOverlapColor = characterOverlapTestPassed ? Color.cyan : Color.red;
				RuntimeGizmos.Get().Box(characterOverlapCenter, edgeRotation, characterHalfExtents * 2.0f, characterOverlapColor);
#endif // ENABLE_HOUSING_GIZMOS

				return characterOverlapTestPassed;
			}

			if (!TestOverlaps(Quaternion.Euler(0.0f, placementRotation, 0.0f), ref obstructionHint))
			{
				return false;
			}

			if (!TestOverlaps(Quaternion.Euler(0.0f, placementRotation + 120.0f, 0.0f), ref obstructionHint))
			{
				return false;
			}

			if (!TestOverlaps(Quaternion.Euler(0.0f, placementRotation - 120.0f, 0.0f), ref obstructionHint))
			{
				return false;
			}

			return true;
		}

		internal EHousingPlacementResult ValidateSquareFloorPlacement(float terrainTestHeight, ref Vector3 placementPosition, float placementRotation, ref string obstructionHint)
		{
			SnapFloorPlacementToEdge(false, ref placementPosition);
			if (!IsFloorAboveGround(placementPosition, terrainTestHeight))
			{
				return EHousingPlacementResult.MissingGround;
			}

			Vector3 overlapCenter = placementPosition + new Vector3(0.0f, FOUNDATION_CENTER_OFFSET - FOUNDATION_TOP_MARGIN, 0.0f);
			Vector3 overlapHalfExtents = new Vector3(HALF_EDGE_LENGTH + PLACEMENT_OVERLAP_PADDING, HALF_FOUNDATION_HEIGHT - HALF_FOUNDATION_TOP_MARGIN, HALF_EDGE_LENGTH + PLACEMENT_OVERLAP_PADDING);
			Quaternion overlapRotation = Quaternion.Euler(0.0f, placementRotation, 0.0f);

			Vector3 vertexPosition0 = placementPosition + (overlapRotation * new Vector3(HALF_EDGE_LENGTH, 0.0f, HALF_EDGE_LENGTH));
			Vector3 vertexPosition1 = placementPosition + (overlapRotation * new Vector3(-HALF_EDGE_LENGTH, 0.0f, HALF_EDGE_LENGTH));
			Vector3 vertexPosition2 = placementPosition + (overlapRotation * new Vector3(HALF_EDGE_LENGTH, 0.0f, -HALF_EDGE_LENGTH));
			Vector3 vertexPosition3 = placementPosition + (overlapRotation * new Vector3(-HALF_EDGE_LENGTH, 0.0f, -HALF_EDGE_LENGTH));

			HousingVertex vertex0 = FindVertex(vertexPosition0);
			HousingVertex vertex1 = FindVertex(vertexPosition1);
			HousingVertex vertex2 = FindVertex(vertexPosition2);
			HousingVertex vertex3 = FindVertex(vertexPosition3);

			ignoreDrops.Clear();
			IgnoreVertexPillarsAndWalls(vertex0);
			IgnoreVertexPillarsAndWalls(vertex1);
			IgnoreVertexPillarsAndWalls(vertex2);
			IgnoreVertexPillarsAndWalls(vertex3);
			IgnoreVertexFloorsExceptNearPosition(vertex0, overlapCenter, HALF_EDGE_LENGTH);
			IgnoreVertexFloorsExceptNearPosition(vertex1, overlapCenter, HALF_EDGE_LENGTH);
			IgnoreVertexFloorsExceptNearPosition(vertex2, overlapCenter, HALF_EDGE_LENGTH);
			IgnoreVertexFloorsExceptNearPosition(vertex3, overlapCenter, HALF_EDGE_LENGTH);

			// Low-hanging "roofs" like ramps and stairs may collide.
			HousingVertex vertexAbove0 = FindVertex(vertexPosition0 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			HousingVertex vertexAbove1 = FindVertex(vertexPosition1 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			HousingVertex vertexAbove2 = FindVertex(vertexPosition2 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			HousingVertex vertexAbove3 = FindVertex(vertexPosition3 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			IgnoreVertexFloors(vertexAbove0);
			IgnoreVertexFloors(vertexAbove1);
			IgnoreVertexFloors(vertexAbove2);
			IgnoreVertexFloors(vertexAbove3);

			int overlapCount = Physics.OverlapBoxNonAlloc(overlapCenter, overlapHalfExtents, overlapBuffer, overlapRotation, HOUSE_OVERLAP_LAYER_MASK, QueryTriggerInteraction.Collide);
			bool overlapTestPassed = CanIgnoreOverlaps(overlapCount, ref obstructionHint);

#if ENABLE_HOUSING_GIZMOS
			Color color = overlapTestPassed ? Color.green : Color.red;
			RuntimeGizmos.Get().Box(overlapCenter, overlapRotation, overlapHalfExtents * 2.0f, color);
			RuntimeGizmos.Get().Label(placementPosition + new Vector3(0.0f, WALL_HEIGHT * 0.25f, 0.0f), overlapCount.ToString(), color);

			if (vertex0 != null)
				RuntimeGizmos.Get().Cube(vertex0.position, 0.2f, color);
			if (vertex1 != null)
				RuntimeGizmos.Get().Cube(vertex1.position, 0.2f, color);
			if (vertex2 != null)
				RuntimeGizmos.Get().Cube(vertex2.position, 0.2f, color);
			if (vertex3 != null)
				RuntimeGizmos.Get().Cube(vertex3.position, 0.2f, color);
#endif // ENABLE_HOUSING_GIZMOS

			if (!overlapTestPassed)
			{
				return EHousingPlacementResult.Obstructed;
			}

			Vector3 characterOverlapHalfExtents = overlapHalfExtents + new Vector3(CHARACTER_OVERLAP_PADDING, CHARACTER_OVERLAP_PADDING, CHARACTER_OVERLAP_PADDING);
			bool characterOverlapTestPassed = !Physics.CheckBox(overlapCenter, characterOverlapHalfExtents, overlapRotation, CHARACTER_OVERLAP_LAYER_MASK, QueryTriggerInteraction.Collide);

#if ENABLE_HOUSING_GIZMOS
			Color characterOverlapColor = characterOverlapTestPassed ? Color.green : Color.red;
			RuntimeGizmos.Get().Box(overlapCenter, overlapRotation, characterOverlapHalfExtents * 2.0f, characterOverlapColor);
#endif // ENABLE_HOUSING_GIZMOS

			return characterOverlapTestPassed ? EHousingPlacementResult.Success : EHousingPlacementResult.Obstructed;
		}

		internal EHousingPlacementResult ValidateTriangleFloorPlacement(float terrainTestHeight, ref Vector3 placementPosition, float placementRotation, ref string obstructionHint)
		{
			SnapFloorPlacementToEdge(false, ref placementPosition);
			if (!IsFloorAboveGround(placementPosition, terrainTestHeight))
			{
				return EHousingPlacementResult.MissingGround;
			}

			// Along base
			Quaternion rotation = Quaternion.Euler(0.0f, placementRotation, 0.0f);
			Vector3 center = placementPosition + (rotation * new Vector3(0.0f, 0.0f, TRIANGLE_CENTER_PIVOT_OFFSET));

			Vector3 vertexPosition0 = placementPosition + (rotation * new Vector3(HALF_EDGE_LENGTH, 0.0f, -HALF_EDGE_LENGTH));
			Vector3 vertexPosition1 = placementPosition + (rotation * new Vector3(-HALF_EDGE_LENGTH, 0.0f, -HALF_EDGE_LENGTH));
			Vector3 vertexPosition2 = placementPosition + (rotation * new Vector3(0.0f, 0.0f, TRIANGLE_APEX_PIVOT_OFFSET));

			HousingVertex vertex0 = FindVertex(vertexPosition0);
			HousingVertex vertex1 = FindVertex(vertexPosition1);
			HousingVertex vertex2 = FindVertex(vertexPosition2);

			ignoreDrops.Clear();
			IgnoreVertexPillarsAndWalls(vertex0);
			IgnoreVertexPillarsAndWalls(vertex1);
			IgnoreVertexPillarsAndWalls(vertex2);
			IgnoreVertexFloorsExceptNearPosition(vertex0, center, TRIANGLE_INNER_RADIUS);
			IgnoreVertexFloorsExceptNearPosition(vertex1, center, TRIANGLE_INNER_RADIUS);
			IgnoreVertexFloorsExceptNearPosition(vertex2, center, TRIANGLE_INNER_RADIUS);

			// Low-hanging "roofs" like ramps and stairs may collide.
			HousingVertex vertexAbove0 = FindVertex(vertexPosition0 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			HousingVertex vertexAbove1 = FindVertex(vertexPosition1 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			HousingVertex vertexAbove2 = FindVertex(vertexPosition2 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			IgnoreVertexFloors(vertexAbove0);
			IgnoreVertexFloors(vertexAbove1);
			IgnoreVertexFloors(vertexAbove2);

			bool overlapTestPassed = TestTriangleOverlapsCommon(center, placementRotation, FOUNDATION_CENTER_OFFSET - FOUNDATION_TOP_MARGIN, HALF_FOUNDATION_HEIGHT - HALF_FOUNDATION_TOP_MARGIN, ref obstructionHint);
			return overlapTestPassed ? EHousingPlacementResult.Success : EHousingPlacementResult.Obstructed;
		}

		internal EHousingPlacementResult ValidateSquareRoofPlacement(ref Vector3 placementPosition, float placementRotation, ref string obstructionHint)
		{
			bool inSlot = SnapFloorPlacementToEdge(true, ref placementPosition);
			if (!inSlot)
			{
				return EHousingPlacementResult.MissingSlot;
			}

			Vector3 overlapCenter = placementPosition;
			Vector3 overlapHalfExtents = new Vector3(HALF_EDGE_LENGTH + PLACEMENT_OVERLAP_PADDING, HALF_ROOF_THICKNESS - FOUNDATION_TOP_MARGIN, HALF_EDGE_LENGTH + PLACEMENT_OVERLAP_PADDING);
			Quaternion overlapRotation = Quaternion.Euler(0.0f, placementRotation, 0.0f);

			Vector3 vertexPosition0 = placementPosition + (overlapRotation * new Vector3(HALF_EDGE_LENGTH, 0.0f, HALF_EDGE_LENGTH));
			Vector3 vertexPosition1 = placementPosition + (overlapRotation * new Vector3(-HALF_EDGE_LENGTH, 0.0f, HALF_EDGE_LENGTH));
			Vector3 vertexPosition2 = placementPosition + (overlapRotation * new Vector3(HALF_EDGE_LENGTH, 0.0f, -HALF_EDGE_LENGTH));
			Vector3 vertexPosition3 = placementPosition + (overlapRotation * new Vector3(-HALF_EDGE_LENGTH, 0.0f, -HALF_EDGE_LENGTH));

			HousingVertex vertex0 = FindVertex(vertexPosition0);
			HousingVertex vertex1 = FindVertex(vertexPosition1);
			HousingVertex vertex2 = FindVertex(vertexPosition2);
			HousingVertex vertex3 = FindVertex(vertexPosition3);

			HousingVertex vertexBelow0 = FindVertex(vertexPosition0 + new Vector3(0.0f, -WALL_HEIGHT, 0.0f));
			HousingVertex vertexBelow1 = FindVertex(vertexPosition1 + new Vector3(0.0f, -WALL_HEIGHT, 0.0f));
			HousingVertex vertexBelow2 = FindVertex(vertexPosition2 + new Vector3(0.0f, -WALL_HEIGHT, 0.0f));
			HousingVertex vertexBelow3 = FindVertex(vertexPosition3 + new Vector3(0.0f, -WALL_HEIGHT, 0.0f));

			ignoreDrops.Clear();
			IgnoreVertexPillarsAndWalls(vertex0);
			IgnoreVertexPillarsAndWalls(vertex1);
			IgnoreVertexPillarsAndWalls(vertex2);
			IgnoreVertexPillarsAndWalls(vertex3);
			IgnoreVertexFloorsExceptNearPosition(vertex0, overlapCenter, HALF_EDGE_LENGTH);
			IgnoreVertexFloorsExceptNearPosition(vertex1, overlapCenter, HALF_EDGE_LENGTH);
			IgnoreVertexFloorsExceptNearPosition(vertex2, overlapCenter, HALF_EDGE_LENGTH);
			IgnoreVertexFloorsExceptNearPosition(vertex3, overlapCenter, HALF_EDGE_LENGTH);
			IgnoreVertexPillarsAndWalls(vertexBelow0);
			IgnoreVertexPillarsAndWalls(vertexBelow1);
			IgnoreVertexPillarsAndWalls(vertexBelow2);
			IgnoreVertexPillarsAndWalls(vertexBelow3);

			// Low-hanging "roofs" like ramps and stairs may collide.
			HousingVertex vertexAbove0 = FindVertex(vertexPosition0 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			HousingVertex vertexAbove1 = FindVertex(vertexPosition1 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			HousingVertex vertexAbove2 = FindVertex(vertexPosition2 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			HousingVertex vertexAbove3 = FindVertex(vertexPosition3 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			IgnoreVertexFloors(vertexAbove0);
			IgnoreVertexFloors(vertexAbove1);
			IgnoreVertexFloors(vertexAbove2);
			IgnoreVertexFloors(vertexAbove3);

			int overlapCount = Physics.OverlapBoxNonAlloc(overlapCenter, overlapHalfExtents, overlapBuffer, overlapRotation, HOUSE_OVERLAP_LAYER_MASK, QueryTriggerInteraction.Collide);
			bool overlapTestPassed = CanIgnoreOverlaps(overlapCount, ref obstructionHint);

#if ENABLE_HOUSING_GIZMOS
			Color color = overlapTestPassed ? Color.green : Color.red;
			RuntimeGizmos.Get().Box(overlapCenter, overlapRotation, overlapHalfExtents * 2.0f, color);
			RuntimeGizmos.Get().Label(placementPosition + new Vector3(0.0f, WALL_HEIGHT * 0.25f, 0.0f), overlapCount.ToString(), color);

			if (vertex0 != null)
				RuntimeGizmos.Get().Cube(vertex0.position, 0.2f, color);
			if (vertex1 != null)
				RuntimeGizmos.Get().Cube(vertex1.position, 0.2f, color);
			if (vertex2 != null)
				RuntimeGizmos.Get().Cube(vertex2.position, 0.2f, color);
			if (vertex3 != null)
				RuntimeGizmos.Get().Cube(vertex3.position, 0.2f, color);
#endif // ENABLE_HOUSING_GIZMOS

			if (!overlapTestPassed)
			{
				return EHousingPlacementResult.Obstructed;
			}

			Vector3 characterOverlapHalfExtents = overlapHalfExtents + new Vector3(CHARACTER_OVERLAP_PADDING, CHARACTER_OVERLAP_PADDING, CHARACTER_OVERLAP_PADDING);
			bool characterOverlapTestPassed = !Physics.CheckBox(overlapCenter, characterOverlapHalfExtents, overlapRotation, CHARACTER_OVERLAP_LAYER_MASK, QueryTriggerInteraction.Collide);

#if ENABLE_HOUSING_GIZMOS
			Color characterOverlapColor = characterOverlapTestPassed ? Color.green : Color.red;
			RuntimeGizmos.Get().Box(overlapCenter, overlapRotation, characterOverlapHalfExtents * 2.0f, characterOverlapColor);
#endif // ENABLE_HOUSING_GIZMOS

			return characterOverlapTestPassed ? EHousingPlacementResult.Success : EHousingPlacementResult.Obstructed;
		}

		internal EHousingPlacementResult ValidateTriangleRoofPlacement(ref Vector3 placementPosition, float placementRotation, ref string obstructionHint)
		{
			bool inSlot = SnapFloorPlacementToEdge(true, ref placementPosition);
			if (!inSlot)
			{
				return EHousingPlacementResult.MissingSlot;
			}

			Quaternion rotation = Quaternion.Euler(0.0f, placementRotation, 0.0f);
			Vector3 center = placementPosition + (rotation * new Vector3(0.0f, 0.0f, TRIANGLE_CENTER_PIVOT_OFFSET));

			Vector3 vertexPosition0 = placementPosition + (rotation * new Vector3(HALF_EDGE_LENGTH, 0.0f, -HALF_EDGE_LENGTH));
			Vector3 vertexPosition1 = placementPosition + (rotation * new Vector3(-HALF_EDGE_LENGTH, 0.0f, -HALF_EDGE_LENGTH));
			Vector3 vertexPosition2 = placementPosition + (rotation * new Vector3(0.0f, 0.0f, TRIANGLE_APEX_PIVOT_OFFSET));

			HousingVertex vertex0 = FindVertex(vertexPosition0);
			HousingVertex vertex1 = FindVertex(vertexPosition1);
			HousingVertex vertex2 = FindVertex(vertexPosition2);

			HousingVertex vertexBelow0 = FindVertex(vertexPosition0 + new Vector3(0.0f, -WALL_HEIGHT, 0.0f));
			HousingVertex vertexBelow1 = FindVertex(vertexPosition1 + new Vector3(0.0f, -WALL_HEIGHT, 0.0f));
			HousingVertex vertexBelow2 = FindVertex(vertexPosition2 + new Vector3(0.0f, -WALL_HEIGHT, 0.0f));

			ignoreDrops.Clear();
			IgnoreVertexPillarsAndWalls(vertex0);
			IgnoreVertexPillarsAndWalls(vertex1);
			IgnoreVertexPillarsAndWalls(vertex2);
			IgnoreVertexFloorsExceptNearPosition(vertex0, center, TRIANGLE_INNER_RADIUS);
			IgnoreVertexFloorsExceptNearPosition(vertex1, center, TRIANGLE_INNER_RADIUS);
			IgnoreVertexFloorsExceptNearPosition(vertex2, center, TRIANGLE_INNER_RADIUS);
			IgnoreVertexPillarsAndWalls(vertexBelow0);
			IgnoreVertexPillarsAndWalls(vertexBelow1);
			IgnoreVertexPillarsAndWalls(vertexBelow2);

			// Low-hanging "roofs" like ramps and stairs may collide.
			HousingVertex vertexAbove0 = FindVertex(vertexPosition0 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			HousingVertex vertexAbove1 = FindVertex(vertexPosition1 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			HousingVertex vertexAbove2 = FindVertex(vertexPosition2 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			IgnoreVertexFloors(vertexAbove0);
			IgnoreVertexFloors(vertexAbove1);
			IgnoreVertexFloors(vertexAbove2);

			bool overlapTestPassed = TestTriangleOverlapsCommon(center, placementRotation, 0.0f, HALF_ROOF_THICKNESS - HALF_FOUNDATION_TOP_MARGIN, ref obstructionHint);
			return overlapTestPassed ? EHousingPlacementResult.Success : EHousingPlacementResult.Obstructed;
		}

		/// <summary>
		/// Ensure wall fits in an empty slot.
		/// </summary>
		internal EHousingPlacementResult ValidateWallPlacement(ref Vector3 pendingPlacementPosition, float pivotOffset, bool requiresPillars, bool requiresFullHeightPillars, ref string obstructionHint)
		{
			Vector3 edgePosition = pendingPlacementPosition + new Vector3(0.0f, -pivotOffset, 0.0f);
			Vector3 vertexPosition0;
			Vector3 vertexPosition1;
			HousingVertex vertex0;
			HousingVertex vertex1;
			float rotation;

			HousingEdge lowerEdge = FindEdge(edgePosition);
			if (lowerEdge == null)
			{
				// Maybe we are snapping onto the bottom of an existing wall?
				HousingEdge upperEdge = FindEdge(edgePosition + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
				if (upperEdge == null)
				{
					return EHousingPlacementResult.MissingSlot;
				}

				if (upperEdge.vertex0 == null || upperEdge.vertex1 == null)
				{
					// This case should never happen because floating wall creates verts, but just to be safe show missing pillar.
					return EHousingPlacementResult.MissingPillar;
				}

				// Copy slot's exact transform. On the server this prevents drift from server->client->server compression.
				edgePosition = upperEdge.position + new Vector3(0.0f, -WALL_HEIGHT, 0.0f);
				vertexPosition0 = upperEdge.vertex0.position + new Vector3(0.0f, -WALL_HEIGHT, 0.0f);
				vertexPosition1 = upperEdge.vertex1.position + new Vector3(0.0f, -WALL_HEIGHT, 0.0f);
				vertex0 = FindVertex(vertexPosition0);
				vertex1 = FindVertex(vertexPosition1);
				rotation = upperEdge.rotation;
			}
			else
			{
				if (!lowerEdge.walls.IsEmpty())
				{
					return EHousingPlacementResult.Obstructed;
				}

				if (lowerEdge.vertex0 == null || lowerEdge.vertex1 == null)
				{
					// This case should never happen because floating wall creates verts, but just to be safe show missing pillar.
					return EHousingPlacementResult.MissingPillar;
				}

				// Copy slot's exact transform. On the server this prevents drift from server->client->server compression.
				edgePosition = lowerEdge.position;
				vertexPosition0 = lowerEdge.vertex0.position;
				vertexPosition1 = lowerEdge.vertex1.position;
				vertex0 = lowerEdge.vertex0;
				vertex1 = lowerEdge.vertex1;
				rotation = lowerEdge.rotation;
			}

			pendingPlacementPosition = edgePosition + new Vector3(0.0f, pivotOffset, 0.0f);

			if (requiresPillars)
			{
				if (vertex0 == null || vertex1 == null || vertex0.pillars.IsEmpty() || vertex1.pillars.IsEmpty())
				{
					// Missing pillar.
					return EHousingPlacementResult.MissingPillar;
				}

				if (requiresFullHeightPillars)
				{
					if (!vertex0.HasFullHeightPillar() || !vertex1.HasFullHeightPillar())
					{
						// Missing pillar.
						return EHousingPlacementResult.MissingPillar;
					}
				}
			}

			Vector3 topOfModelVertexPosition0 = vertexPosition0 + new Vector3(0.0f, pivotOffset * 2.0f, 0.0f);
			Vector3 topOfModelVertexPosition1 = vertexPosition1 + new Vector3(0.0f, pivotOffset * 2.0f, 0.0f);
			Vector3 topOfModelEdgePosition = edgePosition + new Vector3(0.0f, pivotOffset * 2.0f, 0.0f);
			if (!UndergroundAllowlist.IsPositionBuildable(topOfModelVertexPosition0) && !UndergroundAllowlist.IsPositionBuildable(topOfModelVertexPosition1) && !UndergroundAllowlist.IsPositionBuildable(topOfModelEdgePosition))
			{
				// Top of entire wall or rampart would be underground.
				return EHousingPlacementResult.ObstructedByGround;
			}

			HousingVertex vertexAbove0 = FindVertex(vertexPosition0 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			HousingVertex vertexBelow0 = FindVertex(vertexPosition0 + new Vector3(0.0f, -WALL_HEIGHT, 0.0f));
			HousingVertex vertexAbove1 = FindVertex(vertexPosition1 + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			HousingVertex vertexBelow1 = FindVertex(vertexPosition1 + new Vector3(0.0f, -WALL_HEIGHT, 0.0f));

			Vector3 overlapCenter = edgePosition + new Vector3(0.0f, HALF_WALL_HEIGHT, 0.0f);
			Vector3 overlapHalfExtents = new Vector3(HALF_EDGE_LENGTH + PLACEMENT_OVERLAP_PADDING, HALF_WALL_HEIGHT + PLACEMENT_OVERLAP_PADDING, 0.25f + PLACEMENT_OVERLAP_PADDING);
			Quaternion overlapRotation = Quaternion.Euler(0.0f, rotation, 0.0f);

			ignoreDrops.Clear();
			IgnoreVertexPillarsFloorsAndWalls(vertex0);
			IgnoreVertexPillarsFloorsAndWalls(vertex1);

			// Overlapping pillar, wall, and roof above is expected.
			IgnoreVertexPillarsFloorsAndWalls(vertexAbove0);
			IgnoreVertexPillarsFloorsAndWalls(vertexAbove1);

			// Overlapping pillar and wall below is expected.
			IgnoreVertexPillarsAndWalls(vertexBelow0);
			IgnoreVertexPillarsAndWalls(vertexBelow1);

			// Downward ramp two floors above can slightly interact top of our wall.
			HousingVertex vertexTwoAbove0 = FindVertex(vertexPosition0 + new Vector3(0.0f, WALL_HEIGHT * 2.0f, 0.0f));
			HousingVertex vertexTwoAbove1 = FindVertex(vertexPosition1 + new Vector3(0.0f, WALL_HEIGHT * 2.0f, 0.0f));
			IgnoreVertexFloors(vertexTwoAbove0);
			IgnoreVertexFloors(vertexTwoAbove1);

			int overlapCount = Physics.OverlapBoxNonAlloc(overlapCenter, overlapHalfExtents, overlapBuffer, overlapRotation, HOUSE_OVERLAP_LAYER_MASK, QueryTriggerInteraction.Collide);
			bool overlapTestPassed = CanIgnoreOverlaps(overlapCount, ref obstructionHint);

#if ENABLE_HOUSING_GIZMOS
			Color color = overlapTestPassed ? Color.green : Color.red;
			RuntimeGizmos.Get().Box(overlapCenter, overlapRotation, overlapHalfExtents * 2.0f, color);
			RuntimeGizmos.Get().Label(edgePosition + new Vector3(0.0f, WALL_HEIGHT * 0.25f, 0.0f), overlapCount.ToString(), color);
#endif // ENABLE_HOUSING_GIZMOS

			if (!overlapTestPassed)
			{
				return EHousingPlacementResult.Obstructed;
			}

			Vector3 characterOverlapHalfExtents = overlapHalfExtents + new Vector3(CHARACTER_OVERLAP_PADDING, CHARACTER_OVERLAP_PADDING, CHARACTER_OVERLAP_PADDING);
			bool characterOverlapTestPassed = !Physics.CheckBox(overlapCenter, characterOverlapHalfExtents, overlapRotation, CHARACTER_OVERLAP_LAYER_MASK, QueryTriggerInteraction.Collide);

#if ENABLE_HOUSING_GIZMOS
			Color characterOverlapColor = characterOverlapTestPassed ? Color.green : Color.red;
			RuntimeGizmos.Get().Box(overlapCenter, overlapRotation, characterOverlapHalfExtents * 2.0f, characterOverlapColor);
#endif // ENABLE_HOUSING_GIZMOS

			return characterOverlapTestPassed ? EHousingPlacementResult.Success : EHousingPlacementResult.Obstructed;
		}

		/// <summary>
		/// Ensure pillar fits in an empty slot.
		/// </summary>
		internal EHousingPlacementResult ValidatePillarPlacement(ref Vector3 pendingPlacementPosition, float pivotOffset, ref string obstructionHint)
		{
			Vector3 vertexPosition = pendingPlacementPosition + new Vector3(0.0f, -pivotOffset, 0.0f);

			HousingVertex vertex = FindVertex(vertexPosition);
			if (vertex == null)
			{
				// Maybe we are snapping onto the bottom of an existing pillar?
				HousingVertex upperVertex = FindVertex(vertexPosition + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
				if (upperVertex == null)
				{
					return EHousingPlacementResult.MissingSlot;
				}

				// Copy slot's exact transform. On the server this prevents drift from server->client->server compression.
				vertexPosition = upperVertex.position + new Vector3(0.0f, -WALL_HEIGHT, 0.0f);
			}
			else
			{
				if (!vertex.pillars.IsEmpty())
				{
					// Not empty.
					return EHousingPlacementResult.Obstructed;
				}

				// Copy slot's exact transform. On the server this prevents drift from server->client->server compression.
				vertexPosition = vertex.position;
			}

			pendingPlacementPosition = vertexPosition + new Vector3(0.0f, pivotOffset, 0.0f);

			Vector3 topOfModelPosition = vertexPosition + new Vector3(0.0f, pivotOffset * 2.0f, 0.0f);
			if (!UndergroundAllowlist.IsPositionBuildable(topOfModelPosition))
			{
				// Top of pillar or post would be underground.
				return EHousingPlacementResult.ObstructedByGround;
			}

			HousingVertex vertexAbove = FindVertex(vertexPosition + new Vector3(0.0f, WALL_HEIGHT, 0.0f));
			HousingVertex vertexBelow = FindVertex(vertexPosition + new Vector3(0.0f, -WALL_HEIGHT, 0.0f));

			Vector3 overlapCenter = vertexPosition + new Vector3(0.0f, HALF_WALL_HEIGHT, 0.0f);
			Vector3 overlapHalfExtents = new Vector3(0.35f + PLACEMENT_OVERLAP_PADDING, HALF_WALL_HEIGHT + PLACEMENT_OVERLAP_PADDING, 0.35f + PLACEMENT_OVERLAP_PADDING);

			ignoreDrops.Clear();
			IgnoreVertexPillarsFloorsAndWalls(vertex);
			IgnoreVertexPillarsFloorsAndWalls(vertexAbove);
			IgnoreVertexPillarsAndWalls(vertexBelow);

			// Downward ramp two floors above can slightly interact top of our pillar.
			HousingVertex vertexTwoAbove = FindVertex(vertexPosition + new Vector3(0.0f, WALL_HEIGHT * 2.0f, 0.0f));
			IgnoreVertexFloors(vertexTwoAbove);

			int overlapCount = Physics.OverlapBoxNonAlloc(overlapCenter, overlapHalfExtents, overlapBuffer, Quaternion.identity, HOUSE_OVERLAP_LAYER_MASK, QueryTriggerInteraction.Collide);
			bool overlapTestPassed = CanIgnoreOverlaps(overlapCount, ref obstructionHint);

#if ENABLE_HOUSING_GIZMOS
			Color color = overlapTestPassed ? Color.green : Color.red;
			RuntimeGizmos.Get().Box(overlapCenter, overlapHalfExtents * 2.0f, color);
			RuntimeGizmos.Get().Label(vertexPosition + new Vector3(0.0f, WALL_HEIGHT * 0.25f, 0.0f), overlapCount.ToString(), color);
#endif // ENABLE_HOUSING_GIZMOS

			if (!overlapTestPassed)
			{
				return EHousingPlacementResult.Obstructed;
			}

			Vector3 characterOverlapHalfExtents = overlapHalfExtents + new Vector3(CHARACTER_OVERLAP_PADDING, CHARACTER_OVERLAP_PADDING, CHARACTER_OVERLAP_PADDING);
			bool characterOverlapTestPassed = !Physics.CheckBox(overlapCenter, characterOverlapHalfExtents, Quaternion.identity, CHARACTER_OVERLAP_LAYER_MASK, QueryTriggerInteraction.Collide);

#if ENABLE_HOUSING_GIZMOS
			Color characterOverlapColor = characterOverlapTestPassed ? Color.green : Color.red;
			RuntimeGizmos.Get().Box(overlapCenter, characterOverlapHalfExtents * 2.0f, characterOverlapColor);
#endif // ENABLE_HOUSING_GIZMOS

			return characterOverlapTestPassed ? EHousingPlacementResult.Success : EHousingPlacementResult.Obstructed;
		}

		internal void DrawGizmos()
		{
#if ENABLE_HOUSING_GIZMOS
			edgesGrid.DrawGrid(MainCamera.instance.transform.position, new Color(0.1f, 0.1f, 0.1f, 0.75f));

			if (edgesGrid != null)
			{
				DrawEdges();
			}
			if (verticesGrid != null)
			{
				DrawVertices();
			}
#endif // ENABLE_HOUSING_GIZMOS
		}

#if ENABLE_HOUSING_GIZMOS
		private void DrawEdges()
		{
			foreach (HousingEdge edge in edgesGrid.EnumerateItemsInSquare(MainCamera.instance.transform.position, 32.0f))
			{
				int connections = edge.backwardFloors.Count + edge.forwardFloors.Count;
				Color color = connections == 2 ? Color.green : (connections == 1 ? Color.yellow : Color.red);
				RuntimeGizmos.Get().Line(edge.position - edge.direction * 0.2f, edge.position + edge.direction * 0.2f, color);
				RuntimeGizmos.Get().Cube(edge.position - edge.direction * 0.2f, 0.1f, color);
				RuntimeGizmos.Get().Cube(edge.position + edge.direction * 0.2f, 0.1f, color);
				RuntimeGizmos.Get().Label(edge.position, connections.ToString());

				if (edge.vertex0 != null)
					RuntimeGizmos.Get().LineToward(edge.position, edge.vertex0.position, 0.5f, color);

				if (edge.vertex1 != null)
					RuntimeGizmos.Get().LineToward(edge.position, edge.vertex1.position, 0.5f, color);

				// Wall is green if occupied, yellow if empty.
				Quaternion rotation = Quaternion.Euler(0.0f, edge.rotation, 0.0f);
				RuntimeGizmos.Get().Box(edge.position + new Vector3(0.0f, HALF_WALL_HEIGHT, 0.0f), rotation, new Vector3(1.0f, 1.0f, 0.25f), edge.walls.IsEmpty() ? Color.yellow : Color.green);
			}
		}

		private void DrawVertices()
		{
			foreach (HousingVertex vertex in verticesGrid.EnumerateItemsInSquare(MainCamera.instance.transform.position, 32.0f))
			{
				int connections = vertex.floors.Count;
				Color color = connections < 1 ? Color.red : (connections == 1 ? Color.yellow : Color.green);
				RuntimeGizmos.Get().Cube(vertex.position, 0.1f, color);
				RuntimeGizmos.Get().Label(vertex.position, connections.ToString());

				foreach (HousingEdge edge in vertex.edges)
				{
					RuntimeGizmos.Get().LineToward(vertex.position, edge.position, 0.5f, color);
				}

				// Pillar is green if occupied, yellow if empty.
				RuntimeGizmos.Get().Box(vertex.position + new Vector3(0.0f, HALF_WALL_HEIGHT, 0.0f), new Vector3(0.5f, WALL_HEIGHT, 0.5f), vertex.pillars.IsEmpty() ? Color.yellow : Color.green);
			}
		}
#endif // ENABLE_HOUSING_GIZMOS

		/// <summary>
		/// Nelson 2024-06-26: With structure rotation replicated as a quaternion we need to be smarter about extracting
		/// yaw from model transform. Quaternion.eulerAngles.y isn't necessarily the yaw anymore.
		/// </summary>
		internal static float GetModelYaw(Transform modelTransform)
		{
			Vector3 y = modelTransform.TransformDirection(Vector3.up);
			if (y.y * y.y > 0.999f)
			{
				// Transform points straight up or down so we give up.
				return 0.0f;
			}

			// Looking down from above, what we used to consider "yaw" is 0.0 when +y is along -z in world-space, 90.0
			// along -x, 180.0 along +z, and 270.0 along +x.
			Vector2 direction = new Vector2(-y.z, -y.x).normalized;
			float angle = Mathf.Atan2(direction.y, direction.x);
			return Mathf.Rad2Deg * angle;
		}

		private RegionList<HousingEdge> edgesGrid;
		private RegionList<HousingVertex> verticesGrid;

		private Collider[] overlapBuffer = new Collider[50];

		/// <summary>
		/// Working buffer for placement overlap tests.
		/// </summary>
		private HashSet<StructureDrop> ignoreDrops = new HashSet<StructureDrop>();
	}
}
