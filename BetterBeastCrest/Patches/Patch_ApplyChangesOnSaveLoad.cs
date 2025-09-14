using GlobalSettings;
using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(GameManager), "SetLoadedGameData", typeof(SaveGameData), typeof(int))]
    public class Patch_ApplyChangesOnSaveLoad
    {
        [HarmonyPostfix]
        public static void SyncCrestUnlocks()
        {
            if (Helpers.IsBeastCrest2Unlocked && Gameplay.WarriorCrest.Slots.Length < Helpers.WarriorCrest2SlotCount)
                BeastCrestModifier.UnlockWarrior2();
            if (Helpers.IsBeastCrest3Unlocked)
                BeastCrestModifier.UnlockWarrior3();

            BeastCrestModifier.AdjustRageDuration();
        }
    }
}
