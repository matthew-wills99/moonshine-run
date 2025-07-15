using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class InputManager : MonoBehaviour
{
    public static Vector2 Movement;
    public static int CurrentlySelectedInventorySlot = 0;
    public static bool BuildMode = false;
    public static bool IsPlacing = false;
    public static bool IsDestroying = false;
    public static bool IsRotating = false;
    public static Vector3 lastMousePosition;

    public static bool Interacting = false;
    public static bool InteractHolding = false;

    private PlayerInput playerInput;

    private InputAction moveAction;

    // inventory
    private InputAction number1;
    private InputAction number2;
    private InputAction number3;
    private InputAction number4;
    private InputAction number5;
    private InputAction number6;
    private InputAction number7;

    // build mode
    private InputAction buildMode;
    private InputAction place;
    private InputAction destroy;
    private InputAction rotate;

    // interacting
    private InputAction interact;

    private List<InputAction> inventorySlotActions;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];

        number1 = playerInput.actions["Inventory Slot 1"];
        number2 = playerInput.actions["Inventory Slot 2"];
        number3 = playerInput.actions["Inventory Slot 3"];
        number4 = playerInput.actions["Inventory Slot 4"];
        number5 = playerInput.actions["Inventory Slot 5"];
        number6 = playerInput.actions["Inventory Slot 6"];
        number7 = playerInput.actions["Inventory Slot 7"];

        buildMode = playerInput.actions["Build Mode"];
        place = playerInput.actions["Place"];
        destroy = playerInput.actions["Destroy"];
        rotate = playerInput.actions["Rotate"];

        interact = playerInput.actions["Interact"];

        interact.performed += OnInteractPerformed;
        interact.canceled += OnInteractCanceled;

        inventorySlotActions = new List<InputAction>
        {
            number1,
            number2,
            number3,
            number4,
            number5,
            number6,
            number7
        };
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if(ctx.interaction is HoldInteraction)
        {
            InteractHolding = true;
            Interacting = false;
        }
        else if(ctx.interaction is PressInteraction)
        {
            Interacting = true;
        }
    }

    private void OnInteractCanceled(InputAction.CallbackContext ctx)
    {
        InteractHolding = false;
    }

    private void LateUpdate()
    {
        Interacting = false;
    }

    private void Update()
    {
        Movement = moveAction.ReadValue<Vector2>();
        
        foreach(InputAction action in inventorySlotActions)
        {
            // ensure we get the most recently pressed button
            if(action.WasPressedThisFrame())
            {
                CurrentlySelectedInventorySlot = inventorySlotActions.IndexOf(action);
                //Debug.Log($"Currently selected inventory slot: {CurrentlySelectedInventorySlot}");
            }
        }

        if(buildMode.WasPressedThisFrame())
        {
            BuildMode = !BuildMode; // toggle build mode
            Debug.Log($"Build mode: {BuildMode}");
        }


        // can be foreach kvp 

        if(place.WasPressedThisFrame()) IsPlacing = true;
        else IsPlacing = false;

        if(destroy.WasPressedThisFrame()) IsDestroying = true;
        else IsDestroying = false;

        if(rotate.WasPressedThisFrame()) IsRotating = true;
        else IsRotating = false;
    }

    public static void SetBuildMode(bool state)
    {
        BuildMode = state;
    }
}
