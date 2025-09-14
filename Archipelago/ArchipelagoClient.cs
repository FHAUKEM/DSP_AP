using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using DSP_AP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DSP_AP.Archipelago;

public class ArchipelagoClient
{
    public const string APVersion = "0.6.3";
    private const string Game = "Dyson Sphere Program";

    public static bool Authenticated;
    private bool attemptingConnection;

    public static ArchipelagoData ServerData = new();
    private DeathLinkHandler DeathLinkHandler;
    private ArchipelagoSession session;

    /// <summary>
    /// call to connect to an Archipelago session. Connection info should already be set up on ServerData
    /// </summary>
    /// <returns></returns>
    public void Connect()
    {
        if (Authenticated || attemptingConnection) return;

        try
        {
            session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri);
            SetupSession();
        }
        catch (Exception e)
        {
            Plugin.BepinLogger.LogError(e);
        }

        TryConnect();
    }

    /// <summary>
    /// add handlers for Archipelago events
    /// </summary>
    private void SetupSession()
    {
        session.MessageLog.OnMessageReceived += message => ArchipelagoConsole.LogMessage(message.ToString());
        session.Items.ItemReceived += OnItemReceived;
        session.Socket.ErrorReceived += OnSessionErrorReceived;
        session.Socket.SocketClosed += OnSessionSocketClosed;
    }

    /// <summary>
    /// attempt to connect to the server with our connection info
    /// </summary>
    private void TryConnect()
    {
        try
        {
            // it's safe to thread this function call but unity notoriously hates threading so do not use excessively
            ThreadPool.QueueUserWorkItem(
                _ => HandleConnectResult(
                    session.TryConnectAndLogin(
                        Game,
                        ServerData.SlotName,
                        ItemsHandlingFlags.AllItems,
                        new System.Version(APVersion),
                        password: ServerData.Password,
                        requestSlotData: ServerData.NeedSlotData
                    )));
        }
        catch (Exception e)
        {
            Plugin.BepinLogger.LogError(e);
            HandleConnectResult(new LoginFailure(e.ToString()));
            attemptingConnection = false;
        }
    }

    /// <summary>
    /// handle the connection result and do things
    /// </summary>
    /// <param name="result"></param>
    private void HandleConnectResult(LoginResult result)
    {
        string outText;
        if (result.Successful)
        {
            var success = (LoginSuccessful)result;

            ServerData.SetupSession(success.SlotData, session.RoomState.Seed);
            Authenticated = true;

            DeathLinkHandler = new(session.CreateDeathLinkService(), ServerData.SlotName);
            session.Locations.CompleteLocationChecksAsync(ServerData.CheckedLocations.ToArray());
            outText = $"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}!";

            ArchipelagoConsole.LogMessage(outText);
        }
        else
        {
            var failure = (LoginFailure)result;
            outText = $"Failed to connect to {ServerData.Uri} as {ServerData.SlotName}.";
            outText = failure.Errors.Aggregate(outText, (current, error) => current + $"\n    {error}");

            Plugin.BepinLogger.LogError(outText);

            Authenticated = false;
            Disconnect();
        }

        ArchipelagoConsole.LogMessage(outText);
        attemptingConnection = false;
    }

    /// <summary>
    /// something went wrong, or we need to properly disconnect from the server. cleanup and re null our session
    /// </summary>
    public void Disconnect()
    {
        Plugin.BepinLogger.LogDebug("disconnecting from server...");
        session?.Socket.DisconnectAsync();
        session = null;
        Authenticated = false;
    }

    public void SendMessage(string message)
    {
        session.Socket.SendPacketAsync(new SayPacket { Text = message });
    }


    public void CheckLocationsAsync()
    {
        if (Authenticated)
        {
            session.Locations.CompleteLocationChecksAsync(ServerData.CheckedLocations.ToArray());
            ArchipelagoConsole.LogMessage($"Sent location checks to server!");

            if (ServerData.CheckedLocations.Contains(1508))
            {
                StatusUpdatePacket goalCompletePacket = new StatusUpdatePacket
                {
                    Status = ArchipelagoClientState.ClientGoal
                };

                session.Socket.SendPacketAsync(goalCompletePacket);

                ArchipelagoConsole.LogMessage($"Sent Goal Complete to server!");
            }
        }
        else
        {
            ArchipelagoConsole.LogMessage($"Server not connected. Locations will be sent on next server connect.");
        }
    }


    /// <summary>
    /// we received an item so reward it here
    /// </summary>
    /// <param name="helper">item helper which we can grab our item from</param>
    private void OnItemReceived(ReceivedItemsHelper helper)
    {
        var receivedItem = helper.DequeueItem();
        if (helper.Index <= ServerData.Index) return;

        Plugin.BepinLogger.LogDebug("Item received with id: " + receivedItem.ItemId);

        ServerData.Index++;

        ServerData.ReceivedLocations.Add(receivedItem.ItemId);
        int techId = (int)receivedItem.ItemId;
        Plugin.BepinLogger.LogDebug($"Unlocking rewards for research id: {techId}");
        APTechShadow techProto = Plugin.APTechProtos.FirstOrDefault(t => t.ID == techId);

        if (techProto != null)
        {
            GameHistoryData history = GameMain.history;
            if (!history.techStates.ContainsKey(techId))
            {
                Plugin.BepinLogger.LogDebug($"No techstate found for id {techId}");
                return;
            }
            int maxLevel = history.techStates[techId].maxLevel;
            for (int i = 0; i < techProto.UnlockRecipes.Length; i++)
            {
                history.UnlockRecipe(techProto.UnlockRecipes[i]);
            }
            for (int j = 0; j < techProto.UnlockFunctions.Length; j++)
            {
                history.UnlockTechFunction(techProto.UnlockFunctions[j], techProto.UnlockValues[j], maxLevel);
            }
            for (int k = 0; k < techProto.AddItems.Length; k++)
            {
                int itemId = techProto.AddItems[k];
                int itemCountVal = techProto.AddItemCounts[k];
            }
            Plugin.BepinLogger.LogInfo($"Unlocked research rewards for tech ID: {techId}");
        }
        else
        {
            Plugin.BepinLogger.LogDebug($"Shadow TechProto not found for id {techId}");
        }
    }

    /// <summary>
    /// something went wrong with our socket connection
    /// </summary>
    /// <param name="e">thrown exception from our socket</param>
    /// <param name="message">message received from the server</param>
    private void OnSessionErrorReceived(Exception e, string message)
    {
        Plugin.BepinLogger.LogError(e);
        ArchipelagoConsole.LogMessage(message);
    }

    /// <summary>
    /// something went wrong closing our connection. disconnect and clean up
    /// </summary>
    /// <param name="reason"></param>
    private void OnSessionSocketClosed(string reason)
    {
        Plugin.BepinLogger.LogError($"Connection to Archipelago lost: {reason}");
        Disconnect();
    }
}