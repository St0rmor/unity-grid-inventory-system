# unity-grid-inventory-system
A grid based inventory system that support different sizes of items in Unity with UI toolkit.

## Introduction
This system is extremely simple and can be easily integrated into any games. New Unity input system is also used just for rotating the item and it can be replaced.

## Usage
`UI` the the UIDocument which contains the inventory UI.

`OpenInventory` the Button to open or close the inventory.

`margin` the margin between inventory grids.

`gridSize` inventory grid size.

`rotateAction` the inputAction to triger item rotation function.

`itemType` is a ScriptableObject that defines an item in the game.

`item` is a C# class that is used as a data container to be stored in the inventory to represent an item instance.

`Dictionary<Item, Vector2Int>` is the data type that stores the data of items and their grid positions in the inventory. Use this as the input of the `showInventory()`
