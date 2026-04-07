using System.Collections.Generic;
using UnityEngine;

public static class SaveLoadManager
{
    private const string META_CURRENCY_KEY = "MetaCurrency";
    private const string LEVEL_COMPLETED_KEY_PREFIX = "LevelCompleted_";
    private const string PROGRESSION_INITIALIZED_KEY = "LevelProgression_Initialized";
    private const string SELECTED_SLOT_KEY = "SelectedSaveSlotIndex";

    public static int SelectedSlotIndex => PlayerPrefs.GetInt(SELECTED_SLOT_KEY, 1);

    public static void SetSelectedSlot(int slotIndex)
    {
        PlayerPrefs.SetInt(SELECTED_SLOT_KEY, slotIndex);
        PlayerPrefs.Save();
    }

    public static void SaveSlot(int slotIndex)
    {
        var data = new SaveSlotData();
        data.metaCurrency = PlayerPrefs.GetInt(META_CURRENCY_KEY, 0);
        data.lastCompletedLevel = LevelProgressionManager.LastCompletedLevel;
        MetaUpgradeState.ExportToSaveData(data);

        string[] order = LevelProgressionManager.GetProgressionOrder();
        for (int i = 0; i < order.Length; i++)
        {
            string sceneName = order[i];
            if (string.IsNullOrEmpty(sceneName)) continue;

            int completed = PlayerPrefs.GetInt(LEVEL_COMPLETED_KEY_PREFIX + sceneName, 0);
            if (completed == 1)
                data.completedScenes.Add(sceneName);
        }

        SaveSystem.Save(slotIndex, data);
        PlayerPrefs.Save();
        Debug.Log($"[SaveLoadManager] Saved slot {slotIndex} (meta={data.metaCurrency}, completed={data.completedScenes.Count}).");
    }

    public static bool LoadSlot(int slotIndex)
    {
        if (!SaveSystem.TryLoad(slotIndex, out SaveSlotData data) || data == null)
        {
            Debug.LogWarning($"[SaveLoadManager] No save found for slot {slotIndex}.");
            return false;
        }

        SetSelectedSlot(slotIndex);

        // Restore meta currency
        PlayerPrefs.SetInt(META_CURRENCY_KEY, data.metaCurrency);

        // Restore level progression
        string[] order = LevelProgressionManager.GetProgressionOrder();
        HashSet<string> completedSet = new HashSet<string>(data.completedScenes ?? new List<string>());

        for (int i = 0; i < order.Length; i++)
        {
            string sceneName = order[i];
            if (string.IsNullOrEmpty(sceneName)) continue;

            bool isCompleted = completedSet.Contains(sceneName);
            PlayerPrefs.SetInt(LEVEL_COMPLETED_KEY_PREFIX + sceneName, isCompleted ? 1 : 0);
        }

        // Prevent LevelProgressionManager from clearing progression on first run.
        PlayerPrefs.SetInt(PROGRESSION_INITIALIZED_KEY, 1);

        LevelProgressionManager.LastCompletedLevel = data.lastCompletedLevel;
        MetaUpgradeState.ImportFromSaveData(data);
        PlayerPrefs.Save();

        Debug.Log($"[SaveLoadManager] Loaded slot {slotIndex} (meta={data.metaCurrency}, completed={data.completedScenes?.Count ?? 0}).");
        return true;
    }

    /// <summary>
    /// Starts a new game in a specific slot:
    /// - Selects the slot
    /// - If a save exists, loads it
    /// - Otherwise clears meta + level completion progression back to defaults
    /// </summary>
    public static void StartNewGameInSlot(int slotIndex)
    {
        SetSelectedSlot(slotIndex);

        if (SaveSystem.HasSave(slotIndex))
        {
            LoadSlot(slotIndex);
            return;
        }

        // Reset meta currency
        PlayerPrefs.SetInt(META_CURRENCY_KEY, 0);

        // Reuse the existing overworld return-cutscene pipeline.
        // Add a DialogueManager return cutscene config with completedLevelSceneName = "NewGameIntro".
        LevelProgressionManager.LastCompletedLevel = "NewGameIntro";
        
        // Reset level completion progression
        string[] order = LevelProgressionManager.GetProgressionOrder();
        for (int i = 0; i < order.Length; i++)
        {
            string sceneName = order[i];
            if (string.IsNullOrEmpty(sceneName)) continue;
            PlayerPrefs.SetInt(LEVEL_COMPLETED_KEY_PREFIX + sceneName, 0);
        }

        // Ensure LevelProgressionManager won't "wipe" again on first run
        PlayerPrefs.SetInt(PROGRESSION_INITIALIZED_KEY, 1);

        MetaUpgradeState.ResetAllRanks();

        PlayerPrefs.Save();
        Debug.Log($"[SaveLoadManager] Started new game in slot {slotIndex} (fresh meta).");
    }

    /// <summary>
    /// Deletes save data for the given slot (removes the slot json file).
    /// Does not change SelectedSlotIndex; saving after deletion will create a new save.
    /// </summary>
    public static bool DeleteSlot(int slotIndex)
    {
        bool ok = SaveSystem.DeleteSave(slotIndex);
        PlayerPrefs.Save();
        Debug.Log($"[SaveLoadManager] Delete slot {slotIndex}: {(ok ? "OK" : "Nothing to delete")}");
        return ok;
    }
}

