using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;

namespace DSP_AP.Utils;

public static class ArchipelagoDataStorage
{
    public static void SaveToFile(string filePath, List<long> data)
    {
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, json);
            Plugin.BepinLogger.LogInfo($"Created Archipelago data file: {filePath}");
        }
        catch (Exception ex)
        {
            Plugin.BepinLogger.LogError($"Failed to save checked locations: {ex.Message}");
        }
    }

    public static List<long> LoadFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Plugin.BepinLogger.LogWarning($"Checked locations file not found: {filePath}. A new file will be created.");
                return new List<long>();
            }

            string json = File.ReadAllText(filePath);
            var loaded = JsonConvert.DeserializeObject<List<long>>(json);
            Plugin.BepinLogger.LogInfo($"Loaded Archipelago data from file: {filePath}");
            return loaded ?? new List<long>();
        }
        catch (Exception ex)
        {
            Plugin.BepinLogger.LogError($"Failed to load checked locations: {ex.Message}");
            return new List<long>();
        }
    }
}
