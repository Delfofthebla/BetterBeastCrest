using System.IO;
using BepInEx;
using BepInEx.Configuration;
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
        public const string PLUGIN_VERSION = "2.0.0";

        public static ManualLogSource Log;
        public static ModConfig Config;

        public void Awake()
        {
            Log = Logger;
            LogInfo("Better Beast Crest Loading...");

            Config = new ModConfig();

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();

            foreach (var patchedMethod in harmony.GetPatchedMethods())
                LogInfo($"Patched {patchedMethod.DeclaringType?.FullName}:{patchedMethod}");
            LogInfo("patching done.");
        }
        
        public static void LogInfo(string msg) => Log.LogInfo("[BetterBeastCrest]: " + msg);
    }
}
