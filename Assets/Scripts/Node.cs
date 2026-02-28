using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField]
    Node[] connectedNodes;
    [SerializeField]
    float[] distances;
    private Dictionary<Node, float> connections;
    public Dictionary<Node, float> GetConnections { get => connections; }
    public float GetHeuristicDistance(Node node)
    {
        if (connections.ContainsKey(node)) return Mathf.Min(connections[node], Vector3.Distance(transform.position, node.transform.position));
        float minDistFrom = float.MaxValue;
        foreach (Node connectedNode in connections.Keys)
        {
            float dist = Mathf.Min(connections[connectedNode], Vector3.Distance(transform.position, connectedNode.transform.position));//connections[connectedNode] + connectedNode.GetHeuristicDistance(node);
            if (dist < minDistFrom) minDistFrom = dist;
        }
        float minDistTo = float.MaxValue;
        foreach (Node connectedNode in node.GetConnections.Keys)
        {
            float dist = Mathf.Min(node.GetConnections[connectedNode], Vector3.Distance(node.transform.position, connectedNode.transform.position));//node.GetConnections[connectedNode] + connectedNode.GetHeuristicDistance(this);
            if (dist < minDistTo) minDistTo = dist;
        }
        return minDistFrom + minDistTo;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        connections = new Dictionary<Node, float>();
        for (int i = 0; i < connectedNodes.Length; i++)
        {
            connections[connectedNodes[i]] = distances[i];
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
