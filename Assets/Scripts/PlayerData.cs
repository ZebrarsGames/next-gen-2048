using System;

[Serializable]
public class PlayerData
{
    public int highScore;
    public int maxTile = 2048;
    public int rows = 4;
    public int columns = 4;
    public float animDuration = 0.05f;
}