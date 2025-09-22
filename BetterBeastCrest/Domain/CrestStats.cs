using System.Collections.Generic;

namespace BetterBeastCrest.Domain
{
    public class CrestStats
    {
        public int ImmediateHeal { get; }
        public int MaxLifeLeech { get; }
        public int RageDuration { get; }

        // Only additional mod slots (TopLeft/TopRight)
        public IReadOnlyList<ExtraToolSlot> ExtraToolSlots { get; }

        public CrestStats(int immediateHeal, int maxLifeLeech, int rageDuration, List<ExtraToolSlot>? modToolSlots = null)
        {
            ImmediateHeal = immediateHeal;
            MaxLifeLeech = maxLifeLeech;
            RageDuration = rageDuration;
            ExtraToolSlots = modToolSlots ?? new List<ExtraToolSlot>();
        }

        public int TotalHealing => ImmediateHeal + MaxLifeLeech;
    }

    public class ExtraToolSlot
    {
        public ExtraToolSlotPosition Position { get; }
        public bool RequiresUnlocking { get; }
        public ToolItemType SlotColor { get; }

        public ExtraToolSlot(ExtraToolSlotPosition position, bool requiresUnlocking, ToolItemType slotColor)
        {
            Position = position;
            RequiresUnlocking = requiresUnlocking;
            SlotColor = slotColor;
        }
    }

    public enum ExtraToolSlotPosition
    {
        TopLeft,
        TopRight
    }
}
