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
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public Node[] AStar(Node start, Node end, Node[] previousNodes = null, float g = 0f)
    {
        if (previousNodes == null) previousNodes = emptyNodes;
        float h = start.GetHeuristicDistance(end);
        if (h == 0f) return new Node[] { start };
        Node[] nextNodes = new Node[start.GetConnections.Count];
        for (int i = 0; i < start.GetConnections.Count; i++)
        {
            nextNodes[i] = (Node)start.GetConnections.Keys.ToArray().GetValue(i);
        }
        nextNodes = nextNodes.OrderBy(n => g + start.GetConnections[n] + n.GetHeuristicDistance(end)).ToArray();
        List<Node> newPreviousNodes = new List<Node>(previousNodes) { start };
        foreach (Node nextNode in nextNodes)
        {
            if (previousNodes.Contains(nextNode)) continue;
            Node[] path = AStar(nextNode, end, newPreviousNodes.ToArray(), g + start.GetConnections[nextNode]);
            if (path.Length > 0) return new Node[] { start }.Concat(path).ToArray();
        }
        return new Node[] { start };
    }
}
