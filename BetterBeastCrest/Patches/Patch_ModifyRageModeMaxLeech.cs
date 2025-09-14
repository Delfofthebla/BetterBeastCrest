using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(HeroController), "GetRageModeHealCap")]
    public class Patch_ModifyRageModeMaxLeech
    {
        private static void Postfix(ref int __result)
        {
            if (Helpers.IsBeastCrest3Unlocked)
                __result = Plugin.MaximumLifeLeech3.Value;
            else if (Helpers.IsBeastCrest2Unlocked)
                __result = Plugin.MaximumLifeLeech2.Value;
            else
                __result = Plugin.MaximumLifeLeech1.Value;
        }
    }
}