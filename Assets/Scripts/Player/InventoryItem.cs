using UnityEngine;

[System.Serializable]
public struct InventoryItem
{
    public enum ItemType
    {
        NONE,
        REPAIR_KIT,
        LAZER,
        MACHINE_GUN,
    }
    public ItemType item;


}
