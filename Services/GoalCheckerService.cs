using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using DSP_AP;
using DSP_AP.Utils;
using System.Collections.Generic;
using System.Linq;

public static class GoalCheckerService
{
    public static void CheckForGoalCompletion(ArchipelagoSession session, IEnumerable<long> checkedLocations)
    {
        if (checkedLocations.Contains(Plugin.GoalTechID))
        {
            session.Socket.SendPacketAsync(new StatusUpdatePacket
            {
                Status = ArchipelagoClientState.ClientGoal
            });

            ArchipelagoConsole.LogMessage("Sent Goal Complete to server!");
        }
    }
}