using GlobalSettings;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(HeroController), "BindCompleted")]
    public static class Patch_HealOnRageMode
    {
        private static void Postfix(HeroController __instance)
        {
            if (!Gameplay.WarriorCrest.IsEquipped)
                return;

            int healValue;
            if (Helpers.IsBeastCrest3Unlocked)
                healValue = Plugin.Crest3.ImmediateHeal;
            else if (Helpers.IsBeastCrest2Unlocked)
                healValue = Plugin.Crest2.ImmediateHeal;
            else
                healValue = Plugin.Crest1.ImmediateHeal;
            
            if (healValue > 0)
                __instance.AddHealth(healValue);
        }
    }
}
