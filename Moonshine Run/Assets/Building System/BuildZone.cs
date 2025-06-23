using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildZone : MonoBehaviour
{
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

    void Awake()
    {
        corner1Pos = Vector2Int.RoundToInt(corner1.transform.position);
        corner2Pos = Vector2Int.RoundToInt(corner2.transform.position);
        max = Vector2Int.Max(corner1Pos, corner2Pos);
        min = Vector2Int.Min(corner1Pos, corner2Pos);

        buildZoneObjects = new Dictionary<Vector2Int, bool>();
        // initialize with
        for(int x = min.x; x < max.x; x++)
        {
            for(int y = min.y; y < max.y; y++)
            {
                buildZoneObjects.Add(new Vector2Int(x, y), false);
                Debug.Log($"Added: ({x}, {y})");
            }
        }

        GenerateGridSquares();
    }

    void Update()
    {
        CheckGridVisibility();
        UpdateGridVisibility();
        UpdateCellIndicatorPosition();
        
        // want to show item location before placing it, make it snap to new location each time mouse moves grid

        if(InputManager.BuildMode)
        {
            ItemStack stack;
            InventoryManager.GetItemInSlot(InputManager.CurrentlySelectedInventorySlot, out stack);
            if(stack && stack.GetItem().IsPlaceable)
            {
                Item item = stack.GetItem();
                // hide cell indicator when we are previewing the placement of an item
                if(cellIndicator.activeSelf)
                {
                    cellIndicator.SetActive(false);
                }
                Debug.Log($"here");
                if(!stack && previewItemPrefab)
                {
                    Debug.Log($"Null");
                    Destroy(previewItemPrefab);
                    currentRotation = 0;
                }

                // if we have selected a new item
                if(lastSelectedInventorySlot != InputManager.CurrentlySelectedInventorySlot)
                {
                    sizeX = item.Size.x;
                    sizeY = item.Size.y;

                    if(previewItemPrefab)
                    {
                        Destroy(previewItemPrefab);
                        currentRotation = 0;
                    }
                    lastSelectedInventorySlot = InputManager.CurrentlySelectedInventorySlot;
                    previewItemPrefab = Instantiate(item.ItemPrefab, cellIndicator.transform.position, quaternion.identity);
                    previewItemPrefab.name = "Object Placement Preview";
                    previewItemPrefab.GetComponent<Collider2D>().enabled = false;
                }

                // if there is currently a selected item
                if(previewItemPrefab)
                {
                    HandleRotation(item);
                    HandlePrefabPlacementLocation(item);
                }
            }
            else if((!stack || !stack.GetItem().IsPlaceable) && previewItemPrefab)
            {
                Debug.Log($"Null here");
                Destroy(previewItemPrefab);
                lastSelectedInventorySlot = -1;
            }
            else if(!cellIndicator.activeSelf)
            {
                cellIndicator.SetActive(true);
            }
        }
        else
        {
            if(previewItemPrefab)
            {
                Destroy(previewItemPrefab);
            }
            lastSelectedInventorySlot = -1; // IMPORTANT
        }
        
        // check for placement or destroy

    }

    private void HandleRotation(Item item)
    {
        if(InputManager.IsRotating)
        {
            SwapItemSize();
            currentRotation += 90;
        }
    }

    private void HandlePrefabPlacementLocation(Item item)
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 cellCentre = cellIndicator.transform.position;

        Vector2 localOffset = mouseWorldPos - cellCentre;
        Vector2 anchorOffset = Vector2.zero;

        if(sizeX > 1)
        {
            if(sizeX % 2 == 0)
            {
                anchorOffset.x = (localOffset.x < 0f) ? (sizeX / 2) : (sizeX / 2 - 1);
            }
            else
            {
                anchorOffset.x = sizeX / 2;
            }
        }
        
        if(sizeY > 1)
        {
            if(sizeY % 2 == 0)
            {
                anchorOffset.y = (localOffset.y < 0f) ? (sizeY / 2) : (sizeY / 2 - 1);
            }
            else
            {
                anchorOffset.y = sizeY / 2;
            }
        }
        

        Vector2 anchorCellWorld = cellCentre - new Vector2(anchorOffset.x, anchorOffset.y);
        anchorCellWorld = ClampToAvailableSpaceInBuildZone(anchorCellWorld);
        Vector2 centreOffset = new Vector2((sizeX - 1) * 0.5f, (sizeY - 1) * 0.5f);

        previewItemPrefab.transform.position = anchorCellWorld + centreOffset;
        previewItemPrefab.transform.localEulerAngles = new Vector3(0, 0, currentRotation);

        if(IsObstructed(anchorCellWorld)) { previewItemPrefab.GetComponent<SpriteRenderer>().color = unableToBuildColour; }
        else { previewItemPrefab.GetComponent<SpriteRenderer>().color = ableToBuildColour; }


        if(InputManager.IsPlacing)
        {
            if(IsObstructed(anchorCellWorld))
            {
                Debug.Log("Cannot place here");
            }
            else
            {
                Place(anchorCellWorld);
                GameObject placedObject = Instantiate(previewItemPrefab.gameObject, previewItemPrefab.transform.position, previewItemPrefab.transform.rotation);
                placedObject.name = $"{item.Name} : ({anchorCellWorld.x}, {anchorCellWorld.y})";
                placedObject.GetComponent<SpriteRenderer>().color = Color.white;
                placedObject.GetComponent<Collider2D>().enabled = true;
                Destroy(previewItemPrefab);
                InventoryManager.ConsumeItemInSlot(InputManager.CurrentlySelectedInventorySlot, 1);

                Debug.Log($"Placed");


                // i want to exit build mode when i place an object
                InputManager.SetBuildMode(false);
            }
        }
    }

    private void SwapItemSize()
    {
        int temp = sizeX;
        sizeX = sizeY;
        sizeY = temp;
    }

    private Vector2 ClampToAvailableSpaceInBuildZone(Vector2 anchorCellWorld)
    {
        Vector2Int cellPos = Vector2Int.FloorToInt(anchorCellWorld);

        int minX = Mathf.RoundToInt(min.x);
        int minY = Mathf.RoundToInt(min.y);
        int maxX = Mathf.RoundToInt(max.x) - sizeX;
        int maxY = Mathf.RoundToInt(max.y) - sizeY;

        cellPos.x = Mathf.Clamp(cellPos.x, minX, maxX);
        cellPos.y = Mathf.Clamp(cellPos.y, minY, maxY);
        
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(cellPos);
        visited.Add(cellPos);

        while(queue.Count > 0)
        {
            Vector2Int currentSpace = queue.Dequeue();
            if(IsAreaFree(currentSpace))
            {
                Debug.Log($"{currentSpace + new Vector2(0.5f, 0.5f)} if 0.5 works");
                return currentSpace + new Vector2(0.5f, 0.5f);
            }

            for(int x = -1; x < 1; x++)
            {
                for(int y = -1; y < 1; y++)
                {
                    // refactor this its ugly as fuck.
                    Vector2Int neighbourSpace = currentSpace + new Vector2Int(x, y);
                    if
                    (
                        (x == 0 && y == 0) 
                        || neighbourSpace.x < minX || neighbourSpace.y < minY || neighbourSpace.x > maxX || neighbourSpace.y > maxY
                    )
                    {
                        continue; // do not check + (0, 0) because that is where we already are
                        // also skip if we are out of bounds
                    }

                    if(!visited.Contains(neighbourSpace))
                    {
                        visited.Add(neighbourSpace);
                        queue.Enqueue(neighbourSpace);
                    }
                }
            }
        }

        return new Vector2(cellPos.x, cellPos.y) + new Vector2(0.5f, 0.5f); // might be shit, maybe show red icon?
    }

    private bool IsObstructed(Vector2 anchorCellWorld)
    {
        Vector2Int cellPos = Vector2Int.FloorToInt(anchorCellWorld);

        int minX = Mathf.RoundToInt(min.x);
        int minY = Mathf.RoundToInt(min.y);
        int maxX = Mathf.RoundToInt(max.x) - sizeX;
        int maxY = Mathf.RoundToInt(max.y) - sizeY;

        cellPos.x = Mathf.Clamp(cellPos.x, minX, maxX);
        cellPos.y = Mathf.Clamp(cellPos.y, minY, maxY);

        Debug.Log($"x: {cellPos.x}, y: {cellPos.y}");

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

    private bool IsAreaFree(Vector2Int anchor)
    {
        for(int x = 0; x < sizeX; x++)
        {
            for(int y = 0; y < sizeY; y++)
            {
                Vector2Int pos = new Vector2Int(anchor.x + x, anchor.y + y);
                if(buildZoneObjects.TryGetValue(pos, out bool occupied) && occupied)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void Place(Vector2 anchorCellWorld)
    {
        Vector2Int cellPos = Vector2Int.FloorToInt(anchorCellWorld);

        int minX = Mathf.RoundToInt(min.x);
        int minY = Mathf.RoundToInt(min.y);
        int maxX = Mathf.RoundToInt(max.x) - sizeX;
        int maxY = Mathf.RoundToInt(max.y) - sizeY;

        cellPos.x = Mathf.Clamp(cellPos.x, minX, maxX);
        cellPos.y = Mathf.Clamp(cellPos.y, minY, maxY);

        for(int x = cellPos.x; x < cellPos.x + sizeX; x++)
        {
            for(int y = cellPos.y; y < cellPos.y + sizeY; y++)
            {
                if(buildZoneObjects[new Vector2Int(x, y)] == true)
                {
                    Debug.LogError("What the fuck");
                }
                else
                {
                    buildZoneObjects[new Vector2Int(x, y)] = true;
                }
            }
        }
    }

    private void CheckGridVisibility()
    {
        if(InputManager.BuildMode && !gridShowing)
        {
            gridShowing = true;
        }
        else if(!InputManager.BuildMode && gridShowing)
        {
            gridShowing = false;
        }
    }

    private void UpdateCellIndicatorPosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPos = grid.WorldToCell(mouseWorldPos);
        mouseWorldPos.z = 0;
        gridPos.z = 0;

        mouseIndicator.transform.position = mouseWorldPos;
        cellIndicator.transform.position = grid.CellToWorld(gridPos) + new Vector3(0.5f, 0.5f, 0);
    }

    private void GenerateGridSquares()
    {
        for(float x = min.x; x < max.x; x++)
        {
            for(float y = min.y; y < max.y; y++)
            {
                GameObject square = Instantiate(gridSquare, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
                square.transform.SetParent(gridSquareContainer.transform);
                square.name = $"Grid Square ({x}, {y})";

                Color colour = square.GetComponent<SpriteRenderer>().color;
                if(((int)x + (int)y) % 2 == 0)
                {
                    colour.a = evenAlphaLevel;
                }
                else
                {
                    colour.a = oddAlphaLevel;
                }
                square.GetComponent<SpriteRenderer>().color = colour;
            }
        }
    }
    
    private void UpdateGridVisibility()
    {
        // if in build mode, and within build zone bounds, and not yet currently active, show indicator
        if(gridShowing && IsWithinBuildZoneBounds(cellIndicator.transform.position) && !cellIndicator.activeSelf)
        {
            cellIndicator.SetActive(true);
        }
        // if not in build mode, or not within build zone bounds, and currently active, hide indicator
        else if((!gridShowing || !IsWithinBuildZoneBounds(cellIndicator.transform.position)) && cellIndicator.activeSelf)
        {
            cellIndicator.SetActive(false);
        }

        if(gridShowing && !gridSquareContainer.activeSelf)
        {
            gridSquareContainer.SetActive(true);
        }
        else if(!gridShowing && gridSquareContainer.activeSelf)
        {
            gridSquareContainer.SetActive(false);
        }
    }

    private bool IsWithinBuildZoneBounds(Vector2 pos)
    {
        return pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y;
    }
}
