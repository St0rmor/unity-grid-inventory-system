using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    public List<ItemData> inventory1;
    public List<ItemData> inventory2;


    private PanelRenderer panelRenderer;
    [NonSerialized] public VisualElement Root;
    private GridContainer InventoryContainter1;
    private GridContainer inventoryContainter2;
    
    
    void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        Root = rootElement;
        InventoryContainter1 = Root.Q<GridContainer>("InventoryContainter1");
        inventoryContainter2 = Root.Q<GridContainer>("inventoryContainter2");
        InventoryContainter1.itemDatas = inventory1;
        inventoryContainter2.itemDatas = inventory1;
    }


    void OnEnable()
    {        
        // display inventory
        InventoryContainter1.Show();
        inventoryContainter2.Show();        
    }

    void OnDisable()
    {
        // hide inventory
        InventoryContainter1.Hide();
        inventoryContainter2.Hide();
    }



    void Update()
    {
        if (Root != null) DragItem();
    }



    [NonSerialized] public ItemElement draggedItem;
    Vector2 dragOffset;
    Vector2 oldPos = Vector2.zero;
    bool oldRotated;
    void DragItem()
    {
        Vector2 screenPos = Input.mousePosition;
        screenPos.y = Screen.height - screenPos.y;
        Vector2 mousePos = RuntimePanelUtils.ScreenToPanel(Root.panel, screenPos);

        
        if (Input.GetMouseButtonDown(0))
        {
            VisualElement picked = Root.panel.Pick(mousePos);
            ItemElement item = picked as ItemElement ?? picked?.GetFirstAncestorOfType<ItemElement>();

            if (item != null)
            {
                draggedItem = item;
                Vector2 localMouse = draggedItem.parent.WorldToLocal(mousePos);
                dragOffset = localMouse - draggedItem.layout.position;
                oldPos = new Vector2(draggedItem.style.left.value.value, draggedItem.style.top.value.value);
                oldRotated = draggedItem.itemData.rotated;
                draggedItem.parentContainer.SetOccupancy(draggedItem.itemData.gridPosition, draggedItem.itemData.type.Size, oldRotated, false);
                draggedItem.BringToFront();
                draggedItem.parent.BringToFront();
            }  
        }

        if (Input.GetMouseButton(0) && draggedItem != null)
        {
            if (InputManager.RollAction.WasPressedThisFrame())
            {
                draggedItem.itemData.rotated = !draggedItem.itemData.rotated;
                screenPos = Input.mousePosition;
                screenPos.y = Screen.height - screenPos.y;
                mousePos = RuntimePanelUtils.ScreenToPanel(Root.panel, screenPos);
                Vector2 delta = draggedItem.worldBound.center - mousePos;
                draggedItem.style.rotate = new Rotate((draggedItem.itemData.rotated? 1:0) * 90f);
                dragOffset += delta;
            }
            Vector2 localMouse = draggedItem.parent.WorldToLocal(mousePos);
            Vector2 newPos = localMouse - dragOffset;

            draggedItem.style.left = newPos.x;
            draggedItem.style.top = newPos.y;
        }

        if (Input.GetMouseButtonUp(0) && draggedItem != null)
        {
            Rect itemRect = draggedItem.worldBound;
            Vector2 itemPos = itemRect.position;

            GridElement closestGrid = null;
            float closestDistance = float.MaxValue;

            foreach (GridElement grid in Root.Query<GridElement>().ToList())
            {
                Rect gridRect = grid.worldBound;
                if (!itemRect.Overlaps(gridRect)) continue;
                float distance = Vector2.SqrMagnitude(itemPos - gridRect.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestGrid = grid;
                }
            }

            if (closestGrid != null)
            {
                GridContainer gridContainer = (GridContainer)closestGrid.parent;

                if (gridContainer.CheckOccupancy(closestGrid.gridPosition, draggedItem.itemData.type.Size, draggedItem.itemData.rotated))
                {

                    gridContainer.SetOccupancy(closestGrid.gridPosition, draggedItem.itemData.type.Size, draggedItem.itemData.rotated,true);
                    draggedItem.SetPosition(closestGrid);
                    
                    draggedItem.parentContainer.itemDatas.Remove(draggedItem.itemData);
                    draggedItem.itemData.gridPosition = closestGrid.gridPosition;
                    draggedItem.parentContainer = gridContainer;
                    gridContainer.itemDatas.Add(draggedItem.itemData);
                }
                else
                    {
                    draggedItem.itemData.rotated = oldRotated;
                    draggedItem.UpdateRotation();
                    draggedItem.style.left = oldPos.x;
                    draggedItem.style.top = oldPos.y;
                    draggedItem.parentContainer.SetOccupancy(draggedItem.itemData.gridPosition, draggedItem.itemData.type.Size, oldRotated, true);
                }
            }
            else
            {
                draggedItem.itemData.rotated = oldRotated;
                draggedItem.UpdateRotation();
                draggedItem.style.left = oldPos.x;
                draggedItem.style.top = oldPos.y;
                draggedItem.parentContainer.SetOccupancy(draggedItem.itemData.gridPosition, draggedItem.itemData.type.Size, oldRotated, true);
            }
            draggedItem = null;
        }
    }

}
