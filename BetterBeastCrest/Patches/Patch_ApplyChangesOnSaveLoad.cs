using GlobalSettings;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(GameManager), "SetLoadedGameData", typeof(SaveGameData), typeof(int))]
    public static class Patch_ApplyChangesOnSaveLoad
    {
        [HarmonyPrefix]
        public static void RevertBeforeLoad()
        {
            BeastCrestModifier.CacheOriginalValuesIfNecessary();
            BeastCrestModifier.ResetModStateIfNecessary();
        }
        
        [HarmonyPostfix]
        public static void SyncCrestUnlocks()
        {
            if (Helpers.IsBeastCrest1Unlocked)
                BeastCrestModifier.ApplyWarrior1Changes();
            if (Helpers.IsBeastCrest2Unlocked)
                BeastCrestModifier.UnlockWarrior2();
            if (Helpers.IsBeastCrest3Unlocked)
                BeastCrestModifier.UnlockWarrior3();
            
            BeastCrestModifier.AdjustRageDuration();
        }
    }
}
