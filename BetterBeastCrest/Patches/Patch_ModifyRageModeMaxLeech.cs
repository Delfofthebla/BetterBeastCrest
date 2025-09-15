using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(HeroController), "GetRageModeHealCap")]
    public static class Patch_ModifyRageModeMaxLeech
    {
        private static void Postfix(ref int __result)
        {
            if (Helpers.IsBeastCrest3Unlocked)
                __result = Plugin.Crest3.MaxLifeLeech;
            else if (Helpers.IsBeastCrest2Unlocked)
                __result = Plugin.Crest2.MaxLifeLeech;
            else
                __result = Plugin.CrestDefault.MaxLifeLeech;
        }
    }
}