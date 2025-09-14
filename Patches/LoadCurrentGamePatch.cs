using HarmonyLib;
using DSP_AP.Archipelago;

namespace DSP_AP.Patches
{
    [HarmonyPatch(typeof(GameSave), "LoadCurrentGame")]
    public class LoadCurrentGamePatch
    {
        [HarmonyPostfix]
        public static void Postfix(bool __result)
        {
            if (DSPGame.Game.isMenuDemo) return;

            int counter = 0;
            foreach (var techStateKVP in GameMain.history.techStates)
            {
                int techId = techStateKVP.Key;
                TechState state = techStateKVP.Value;

                if (state.unlocked && !ArchipelagoClient.ServerData.CheckedLocations.Contains(techId))
                {
                    ArchipelagoClient.ServerData.CheckedLocations.Add(techId);
                    counter++;
                }
            }

            Plugin.BepinLogger.LogInfo($"Loaded {counter} unsent locations from savefile.");
            Plugin.ArchipelagoClient.CheckLocationsAsync();
        }
    }
}
