using UnityEngine;


[CreateAssetMenu(fileName = "NewItemType", menuName = "Scriptable Objects/ItemType")]
public class ItemType : ScriptableObject
{
    public string Name;
    public string Description;
    public Sprite Icon;
    public Vector2Int Size;
}


public class Item
{
    public ItemType type {get; private set;}
    public Vector2Int actualSize;
    public int rotationIndex = 0;
    public Item(ItemType ItemType)
    {
        type = ItemType;
        actualSize = type.Size;
    }
    public Item Clone()
    {
        Item newItem = new Item(type);
        newItem.actualSize = actualSize;
        return newItem;
    }
}
