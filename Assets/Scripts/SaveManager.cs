using UnityEngine;
using System.IO;
using System.Threading.Tasks;

public static class SaveManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

    public static async void Save(int highScore)
    {
        PlayerData playerData = new PlayerData();
        playerData.highScore = highScore;
        
        playerData.playerSettings.Add(new PlayerSettingItem("MaxTile", PlayerPrefs.GetInt("MaxTile", 2048).ToString()));
        playerData.playerSettings.Add(new PlayerSettingItem("Rows", PlayerPrefs.GetInt("Rows", 4).ToString()));
        playerData.playerSettings.Add(new PlayerSettingItem("Columns", PlayerPrefs.GetInt("Columns", 4).ToString()));
        
        float animDuration = PlayerPrefs.GetFloat("AnimDuration", 0.05f);
        playerData.playerSettings.Add(new PlayerSettingItem("AnimDuration", animDuration.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        string json = JsonUtility.ToJson(playerData);

        try
        {
            await Task.Run(() => File.WriteAllText(SavePath, json));
        }
        catch(System.Exception e)
        {
            Debug.LogError($"Ошибка при сохранении файла: {e.Message}");
        }
    }

    public static PlayerData GetPlayerData()
    {
        try
        {
            if(!File.Exists(SavePath))
            {
                Debug.Log("Файл сохранения отсутствует. Возвращаем новые данные.");
                return new PlayerData();
            }

            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        catch(System.Exception e)
        {
            Debug.LogError($"Ошибка при чтении файла сохранения (файл поврежден): {e.Message}");
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
                Debug.Log("Файл сохранения успешно удален.");
            }
            else
            {
                Debug.Log("Попытка удаления не удалась: файл сохранения не существует.");
            }
        }
        catch(System.Exception e)
        {
            Debug.LogError($"Критическая ошибка при попытке удалить файл: {e.Message}");
        }
    }
}