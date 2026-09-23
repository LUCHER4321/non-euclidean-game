using UnityEngine;
using System.Collections.Generic;

public class GraphNavMeshBuilderST : MonoBehaviour
{
    public static GraphNavMeshBuilderST Instance { get; private set; }
    [SerializeField] float vertexTolerance = 0.05f;

    public float GetVertexTolerance { get => vertexTolerance; }

    private Vector3[] GetWorldVertices(MeshFilter mf)
    {
        Vector3[] localVerts = mf.sharedMesh.vertices;
        Vector3[] worldVerts = new Vector3[localVerts.Length];
        for (int i = 0; i < localVerts.Length; i++) worldVerts[i] = mf.transform.TransformPoint(localVerts[i]);
        return worldVerts;
    }

    private bool ShareEdge(Vector3[] vertsA, Vector3[] vertsB)
    {
        int sharedCount = 0;
        foreach (Vector3 a in vertsA) foreach (Vector3 b in vertsB) if (Vector3.Distance(a, b) <= vertexTolerance)
        {
            sharedCount++;
            break;
        }
        if (sharedCount >= 2) return true;
        for (int i = 0; i < vertsA.Length; i++)
        {
            Vector3 a1 = vertsA[i];
            Vector3 a2 = vertsA[(i + 1) % vertsA.Length];
            Vector3 dirA = (a2 - a1).normalized;
            for (int j = 0; j < vertsB.Length; j++)
            {
                Vector3 b1 = vertsB[j];
                Vector3 b2 = vertsB[(j + 1) % vertsB.Length];
                Vector3 dirB = (b2 - b1).normalized;
                if (Mathf.Abs(Vector3.Dot(dirA, dirB)) > 0.99f)
                {
                    float distance = Vector3.Cross(dirA, b1 - a1).magnitude;
                    if (distance <= vertexTolerance)
                    {
                        float proj1 = Vector3.Dot(a1 - b1, dirB);
                        float proj2 = Vector3.Dot(a2 - b1, dirB);
                        float minProj = Mathf.Min(proj1, proj2);
                        float maxProj = Mathf.Max(proj1, proj2);
                        float lengthB = Vector3.Distance(b1, b2);
                        if (maxProj > vertexTolerance && minProj < lengthB - vertexTolerance) return true;
                    }
                }
            }
        }
        return false;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BuildGraph()
    {
        List<QuadNodeData> allQuadNodes = new List<QuadNodeData>();
        foreach (Room room in RoomST.Instance.GetRooms)
        {
            AddNodesFromMeshFilters(room.GetFloor, allQuadNodes);
            AddNodesFromMeshFilters(room.GetWalls, allQuadNodes);
            AddNodesFromMeshFilters(room.GetCeil, allQuadNodes);
            foreach (PortalGate portalGate in room.GetPortalGates)
            {
                List<QuadNodeData> portalWallNodes = new List<QuadNodeData>();
                AddNodesFromMeshFilters(portalGate.GetWalls, portalWallNodes);
                List<Node> pgConnections = new List<Node>();
                if (portalGate.connectedNodes != null) pgConnections.AddRange(portalGate.connectedNodes);
                foreach (QuadNodeData pwn in portalWallNodes)
                {
                    pwn.node.connectedNodes = new Node[] { portalGate };
                    pgConnections.Add(pwn.node);
                }
                portalGate.connectedNodes = pgConnections.ToArray();
                allQuadNodes.AddRange(portalWallNodes);
            }
        }
        for (int i = 0; i < allQuadNodes.Count; i++)
        {
            List<Node> connected = new List<Node>();
            if (allQuadNodes[i].node.connectedNodes != null) connected.AddRange(allQuadNodes[i].node.connectedNodes);
            for (int j = 0; j < allQuadNodes.Count; j++)
            {
                if (i == j) continue;
                if (ShareEdge(allQuadNodes[i].worldVertices, allQuadNodes[j].worldVertices)) connected.Add(allQuadNodes[j].node);
            }
            allQuadNodes[i].node.connectedNodes = connected.ToArray();
        }
    }

    private void AddNodesFromMeshFilters(MeshFilter[] meshFilters, List<QuadNodeData> nodeDataList)
    {
        if (meshFilters == null) return;
        foreach (MeshFilter mf in meshFilters)
        {
            if (mf == null || mf.sharedMesh == null) continue;
            Node node = mf.gameObject.GetComponent<Node>();
            if (node == null) node = mf.gameObject.AddComponent<Node>();
            QuadNodeData data = new QuadNodeData();
            data.node = node;
            data.worldVertices = GetWorldVertices(mf);
            nodeDataList.Add(data);
        }
    }

    private class QuadNodeData
    {
        public Node node;
        public Vector3[] worldVertices;
    }
}
