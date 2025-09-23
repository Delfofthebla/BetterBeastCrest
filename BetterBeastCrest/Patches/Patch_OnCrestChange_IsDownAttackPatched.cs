using BetterBeastCrest.Domain;
using BetterBeastCrest.Extensions;
using BetterBeastCrest.Services;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(ToolItemManager))]
    public class Patch_OnCrestChange_IsDownAttackPatched
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ToolItemManager), "SetEquippedCrest")]
        public static void ResetIsDownAttackPatched(ToolItemManager __instance, ref string crestId)
        {
            var equippedCrest = crestId.ToCrest();
            if (equippedCrest == CrestType.Beast)
                return;

            DownAttackModifier.RevertDownAttackPatch(HeroController.UnsafeInstance);
            Plugin.Log.LogInfo("Beast Crest Down Attack patch state reset.");
        }
    }
}
