using BetterBeastCrest.Services;
using GlobalSettings;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch]
    public static class Patch_DownAttack_ModifyAttackPattern
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "DownAttack")]
        public static void DownAttackPrefix(HeroController __instance, ref bool isSlashing)
        {
            if (!Gameplay.WarriorCrest.IsEquipped)
                return;

            DownAttackModifier.PatchDownAttackIfNecessary(__instance);
        }
    }
}
