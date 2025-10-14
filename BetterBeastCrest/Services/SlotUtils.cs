using System.Linq;
using UnityEngine;

namespace BetterBeastCrest.Services
{
    public static class SlotUtils
    {
        public static int GetTopCenterIndex(ToolCrest.SlotInfo[] slots, ToolItemType color)
            => GetClosestSlotIndex(slots, color, verticalPosition: VerticalPosition.Top);

        public static int GetMiddleCenterIndex(ToolCrest.SlotInfo[] slots, ToolItemType color)
            => GetClosestSlotIndex(slots, color, verticalPosition: VerticalPosition.Middle);

        public static int GetBottomCenterIndex(ToolCrest.SlotInfo[] slots, ToolItemType color)
            => GetClosestSlotIndex(slots, color, verticalPosition: VerticalPosition.Bottom);

        private static int GetClosestSlotIndex(ToolCrest.SlotInfo[]? slots, ToolItemType type, VerticalPosition verticalPosition)
        {
            if (slots == null || slots.Length == 0)
                return -1;

            var centerX = slots.Average(s => s.Position.x);

            var filtered = slots
                .Select((s, i) => new { Slot = s, Index = i })
                .Where(x => x.Slot.Type == type)
                .OrderByDescending(x => x.Slot.Position.y)
                .ToList();

            if (filtered.Count == 0)
                return -1;
            
            var targetIndex = verticalPosition switch
            {
                VerticalPosition.Top => 0,
                VerticalPosition.Middle => filtered.Count / 2,
                VerticalPosition.Bottom => filtered.Count - 1,
                _ => filtered.Count / 2
            };

            // Among candidates at similar Y, pick one closest to centerX
            var targetY = filtered[targetIndex].Slot.Position.y;
            var candidates = filtered.Where(x => Mathf.Abs(x.Slot.Position.y - targetY) < 0.01f).ToList();

            return candidates
                .OrderBy(x => Mathf.Abs(x.Slot.Position.x - centerX))
                .FirstOrDefault()?.Index ?? filtered[targetIndex].Index;
        }
        
        private enum VerticalPosition { Top, Middle, Bottom }
    }
}
