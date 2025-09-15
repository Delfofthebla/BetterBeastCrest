using GlobalSettings;

namespace BetterBeastCrest
{
    public class Helpers
    {
        public static int WarriorCrest1SlotCount = 5;
        public static int WarriorCrest2SlotCount = 6;
        //public static int WarriorCrest3SlotCount = 7;
        
        public static bool IsBeastCrest1Unlocked => Gameplay.WarriorCrest.IsUnlocked;
        public static bool IsBeastCrest2Unlocked => Gameplay.HunterCrest2.IsUnlocked;
        public static bool IsBeastCrest3Unlocked => Gameplay.HunterCrest3.IsUnlocked;
    }
}
