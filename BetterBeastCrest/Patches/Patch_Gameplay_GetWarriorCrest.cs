using GlobalSettings;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(Gameplay), "get_WarriorCrest")]
    public class Patch_Gameplay_GetWarriorCrest
    {
        private static void Postfix(ToolCrest __result)
        {
            try
            {
                if (Plugin.WarriorCrest1 == null || Plugin.WarriorCrest2 == null || Plugin.WarriorCrest3 == null)
                {
                    Plugin.Log.LogWarning($"[BetterBeastCrest]: get_WarriorCrest postfix failed due to uninitialized WarriorCrest Upgrades");
                    return;
                }

                if (Helpers.IsBeastCrest3Unlocked)
                    __result = Plugin.WarriorCrest3;
                else if (Helpers.IsBeastCrest2Unlocked)
                    __result = Plugin.WarriorCrest2;
                else
                    __result = Plugin.WarriorCrest1;
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"[BetterBeastCrest]: get_WarriorCrest postfix failed due to exception: {ex}");
            }
        }
    }
}