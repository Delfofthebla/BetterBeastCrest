using BetterBeastCrest.Services;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(GameManager), "SetLoadedGameData", typeof(SaveGameData), typeof(int))]
    public static class Patch_LoadSave_ApplyChanges
    {
        [HarmonyPrefix]
        public static void RevertBeforeLoad()
        {
            Plugin.Config.ReloadConfig();
            DownAttackModifier.ShouldRevertPatchWhenAble = true;
            
            if (!BeastCrestModifier.CacheOriginalValuesIfNecessary())
                BeastCrestModifier.ResetModStateIfAble();
        }

        [HarmonyPostfix]
        public static void PerformSaveLoadActions()
        {
            UnlockAndModifyCrests();
        }

        private static void UnlockAndModifyCrests()
        {
            if (Helpers.IsBeastCrest1Unlocked)
                BeastCrestModifier.ApplyRank1Changes();
            if (Helpers.IsBeastCrest2Unlocked)
                BeastCrestModifier.UnlockRank2();
            if (Helpers.IsBeastCrest3Unlocked)
                BeastCrestModifier.UnlockRank3();

            BeastCrestModifier.MakeGlobalChanges();
        }
    }
}
