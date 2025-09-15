using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace BetterBeastCrest
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "delfofthebla.silksong.betterbeastcrest";
        public const string PLUGIN_NAME = "Better Beast Crest";
        public const string PLUGIN_VERSION = "1.0.0";

        public static ConfigFile Config;
        public static ManualLogSource Log;

        public static CrestStats CrestDefault;
        public static CrestStats Crest1;
        public static CrestStats Crest2;
        public static CrestStats Crest3;
        
        private static ConfigEntry<int> ImmediateHealthOnBind1;
        private static ConfigEntry<int> MaximumLifeLeech1;
        private static ConfigEntry<int> RageDurationIncrease1;
        
        private static ConfigEntry<int> ImmediateHealthOnBind2;
        private static ConfigEntry<int> MaximumLifeLeech2;
        private static ConfigEntry<int> RageDurationIncrease2;
        private static ConfigEntry<ToolItemType> ToolSlotColor2;
        private static ConfigEntry<bool> EnableNewToolSlot2;
        
        private static ConfigEntry<int> ImmediateHealthOnBind3;
        private static ConfigEntry<int> MaximumLifeLeech3;
        private static ConfigEntry<int> RageDurationIncrease3;
        private static ConfigEntry<ToolItemType> ToolSlotColor3;
        private static ConfigEntry<bool> EnableNewToolSlot3;

        public void Awake()
        {
            Log = Logger;
            Logger.LogInfo("[BetterBeastCrest]: Better Beast Crest Loading...");

            Config = new ConfigFile(Path.Combine(Paths.ConfigPath, "BetterBeastCrest.cfg"), true);
            
            ImmediateHealthOnBind1 = Config.Bind("BeastCrestStage1", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with Beast Crest Rank 1. (Separate from the rage lifesteal)");
            MaximumLifeLeech1 = Config.Bind("BeastCrestStage1", "MaximumLifeLeech", 2, "The maximum amount of masks you can restore by attacking after binding for Rank 1. (You can also use negative numbers if you wish to decrease it)");
            RageDurationIncrease1 = Config.Bind("BeastCrestStage2", "RageDurationIncrease", 0, "The percentage increase in rage duration for Rank 1. (You can also use negative numbers if you wish to decrease it)");
            
            ImmediateHealthOnBind2 = Config.Bind("BeastCrestStage2", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with Beast Crest Rank 2. (Separate from the rage lifesteal)");
            MaximumLifeLeech2 = Config.Bind("BeastCrestStage2", "MaximumLifeLeech", 3, "The maximum amount of masks you can restore by attacking after binding for Rank 2. (You can also use negative numbers if you wish to decrease it)");
            RageDurationIncrease2 = Config.Bind("BeastCrestStage2", "RageDurationIncrease", 0, "The percentage increase in rage duration for Rank 2. (You can also use negative numbers if you wish to decrease it)");
            EnableNewToolSlot2 = Config.Bind("BeastCrestStage2", "EnableNewToolSlot", true, "Enable the tool slot for this rank.");
            ToolSlotColor2 = Config.Bind("BeastCrestStage2", "ToolSlotColor", ToolItemType.Blue, "The tool slot color for this rank. ==Only Blue and Yellow are supported==");
            
            ImmediateHealthOnBind3 = Config.Bind("BeastCrestStage3", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with Beast Crest Rank 3. (Separate from the rage lifesteal)");
            MaximumLifeLeech3 = Config.Bind("BeastCrestStage3", "MaximumLifeLeech", 3, "The maximum amount of masks you can restore by attacking after binding for Rank 3. (You can also use negative numbers if you wish to decrease it)");
            RageDurationIncrease3 = Config.Bind("BeastCrestStage3", "RageDurationIncrease", 20, "The percentage increase in rage duration for Rank 3. (You can also use negative numbers if you wish to decrease it)");
            EnableNewToolSlot3 = Config.Bind("BeastCrestStage3", "EnableNewToolSlot", true, "Enable the tool slot for this rank.");
            ToolSlotColor3 = Config.Bind("BeastCrestStage3", "ToolSlotColor", ToolItemType.Yellow, "The tool slot color for this rank. ==Only Blue and Yellow are supported== It's Yellow by default because 2 blues is VERY strong :)");
            
            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
            
            // Initialize all our crest stats
            CrestDefault = new CrestStats(1, 2, 0, false, default);
            Crest1 = new CrestStats(
                ImmediateHealthOnBind1.Value,
                MaximumLifeLeech1.Value,
                RageDurationIncrease1.Value,
                false,
                default
            );
            Crest2 = new CrestStats(
                ImmediateHealthOnBind2.Value,
                MaximumLifeLeech2.Value,
                RageDurationIncrease2.Value,
                EnableNewToolSlot2.Value,
                ToolSlotColor2.Value
            );
            Crest3 = new CrestStats(
                ImmediateHealthOnBind3.Value,
                MaximumLifeLeech3.Value,
                RageDurationIncrease3.Value,
                EnableNewToolSlot3.Value,
                ToolSlotColor3.Value
            );
            
            foreach (var patchedMethod in harmony.GetPatchedMethods())
                Logger.LogInfo($"[BetterBeastCrest]: Patched {patchedMethod.DeclaringType?.FullName}:{patchedMethod}");
            Logger.LogInfo($"[BetterBeastCrest]: patching done.");
        }
    }
}
