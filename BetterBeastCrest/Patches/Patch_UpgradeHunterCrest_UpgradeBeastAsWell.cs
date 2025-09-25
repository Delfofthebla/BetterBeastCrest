using BetterBeastCrest.Services;
using GlobalSettings;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(ToolCrest), "Unlock")]
    public static class Patch_UpgradeHunterCrest_UpgradeBeastAsWell
    {
        [HarmonyPostfix]
        public static void UpgradeBeastCrest(ToolCrest __instance)
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
