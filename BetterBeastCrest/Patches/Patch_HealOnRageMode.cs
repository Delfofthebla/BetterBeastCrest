using GlobalSettings;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(HeroController), "BindCompleted")]
    public class Patch_HealOnRageMode
    {
        private static void Postfix(HeroController __instance)
        {
            if (!(Plugin.WarriorCrest1.IsEquipped || Plugin.WarriorCrest2.IsEquipped || Plugin.WarriorCrest3.IsEquipped))
                return;

            int healValue;
            if (Helpers.IsBeastCrest3Unlocked)
                healValue = Plugin.ImmediateHealthOnBind3.Value;
            else if (Helpers.IsBeastCrest2Unlocked)
                healValue = Plugin.ImmediateHealthOnBind2.Value;
            else
                healValue = Plugin.ImmediateHealthOnBind1.Value;
            
            if (healValue > 0)
                __instance.AddHealth(healValue);
        }
    }
}
