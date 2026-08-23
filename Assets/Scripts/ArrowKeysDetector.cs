using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowKeysDetector : MonoBehaviour, IInputDetector
{
    private InputAction _moveAction;

    private void Awake()
    {
        _moveAction = new InputAction("Move", type: InputActionType.Button);
        
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");
    }

    private void OnEnable() => _moveAction.Enable();
    private void OnDisable() => _moveAction.Disable();

    private void OnDestroy()
    {
        _moveAction?.Dispose();
    }

    public InputDirection? DetectInputDirection()
    {
        if(!_moveAction.triggered) return null;

        Vector2 input = _moveAction.ReadValue<Vector2>();

        if(input.y > 0f) return InputDirection.Up;
        if(input.y < 0f) return InputDirection.Down;
        if(input.x < 0f) return InputDirection.Left;
        if(input.x > 0f) return InputDirection.Right;

        return null;
    }
}