using GlobalSettings;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(ToolCrest), "Unlock")]
    public class Patch_PiggyBackOffHunterUnlocks
    {
        public static void PostFix(ToolCrest __instance)
        {
            if (__instance == Gameplay.HunterCrest2)
                Plugin.WarriorCrest2?.Unlock();
            else if (__instance == Gameplay.HunterCrest3)
                Plugin.WarriorCrest3?.Unlock();
        }
    }
}
