using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BuildZone : MonoBehaviour
{
    private static Vector2 OFFSET = new Vector2(0.5f, 0.5f);

    public Transform corner1;
    public Transform corner2;

    private Vector2Int corner1Pos;
    private Vector2Int corner2Pos;

    [SerializeField] private GameObject mouseIndicator, cellIndicator, gridSquare, gridSquareContainer;
    [SerializeField] private Grid grid;
    [SerializeField] private Color ableToBuildColour, unableToBuildColour;

    [SerializeField] private float evenAlphaLevel;
    [SerializeField] private float oddAlphaLevel;

    // game should not start with build mode enabled
    private bool gridShowing = false;

    private GameObject previewItemPrefab;
    private int lastSelectedInventorySlot = -1;

    private Dictionary<Vector2Int, bool> buildZoneObjects;

    private Vector2Int max;
    private Vector2Int min;
    private int currentRotation = 0;
    private int sizeX = 0;
    private int sizeY = 0;

    private int minX;
    private int minY;
    private int maxX;
    private int maxY;

    private Vector2 anchorOffset;
    private Vector2 anchorCellWorld;

    private Item currentItem;
    void Awake()
    {
        corner1Pos = Vector2Int.RoundToInt(corner1.transform.position);
        corner2Pos = Vector2Int.RoundToInt(corner2.transform.position);
        max = Vector2Int.Max(corner1Pos, corner2Pos);
        min = Vector2Int.Min(corner1Pos, corner2Pos);

        buildZoneObjects = new Dictionary<Vector2Int, bool>();
        // initialize empty object grid
        for(int x = min.x; x < max.x; x++)
        {
            for(int y = min.y; y < max.y; y++)
            {
                buildZoneObjects.Add(new Vector2Int(x, y), false);
            }
        }

        GenerateGridSquares();
    }

    private void BuildModeLoop()
    {
        ItemStack stack;
        InventoryManager.GetItemInSlot(InputManager.CurrentlySelectedInventorySlot, out stack);
        if(stack && stack.GetItem().IsPlaceable)
        {
            currentItem = stack.GetItem();
            // hide cell indicator when we are previewing the placement of an item
            if(cellIndicator.activeSelf) cellIndicator.SetActive(false);

            // if we have selected a new item
            if(lastSelectedInventorySlot != InputManager.CurrentlySelectedInventorySlot)
            {
                sizeX = currentItem.Size.x;
                sizeY = currentItem.Size.y;

                if(previewItemPrefab)
                {
                    Destroy(previewItemPrefab);
                    currentRotation = 0;
                }
                lastSelectedInventorySlot = InputManager.CurrentlySelectedInventorySlot;
                previewItemPrefab = Instantiate(currentItem.ItemPrefab, cellIndicator.transform.position, quaternion.identity);
                previewItemPrefab.name = "Object Placement Preview";
                previewItemPrefab.GetComponent<Collider2D>().enabled = false;
            }

            // if there is currently a selected item
            if(previewItemPrefab)
            {
                HandleRotation();
                HandlePrefabPlacementLocation();
            }
        }
        else if((!stack || !stack.GetItem().IsPlaceable) && previewItemPrefab)
        {
            Destroy(previewItemPrefab);
            currentRotation = 0;
            lastSelectedInventorySlot = -1;
        }
    }

    void Update()
    {
        // Maintain correct gridShowing value
        gridShowing = (InputManager.BuildMode && !gridShowing) ? true : (!InputManager.BuildMode && gridShowing) ? false : gridShowing;

        UpdateGridVisibility();
        UpdateCellIndicatorPosition();
        
        // want to show item location before placing it, make it snap to new location each time mouse moves grid

        if(InputManager.BuildMode) 
        {
            BuildModeLoop();
            if(InputManager.IsPlacing) PlaceObject();
        }
        else if(previewItemPrefab) StopBuildModeLoop();
    }

    private void StopBuildModeLoop()
    {
        if(previewItemPrefab) Destroy(previewItemPrefab);
        
        lastSelectedInventorySlot = -1;
    }

    private void HandleRotation()
    {
        if(InputManager.IsRotating)
        {
            SwapItemSize();
            currentRotation += 90;
        }
    }

    private void PlaceObject()
    {
        if(!IsObstructed(anchorCellWorld) && previewItemPrefab)
        {
            Place(anchorCellWorld);
            GameObject placedObject = Instantiate(previewItemPrefab.gameObject, previewItemPrefab.transform.position, previewItemPrefab.transform.rotation);
            placedObject.name = $"{currentItem.Name} : ({anchorCellWorld.x}, {anchorCellWorld.y})";
            placedObject.GetComponent<SpriteRenderer>().color = Color.white;
            placedObject.GetComponent<Collider2D>().enabled = true;
            Destroy(previewItemPrefab);
            InventoryManager.ConsumeItemInSlot(InputManager.CurrentlySelectedInventorySlot, 1);

            // i want to exit build mode when i place an object
            InputManager.SetBuildMode(false);
        }
    }

    private void CalculateAnchorOffset(Vector2 localOffset)
    {
        // anchor offset is used to determine where the center of the object is relative to the mouse when staging to place the object

        // we do not need to calculate anchor offset if the axis is only size 1
        if(sizeX > 1)
        {
            if(sizeX % 2 == 0) anchorOffset.x = (localOffset.x < 0f) ? (sizeX / 2) : (sizeX / 2 - 1);

            else anchorOffset.x = sizeX / 2;
        }
        
        if(sizeY > 1)
        {
            if(sizeY % 2 == 0) anchorOffset.y = (localOffset.y < 0f) ? (sizeY / 2) : (sizeY / 2 - 1);

            else anchorOffset.y = sizeY / 2;
        }
    }

    private void HandlePrefabPlacementLocation()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 cellCentre = cellIndicator.transform.position;

        Vector2 localOffset = mouseWorldPos - cellCentre;
        anchorOffset = Vector2.zero;

        CalculateAnchorOffset(localOffset);

        anchorCellWorld = cellCentre - new Vector2(anchorOffset.x, anchorOffset.y);
        anchorCellWorld = ClampToAvailableSpaceInBuildZone(anchorCellWorld);
        Vector2 centreOffset = new Vector2((sizeX - 1) * 0.5f, (sizeY - 1) * 0.5f);

        previewItemPrefab.transform.position = anchorCellWorld + centreOffset;
        previewItemPrefab.transform.localEulerAngles = new Vector3(0, 0, currentRotation);

        if(IsObstructed(anchorCellWorld)) { previewItemPrefab.GetComponent<SpriteRenderer>().color = unableToBuildColour; }
        else { previewItemPrefab.GetComponent<SpriteRenderer>().color = ableToBuildColour; }
    }

    private Vector2 ClampToAvailableSpaceInBuildZone(Vector2 pos)
    {
        Vector2Int cellPos = GetCellPos(pos);
        
        Queue<Vector2Int> queue = new Queue<Vector2Int>(); // cells to visit
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>(); // cells that have been visited

        queue.Enqueue(cellPos); // start at first one obviously
        visited.Add(cellPos);

        while(queue.Count > 0)
        {
            Vector2Int currentSpace = queue.Dequeue();
            if(IsAreaFree(currentSpace)) return currentSpace + OFFSET;

            // how did i get -1 to 1? i think this should have something to do with the size of the object or something?
            for(int x = -1; x < 1; x++)
            {
                for(int y = -1; y < 1; y++)
                {
                    Vector2Int neighbourSpace = currentSpace + new Vector2Int(x, y);

                    // do not check 0, 0 or any position out of bounds.
                    if((x == 0 && y == 0) || !InBounds(neighbourSpace)) continue;

                    if(!visited.Contains(neighbourSpace))
                    {
                        visited.Add(neighbourSpace);
                        queue.Enqueue(neighbourSpace);
                    }
                }
            }
        }

        return new Vector2(cellPos.x, cellPos.y) + OFFSET;
    }

    

    private bool IsObstructed(Vector2 pos)
    {
        Vector2Int cellPos = GetCellPos(pos);

        for(int x = cellPos.x; x < cellPos.x + sizeX; x++)
        {
            for(int y = cellPos.y; y < cellPos.y + sizeY; y++)
            {
                if(buildZoneObjects[new Vector2Int(x, y)] == true)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsAreaFree(Vector2Int pos)
    {
        for(int x = 0; x < sizeX; x++)
        {
            for(int y = 0; y < sizeY; y++)
            {
                // if this coordinate exists in the build zone, and is populated, its not free
                if(buildZoneObjects.TryGetValue(new Vector2Int(pos.x + x, pos.y + y), out bool occupied) && occupied) return false;
            }
        }
        return true;
    }

    private void Place(Vector2 pos)
    {
        Vector2Int cellPos = GetCellPos(pos);

        for(int x = cellPos.x; x < cellPos.x + sizeX; x++)
        {
            for(int y = cellPos.y; y < cellPos.y + sizeY; y++)
            {
                if(buildZoneObjects[new Vector2Int(x, y)] == true) Debug.LogError("Build Zone: Something went wrong with the place function, object location already occupied.");
                else buildZoneObjects[new Vector2Int(x, y)] = true;
            }
        }
    }

    private void UpdateExtremes()
    {
        minX = Mathf.RoundToInt(min.x);
        minY = Mathf.RoundToInt(min.y);
        maxX = Mathf.RoundToInt(max.x) - sizeX;
        maxY = Mathf.RoundToInt(max.y) - sizeY;
    }

    private Vector2Int GetCellPos(Vector2 pos)
    {
        Vector2Int cellPos = Vector2Int.FloorToInt(pos);
        UpdateExtremes();
        cellPos.x = Mathf.Clamp(cellPos.x, minX, maxX);
        cellPos.y = Mathf.Clamp(cellPos.y, minY, maxY);
        return cellPos;
    }

    private void UpdateCellIndicatorPosition()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPos = grid.WorldToCell(mouseWorldPos);

        mouseIndicator.transform.position = mouseWorldPos;
        cellIndicator.transform.position = (Vector2)grid.CellToWorld(gridPos) + OFFSET;
    }

    private void GenerateGridSquares()
    {
        for(float x = min.x; x < max.x; x++)
        {
            for(float y = min.y; y < max.y; y++)
            {
                GameObject square = Instantiate(gridSquare, new Vector2(x, y) + OFFSET, Quaternion.identity);
                square.transform.SetParent(gridSquareContainer.transform);
                square.name = $"Grid Square ({x}, {y})";

                Color colour = square.GetComponent<SpriteRenderer>().color;
                colour.a = ((int)x + (int)y) % 2 == 0 ? evenAlphaLevel : oddAlphaLevel;
                square.GetComponent<SpriteRenderer>().color = colour;
            }
        }
    }
    
    private void UpdateGridVisibility()
    {        
        // Show the cell indicator when hovering over a tile in the build zone
        if(gridShowing && InBounds(cellIndicator.transform.position)) cellIndicator.SetActive(true);
        else if(!gridShowing || !InBounds(cellIndicator.transform.position)) cellIndicator.SetActive(false);
        
        if(gridShowing && !gridSquareContainer.activeSelf) gridSquareContainer.SetActive(true);
        else if(!gridShowing && gridSquareContainer.activeSelf) gridSquareContainer.SetActive(false);
    }

    private bool InBounds(Vector2 pos)
    {
        return pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y;
    }

    // TODO: this is trash
    private void SwapItemSize()
    {
        int temp = sizeX;
        sizeX = sizeY;
        sizeY = temp;
    }
}