using BetterBeastCrest.Services;
using GlobalSettings;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(HeroController), "DownAttack")]
    public static class Patch_DownAttack_ModifyAttackPattern
    {
        [HarmonyPrefix]
        public static void ModifyAttackPattern(HeroController __instance, ref bool isSlashing)
        {
            if (!Gameplay.WarriorCrest.IsEquipped)
                return;

            DownAttackModifier.PatchDownAttackIfNecessary(__instance);
        }
    }
}
