using DSP_AP.Partials;
using System.Linq;

namespace DSP_AP.GameLogic;

public static class TechUnlockService
{
    public static void ApplyTechRewards(GameHistoryData history, long ItemId)
    {
        Plugin.BepinLogger.LogDebug($"Unlocking rewards for research id: {ItemId}");

        int techId = (int)ItemId;
        TechProtoPartial techProto = Plugin.APTechProtos.FirstOrDefault(t => t.ID == techId);

        if (techProto != null)
        {
            Plugin.BepinLogger.LogInfo($"Partial copy of TechProto not found for id {techId}");
            return;
        }

        if (!history.techStates.ContainsKey(techId))
        {
            Plugin.BepinLogger.LogDebug($"No techstate found for id {techId}");
            return;
        }

        int maxLevel = history.techStates[techId].maxLevel;

        foreach (int recipe in techProto.UnlockRecipes)
            history.UnlockRecipe(recipe);

        for (int i = 0; i < techProto.UnlockFunctions.Length; i++)
            history.UnlockTechFunction(techProto.UnlockFunctions[i], techProto.UnlockValues[i], maxLevel);

        for (int i = 0; i < techProto.AddItems.Length; i++)
        {
            // TODO: Check whether the items were given out before (awarded flag)

            int itemId = techProto.AddItems[i];
            int itemCountVal = techProto.AddItemCounts[i];

            // TODO: Add the items
            // TODO: Set awarded flag to true
        }

        Plugin.BepinLogger.LogDebug($"Unlocked research rewards for tech ID: {techId}");
    }
}
