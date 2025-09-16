using HarmonyLib;
using DSP_AP.Archipelago;

namespace DSP_AP.Patches
{
    [HarmonyPatch(typeof(AdvisorLogic), "NotifyOnUnlockTech")]
    public class NotifyOnUnlockTechPatch
    {
        [HarmonyPostfix]
        public static void Postfix(GameHistoryData __instance, int techId)
        {
            var tech = LDB.techs.Select(techId);
            Plugin.BepinLogger.LogInfo($"Tech unlocked: {(Localization.CanTranslate(tech.Name) ? tech.Name.Translate() : tech.Name)} ({techId})");

            Plugin.ArchipelagoClient.CheckLocationsAsync();
        }
    }
}
