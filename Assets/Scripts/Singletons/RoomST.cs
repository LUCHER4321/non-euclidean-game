using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomST : MonoBehaviour
{
    [System.Serializable]
    struct PortalPair
    {
        public PortalGate pg0;
        public PortalGate pg1;

        public PortalPair(PortalGate a, PortalGate b)
        {
            pg0 = a;
            pg1 = b;
        }
    }
    public static RoomST Instance { get; private set; }
    [SerializeField]
    Room[] rooms;
    [SerializeField]
    PortalPair[] portalPairs;
    [SerializeField]
    GameObject portalPairPrefab;
    public Room[] GetRooms { get => rooms; }

    public Vector3 GetRandomSpawnPoint()
    {
        if (rooms == null || rooms.Length == 0) return Vector3.zero;
        List<(MeshFilter spawn, float area)> spawnAreas = new List<(MeshFilter, float)>();
        float totalArea = 0f;
        foreach (Room room in rooms)
        {
            if (room.GetSpawns == null) continue;
            foreach (MeshFilter spawn in room.GetSpawns)
            {
                if (spawn == null || spawn.sharedMesh == null) continue;
                Vector3 localSize = spawn.sharedMesh.bounds.size;
                Vector3 scale = spawn.transform.lossyScale;
                float w = localSize.x * scale.x;
                float h = localSize.y * scale.y;
                float d = localSize.z * scale.z;
                float area = Mathf.Abs(w * h) + Mathf.Abs(w * d) + Mathf.Abs(h * d);
                if (area > 0)
                {
                    spawnAreas.Add((spawn, area));
                    totalArea += area;
                }
            }
        }
        if (spawnAreas.Count == 0) return Vector3.zero;
        float randomValue = Random.Range(0f, totalArea);
        MeshFilter selectedSpawn = spawnAreas[0].spawn;
        foreach (var sa in spawnAreas)
        {
            randomValue -= sa.area;
            if (randomValue <= 0f)
            {
                selectedSpawn = sa.spawn;
                break;
            }
        }
        Bounds bounds = selectedSpawn.sharedMesh.bounds;
        Vector3 randomLocalPoint = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
        return selectedSpawn.transform.TransformPoint(randomLocalPoint);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateRandomPortalPairs();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private bool IsCompatible(PortalGate a, PortalGate b)
    {
        if (a.type == PortalGateType.HORIZONTAL && b.type == PortalGateType.HORIZONTAL) return true;
        if (a.type == PortalGateType.UP && b.type == PortalGateType.DOWN) return true;
        if (a.type == PortalGateType.DOWN && b.type == PortalGateType.UP) return true;
        return false;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void GenerateRandomPortalPairs()
    {
        if (rooms == null || rooms.Length == 0) return;
        int horizontalCount = 0;
        int upCount = 0;
        int downCount = 0;
        //int totalGates = 0;
        foreach (Room room in rooms)
        {
            PortalGate[] gates = room.GetPortalGates;
            foreach (PortalGate gate in gates)
            {
                if (gate.type == PortalGateType.HORIZONTAL) horizontalCount++;
                else if (gate.type == PortalGateType.UP) upCount++;
                else if (gate.type == PortalGateType.DOWN) downCount++;
            }
        }
        if (horizontalCount % 2 != 0 || upCount != downCount) return;
        List<PortalPair> createdPairs = new List<PortalPair>();
        Dictionary<Room, List<PortalGate>> availablePortals = new Dictionary<Room, List<PortalGate>>();
        foreach (Room r in rooms) availablePortals[r] = new List<PortalGate>(r.GetPortalGates);
        List<Room> connectedRooms = new List<Room>();
        List<Room> unconnectedRooms = new List<Room>(rooms);
        Room startRoom = unconnectedRooms[0];
        connectedRooms.Add(startRoom);
        unconnectedRooms.Remove(startRoom);
        while (unconnectedRooms.Count > 0)
        {
            var possibleConnections = new List<(Room cRoom, PortalGate cGate, Room uRoom, PortalGate uGate)>();
            foreach (Room cRoom in connectedRooms) foreach (PortalGate cGate in availablePortals[cRoom]) foreach (Room uRoom in unconnectedRooms) foreach (PortalGate uGate in availablePortals[uRoom]) if (IsCompatible(cGate, uGate)) possibleConnections.Add((cRoom, cGate, uRoom, uGate));
            if (possibleConnections.Count == 0) return;
            var chosen = possibleConnections[Random.Range(0, possibleConnections.Count)];
            createdPairs.Add(new PortalPair(chosen.cGate, chosen.uGate));
            availablePortals[chosen.cRoom].Remove(chosen.cGate);
            availablePortals[chosen.uRoom].Remove(chosen.uGate);
            connectedRooms.Add(chosen.uRoom);
            unconnectedRooms.Remove(chosen.uRoom);
        }
        List<PortalGate> leftoverHorizontal = new List<PortalGate>();
        List<PortalGate> leftoverUp = new List<PortalGate>();
        List<PortalGate> leftoverDown = new List<PortalGate>();
        foreach (Room r in rooms) foreach (PortalGate gate in availablePortals[r])
        {
            if (gate.type == PortalGateType.HORIZONTAL) leftoverHorizontal.Add(gate);
            else if (gate.type == PortalGateType.UP) leftoverUp.Add(gate);
            else if (gate.type == PortalGateType.DOWN) leftoverDown.Add(gate);
        }
        Shuffle(leftoverHorizontal);
        Shuffle(leftoverUp);
        Shuffle(leftoverDown);
        for (int i = 0; i < leftoverHorizontal.Count; i += 2) createdPairs.Add(new PortalPair(leftoverHorizontal[i], leftoverHorizontal[i + 1]));
        for (int i = 0; i < leftoverUp.Count; i++) createdPairs.Add(new PortalPair(leftoverUp[i], leftoverDown[i]));
        portalPairs = createdPairs.ToArray();
        foreach (PortalPair pair in portalPairs)
        {
            GameObject pp = Instantiate(portalPairPrefab, Vector3.zero, Quaternion.identity);
            PortalGate[] pg = new PortalGate[2] { pair.pg0, pair.pg1 };
            Transform[] portals = new Transform[2] { pp.transform.GetChild(0), pp.transform.GetChild(1) };
            for (int i = 0; i < 2; i++)
            {
                pg[i].connectedPortalNode = pg[1 - i];
                portals[i].transform.parent = pg[i].transform;
                portals[i].transform.localPosition = Vector3.zero;
                portals[i].transform.localRotation = Quaternion.identity;
            }
            for (int i = 0; i < 2; i++) pg[i].PStart();
            Destroy(pp);
        }
    }
}
