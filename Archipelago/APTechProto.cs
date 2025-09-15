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

    [Serializable]
    public class RecipeShadow
    {
        public int ID;
        public string Name;
        public int[] Items;
        public int[] ItemCounts;
        public int[] Results;
        public int[] ResultCounts;
        public bool NonProductive;

        public RecipeShadow(RecipeProto proto)
        {
            ID = proto.ID;
            Name = Localization.CanTranslate(proto.Name) ? proto.Name.Translate() : proto.Name;
            Items = (int[])proto.Items?.Clone();
            ItemCounts = (int[])proto.ItemCounts?.Clone();
            Results = (int[])proto.Results?.Clone();
            ResultCounts = (int[])proto.ResultCounts?.Clone();
            NonProductive = proto.NonProductive;
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

        public static void DumpRecipes()
        {
            string filePath = Path.Combine(Plugin.PluginPath, "RecipeProtos.json");

            var sourceArray = LDB.recipes.dataArray;
            RecipeShadow[] recipes = new RecipeShadow[sourceArray.Length];

            for (int i = 0; i < sourceArray.Length; i++)
            {
                if (sourceArray[i] != null)
                {
                    // Make shadow copy of relevant information
                    var shadow = new RecipeShadow(sourceArray[i]);
                    recipes[i] = shadow;
                }
            }

            string json = JsonConvert.SerializeObject(recipes, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
