using System;
using System.Linq;
using System.Reflection;
using GlobalSettings;
using HarmonyLib;
using UnityEngine;

namespace BetterBeastCrest
{
    public static class BeastCrestModifier
    {
        private static readonly FieldInfo _rageDurationField = AccessTools.Field(typeof(Gameplay), "warriorRageDuration");
        private static readonly FieldInfo _descriptionField = AccessTools.Field(typeof(ToolCrest), "description");
        private static readonly FieldInfo _slotsField = AccessTools.Field(typeof(ToolCrest), "slots");
        private static readonly float _verticalGapDistance = 1.72f;

        public static void UnlockWarrior2()
        {
            AddWarrior2ToolSlot();

            LocalizationInjector.Append(
                Gameplay.WarriorCrest.Description,
                BuildDescription("Rank 2")
            );
        }

        public static void UnlockWarrior3()
        {
            //MoveWarrior2SlotAndAddWarrior3ToolSlot();
            LocalizationInjector.Append(
                Gameplay.WarriorCrest.Description,
                BuildDescription("Rank 3")
            );
        }
        
        public static void AddWarrior2ToolSlot()
        {
            var original = Gameplay.WarriorCrest.Slots;
            var newArray = new ToolCrest.SlotInfo[original.Length + 1];
            Array.Copy(original, newArray, original.Length);

            // Find the topmost slot to base our position on
            var topSlot = original.OrderByDescending(s => s.Position.y).First();
            var slot = new ToolCrest.SlotInfo
            {
                Type = ToolItemType.Blue,
                IsLocked = true,
                Position = new Vector2(topSlot.Position.x, topSlot.Position.y + _verticalGapDistance)
            };
            newArray[original.Length] = slot;
            
            _slotsField.SetValue(Gameplay.WarriorCrest, newArray);
        }

        public static void AdjustRageDuration()
        {
            var method = typeof(GlobalSettingsBase<Gameplay>).GetMethod("Get", BindingFlags.Static | BindingFlags.NonPublic);
            var gameplayInstance = (Gameplay) method.Invoke(null, new object[] {"Global Gameplay Settings"});
            
            if (Helpers.IsBeastCrest3Unlocked && Plugin.RageDurationIncrease3.Value != 0)
                _rageDurationField.SetValue(gameplayInstance, Gameplay.WarriorRageDuration * (1f + (Plugin.RageDurationIncrease3.Value / 100f)));
            else if (Helpers.IsBeastCrest2Unlocked && Plugin.RageDurationIncrease2.Value != 0)
                _rageDurationField.SetValue(gameplayInstance, Gameplay.WarriorRageDuration * (1f + (Plugin.RageDurationIncrease2.Value / 100f)));
        }
        
        //
        // public static void MoveWarrior2SlotAndAddWarrior3ToolSlot()
        // {
        //     var original = Gameplay.WarriorCrest.Slots;
        //     var newArray = new ToolCrest.SlotInfo[original.Length + 1];
        //     Array.Copy(original, newArray, original.Length);
        //     
        //     var leftMostSlot = newArray.Where(x => x.Type == ToolItemType.Yellow).OrderByDescending(s => s.Position.x).Last();
        //     var warrior2BlueSlot = newArray.First(s => s.Type == ToolItemType.Blue);
        //     warrior2BlueSlot.Position = new Vector2(leftMostSlot.Position.x - 0.6f, leftMostSlot.Position.y + _verticalGapDistance);
        //
        //     // Find the rightmost slot to base our position on
        //     var rightMostSlot = original.Where(x => x.Type == ToolItemType.Yellow).OrderByDescending(s => s.Position.x).First();
        //     var slot = new ToolCrest.SlotInfo
        //     {
        //         Type = ToolItemType.Blue,
        //         IsLocked = false,
        //         Position = new Vector2(rightMostSlot.Position.x + 0.6f, rightMostSlot.Position.y + _verticalGapDistance)
        //     };
        //     newArray[original.Length] = slot;
        //     
        //     _slotsField.SetValue(Gameplay.WarriorCrest, newArray);
        // }

        private static string BuildDescription(string rankPrefix)
        {
            var description = rankPrefix.Trim() + " ";
            var healingWasIncreased = Plugin.ImmediateHealthOnBind2.Value + Plugin.MaximumLifeLeech2.Value > 3;
            var healingWasDecreased = Plugin.ImmediateHealthOnBind2.Value + Plugin.MaximumLifeLeech2.Value < 3;
            var rageDurationIncreased = Plugin.RageDurationIncrease2.Value > 0;
            var rageDurationDecreased = Plugin.RageDurationIncrease2.Value < 0;
            
            if (healingWasIncreased)
                description += "Rage restores more health";
            else if (healingWasDecreased)
                description += "Rage restores less health";
            
            if ((healingWasDecreased || healingWasIncreased) && (rageDurationDecreased || rageDurationIncreased))
                description += ", ";    // We already have started the description, let's just add a comma
            else
                description += "Rage ";
            
            if (rageDurationIncreased)
                description += "lasts longer";
            else if (rageDurationDecreased)
                description += "expires quicker";
            
            if (healingWasDecreased || healingWasIncreased || rageDurationDecreased || rageDurationIncreased)
                description += " and ";
            
            description += "your tools have expanded.";
            return description;
        }
    }
}
