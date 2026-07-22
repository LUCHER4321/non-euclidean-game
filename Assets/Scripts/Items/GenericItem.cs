using UnityEngine;

public class GenericItem : Item
{

    public override bool CanUse()
    {
        return false;
    }

    public override void Action(bool pressing){}

    public override void Throw(bool pressing){}

    public override bool HandleReload(Item item)
    {
        return false;
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
