using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class PathFinderST : MonoBehaviour
{
    public static PathFinderST Instance { get; private set; }
    private static readonly Node[] emptyNodes = new Node[0];

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public static List<Vector3> GetPath(Vector3 startPosition, Vector3 endPosition)
    {
        Node[] allNodes = Object.FindObjectsByType<Node>(FindObjectsSortMode.None);
        if (allNodes == null || allNodes.Length == 0) return new List<Vector3>();
        Node startNode = null;
        Node endNode = null;
        float minStartDist = float.MaxValue;
        float minEndDist = float.MaxValue;
        foreach (Node node in allNodes)
        {
            float dStart = Vector3.Distance(startPosition, node.transform.position);
            if (dStart < minStartDist)
            {
                minStartDist = dStart;
                startNode = node;
            }
            float dEnd = Vector3.Distance(endPosition, node.transform.position);
            if (dEnd < minEndDist)
            {
                minEndDist = dEnd;
                endNode = node;
            }
        }
        if (startNode == null || endNode == null) return new List<Vector3>();
        List<Node> openSet = new List<Node> { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        Dictionary<Node, float> gScore = new Dictionary<Node, float>();
        Dictionary<Node, float> fScore = new Dictionary<Node, float>();
        foreach (Node node in allNodes)
        {
            gScore[node] = float.MaxValue;
            fScore[node] = float.MaxValue;
        }
        gScore[startNode] = 0f;
        fScore[startNode] = startNode.GetHeuristicDistance(endNode);
        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++) if (fScore[openSet[i]] < fScore[current]) current = openSet[i];
            if (current == endNode) return ReconstructPath(cameFrom, current, startPosition, endPosition);
            openSet.Remove(current);
            closedSet.Add(current);
            bool useDictionary = current.GetConnections != null && current.GetConnections.Count > 0;
            Node[] neighbors = useDictionary ? new List<Node>(current.GetConnections.Keys).ToArray() : current.connectedNodes;
            if (neighbors == null) continue;
            foreach (Node neighbor in neighbors)
            {
                if (neighbor == null || closedSet.Contains(neighbor)) continue;
                float weight = useDictionary ? current.GetConnections[neighbor] : Vector3.Distance(current.transform.position, neighbor.transform.position);
                float tentative_gScore = gScore[current] + weight;
                if (tentative_gScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative_gScore;
                    fScore[neighbor] = gScore[neighbor] + neighbor.GetHeuristicDistance(endNode);
                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                }
            }
        }
        return new List<Vector3>();
    }

    private static List<Vector3> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current, Vector3 startPos, Vector3 endPos)
    {
        List<Vector3> path = new List<Vector3>();
        path.Add(endPos);
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current.transform.position);
            current = cameFrom[current];
        }
        path.Add(startPos);
        path.Reverse();
        return path;
    }
}
