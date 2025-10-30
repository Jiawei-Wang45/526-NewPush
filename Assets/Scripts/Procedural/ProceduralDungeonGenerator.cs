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
	public GameObject[] roomPrefabs;
	public GameObject startRoomPrefab;
	public GameObject endRoomPrefab;
	public GameObject roadPrefab;
	public bool generateOnStart = false;
	public bool assignRoomsToGameManager = true;

	[Header("Grid layout")]
	public int gridWidth = 5;
	public int gridHeight = 5;
	public int roomCount = 9;
	public Vector2 cellSize = new Vector2(10f, 8f);

	[Header("Room sizing")]
	[Range(0.1f, 1f)] public float roomScale = 0.8f;

	[Header("Door settings")]
	public float doorOutsideOffset = 1f;

	// runtime data (kept for convenience/debugging)
	private List<Vector2Int> occupiedCells = new List<Vector2Int>();
	private List<GameObject> instantiatedRooms = new List<GameObject>();
	private Dictionary<int, List<int>> adjacencyGraph = new Dictionary<int, List<int>>();

	private void Start()
	{
		if (Application.isPlaying && generateOnStart) GenerateDungeon();
	}

	[ContextMenu("Generate Dungeon")]
	public void GenerateDungeon()
	{
		ClearGenerated();

		// Algorithm: generate grid cells + adjacency
		var result = ProceduralGraphGenerator.Generate(gridWidth, gridHeight, roomCount);
		occupiedCells = result.occupiedCells;
		adjacencyGraph = result.adjacencyGraph;

		// Instantiation: create rooms and roads in scene
		// compute dungeonOffset so the start room is placed at the player's position (if a player exists)
		Vector3 dungeonOffset = Vector3.zero;
		// find player by layer named "Player" (some projects use layers instead of tags)
		GameObject player = null;
		int playerLayer = LayerMask.NameToLayer("Player");
		if (playerLayer != -1)
		{
			var allGOs = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
			for (int gi = 0; gi < allGOs.Length; gi++)
			{
				var go = allGOs[gi];
				if (go != null && go.layer == playerLayer)
				{
					player = go;
					break;
				}
			}
		}
		if (player != null && result.startIndex >= 0 && result.startIndex < result.occupiedCells.Count)
		{
			var startCell = result.occupiedCells[result.startIndex];
			Vector3 startCenter = new Vector3(startCell.x * cellSize.x, startCell.y * cellSize.y, 0f);
			// world position of the start room without any offset
			Vector3 startWorld = startCenter + this.transform.position;
			// compute offset so start room ends up at player position
			dungeonOffset = player.transform.position - startWorld;
		}

		instantiatedRooms = ProceduralDungeonInstantiator.InstantiateFromGraph(
			result, roomPrefabs, startRoomPrefab, endRoomPrefab, roadPrefab, cellSize, roomScale, dungeonOffset, this.transform, doorOutsideOffset);

		// Optionally assign generated rooms back to GameManager
		if (assignRoomsToGameManager && GameManager.instance != null)
		{
			var rms = new RoomManager[instantiatedRooms.Count];
			for (int i = 0; i < instantiatedRooms.Count; i++) rms[i] = instantiatedRooms[i].GetComponent<RoomManager>();
			GameManager.instance.rooms = rms;
		}
	}

	private void ClearGenerated()
	{
		foreach (var go in instantiatedRooms) if (go != null) DestroyImmediate(go);
		instantiatedRooms.Clear(); occupiedCells.Clear(); adjacencyGraph.Clear();
	}
}
