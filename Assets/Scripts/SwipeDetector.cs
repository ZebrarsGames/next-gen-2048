using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeDetector : MonoBehaviour, IInputDetector
{
    private enum SwipeState
    {
        NotStarted,
        Started
    }

    [Header("Settings")]
    [SerializeField] private float minSwipeDistance = 50f;
    [SerializeField] private float minSwipeDuration = 0.05f;
    [SerializeField] private float maxSwipeDuration = 0.8f;

    private SwipeState _state = SwipeState.NotStarted;
    private Vector2 _startPoint;
    private float _timeSwipeStarted;

    public InputDirection? DetectInputDirection()
    {
        Pointer pointer = Pointer.current;
        if(pointer == null) return null;

        if(_state == SwipeState.NotStarted)
        {
            if(pointer.press.wasPressedThisFrame)
            {
                _timeSwipeStarted = Time.unscaledTime;
                _startPoint = pointer.position.ReadValue();
                _state = SwipeState.Started;
            }
        }
        else if(_state == SwipeState.Started)
        {
            if(pointer.press.wasReleasedThisFrame)
            {
                _state = SwipeState.NotStarted;

                float duration = Time.unscaledTime - _timeSwipeStarted;
                if(duration < minSwipeDuration || duration > maxSwipeDuration)
                    return null;

                Vector2 currentPoint = pointer.position.ReadValue();
                Vector2 swipeDelta = currentPoint - _startPoint;

                if(swipeDelta.sqrMagnitude < minSwipeDistance * minSwipeDistance)
                    return null;

                if(Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                {
                    return swipeDelta.x > 0f ? InputDirection.Right : InputDirection.Left;
                }
                else
                {
                    return swipeDelta.y > 0f ? InputDirection.Up : InputDirection.Down;
                }
            }
        }

        return null;
    }
}