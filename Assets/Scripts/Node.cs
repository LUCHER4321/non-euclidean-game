using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField]
    Node[] connectedNodes;
    [SerializeField]
    Node connectedPortalNode;
    private Dictionary<Node, float> connections;
    public Dictionary<Node, float> GetConnections { get => connections; }
    private static List<Node> AllPortals = new List<Node>();
    public float GetHeuristicDistance(Node node)
    {
        float minDistance = Vector3.Distance(transform.position, node.transform.position);
        foreach (Node portal in AllPortals)
        {
            float distToPortal = Vector3.Distance(transform.position, portal.transform.position);
            float distFromExitToGoal = Vector3.Distance(portal.ConnectedPortalNode.transform.position, node.transform.position);
            float distanceThroughPortal = distToPortal + distFromExitToGoal;
            if (distanceThroughPortal < minDistance) minDistance = distanceThroughPortal;
        }
        return minDistance;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        connections = new Dictionary<Node, float>();
        foreach (Node connectedNode in connectedNodes) connections[connectedNode] = Vector3.Distance(transform.position, connectedNode.transform.position);
        if (connectedPortalNode == null) return;
        AllPortals.Add(this);
        foreach (Node connectedNode in connectedPortalNode.connectedNodes) connections[connectedNode] = Vector3.Distance(transform.position, connectedNode.transform.position);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
