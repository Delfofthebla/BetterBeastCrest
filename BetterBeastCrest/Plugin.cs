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

        public static ConfigEntry<int> ImmediateHealthOnBind1;
        public static ConfigEntry<int> MaximumLifeLeech1;
        
        public static ConfigEntry<int> ImmediateHealthOnBind2;
        public static ConfigEntry<int> MaximumLifeLeech2;
        
        public static ConfigEntry<int> ImmediateHealthOnBind3;
        public static ConfigEntry<int> MaximumLifeLeech3;

        public static ManualLogSource Log;

        public static ToolCrest WarriorCrest1;
        public static ToolCrest WarriorCrest2;
        public static ToolCrest WarriorCrest3;

        public void Awake()
        {
            Log = Logger;
            Logger.LogInfo("[BetterBeastCrest]: Better Beast Crest Loading...");

            var config = new ConfigFile(Path.Combine(Paths.ConfigPath, "BetterBeastCrest.cfg"), true);
            
            ImmediateHealthOnBind1 = config.Bind("BeastCrestStage1", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with the beast crest. (Separate from the lifesteal)");
            MaximumLifeLeech1 = config.Bind("BeastCrestStage1", "MaximumLifeLeech", 2, "The maximum amount of masks you can restore by attacking after binding. (Separate from the immediate heal)");
            
            ImmediateHealthOnBind2 = config.Bind("BeastCrestStage2", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with the beast crest. (Separate from the lifesteal)");
            MaximumLifeLeech2 = config.Bind("BeastCrestStage2", "MaximumLifeLeech", 3, "The maximum amount of masks you can restore by attacking after binding. (Separate from the immediate heal)");
            
            ImmediateHealthOnBind3 = config.Bind("BeastCrestStage3", "HealOnBind", 2, "Specify the amount of masks you want to restore upon binding with the beast crest. (Separate from the lifesteal)");
            MaximumLifeLeech3 = config.Bind("BeastCrestStage3", "MaximumLifeLeech", 3, "The maximum amount of masks you can restore by attacking after binding. (Separate from the immediate heal)");
            
            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
            
            foreach (var patchedMethod in harmony.GetPatchedMethods())
                Logger.LogInfo($"[BetterBeastCrest]: Patched {patchedMethod.DeclaringType?.FullName}:{patchedMethod}");
            Logger.LogInfo($"[BetterBeastCrest]: patching done.");
        }
    }
}
