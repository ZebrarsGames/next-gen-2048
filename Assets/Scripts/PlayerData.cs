using System;
using System.Collections.Generic;

[System.Serializable]
public class PlayerSettingItem
{
    public string key;
    public string value;

    public PlayerSettingItem(string key, string value)
    {
        this.key = key;
        this.value = value;
    }
}

[Serializable]
public class PlayerData
{
    public int highScore;
    public List<PlayerSettingItem> playerSettings = new List<PlayerSettingItem>();
}
