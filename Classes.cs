using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;



public class ItemElement : VisualElement
{
    int gridSize = 120;
    int margin = 8;

    public ItemData itemData;
    public GridContainer parentContainer;


    public ItemElement(ItemData ItemData, GridContainer ParentContainer)
    {

        itemData = ItemData;
        parentContainer = ParentContainer;

        style.position = Position.Absolute;
        style.backgroundImage = new StyleBackground(itemData.type.Icon);
        style.width = gridSize * itemData.type.Size.x + margin * (itemData.type.Size.x-1);
        style.height = gridSize * itemData.type.Size.y + margin * (itemData.type.Size.y-1);
    }

    public void UpdateRotation()
    {
        Vector2 oriPos = new Vector2(style.left.value.value, style.top.value.value);
        Vector2 offset = Vector2.zero;
        if (itemData.rotated) offset = 0.5f * new Vector2(gridSize + margin, -gridSize - margin);
        
        style.rotate = new Rotate((itemData.rotated? 1:0) * 90f);
        //style.translate = new Translate(offset.x, offset.y, 0);
        Vector2 pos = oriPos + offset;
        style.left = pos.x;
        style.top = pos.y;

    }

    public void SetPosition(VisualElement targetElement)
    {
        Vector2 delta = targetElement.worldBound.position - worldBound.position;
        Vector2 orginalPos = new Vector2(style.left.value.value, style.top.value.value);
        Vector2 pos = orginalPos + delta;
        style.left = pos.x;
        style.top = pos.y;
    }

}


[UxmlElement]
public partial class GridElement : VisualElement
{
    public Vector2Int gridPosition;
    public bool occupied;
}


[UxmlElement]
public partial class GridContainer : VisualElement
{
    [UxmlAttribute("ContainerSize")]
    public Vector2Int containerSize;
    public List<ItemData> itemDatas;

    bool initialized = false;

    VisualElement itemContainer;


    public void Show()
    {
        if (!initialized)
        {
            initialized = true;
            itemContainer = new VisualElement();
            itemContainer.style.position = Position.Absolute;
            itemContainer.style.width = Length.Percent(100f);
            itemContainer.style.height = Length.Percent(100f);
            itemContainer.pickingMode = PickingMode.Ignore;
            UIManager.Instance.Cargos.Add(itemContainer);

            int index = 0;
            foreach (VisualElement element in Children())
            {
                if (element is GridElement grid)
                {
                    int x = index % containerSize.x;
                    int y = index / containerSize.x;
                    grid.gridPosition = new Vector2Int(x, y);
                    index++;
                }
            }
        }

        foreach (VisualElement element in Children())
        {
            if (element is GridElement grid)
            {
                grid.occupied = false;
            }
        }

        visible = true;
        foreach (ItemData itemData in itemDatas)
        {
            ItemElement itemElement = new ItemElement(itemData, this);
            itemContainer.Add(itemElement);
            RegisterCallback<GeometryChangedEvent>(evt => InitPlace(evt, itemElement, itemData));
            
            SetOccupancy(itemData.gridPosition, itemData.type.Size, itemData.rotated, true);
        }
    }

    public void InitPlace(GeometryChangedEvent evt, ItemElement itemElement, ItemData itemData)
    {
        GridElement grid = (GridElement)this[PosToIndex(itemData.gridPosition)];
        itemElement.SetPosition(grid);
        itemElement.UpdateRotation();
    }

    public void Hide()
    {
        visible = false;
        itemContainer.Clear();
    }

    public int PosToIndex(Vector2Int Position)
    {
        return Position.y * containerSize.x + Position.x;
    }

    public void SetOccupancy(Vector2Int position, Vector2Int size, bool rotated, bool occupancy)
    {
        Vector2Int[] positions = GetGridPositions(position, size, rotated);
        foreach (Vector2Int pos in positions)
        {
            ((GridElement)this[PosToIndex(pos)]).occupied = occupancy;
        }
    }

    public bool CheckOccupancy(Vector2Int position, Vector2Int size, bool rotated)
    {
        Vector2Int[] positions = GetGridPositions(position, size, rotated);
        bool canPlace = true;
        foreach (Vector2Int pos in positions)
        {
            if (pos.x > containerSize.x - 1){canPlace = false; break;}
            if (pos.y > containerSize.y - 1){canPlace = false; break;}
            if (((GridElement)this[PosToIndex(pos)]).occupied){canPlace = false; break;}
        }
        return canPlace;
    }


    public static Vector2Int[] GetGridPositions(Vector2Int position, Vector2Int size, bool rotated)
    {
        if (rotated) size = new Vector2Int(size.y, size.x);
        Vector2Int[] positions = new Vector2Int[size.x * size.y];
        int i = 0;
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                positions[i] = new Vector2Int(
                    position.x + x,
                    position.y + y
                );
                i++;
            }
        }
        return positions;
    }
}


