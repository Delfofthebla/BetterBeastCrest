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
                BeastCrestModifier.UnlockRank3();
                BeastCrestModifier.AdjustRageStats();
            }
            else if (__instance == Gameplay.HunterCrest2)
            {
                BeastCrestModifier.UnlockRank2();
                BeastCrestModifier.AdjustRageStats();
            }
            else if (__instance == Gameplay.WarriorCrest)
            {
                BeastCrestModifier.ApplyRank1Changes();
                BeastCrestModifier.AdjustRageStats();
            }
        }
    }
}
