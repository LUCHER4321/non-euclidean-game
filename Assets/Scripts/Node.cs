using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Node[] connectedNodes;
    private Dictionary<Node, float> connections;
    public Dictionary<Node, float> GetConnections { get => connections; }
    protected static List<PortalGate> AllPortals = new List<PortalGate>();
    public float GetHeuristicDistance(Node node)
    {
        float minDistance = Vector3.Distance(transform.position, node.transform.position);
        foreach (PortalGate portal in AllPortals)
        {
            float distToPortal = Vector3.Distance(transform.position, portal.transform.position);
            float distFromExitToGoal = Vector3.Distance(portal.connectedPortalNode.transform.position, node.transform.position);
            float distanceThroughPortal = distToPortal + distFromExitToGoal;
            if (distanceThroughPortal < minDistance) minDistance = distanceThroughPortal;
        }
        return minDistance;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        connections = new Dictionary<Node, float>();
        foreach (Node connectedNode in connectedNodes) connections[connectedNode] = Vector3.Distance(transform.position, connectedNode.transform.position);
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected void SetConnection(Node node, float distance)
    {
        if (connections.ContainsKey(node)) connections[node] = distance;
        else connections.Add(node, distance);
    }
}
