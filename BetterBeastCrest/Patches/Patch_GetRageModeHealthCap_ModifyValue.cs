using BetterBeastCrest.Services;
using GlobalSettings;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(HeroController), "GetRageModeHealCap")]
    public static class Patch_GetRageModeHealthCap_ModifyValue
    {
        private static void Postfix(ref int __result)
        {
            int newMax;
            if (Helpers.IsBeastCrest3Unlocked)
                newMax = Plugin.Config.Crest3.MaxLifeLeech;
            else if (Helpers.IsBeastCrest2Unlocked)
                newMax = Plugin.Config.Crest2.MaxLifeLeech;
            else
                newMax = Plugin.Config.Crest1.MaxLifeLeech;

            if (Gameplay.MultibindTool.IsEquipped)
                newMax++;
            
            __result = newMax;
        }
    }
}