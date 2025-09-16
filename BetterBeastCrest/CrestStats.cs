namespace BetterBeastCrest
{
    public class CrestStats
    {
        public int ImmediateHeal { get; }
        public int MaxLifeLeech { get; }
        public int RageDuration { get; }
        public bool ToolSlotEnabled { get; }
        public bool ToolSlotRequiresUnlocking { get; }
        public ToolItemType SlotColor { get; }

        public CrestStats(int immediateHeal, int maxLifeLeech, int rageDuration, bool toolSlotEnabled, bool toolSlotRequiresUnlocking, ToolItemType slotColor)
        {
            ImmediateHeal = immediateHeal;
            MaxLifeLeech = maxLifeLeech;
            RageDuration = rageDuration;
            ToolSlotEnabled = toolSlotEnabled;
            ToolSlotRequiresUnlocking = toolSlotRequiresUnlocking;
            SlotColor = slotColor;
        }

        public int TotalHealing => ImmediateHeal + MaxLifeLeech;
    }
}
