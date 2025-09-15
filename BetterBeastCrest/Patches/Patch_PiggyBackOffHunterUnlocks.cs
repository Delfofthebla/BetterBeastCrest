using GlobalSettings;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    // Harmony was being a baby so I had to write it like this to ensure PatchAll actually picked it up
    [HarmonyPatch(typeof(ToolCrest))]
    [HarmonyPatch("Unlock")]
    public static class Patch_PiggyBackOffHunterUnlocks
    {
        [HarmonyPostfix]
        public static void Postfix(ToolCrest __instance)
        {
            if (__instance == Gameplay.HunterCrest3)
            {
                BeastCrestModifier.UnlockWarrior3();
                BeastCrestModifier.AdjustRageDuration();
            }
            else if (__instance == Gameplay.HunterCrest2)
            {
                BeastCrestModifier.UnlockWarrior2();
                BeastCrestModifier.AdjustRageDuration();
            }
            else if (__instance == Gameplay.WarriorCrest)
            {
                BeastCrestModifier.ApplyWarrior1Changes();
                BeastCrestModifier.AdjustRageDuration();
            }
        }
    }
}
