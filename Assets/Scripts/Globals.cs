using UnityEngine;

public static class Globals
{
    public static int Rows { get; private set; } = 4;
    public static int Columns { get; private set; } = 4;
    public static float AnimationDuration { get; private set; } = 0.05f;

    static Globals()
    {
        Apply();
    }

    public static void Apply()
    {
        Rows = PlayerPrefs.GetInt("Rows", 4);
        Columns = PlayerPrefs.GetInt("Columns", 4);
        AnimationDuration = PlayerPrefs.GetFloat("AnimDuration", 0.05f);
    }
}