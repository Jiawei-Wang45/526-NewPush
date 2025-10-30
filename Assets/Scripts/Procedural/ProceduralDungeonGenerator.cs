using System.Collections.Generic;
using UnityEngine;

// Simple procedural dungeon generator that places rooms on a grid, builds a graph
// based on 4-neighborhood connectivity (N/E/S/W), repairs connectivity between
// components by adding corridors, then instantiates roomPrefabs and roadPrefabs
// to realize the level. Created rooms optionally get assigned back to GameManager.

// Procedural dungeon generator (graph-first):
// 1) Select a connected set of grid cells for rooms
// 2) Build a 4-neighbor adjacency graph between those cells
// 3) (No roads for now) Instantiate roomPrefabs at the centers of the chosen cells
// The generated RoomManager[] can be optionally assigned back to GameManager.instance.rooms
[ExecuteAlways]
public class ProceduralDungeonGenerator : MonoBehaviour
{
	[Header("Prefabs & options")]
	// Multiple room visuals to pick from for non-special rooms
	public GameObject[] roomPrefabs;   // array of possible room prefabs (should contain RoomManager)
	// Special start / end room prefabs
	public GameObject startRoomPrefab;
	public GameObject endRoomPrefab;
	public GameObject roadPrefab;   // prefab for corridor/road segments (placed tiled between rooms)
	public bool generateOnStart = false;
	public bool assignRoomsToGameManager = true;

	[Header("Grid layout")]
	public int gridWidth = 5;
	public int gridHeight = 5;
	public int roomCount = 9;
	public Vector2 cellSize = new Vector2(10f, 8f);

	[Header("Room sizing")]
	[Range(0.1f, 1f)] public float roomScale = 0.8f; // room visual and trigger scale relative to cellSize

	// runtime containers
	private List<Vector2Int> occupiedCells = new List<Vector2Int>();
	private List<GameObject> instantiatedRooms = new List<GameObject>();
	private Dictionary<int, List<int>> adjacencyGraph = new Dictionary<int, List<int>>();

	// chosen start/end room indices (in instantiatedRooms / occupiedCells)
	private int startRoomIndex = -1;
	private int endRoomIndex = -1;

	private void Start()
	{
		if (Application.isPlaying && generateOnStart)
		{
			GenerateDungeon();
		}
	}

	// Create road segments between rooms according to adjacency graph
	private void CreateRoadsFromGraph(Dictionary<int, List<int>> graph)
	{
		if (roadPrefab == null) return;
		var created = new HashSet<string>();
		for (int i = 0; i < occupiedCells.Count; i++)
		{
			foreach (var j in graph[i])
			{
				if (i == j) continue;
				string key = i < j ? $"{i}-{j}" : $"{j}-{i}";
				if (created.Contains(key)) continue;
				created.Add(key);
				CreateRoadBetween(i, j);
			}
		}
	}

	private void CreateRoadBetween(int indexA, int indexB)
	{
		var aCell = occupiedCells[indexA];
		var bCell = occupiedCells[indexB];
		// endpoints: prefer door transform if present, otherwise cell center edge
		Vector3 aPos = GetEndpointForConnection(indexA, bCell);
		Vector3 bPos = GetEndpointForConnection(indexB, aCell);
		TileRoadBetween(aPos, bPos);
	}

	// choose two distinct leaf nodes (degree==1) to be start and end; if not enough leaves, pick lowest-degree nodes
	private (int startIndex, int endIndex) PickStartEndNodes(Dictionary<int, List<int>> graph)
	{
		var leaves = new List<int>();
		for (int i = 0; i < occupiedCells.Count; i++)
		{
			if (graph.TryGetValue(i, out var neighs))
			{
				if (neighs.Count == 1) leaves.Add(i);
			}
		}

		var rng = new System.Random();
		if (leaves.Count >= 2)
		{
			int a = rng.Next(0, leaves.Count);
			int b = rng.Next(0, leaves.Count - 1);
			if (b >= a) b++;
			return (leaves[a], leaves[b]);
		}
		// fallback: pick two nodes with smallest degree
		var nodes = new List<int>();
		for (int i = 0; i < occupiedCells.Count; i++) nodes.Add(i);
		nodes.Sort((x, y) => graph[x].Count.CompareTo(graph[y].Count));
		if (nodes.Count >= 2) return (nodes[0], nodes[1]);
		if (nodes.Count == 1) return (nodes[0], nodes[0]);
		return (-1, -1);
	}

	// Set door modes on instantiated RoomManager objects based on adjacency graph.
	// If a neighbouring cell exists in a given direction and there's an edge in the graph,
	// the corresponding door is set to Normal; otherwise it's PermanentlyLocked.
	private void ApplyDoorModesFromGraph(Dictionary<int, List<int>> graph)
	{
		// build cell->index map
		var map = new Dictionary<Vector2Int, int>();
		for (int i = 0; i < occupiedCells.Count; i++) map[occupiedCells[i]] = i;

		for (int i = 0; i < occupiedCells.Count; i++)
		{
			var roomGO = instantiatedRooms[i];
			if (roomGO == null) continue;
			var rm = roomGO.GetComponent<RoomManager>();
			if (rm == null) continue;

			Vector2Int[] dirs = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
			for (int d = 0; d < dirs.Length; d++)
			{
				var nbCell = occupiedCells[i] + dirs[d];
				bool connected = false;
				if (map.TryGetValue(nbCell, out int nbIndex))
				{
					if (graph.TryGetValue(i, out var neighs) && neighs.Contains(nbIndex)) connected = true;
				}

				var dirEnum = (RoomManager.DoorDirection)d;
				rm.SetDoorMode(dirEnum, connected ? RoomManager.DoorMode.Normal : RoomManager.DoorMode.PermanentlyLocked);
			}

			// After changing modes, apply the visual/active state on the room's doors
			// RoomManager.OpenDoors is private so we invoke a public-friendly method by toggling
			// force open/close via reflection-like direct method if available; simpler: call EnsureDoorsExist then OpenDoors via public API isn't available.
			// Instead, call rm.ForceClearRoom() to ensure OpenDoors is called, then re-apply modes explicitly.
			// We'll directly set door GameObject active states here to reflect mode.
			var all = rm.GetAllDoors();
			for (int k = 0; k < all.Length; k++)
			{
				var door = all[k];
				if (door == null) continue;
				var mode = rm.GetDoorMode((RoomManager.DoorDirection)k);
				if (mode == RoomManager.DoorMode.PermanentlyLocked) door.SetActive(true); else door.SetActive(false);
			}
		}
	}

	// Try to use a RoomManager door world position; fallback to cell center edge
	private Vector3 GetEndpointForConnection(int roomIndex, Vector2Int neighborCell)
	{
		var cell = occupiedCells[roomIndex];
		Vector3 center = new Vector3(cell.x * cellSize.x, cell.y * cellSize.y, 0f) + transform.position;
		Vector2Int dir = neighborCell - cell;

		// map direction to DoorDirection
		RoomManager.DoorDirection doorDir = RoomManager.DoorDirection.North;
		if (dir.x > 0) doorDir = RoomManager.DoorDirection.East;
		else if (dir.x < 0) doorDir = RoomManager.DoorDirection.West;
		else if (dir.y > 0) doorDir = RoomManager.DoorDirection.North;
		else if (dir.y < 0) doorDir = RoomManager.DoorDirection.South;

		var roomGO = instantiatedRooms[roomIndex];
		if (roomGO != null)
		{
			var rm = roomGO.GetComponent<RoomManager>();
			if (rm != null && rm.HasDoor(doorDir))
			{
				var door = rm.GetDoor(doorDir);
				if (door != null) return door.transform.position;
			}
		}

		// fallback: use center offset toward direction by half cell size
		Vector3 offset = Vector3.zero;
		if (dir.x > 0) offset = new Vector3(cellSize.x * 0.5f, 0f, 0f);
		else if (dir.x < 0) offset = new Vector3(-cellSize.x * 0.5f, 0f, 0f);
		else if (dir.y > 0) offset = new Vector3(0f, cellSize.y * 0.5f, 0f);
		else if (dir.y < 0) offset = new Vector3(0f, -cellSize.y * 0.5f, 0f);

		return center + offset;
	}

	// Tile roadPrefab segments between two points without scaling the prefab
	private void TileRoadBetween(Vector3 from, Vector3 to)
	{
		if (roadPrefab == null) return;
		Vector3 dir = to - from;
		float totalLength = dir.magnitude;
		if (totalLength <= 0.001f) return;
		Vector3 unit = dir.normalized;

		float segLen = MeasureRoadPrefabLength();
		if (segLen <= 0f) segLen = Mathf.Max(cellSize.x, cellSize.y);

		int count = Mathf.Max(1, Mathf.CeilToInt(totalLength / segLen));
		for (int i = 0; i < count; i++)
		{
			float t = (i + 0.5f) / count; // center of each segment
			Vector3 pos = Vector3.Lerp(from, to, t);
			var seg = Instantiate(roadPrefab, pos, Quaternion.identity, this.transform);
			seg.transform.right = unit;
		}
	}

	// Measure approximate length of the road prefab along local X axis
	private float MeasureRoadPrefabLength()
	{
		if (roadPrefab == null) return 0f;
		float len = 0f;
		var tmp = Instantiate(roadPrefab);
		var sr = tmp.GetComponentInChildren<SpriteRenderer>();
		if (sr != null) len = sr.bounds.size.x;
		else
		{
			var mr = tmp.GetComponentInChildren<MeshRenderer>();
			if (mr != null) len = mr.bounds.size.x;
			else
			{
				var bc = tmp.GetComponentInChildren<BoxCollider2D>();
				if (bc != null) len = bc.size.x;
			}
		}
		DestroyImmediate(tmp);
		return len;
	}

	[ContextMenu("Generate Dungeon")]
	public void GenerateDungeon()
	{
		ClearGenerated();
		PickRoomCells();
		adjacencyGraph = BuildAdjacencyGraph();
		RepairConnectivity(adjacencyGraph);
		(var sIdx, var eIdx) = PickStartEndNodes(adjacencyGraph);
		startRoomIndex = sIdx; endRoomIndex = eIdx;
		InstantiateRoomsAtCells();
		ApplyDoorModesFromGraph(adjacencyGraph);
		CreateRoadsFromGraph(adjacencyGraph);

		if (assignRoomsToGameManager && GameManager.instance != null)
		{
			var rms = new RoomManager[instantiatedRooms.Count];
			for (int i = 0; i < instantiatedRooms.Count; i++)
				rms[i] = instantiatedRooms[i].GetComponent<RoomManager>();
			GameManager.instance.rooms = rms;
		}
	}

	private void ClearGenerated()
	{
		foreach (var go in instantiatedRooms)
		{
			if (go != null)
				DestroyImmediate(go);
		}
		instantiatedRooms.Clear();
		occupiedCells.Clear();
		adjacencyGraph.Clear();
	}

	// Grow a connected set of room cells starting from the grid center (ensures adjacency)
	private void PickRoomCells()
	{
		occupiedCells.Clear();
		int maxRooms = Mathf.Clamp(roomCount, 1, gridWidth * gridHeight);
		var rng = new System.Random();

		// start from center cell (or nearest integer)
		int startX = gridWidth / 2;
		int startY = gridHeight / 2;
		var frontier = new List<Vector2Int>();
		var added = new HashSet<Vector2Int>();
		var start = new Vector2Int(startX, startY);
		frontier.Add(start);
		added.Add(start);

		while (occupiedCells.Count < maxRooms && frontier.Count > 0)
		{
			int idx = rng.Next(0, frontier.Count);
			var cell = frontier[idx];
			frontier.RemoveAt(idx);
			occupiedCells.Add(cell);

			Vector2Int[] dirs = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
			foreach (var d in dirs)
			{
				var nb = cell + d;
				if (nb.x < 0 || nb.y < 0 || nb.x >= gridWidth || nb.y >= gridHeight) continue;
				if (added.Contains(nb)) continue;
				frontier.Add(nb);
				added.Add(nb);
			}
		}
	}

	// Build adjacency graph based on 4-neighborhood (N/E/S/W) of occupied grid cells
	private Dictionary<int, List<int>> BuildAdjacencyGraph()
	{
		var map = new Dictionary<Vector2Int, int>();
		for (int i = 0; i < occupiedCells.Count; i++) map[occupiedCells[i]] = i;

		var graph = new Dictionary<int, List<int>>();
		for (int i = 0; i < occupiedCells.Count; i++) graph[i] = new List<int>();

		Vector2Int[] dirs = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
		for (int i = 0; i < occupiedCells.Count; i++)
		{
			var c = occupiedCells[i];
			foreach (var d in dirs)
			{
				var nb = c + d;
				if (map.TryGetValue(nb, out int j))
				{
					if (!graph[i].Contains(j)) graph[i].Add(j);
					if (!graph[j].Contains(i)) graph[j].Add(i);
				}
			}
		}

		return graph;
	}

	// Ensure the graph is fully connected by connecting nearest nodes between disconnected components
	private void RepairConnectivity(Dictionary<int, List<int>> graph)
	{
		var comps = ConnectedComponents(graph);
		while (comps.Count > 1)
		{
			int bestA = -1, bestB = -1; float bestDist = float.MaxValue;
			var compA = comps[0];
			for (int k = 1; k < comps.Count; k++)
			{
				var compB = comps[k];
				foreach (var a in compA)
				foreach (var b in compB)
				{
					var pa = occupiedCells[a];
					var pb = occupiedCells[b];
					float dist = Mathf.Abs(pa.x - pb.x) + Mathf.Abs(pa.y - pb.y);
					if (dist < bestDist)
					{
						bestDist = dist; bestA = a; bestB = b;
					}
				}
			}

			if (bestA >= 0 && bestB >= 0)
			{
				graph[bestA].Add(bestB);
				graph[bestB].Add(bestA);
			}

			comps = ConnectedComponents(graph);
		}
	}

	private List<List<int>> ConnectedComponents(Dictionary<int, List<int>> graph)
	{
		var comps = new List<List<int>>();
		var visited = new HashSet<int>();
		for (int i = 0; i < occupiedCells.Count; i++)
		{
			if (visited.Contains(i)) continue;
			var stack = new Stack<int>();
			var comp = new List<int>();
			stack.Push(i);
			visited.Add(i);
			while (stack.Count > 0)
			{
				var v = stack.Pop();
				comp.Add(v);
				foreach (var nb in graph[v])
				{
					if (!visited.Contains(nb)) { visited.Add(nb); stack.Push(nb); }
				}
			}
			comps.Add(comp);
		}
		return comps;
	}

	// Instantiate roomPrefabs at grid cell centers (no roads)
	private void InstantiateRoomsAtCells()
	{
		instantiatedRooms.Clear();

		// If startRoomIndex is set and a player exists, compute dungeon offset so the start room
		// appears at the player's position and the rest of the dungeon shifts accordingly.
		Vector3 dungeonOffset = Vector3.zero;
		if (startRoomIndex >= 0)
		{
			var player = UnityEngine.Object.FindFirstObjectByType<PlayerControllerTest>();
			if (player != null && startRoomIndex < occupiedCells.Count)
			{
				var startCell = occupiedCells[startRoomIndex];
				Vector3 startCenter = new Vector3(startCell.x * cellSize.x, startCell.y * cellSize.y, 0f) + transform.position;
				dungeonOffset = player.transform.position - startCenter;
			}
		}
		for (int i = 0; i < occupiedCells.Count; i++)
		{
			var cell = occupiedCells[i];
			// Default world position is the grid cell center, plus any dungeon offset so start room maps to player
			Vector3 worldPos = new Vector3(cell.x * cellSize.x, cell.y * cellSize.y, 0f) + transform.position + dungeonOffset;
			GameObject roomInstance = null;
			// choose prefab: start, end, or random from roomPrefabs
			GameObject prefabToUse = null;
			if (i == startRoomIndex && startRoomPrefab != null) prefabToUse = startRoomPrefab;
			else if (i == endRoomIndex && endRoomPrefab != null) prefabToUse = endRoomPrefab;
			else if (roomPrefabs != null && roomPrefabs.Length > 0) prefabToUse = roomPrefabs[UnityEngine.Random.Range(0, roomPrefabs.Length)];
			if (prefabToUse != null)
			{
				roomInstance = Instantiate(prefabToUse, worldPos, Quaternion.identity, this.transform);
			}
			else
			{
				roomInstance = new GameObject($"Room_{i}");
				roomInstance.transform.position = worldPos;
				roomInstance.transform.SetParent(this.transform, false);
			}

			// Ensure a RoomManager is present
			var rm = roomInstance.GetComponent<RoomManager>();
			if (rm == null) rm = roomInstance.AddComponent<RoomManager>();

			// Add a trigger if none assigned
			if (rm.roomTrigger == null)
			{
				var trigger = roomInstance.AddComponent<BoxCollider2D>();
				trigger.isTrigger = true;
				trigger.size = cellSize * roomScale;
				rm.roomTrigger = trigger;
			}

			// ensure visual scale is slightly smaller than the cell so roads could fit later
			roomInstance.transform.localScale = Vector3.one * roomScale;

			// Ensure doors are created/positioned now so generator can read door transforms immediately
			rm.InitializeDoors();

			instantiatedRooms.Add(roomInstance);
		}
	}

}
