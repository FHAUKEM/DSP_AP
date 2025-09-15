using Newtonsoft.Json;
using System;
using System.IO;

namespace DSP_AP.Partials
{
    [Serializable]
    public class RecipeProtoPartial
    {
        public int ID;
        public string Name;
        public int[] Items;
        public int[] ItemCounts;
        public int[] Results;
        public int[] ResultCounts;
        public bool NonProductive;

        public RecipeProtoPartial(RecipeProto proto)
        {
            ID = proto.ID;
            Name = Localization.CanTranslate(proto.Name) ? proto.Name.Translate() : proto.Name;
            Items = (int[])proto.Items?.Clone();
            ItemCounts = (int[])proto.ItemCounts?.Clone();
            Results = (int[])proto.Results?.Clone();
            ResultCounts = (int[])proto.ResultCounts?.Clone();
            NonProductive = proto.NonProductive;
        }

        public static void DumpRecipes()
        {
            string filePath = Path.Combine(Plugin.PluginPath, "RecipeProtos.json");

            var sourceArray = LDB.recipes.dataArray;
            RecipeProtoPartial[] recipes = new RecipeProtoPartial[sourceArray.Length];

            for (int i = 0; i < sourceArray.Length; i++)
            {
                if (sourceArray[i] != null)
                {
                    // Make partial copy of relevant information
                    var partial = new RecipeProtoPartial(sourceArray[i]);
                    recipes[i] = partial;
                }
            }

            string json = JsonConvert.SerializeObject(recipes, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
