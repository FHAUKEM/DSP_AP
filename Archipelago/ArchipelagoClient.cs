using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using DSP_AP.GameLogic;
using DSP_AP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DSP_AP.Archipelago;

public class ArchipelagoClient
{
    #region Constants
    public const string APVersion = "0.6.3";
    private const string Game = "Dyson Sphere Program";
    #endregion

    #region Static Fields
    public static bool Authenticated;
    public static ArchipelagoData ServerData = new();
    #endregion

    #region Instance Fields
    private bool attemptingConnection;
    private DeathLinkHandler DeathLinkHandler;
    private ArchipelagoSession session;
    #endregion

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
            CheckLocationsAsync();
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
            List<long> locations = TechUnlockService.GetUnlockedTechIds();
            session.Locations.CompleteLocationChecksAsync(locations.ToArray());
            ArchipelagoConsole.LogMessage($"Sent location checks to server!");
            GoalCheckerService.CheckForGoalCompletion(session, locations);
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

        TechUnlockService.ApplyTechRewards(GameMain.history, receivedItem.ItemId);
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
        session = null;
        Authenticated = false;
        attemptingConnection = false;
    }

    /// <summary>
    /// something went wrong closing our connection. disconnect and clean up
    /// </summary>
    /// <param name="reason"></param>
    private void OnSessionSocketClosed(string reason)
    {
        Plugin.BepinLogger.LogError($"Connection to Archipelago lost: {reason}");
        Disconnect();
        attemptingConnection = false;
    }
}