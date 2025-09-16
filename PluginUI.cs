using BepInEx;
using DSP_AP.Archipelago;
using DSP_AP.Partials;
using DSP_AP.Utils;
using UnityEngine;

namespace DSP_AP
{
    public static class PluginUI
    {
        public static void DrawModLabel()
        {
            GUI.Label(new Rect(16, 16, 300, 20), Plugin.ModDisplayInfo);
            GUI.Label(new Rect(16, 32, 300, 20), "Connect only in game!");
            ArchipelagoConsole.OnGUI();
        }

        public static void DrawStatusUI()
        {
            string statusMessage;
            if (ArchipelagoClient.Authenticated)
            {
                statusMessage = " Status: Connected";
                GUI.Label(new Rect(16, 50, 300, 20), $"Archipelago v{ArchipelagoClient.APVersion}" + statusMessage);
                // Cursor.visible = false; // Uncomment if needed
            }
            else
            {
                statusMessage = " Status: Disconnected";
                GUI.Label(new Rect(16, 50, 300, 20), $"Archipelago v{ArchipelagoClient.APVersion}" + statusMessage);

                DrawConnectionFields();

                // Cursor.visible = true; // Uncomment if needed
                if (GUI.Button(new Rect(16, 130, 100, 20), "Connect") &&
                    !ArchipelagoClient.ServerData.SlotName.IsNullOrWhiteSpace())
                {
                    Plugin.ArchipelagoClient.Connect();
                }
            }
        }

        private static void DrawConnectionFields()
        {
            GUI.Label(new Rect(16, 70, 150, 20), "Host: ");
            GUI.Label(new Rect(16, 90, 150, 20), "Player Name: ");
            GUI.Label(new Rect(16, 110, 150, 20), "Password: ");

            ArchipelagoClient.ServerData.Uri = GUI.TextField(
                new Rect(150, 70, 150, 20),
                ArchipelagoClient.ServerData.Uri);

            ArchipelagoClient.ServerData.SlotName = GUI.TextField(
                new Rect(150, 90, 150, 20),
                ArchipelagoClient.ServerData.SlotName);

            ArchipelagoClient.ServerData.Password = GUI.TextField(
                new Rect(150, 110, 150, 20),
                ArchipelagoClient.ServerData.Password);
        }

        public static void DrawDebugButtons()
        {
            if (ArchipelagoConsole.Hidden) return;

            int buttonWidth = 100;
            int buttonHeight = 20;
            int buttonYStart = 50;
            int buttonIndex = 0;

            if (GUI.Button(new Rect(
                    Screen.width - buttonWidth,
                    buttonHeight * buttonIndex++ + buttonYStart,
                    buttonWidth, buttonHeight),
                    "Disconnect"))
            {
                Plugin.ArchipelagoClient.Disconnect();
            }

            if (GUI.Button(new Rect(
                    Screen.width - buttonWidth,
                    buttonHeight * buttonIndex++ + buttonYStart,
                    buttonWidth, buttonHeight),
                    "Dump techs"))
            {
                TechProtoPartial.DumpAPTechs();
            }

            if (GUI.Button(new Rect(
                    Screen.width - buttonWidth,
                    buttonHeight * buttonIndex++ + buttonYStart,
                    buttonWidth, buttonHeight),
                    "Dump recipes"))
            {
                RecipeProtoPartial.DumpRecipes();
            }
        }
    }
}
