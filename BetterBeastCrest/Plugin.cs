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
        
        public static ConfigEntry<int> ImmediateHealthOnBind1;
        public static ConfigEntry<int> MaximumLifeLeech1;
        
        public static ConfigEntry<int> ImmediateHealthOnBind2;
        public static ConfigEntry<int> MaximumLifeLeech2;
        public static ConfigEntry<int> RageDurationIncrease2;
        
        public static ConfigEntry<int> ImmediateHealthOnBind3;
        public static ConfigEntry<int> MaximumLifeLeech3;
        public static ConfigEntry<int> RageDurationIncrease3;

        public static ManualLogSource Log;

        public void Awake()
        {
            Log = Logger;
            Logger.LogInfo("[BetterBeastCrest]: Better Beast Crest Loading...");

            Config = new ConfigFile(Path.Combine(Paths.ConfigPath, "BetterBeastCrest.cfg"), true);
            
            ImmediateHealthOnBind1 = Config.Bind("BeastCrestStage1", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with Beast Crest Rank 1. (Separate from the rage lifesteal)");
            MaximumLifeLeech1 = Config.Bind("BeastCrestStage1", "MaximumLifeLeech", 2, "The maximum amount of masks you can restore by attacking after binding for Rank 1. (You can also use negative numbers if you wish to decrease it)");
            
            ImmediateHealthOnBind2 = Config.Bind("BeastCrestStage2", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with Beast Crest Rank 2. (Separate from the rage lifesteal)");
            MaximumLifeLeech2 = Config.Bind("BeastCrestStage2", "MaximumLifeLeech", 3, "The maximum amount of masks you can restore by attacking after binding for Rank 2. (You can also use negative numbers if you wish to decrease it)");
            RageDurationIncrease2 = Config.Bind("BeastCrestStage2", "RageDurationIncrease", 10, "The percentage increase in rage duration for Rank 2. (You can also use negative numbers if you wish to decrease it)");
            
            ImmediateHealthOnBind3 = Config.Bind("BeastCrestStage3", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with Beast Crest Rank 3. (Separate from the rage lifesteal)");
            MaximumLifeLeech3 = Config.Bind("BeastCrestStage3", "MaximumLifeLeech", 4, "The maximum amount of masks you can restore by attacking after binding for Rank 3. (You can also use negative numbers if you wish to decrease it)");
            RageDurationIncrease3 = Config.Bind("BeastCrestStage3", "RageDurationIncrease", 30, "The percentage increase in rage duration for Rank 3. (You can also use negative numbers if you wish to decrease it)");
            
            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
            
            foreach (var patchedMethod in harmony.GetPatchedMethods())
                Logger.LogInfo($"[BetterBeastCrest]: Patched {patchedMethod.DeclaringType?.FullName}:{patchedMethod}");
            Logger.LogInfo($"[BetterBeastCrest]: patching done.");
        }

    }
}
