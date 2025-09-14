using GlobalSettings;

namespace BetterBeastCrest
{
    public class Helpers
    {
        public static bool IsBeastCrest2Unlocked => Gameplay.HunterCrest2.IsUnlocked;
        public static bool IsBeastCrest3Unlocked => Gameplay.HunterCrest3.IsUnlocked;
    }
}
