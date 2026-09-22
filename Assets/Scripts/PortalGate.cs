using UnityEngine;
using System.Linq;

public enum PortalGateType
{
    HORIZONTAL,
    UP,
    DOWN
}

public class PortalGate : Node
{
    public PortalGate connectedPortalNode;
    public PortalGateType type;
    public Portal portal { get; private set; }
    [SerializeField]
    MeshFilter[] walls;
    public MeshFilter[] GetWalls { get => walls; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        if (connectedPortalNode == null) return;
        if (!AllPortals.Contains(this)) AllPortals.Add(this);
        SetConnection(connectedPortalNode, 0f);
        portal = GetComponentsInChildren<Portal>().FirstOrDefault(x => x.GetTeleport);
    }

    public void PStart()
    {
        Start();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
