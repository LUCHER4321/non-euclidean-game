using UnityEngine;

public class Food : Item
{
    FoodSO foodData { get => itemData as FoodSO; }

    public override bool CanUse()
    {
        return owner.health < owner.characterSO.GetMaxHealth && foodData != null;
    }

    public override void Action(bool pressing)
    {
        if (pressing || foodData == null) return;
        owner.health += foodData.GetHealthRestore;
        owner.health = Mathf.Min(owner.health, owner.characterSO.GetMaxHealth);
    }

    public override void Throw(bool pressing) { }

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

    void OnValidate()
    {
        if (itemData != null && !(itemData is FoodSO))
        {
            Debug.LogWarning("Warning! The Food script only accepts an ItemSO itemData of the FoodSO sub-class.");
            itemData = null;
        }
    }
}
