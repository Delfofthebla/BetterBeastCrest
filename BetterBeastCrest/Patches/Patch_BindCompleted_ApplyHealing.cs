using BetterBeastCrest.Services;
using GlobalSettings;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(HeroController), "BindCompleted")]
    public static class Patch_BindCompleted_ApplyHealing
    {
        [HarmonyPostfix]
        private static void ModifyBind(HeroController __instance)
        {
            if (!Gameplay.WarriorCrest.IsEquipped)
                return;

            int healValue;
            if (Helpers.IsBeastCrest3Unlocked)
                healValue = Plugin.Config.Crest3.ImmediateHeal;
            else if (Helpers.IsBeastCrest2Unlocked)
                healValue = Plugin.Config.Crest2.ImmediateHeal;
            else
                healValue = Plugin.Config.Crest1.ImmediateHeal;

            if (healValue > 0)
                __instance.AddHealth(healValue);
        }
    }
}
