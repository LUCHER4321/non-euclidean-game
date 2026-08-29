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
    public PortalGateType type;
    public Portal portal { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
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
