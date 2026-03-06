using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private const string PLAYER_PREFS_BINDING = "InputBindings";

    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnBindingRebind;

    public enum Binding
    {
        MoveUp, MoveDown, MoveLeft, MoveRight, Interact, InteractAlternate, Pause,
    }
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.InteractAlternate.performed += InteractAlternate_performed;
        playerInputActions.Player.Pause.performed += Pause_performed;

        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDING))
        {
            string bindingsJson = PlayerPrefs.GetString(PLAYER_PREFS_BINDING);
            playerInputActions.LoadBindingOverridesFromJson(bindingsJson);
        }
    }
    private void OnDestroy()
    {
        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.InteractAlternate.performed -= InteractAlternate_performed;
        playerInputActions.Player.Pause.performed -= Pause_performed;
        playerInputActions.Dispose();
    }

    private void Pause_performed(InputAction.CallbackContext context)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlternate_performed(InputAction.CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        //Debug.Log(obj);

        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        //// old input manager
        //return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        // new input system
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;
    }

    public string GetBindingText(Binding binding)
    {
        switch (binding)
        {
            case Binding.MoveUp:
                return playerInputActions.Player.Move.bindings[1].ToDisplayString();
            case Binding.MoveDown:
                return playerInputActions.Player.Move.bindings[2].ToDisplayString();
            case Binding.MoveLeft:
                return playerInputActions.Player.Move.bindings[3].ToDisplayString();
            case Binding.MoveRight:
                return playerInputActions.Player.Move.bindings[4].ToDisplayString();
            case Binding.Interact:
                return playerInputActions.Player.Interact.bindings[0].ToDisplayString();
            case Binding.InteractAlternate:
                return playerInputActions.Player.InteractAlternate.bindings[0].ToDisplayString();
            case Binding.Pause:
                return playerInputActions.Player.Pause.bindings[0].ToDisplayString();
            default:
                throw new NotImplementedException();
        }
    }
    public void RebindBinding(Binding binding, Action onActionRebind = null)
    {
        InputAction inputAction = null;
        int bindingIndex = -1;

        if (binding == Binding.MoveUp)
        {
            inputAction = playerInputActions.Player.Move;
            bindingIndex = 1;
        }
        else if (binding == Binding.MoveDown)
        {
            inputAction = playerInputActions.Player.Move;
            bindingIndex = 2;
        }
        else if (binding == Binding.MoveLeft)
        {
            inputAction = playerInputActions.Player.Move;
            bindingIndex = 3;
        }
        else if (binding == Binding.MoveRight)
        {
            inputAction = playerInputActions.Player.Move;
            bindingIndex = 4;
        }
        else if (binding == Binding.Interact)
        {
            inputAction = playerInputActions.Player.Interact;
            bindingIndex = 0;
        }
        else if (binding == Binding.InteractAlternate)
        {
            inputAction = playerInputActions.Player.InteractAlternate;
            bindingIndex = 0;
        }
        else if (binding == Binding.Pause)
        {
            inputAction = playerInputActions.Player.Pause;
            bindingIndex = 0;
        }
        playerInputActions.Player.Disable();
        inputAction.PerformInteractiveRebinding(bindingIndex)
            .OnComplete((callback) =>
            {
                //Debug.Log(callback.action.bindings[1].path);
                //Debug.Log(callback.action.bindings[1].overridePath);
                //Debug.Log(callback.action.bindings.Count);
                callback.Dispose();
                playerInputActions.Enable();
                onActionRebind?.Invoke();
                PlayerPrefs.SetString(PLAYER_PREFS_BINDING, playerInputActions.SaveBindingOverridesAsJson());
                OnBindingRebind?.Invoke(this, EventArgs.Empty);
            })
            .Start();
    }
}