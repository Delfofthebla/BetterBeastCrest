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
            {
                BeastCrestModifier.UnlockWarrior2();
                BeastCrestModifier.AdjustRageDuration();
            }
            else if (__instance == Gameplay.HunterCrest3)
            {
                BeastCrestModifier.UnlockWarrior3();
                BeastCrestModifier.AdjustRageDuration();
            }
        }
    }
}
