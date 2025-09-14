using System;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(GameManager), "SetLoadedGameData", typeof(SaveGameData), typeof(int))]
    public class Patch_SyncExistingSaves
    {
        [HarmonyPostfix]
        public static void SyncCrestUnlocks()
        {
            if (Helpers.IsBeastCrest2Unlocked)
            {
                if (!Plugin.WarriorCrest2?.IsUnlocked ?? false)
                    Plugin.WarriorCrest2.Unlock();
            }

            if (Helpers.IsBeastCrest3Unlocked)
            {
                if (!Plugin.WarriorCrest3?.IsUnlocked ?? false)
                    Plugin.WarriorCrest3.Unlock();
            }
        }
    }

}
