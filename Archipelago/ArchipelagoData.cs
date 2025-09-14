using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DSP_AP.Archipelago;

public class ArchipelagoData
{
    public string Uri;
    public string SlotName;
    public string Password;
    public int Index;
    public string filePath;
    public string saveDirectory = "apsaves";

    public List<long> CheckedLocations;
    public List<long> ReceivedLocations = new();

    /// <summary>
    /// seed for this archipelago data. Can be used when loading a file to verify the session the player is trying to
    /// load is valid to the room it's connecting to.
    /// </summary>
    private string seed;

    private Dictionary<string, object> slotData;

    public bool NeedSlotData => slotData == null;

    public ArchipelagoData()
    {
            Uri = "localhost";
            SlotName = "Player1";
            CheckedLocations = new();
    }

    public ArchipelagoData(string uri, string slotName, string password)
    {
        Uri = uri;
        SlotName = slotName;
        Password = password;
        CheckedLocations = new();
    }

    /// <summary>
    /// assigns the slot data and seed to our data handler. any necessary setup using this data can be done here.
    /// </summary>
    /// <param name="roomSlotData">slot data of your slot from the room</param>
    /// <param name="roomSeed">seed name of this session</param>
    public void SetupSession(Dictionary<string, object> roomSlotData, string roomSeed)
    {
        slotData = roomSlotData;
        seed = roomSeed;

        string fileName = $"{SlotName}.{seed}.json";

        filePath = Path.Combine(Plugin.PluginPath, saveDirectory, fileName);
        LoadCheckedLocations();
    }

    public void SaveCheckedLocations()
    {
        try
        {
            string json = JsonConvert.SerializeObject(CheckedLocations, Formatting.Indented);
            Directory.CreateDirectory(Path.Combine(Plugin.PluginPath, saveDirectory));
            File.WriteAllText(filePath, json);

            Plugin.BepinLogger.LogInfo($"Created Archipelago data file: {filePath}");
        }
        catch (Exception ex)
        {
            Plugin.BepinLogger.LogError($"Failed to save checked locations: {ex.Message}");
        }
    }

    private void LoadCheckedLocations()
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var loaded = JsonConvert.DeserializeObject<List<long>>(json);

                if (loaded == null) return;
                CheckedLocations = CheckedLocations
                    .Union(loaded) // Combines without duplicates
                    .ToList();

                Plugin.BepinLogger.LogInfo($"Loaded Archipelago data from file: {filePath}");
            }
            else
            {
                // Create the file if it doesn't exist
                SaveCheckedLocations();
            }
        }
        catch (Exception ex)
        {
            Plugin.BepinLogger.LogError($"Failed to load checked locations: {ex.Message}");
        }
    }

    /// <summary>
    /// returns the object as a json string to be written to a file which you can then load
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
