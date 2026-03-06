using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;



public class UIManager2 : MonoBehaviour
{
    public static UIManager2 Instance {get; private set;}
    

    // values need to be set
    public UIDocument UI;  // the UIDocument
    public InputAction rotateAction; // the inputAction to triger item rotation function
    private Button OpenInventory; //button to open inventory
    private int margin = 20; // margin between inventory grids
    private int gridSize = 80; //inventory grid size



    private VisualElement Root;
    private VisualElement Inventory;

    void Awake()
    {
        Instance = this;
    }


    void OnEnable()
    {
        Root = UI.rootVisualElement;
        OpenInventory = Root.Q<Button>("OpenInventory");
        Inventory = Root.Q<VisualElement>("Inventory");
    }


    
    private bool[,] occupancy;
    private ItemVisualElement ghostElement;
    private Vector2 pointerOffset;
    private Vector2 lastPointerPos;
    private IVisualElementScheduledItem tick = null;
    private void place(Vector2Int position, Vector2Int actualSize, bool targetOccupancy)
    {
        int w = occupancy.GetLength(0);
        int h = occupancy.GetLength(1);

        for (int dx = 0; dx < actualSize.x; dx++)
        for (int dy = 0; dy < actualSize.y; dy++)
        {
            int x = position.x + dx;
            int y = position.y + dy;
            if (x < 0 || x >= w || y < 0 || y >= h) continue;
            occupancy[x, y] = targetOccupancy;
        }
    }
    private bool canPlace(Vector2Int position, Vector2Int actualSize)
    {
        int w = occupancy.GetLength(0);
        int h = occupancy.GetLength(1);

        for (int dx = 0; dx < actualSize.x; dx++)
        for (int dy = 0; dy < actualSize.y; dy++)
        {
            int x = position.x + dx;
            int y = position.y + dy;

            if (x < 0 || x >= w || y < 0 || y >= h)
                return false;

            if (occupancy[x, y])
                return false;
        }

        return true;
    }

    // Dictionary<Item, Vector2Int> is to store items and their positions in the inventory
    public void ShowInventory(Dictionary<Item, Vector2Int> StorageDict)
    {
        Inventory.visible = !Inventory.visible;
        if (!Inventory.visible)
        {
            Inventory.Q<VisualElement>("Items").Clear();
            return;
        }
        int gridNumber = (int)Math.Sqrt(Inventory.Q<VisualElement>("Grids").childCount);
        print(Inventory.Q<VisualElement>("Grids").childCount);
        occupancy = new bool[gridNumber,gridNumber];
        foreach (Item item in StorageDict.Keys)
        {
            ItemVisualElement itemVisualElement = new ItemVisualElement(item);
            place(StorageDict[item], item.actualSize, true);
            itemVisualElement.style.backgroundColor = new StyleColor(new Color(1,0,0,1));
            itemVisualElement.style.position = Position.Absolute;
            itemVisualElement.style.width = gridSize * item.actualSize.x + margin * (item.actualSize.x-1);
            itemVisualElement.style.height = gridSize * item.actualSize.y + margin * (item.actualSize.y-1);
            itemVisualElement.style.top = margin + StorageDict[item].y * (gridSize + margin);
            itemVisualElement.style.left = margin + StorageDict[item].x * (gridSize + margin);
            itemVisualElement.RegisterCallback<PointerDownEvent>(e =>
            {
                if (e.button != 0) return;
                itemVisualElement.CapturePointer(e.pointerId);
                ghostElement = new ItemVisualElement(itemVisualElement.item.Clone());
                ghostElement.style.backgroundColor = itemVisualElement.style.backgroundColor;
                ghostElement.style.position = itemVisualElement.style.position;
                ghostElement.style.width = itemVisualElement.style.width;
                ghostElement.style.height = itemVisualElement.style.height;
                ghostElement.style.top = itemVisualElement.style.top;
                ghostElement.style.left = itemVisualElement.style.left;
                Inventory.Q<VisualElement>("Items").Add(ghostElement);
                lastPointerPos = e.position;
                pointerOffset = itemVisualElement.WorldToLocal(e.position);
                place(StorageDict[item], item.actualSize, false);
                itemVisualElement.style.opacity = 0.2f;
                tick = itemVisualElement.schedule.Execute(() =>
                {
                    if (rotateAction.WasPressedThisFrame())
                    {
                        ghostElement.Rotate();
                        Vector2 invLocal = Inventory.WorldToLocal(lastPointerPos);
                        Vector2 ghostLocal = ghostElement.WorldToLocal(lastPointerPos);
                        float height = ghostElement.style.height.value.value;
                        float width = ghostElement.style.width.value.value;
                        if (ghostLocal.y > height) pointerOffset.y -= height;
                        if (ghostLocal.x > width) pointerOffset.x -= width;
                        ghostElement.style.left = invLocal.x - pointerOffset.x;
                        ghostElement.style.top  = invLocal.y - pointerOffset.y;
                    }
                }).Every(0);
            });
            itemVisualElement.RegisterCallback<PointerMoveEvent>(e =>
            {
                if (!itemVisualElement.HasPointerCapture(e.pointerId)) return;
                if (ghostElement == null) return;
                lastPointerPos = e.position;
                Vector2 invLocal = Inventory.WorldToLocal(lastPointerPos);
                ghostElement.style.left = invLocal.x - pointerOffset.x;
                ghostElement.style.top  = invLocal.y - pointerOffset.y;
                
            });
            itemVisualElement.RegisterCallback<PointerUpEvent>(e =>
            {
                if (!itemVisualElement.HasPointerCapture(e.pointerId)) return;


                Vector2Int position = new Vector2Int();
                position.x = Mathf.RoundToInt((ghostElement.resolvedStyle.left - margin)/(gridSize+margin));
                position.y = Mathf.RoundToInt((ghostElement.resolvedStyle.top - margin)/(gridSize+margin));
                if (canPlace(position, ghostElement.item.actualSize))
                {
                    itemVisualElement.style.top = margin + position.y * (gridSize + margin);
                    itemVisualElement.style.left = margin + position.x * (gridSize + margin);
                    itemVisualElement.style.width = ghostElement.style.width;
                    itemVisualElement.style.height = ghostElement.style.height;
                    itemVisualElement.item.actualSize = ghostElement.item.actualSize;
                    StorageDict[item] = position;
                    place(position, itemVisualElement.item.actualSize, true);
                }
                ghostElement.RemoveFromHierarchy();
                ghostElement = null;
                itemVisualElement.ReleasePointer(e.pointerId);
                itemVisualElement.style.opacity = 1f;
                tick?.Pause();
            });
            Inventory.Q<VisualElement>("Items").Add(itemVisualElement);
        }
    }
}

