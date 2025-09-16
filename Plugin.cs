using BepInEx;
using BepInEx.Logging;
using DSP_AP.Archipelago;
using DSP_AP.Partials;
using DSP_AP.Utils;
using HarmonyLib;
using System.IO;
using System.Linq;

namespace DSP_AP
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        #region Constants
        public const string PluginGUID = "FHAUKEM.DSP.DSP_AP";
        public const string PluginName = "DSP_AP";
        public const string PluginVersion = "0.0.4";
        public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
        private const string APDisplayInfo = $"Archipelago v{ArchipelagoClient.APVersion}";
        public const long GoalTechID = 1508;
        #endregion

        #region Static Fields
        public static ManualLogSource BepinLogger;
        public static ArchipelagoClient ArchipelagoClient;
        public static string PluginPath;
        public static TechProtoPartial[] APTechProtos;
        public static Plugin Instance;
        #endregion


        private void Awake()
        {
            BepinLogger = base.Logger;
            PluginPath = Path.Combine(Paths.PluginPath, PluginName);

            Harmony harmony = new Harmony(PluginGUID + ".Harmony");
            harmony.PatchAll();

            ArchipelagoClient = new ArchipelagoClient();
            ArchipelagoConsole.Awake();
            APTechProtos = TechInitializationService.CreateTechProtos(BepinLogger);

            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
        }

        private void OnGUI()
        {
            PluginUI.DrawModLabel();
            PluginUI.DrawStatusUI();
            PluginUI.DrawDebugButtons();
        }
    }
}
