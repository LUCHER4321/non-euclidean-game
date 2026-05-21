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
    public Room[] GetRooms { get => rooms; }

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

    public void GenerateRandomPortalPairs()
    {
        if (rooms == null || rooms.Length == 0) return;
        int totalGates = 0;
        foreach (Room room in rooms) totalGates += room.GetPortalGates.Length;
        if (totalGates % 2 != 0 || totalGates < 2 * (rooms.Length - 1)) return;
        List<Room> sortedRooms = rooms.OrderByDescending(r => r.GetPortalGates.Length).ToList();
        List<PortalPair> createdPairs = new List<PortalPair>();
        List<PortalGate> availableConnectedGates = new List<PortalGate>(sortedRooms[0].GetPortalGates);
        for (int i = 1; i < sortedRooms.Count; i++)
        {
            Room nextRoom = sortedRooms[i];
            int randomConnectedIndex = Random.Range(0, availableConnectedGates.Count);
            PortalGate gateFromConnected = availableConnectedGates[randomConnectedIndex];
            availableConnectedGates.RemoveAt(randomConnectedIndex);
            List<PortalGate> nextRoomGates = new List<PortalGate>(nextRoom.GetPortalGates);
            int randomNextIndex = Random.Range(0, nextRoomGates.Count);
            PortalGate gateFromNextRoom = nextRoomGates[randomNextIndex];
            nextRoomGates.RemoveAt(randomNextIndex);
            createdPairs.Add(new PortalPair(gateFromConnected, gateFromNextRoom));
            availableConnectedGates.AddRange(nextRoomGates);
        }
        for (int i = 0; i < availableConnectedGates.Count; i++)
        {
            PortalGate temp = availableConnectedGates[i];
            int randomIndex = Random.Range(i, availableConnectedGates.Count);
            availableConnectedGates[i] = availableConnectedGates[randomIndex];
            availableConnectedGates[randomIndex] = temp;
        }
        for (int i = 0; i < availableConnectedGates.Count; i += 2) createdPairs.Add(new PortalPair(availableConnectedGates[i], availableConnectedGates[i + 1]));
        portalPairs = createdPairs.ToArray();
    }
}
