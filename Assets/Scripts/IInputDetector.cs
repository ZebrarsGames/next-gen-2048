namespace Assets.Scripts
{
    public interface IInputDetector
    {
        InputDirection? DetectInputDirection();
    }

    public enum InputDirection
    {
        Left, Right, Top, Bottom
    }
}
