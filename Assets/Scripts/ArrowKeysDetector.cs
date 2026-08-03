using Assets.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowKeysDetector : MonoBehaviour, IInputDetector
{
    private InputAction moveAction;

    void Awake()
    {
        moveAction = new InputAction("Move", type: InputActionType.Button);
        
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");
    }

    void OnEnable() => moveAction.Enable();
    void OnDisable() => moveAction.Disable();

    public InputDirection? DetectInputDirection()
    {
        if(!moveAction.triggered) return null;

        Vector2 input = moveAction.ReadValue<Vector2>();

        if(input.y > 0) return InputDirection.Top;
        if(input.y < 0) return InputDirection.Bottom;
        if(input.x < 0) return InputDirection.Left;
        if(input.x > 0) return InputDirection.Right;

        return null;
    }
}