using UnityEngine;

public enum PortalGateType
{
    HORIZONTAL,
    UP,
    DOWN
}

public class PortalGate : Node
{
    public PortalGateType type;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
