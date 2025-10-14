using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BetterBeastCrest.Domain.Config;
using HarmonyLib;

namespace BetterBeastCrest
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "delfofthebla.silksong.betterbeastcrest";
        public const string PLUGIN_NAME = "Better Beast Crest";
        public const string PLUGIN_VERSION = "2.3.0";
        public static ManualLogSource Log;
        public static ModConfig ModConfig;

        public void Awake()
        {
            Log = Logger;
            Log.LogInfo("Better Beast Crest Loading...");

            ModConfig = new ModConfig();
            
            var field = typeof(BaseUnityPlugin).GetField($"<{nameof(Config)}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            field!.SetValue(this, ModConfig.ConfigFile);    // Fuck you for making me do this plugin api devs.

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();

            foreach (var patchedMethod in harmony.GetPatchedMethods())
                Log.LogInfo($"Patched {patchedMethod.DeclaringType?.FullName}:{patchedMethod}");
            Log.LogInfo("patching done.");
        }
    }
}
