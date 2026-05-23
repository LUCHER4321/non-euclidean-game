using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField]
    PortalGate[] portalGates;
    [SerializeField]
    MeshFilter[] spawns;
    public PortalGate[] GetPortalGates { get => portalGates; }
    public MeshFilter[] GetSpawns { get => spawns; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
