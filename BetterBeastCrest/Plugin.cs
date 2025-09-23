using BepInEx;
using BepInEx.Logging;
using BetterBeastCrest.Domain;
using HarmonyLib;

namespace BetterBeastCrest
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "delfofthebla.silksong.betterbeastcrest";
        public const string PLUGIN_NAME = "Better Beast Crest";
        public const string PLUGIN_VERSION = "2.0.2";
        
        public static ManualLogSource Log;
        public static ModConfig Config;

        public void Awake()
        {
            Log = Logger;
            Log.LogInfo("Better Beast Crest Loading...");

            Config = new ModConfig();

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();

            foreach (var patchedMethod in harmony.GetPatchedMethods())
                Log.LogInfo($"Patched {patchedMethod.DeclaringType?.FullName}:{patchedMethod}");
            Log.LogInfo("patching done.");
        }
    }
}
