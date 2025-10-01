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
            if (BeastCrestModifier.ModInitialized)
            {
                BeastCrestModifier.ModInitialized = false;
                DownAttackModifier.ShouldRevertPatchWhenAble = true;
                Plugin.ModConfig.ReloadConfig();
            }
            
            if (!BeastCrestModifier.CacheOriginalValuesIfNecessary())
                BeastCrestModifier.ResetModStateIfAble();
        }

        [HarmonyPostfix]
        public static void PerformSaveLoadActions()
        {
            UnlockAndModifyCrests();
            BeastCrestModifier.ModInitialized = true;
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
