using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField]
    PortalGate[] portalGates;
    [SerializeField]
    MeshFilter[] spawns;
    [SerializeField]
    MeshFilter[] floor;
    public PortalGate[] GetPortalGates { get => portalGates; }
    public MeshFilter[] GetSpawns { get => spawns; }
    public MeshFilter[] GetFloor { get => floor; }

    public Vector3 ClosestPoint(Vector3 position)
    {
        Vector3 pos = position - position.y * Vector3.up;
        if (floor == null || floor.Length == 0) return pos;
        Vector3 closestPoint = pos;
        float minSqDistance = float.MaxValue;
        foreach (MeshFilter mf in floor)
        {
            if (mf == null || mf.sharedMesh == null) continue;
            Mesh mesh = mf.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Transform mfTransform = mf.transform;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = mfTransform.TransformPoint(vertices[triangles[i]]);
                Vector3 v1 = mfTransform.TransformPoint(vertices[triangles[i + 1]]);
                Vector3 v2 = mfTransform.TransformPoint(vertices[triangles[i + 2]]);
                Vector3 pointOnTriangle = GetClosestPointOnTriangle(pos, v0, v1, v2);
                Vector3 pOT = pointOnTriangle - pointOnTriangle.y * Vector3.up;
                float sqDist = (pos - pOT).sqrMagnitude;
                if (sqDist < minSqDistance)
                {
                    minSqDistance = sqDist;
                    closestPoint = pointOnTriangle;
                }
            }
        }
        return closestPoint;
    }

    public float Distance(Vector3 position)
    {
        Vector3 closestPoint = ClosestPoint(position);
        Vector3 pos = position - position.y * Vector3.up;
        closestPoint -= closestPoint.y * Vector3.up;
        return Vector3.Distance(position, closestPoint);
    }

    public bool Connected(Room other, out PortalGate gate)
    {
        foreach (PortalGate pg in portalGates) foreach (PortalGate opg in other.portalGates) if (pg.connectedPortalNode == opg)
        {
            gate = pg;
            return true;
        }
        gate = null;
        return false;
    }

    private Vector3 GetClosestPointOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 ap = p - a;
        float d1 = Vector3.Dot(ab, ap);
        float d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0.0f && d2 <= 0.0f) return a;
        Vector3 bp = p - b;
        float d3 = Vector3.Dot(ab, bp);
        float d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0.0f && d4 <= d3) return b;
        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
        {
            float v = d1 / (d1 - d3);
            return a + v * ab;
        }
        Vector3 cp = p - c;
        float d5 = Vector3.Dot(ab, cp);
        float d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0.0f && d5 <= d6) return c;
        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
        {
            float w = d2 / (d2 - d6);
            return a + w * ac;
        }
        float va = d3 * d6 - d5 * d4;
        if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
        {
            float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return b + w * (c - b);
        }
        float denom = 1.0f / (va + vb + vc);
        float vNorm = vb * denom;
        float wNorm = vc * denom;
        return a + ab * vNorm + ac * wNorm;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
