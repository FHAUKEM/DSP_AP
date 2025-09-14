using System;
using System.IO;
using Newtonsoft.Json;

namespace DSP_AP.Archipelago
{
    [Serializable]
    public class APTechShadow
    {
        public int ID;
        public string Name;
        [JsonIgnore]
        public bool APReceived;
        public bool IsHiddenTech;
        public int[] Items;
        public int[] UnlockRecipes;
        public int[] UnlockFunctions;
        [JsonIgnore]
        public double[] UnlockValues;
        [JsonIgnore]
        public int[] AddItems;
        [JsonIgnore]
        public int[] AddItemCounts;
        [JsonIgnore]
        public int maxLevel;

        public APTechShadow(TechProto proto)
        {
            ID = proto.ID;
            Name = Localization.CanTranslate(proto.Name) ? proto.Name.Translate() : proto.Name;
            APReceived = false;
            IsHiddenTech = proto.IsHiddenTech;
            Items = (int[])proto.Items?.Clone();
            UnlockRecipes = (int[])proto.UnlockRecipes?.Clone();
            UnlockFunctions = (int[])proto.UnlockFunctions?.Clone();
            UnlockValues = (double[])proto.UnlockValues?.Clone();
            AddItems = (int[])proto.AddItems?.Clone();
            AddItemCounts = (int[])proto.AddItemCounts?.Clone();
            maxLevel = proto.MaxLevel;
        }
    }

    public static class APTechProto
    {
        public static void DumpAPTechs()
        {
            string filePath = Path.Combine(Plugin.PluginPath, "APTechProtos.json");
            foreach (APTechShadow proto in Plugin.APTechProtos)
            {
                proto.Name = Localization.CanTranslate(proto.Name) ? proto.Name.Translate() : proto.Name;
            }
            string json = JsonConvert.SerializeObject(Plugin.APTechProtos, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
