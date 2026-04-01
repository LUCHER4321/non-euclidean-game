using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class PathFinderST : MonoBehaviour
{
    public static PathFinderST Instance { get; private set; }
    [SerializeField]
    Node[] nodes;
    public Node[] GetNodes { get => nodes; }
    private static readonly Node[] emptyNodes = new Node[0];

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public static Node[] AStar(Node start, Node end)
    {
        if (start == end) return new Node[] { start };
        List<Node> openSet = new List<Node> { start };
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        Dictionary<Node, float> gScore = new Dictionary<Node, float>();
        gScore[start] = 0f;
        Dictionary<Node, float> fScore = new Dictionary<Node, float>();
        fScore[start] = start.GetHeuristicDistance(end);
        while (openSet.Count > 0)
        {
            Node current = openSet.OrderBy(n => fScore.ContainsKey(n) ? fScore[n] : float.MaxValue).First();
            if (current == end) return ReconstructPath(cameFrom, current);
            openSet.Remove(current);
            foreach (var connection in current.GetConnections)
            {
                Node neighbor = connection.Key;
                float costToNeighbor = connection.Value;
                float tentativeGScore = gScore[current] + costToNeighbor;
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + neighbor.GetHeuristicDistance(end);
                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                }
            }
        }
        return emptyNodes;
    }

    private static Node[] ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
    {
        List<Node> totalPath = new List<Node> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Add(current);
        }
        totalPath.Reverse();
        return totalPath.ToArray();
    }
}
