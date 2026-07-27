using UnityEngine;

public class PaintPistol : Item
{
    [SerializeField] int paintAmount = 100, maxPaintAmount = 100;
    [SerializeField] Color paintColor = Color.red;
    [SerializeField] Material hopper;

    private int hopperPropertyID;

    public override bool CanUse()
    {
        return paintAmount > 0;
    }

    public override void Action(bool pressing) { }

    public override void Throw(bool pressing) { }

    public override bool HandleReload(Item item)
    {
        if (paintAmount >= maxPaintAmount) return false;
        paintAmount += Mathf.RoundToInt(item.reloadQuantity);
        UpdateMaterial();
        return true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hopperPropertyID = Shader.PropertyToID("_Paint");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateMaterial()
    {
        hopper.SetFloat(hopperPropertyID, Mathf.Clamp((float)paintAmount / (float)maxPaintAmount, 0, 1));
    }
}
