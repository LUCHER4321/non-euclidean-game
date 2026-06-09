using UnityEngine;

public interface IItem
{
    Item item { get; }
    void Action(bool pressing);
    void Throw(bool pressing);
    void Equip(Character character);
    void Unequip();
    bool CanUse();
    void Reload();
}
