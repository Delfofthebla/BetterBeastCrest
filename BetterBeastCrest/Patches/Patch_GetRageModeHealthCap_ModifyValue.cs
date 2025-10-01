using BetterBeastCrest.Services;
using GlobalSettings;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(HeroController), "GetRageModeHealCap")]
    public static class Patch_GetRageModeHealthCap_ModifyValue
    {
        [HarmonyPostfix]
        private static void ModifyRageHealCap(ref int __result)
        {
            int newMax;
            if (Helpers.IsBeastCrest3Unlocked)
                newMax = Plugin.ModConfig.Crest3.MaxLifeLeech;
            else if (Helpers.IsBeastCrest2Unlocked)
                newMax = Plugin.ModConfig.Crest2.MaxLifeLeech;
            else
                newMax = Plugin.ModConfig.Crest1.MaxLifeLeech;

            if (Gameplay.MultibindTool.IsEquipped)
                newMax++;
            
            __result = newMax;
        }
    }
}