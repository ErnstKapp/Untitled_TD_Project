using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public const string SaveFilePrefix = "save_slot_";
    public const string SaveFileExtension = ".json";

    public static string GetSavePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"{SaveFilePrefix}{slotIndex}{SaveFileExtension}");
    }

    public static bool HasSave(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        return File.Exists(path) && new FileInfo(path).Length > 0;
    }

    public static bool DeleteSave(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        try
        {
            bool exists = File.Exists(path);
            if (!exists)
                return false;

            Debug.Log($"[SaveSystem] Deleting save slot {slotIndex} at '{path}'");
            if (!File.Exists(path))
                return false;

            File.Delete(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryLoad(int slotIndex, out SaveSlotData data)
    {
        data = null;
        string path = GetSavePath(slotIndex);
        if (!File.Exists(path))
            return false;

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json))
                return false;

            data = JsonUtility.FromJson<SaveSlotData>(json);
            return data != null;
        }
        catch
        {
            return false;
        }
    }

    public static void Save(int slotIndex, SaveSlotData data)
    {
        if (data == null)
            return;

        string path = GetSavePath(slotIndex);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }
}

