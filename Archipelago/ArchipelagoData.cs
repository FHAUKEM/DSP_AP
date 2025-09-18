using Newtonsoft.Json;
using System.Collections.Generic;


namespace DSP_AP.Archipelago;

public class ArchipelagoData
{
    #region Public Fields
    public string Uri;
    public string SlotName;
    public string Password;
    public int Index;
    #endregion

    #region Private Fields
    /// <summary>
    /// seed for this archipelago data. Can be used when loading a file to verify the session the player is trying to
    /// load is valid to the room it's connecting to.
    /// </summary>
    private string seed;
    private Dictionary<string, object> slotData;
    #endregion

    #region Properties
    public bool NeedSlotData => slotData == null;
    #endregion

    public ArchipelagoData()
    {
        Uri = "localhost";
        SlotName = "Player1";
    }

    public ArchipelagoData(string uri, string slotName, string password)
    {
        Uri = uri;
        SlotName = slotName;
        Password = password;
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
