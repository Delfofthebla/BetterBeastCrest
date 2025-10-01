using GlobalSettings;

namespace BetterBeastCrest.Services
{
    public class Helpers
    {
        public static readonly float DefaultAttackCooldown = 0.39f;
        
        public static bool IsBeastCrest1Unlocked => Gameplay.WarriorCrest.IsUnlocked;
        public static bool IsBeastCrest2Unlocked => Gameplay.HunterCrest2.IsUnlocked;
        public static bool IsBeastCrest3Unlocked => Gameplay.HunterCrest3.IsUnlocked;
    }
}
