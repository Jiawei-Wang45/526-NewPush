using System.Collections.Generic;
using UnityEngine;

public static class ProceduralGraphGenerator
{
    private static readonly Vector2Int[] dirs =
{
    Vector2Int.up,
    Vector2Int.right,
    Vector2Int.down,
    Vector2Int.left
};
    public class GenerationResult
    {
        public List<Vector2Int> occupiedCells = new List<Vector2Int>();
        public Dictionary<int, List<int>> adjacencyGraph = new Dictionary<int, List<int>>();
        public int startIndex = -1;
        public int endIndex = -1;
    }

    // Generate occupied cells and adjacency graph using the same algorithm as before
    // adjacencyProbability: probability [0..1] that two adjacent cells will be considered connected
    // during initial adjacency construction. Use <1 to allow adjacent-but-not-connected rooms;
    // RepairConnectivity will still ensure the final graph is fully connected.
    public static GenerationResult Generate(int gridWidth, int gridHeight, int roomCount, float adjacencyProbability = 1f)
    {
        var result = new GenerationResult();

        var occupiedCells = result.occupiedCells;
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
            foreach (var d in ProceduralGraphGenerator.dirs)
            {
                var nb = cell + d;
                if (nb.x < 0 || nb.y < 0 || nb.x >= gridWidth || nb.y >= gridHeight) continue;
                if (added.Contains(nb)) continue;
                frontier.Add(nb);
                added.Add(nb);
            }
        }

        // build adjacency graph based on 4-neighborhood
        var map = new Dictionary<Vector2Int, int>();
        for (int i = 0; i < occupiedCells.Count; i++) map[occupiedCells[i]] = i;

        var graph = new Dictionary<int, List<int>>();
        for (int i = 0; i < occupiedCells.Count; i++) graph[i] = new List<int>();

        for (int i = 0; i < occupiedCells.Count; i++)
        {
            var c = occupiedCells[i];
            foreach (var d in dirs)
            {
                var nb = c + d;
                if (map.TryGetValue(nb, out int j))
                {
                    // only handle each unordered pair once (when j > i) to keep decisions symmetric, since the graph is connected in two ways, use j<=i to avoid redundancy
                    if (j <= i) continue;
                    // allow probabilistic omission of adjacent connections
                    if (rng.NextDouble() < Mathf.Clamp01(adjacencyProbability))
                    {
                        if (!graph[i].Contains(j)) graph[i].Add(j);
                        if (!graph[j].Contains(i)) graph[j].Add(i);
                    }
                }
            }
        }

        // repair connectivity
        RepairConnectivity(graph, occupiedCells);

        result.adjacencyGraph = graph;

        // pick start/end nodes
        (int s, int e) = PickStartEndNodes(graph, occupiedCells);
        result.startIndex = s; result.endIndex = e;

        return result;
    }

    private static (int startIndex, int endIndex) PickStartEndNodes(Dictionary<int, List<int>> graph, List<Vector2Int> occupiedCells)
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

    private static void RepairConnectivity(Dictionary<int, List<int>> graph, List<Vector2Int> occupiedCells)
    {
        var comps = ConnectedComponents(graph, occupiedCells.Count);
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

            comps = ConnectedComponents(graph, occupiedCells.Count);
        }
    }

    private static List<List<int>> ConnectedComponents(Dictionary<int, List<int>> graph, int nodeCount)
    {
        var comps = new List<List<int>>();
        var visited = new HashSet<int>();
        for (int i = 0; i < nodeCount; i++)
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
}
