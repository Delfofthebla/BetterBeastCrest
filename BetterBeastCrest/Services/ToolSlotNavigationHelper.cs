using System;
using UnityEngine;

namespace BetterBeastCrest.Services
{
    public static class ToolSlotNavigationHelper
    {
        public static ToolCrest.SlotInfo[] ToFixedNavigation(ToolCrest.SlotInfo[]? slots)
        {
            if (slots == null)
                return Array.Empty<ToolCrest.SlotInfo>();

            for (var index = 0; index < slots.Length; index++)
            {
                var currentPos = slots[index].Position;

                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    if (dir == Direction.None)
                        continue;

                    var closestIndex = -1;
                    var closestDistance = float.MaxValue;

                    for (var index2 = 0; index2 < slots.Length; index2++)
                    {
                        if (index == index2)
                            continue;

                        var otherPos = slots[index2].Position;
                        if (currentPos.GetDirectionTo(otherPos) != dir)
                            continue;

                        var distance = Vector2.Distance(currentPos, otherPos);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestIndex = index2;
                        }
                    }

                    switch (dir)
                    {
                        case Direction.Left:
                            slots[index].NavLeftIndex = closestIndex;
                            break;
                        case Direction.Right:
                            slots[index].NavRightIndex = closestIndex;
                            break;
                        case Direction.Up:
                            slots[index].NavUpIndex = closestIndex;
                            break;
                        case Direction.Down:
                            slots[index].NavDownIndex = closestIndex;
                            break;
                        case Direction.None:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return slots;
        }
        
        private static Direction GetDirectionTo(this Vector2 a, Vector2 b)
        {
            var delta = a - b;
            if (delta.x > 0 && delta.x >= Math.Abs(delta.y))
                return Direction.Left;
            if (delta.x < 0 && -delta.x >= Math.Abs(delta.y))
                return Direction.Right;
            if (delta.y > 0 && delta.y >= Math.Abs(delta.x))
                return Direction.Down;
            if (delta.y < 0 && -delta.y >= Math.Abs(delta.x))
                return Direction.Up;

            return Direction.None;
        }

        private enum Direction
        {
            None,
            Left,
            Right,
            Up,
            Down
        }
    }
}
