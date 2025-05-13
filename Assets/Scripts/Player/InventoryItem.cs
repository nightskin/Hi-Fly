using UnityEngine;

[CreateAssetMenu]
public class InventoryItem :ScriptableObject
{
    public enum Item
    {
        NONE,
        LAZER,
        RAPID_FIRE,
        MISSILE,
        BASIC_REPAIR_KIT,
        EXPRESS_REPAIR_KIT,
        MAX_REPAIR_KIT,
    }
    public Item item;
    public float stock = 0;
    public Sprite image;
}
