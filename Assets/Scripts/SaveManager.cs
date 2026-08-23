using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class SaveManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");
    
    private static readonly PlayerData CachedData = new PlayerData();

    public static async Task SaveAsync(int highScore)
    {
        CachedData.highScore = highScore;
        CachedData.maxTile = PlayerPrefs.GetInt("MaxTile", 2048);
        CachedData.rows = PlayerPrefs.GetInt("Rows", 4);
        CachedData.columns = PlayerPrefs.GetInt("Columns", 4);
        CachedData.animDuration = PlayerPrefs.GetFloat("AnimDuration", 0.05f);

        string json = JsonUtility.ToJson(CachedData);

        try
        {
            using(var writer = new StreamWriter(SavePath, false, Encoding.UTF8))
            {
                await writer.WriteAsync(json);
            }
        }
        catch(Exception e)
        {
            Debug.LogError($"[SaveManager] Ошибка при сохранении файла: {e.Message}");
        }
    }

    public static void SaveSync(int highScore)
    {
        CachedData.highScore = highScore;
        CachedData.maxTile = PlayerPrefs.GetInt("MaxTile", 2048);
        CachedData.rows = PlayerPrefs.GetInt("Rows", 4);
        CachedData.columns = PlayerPrefs.GetInt("Columns", 4);
        CachedData.animDuration = PlayerPrefs.GetFloat("AnimDuration", 0.05f);

        string json = JsonUtility.ToJson(CachedData);

        try
        {
            File.WriteAllText(SavePath, json, Encoding.UTF8);
        }
        catch(Exception e)
        {
            Debug.LogError($"[SaveManager] Ошибка при синхронном сохранении: {e.Message}");
        }
    }

    public static PlayerData GetPlayerData()
    {
        try
        {
            if(!File.Exists(SavePath))
            {
                return new PlayerData();
            }

            string json = File.ReadAllText(SavePath, Encoding.UTF8);
            return JsonUtility.FromJson<PlayerData>(json) ?? new PlayerData();
        }
        catch(Exception e)
        {
            Debug.LogError($"[SaveManager] Файл сохранения поврежден или недоступен: {e.Message}");
            return new PlayerData();
        }
    }

    public static void DeleteSaveFile()
    {
        try
        {
            if(File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("[SaveManager] Файл сохранения удален.");
            }
        }
        catch(Exception e)
        {
            Debug.LogError($"[SaveManager] Ошибка при удалении файла: {e.Message}");
        }
    }
}