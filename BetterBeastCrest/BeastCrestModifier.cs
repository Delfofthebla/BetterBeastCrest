using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GlobalSettings;
using HarmonyLib;
using UnityEngine;

namespace BetterBeastCrest
{
    public static class BeastCrestModifier
    {
        private static readonly MethodInfo _getGameplayMethod = typeof(GlobalSettingsBase<Gameplay>).GetMethod("Get", BindingFlags.Static | BindingFlags.NonPublic)!;
        private static readonly FieldInfo _rageDurationField = AccessTools.Field(typeof(Gameplay), "warriorRageDuration");
        private static readonly FieldInfo _slotsField = AccessTools.Field(typeof(ToolCrest), "slots");
        private static readonly float _verticalGapDistance = 1.72f;
        
        private static ToolCrest.SlotInfo[]? _originalWarriorCrestSlots;
        private static bool _hasPatchedRageDuration;
        private static float _originalRageDuration;

        public static void CacheOriginalValuesIfNecessary()
        {
            if (_originalWarriorCrestSlots == null)
            {
                Plugin.Log.LogInfo("[BetterBeastCrest]: Caching Original Beast Crest Slot Values. Default Rage Duration: " + Gameplay.WarriorRageDuration);
                _originalWarriorCrestSlots = Gameplay.WarriorCrest.Slots.ToArray();
                _originalRageDuration = Gameplay.WarriorRageDuration;
            }
        }

        public static void ResetModStateIfNecessary()
        {
            if (_originalWarriorCrestSlots != null)
            {
                Plugin.Log.LogInfo("[BetterBeastCrest]: Reverting to Original Beast Crest Slot Values"); 
                _slotsField.SetValue(Gameplay.WarriorCrest, _originalWarriorCrestSlots.ToArray());
            }

            if (!Mathf.Approximately(_originalRageDuration, Gameplay.WarriorRageDuration))
            {
                Plugin.Log.LogInfo("[BetterBeastCrest]: Reverting Rage Duration to Default Value of: " + _originalRageDuration + " from: " + Gameplay.WarriorRageDuration);
                _rageDurationField.SetValue(GetGameplayInstance(), _originalRageDuration);
            }
        }

        public static void ApplyWarrior1Changes()
        {
            var description = BuildDescription("Rank 1", Plugin.CrestDefault, Plugin.Crest1);
            if (string.IsNullOrWhiteSpace(description))
                return;
            
            LocalizationInjector.Append(
                Gameplay.WarriorCrest.Description,
                description
            );
        }
    
        public static void UnlockWarrior2()
        {
            if (Plugin.Crest2.ToolSlotEnabled)
                AddWarrior2ToolSlot();
            
            var description = BuildDescription("Rank 2", Plugin.Crest1, Plugin.Crest2);
            if (string.IsNullOrWhiteSpace(description))
                return;
            
            LocalizationInjector.Append(
                Gameplay.WarriorCrest.Description,
                description
            );
        }

        public static void UnlockWarrior3()
        {
            if (Plugin.Crest3.ToolSlotEnabled)
                AddWarrior3ToolSlot();
            
            var description = BuildDescription("Rank 3", Plugin.Crest2, Plugin.Crest3);
            if (string.IsNullOrWhiteSpace(description))
                return;
            
            LocalizationInjector.Append(
                Gameplay.WarriorCrest.Description,
                description
            );
        }

        public static void AdjustRageDuration()
        {
            if (!_hasPatchedRageDuration)
            {
                _originalRageDuration = Gameplay.WarriorRageDuration;
                _hasPatchedRageDuration = true;
            }
            
            var gameplayInstance = GetGameplayInstance();
            if (Helpers.IsBeastCrest3Unlocked && Plugin.Crest3.RageDuration != 0)
                _rageDurationField.SetValue(gameplayInstance, _originalRageDuration * (1f + (Plugin.Crest3.RageDuration / 100f)));
            else if (Helpers.IsBeastCrest2Unlocked && Plugin.Crest2.RageDuration != 0)
                _rageDurationField.SetValue(gameplayInstance, _originalRageDuration * (1f + (Plugin.Crest2.RageDuration / 100f)));
            else if (Helpers.IsBeastCrest1Unlocked && Plugin.CrestDefault.RageDuration != 0)
                _rageDurationField.SetValue(gameplayInstance, _originalRageDuration * (1f + (Plugin.CrestDefault.RageDuration / 100f)));
        }

        public static void AddWarrior2ToolSlot()
        {
            var original = Gameplay.WarriorCrest.Slots;
            var newArray = new ToolCrest.SlotInfo[original.Length + 1];
            Array.Copy(original, newArray, original.Length);

            // Grab our config color, but ensure that it is blue or yellow.
            var type = Plugin.Crest2.SlotColor;
            if (type != ToolItemType.Yellow && type != ToolItemType.Blue)
                type = ToolItemType.Blue;
            
            // Find the topmost slot to base our position on
            var leftMostSlot = newArray.Where(x => x.Type == ToolItemType.Yellow).OrderByDescending(s => s.Position.x).Last();
            var slot = new ToolCrest.SlotInfo
            {
                Type = type,
                IsLocked = true,
                Position = new Vector2(leftMostSlot.Position.x - 0.45f, leftMostSlot.Position.y + _verticalGapDistance),
                NavUpIndex = leftMostSlot.NavUpIndex - 1,
                NavLeftIndex = leftMostSlot.NavLeftIndex - 1,
                NavRightIndex = leftMostSlot.NavRightIndex + 2,

                // These appear to always be -1 for all other tool slots so we will do the same
                NavUpFallbackIndex = -1,
                NavLeftFallbackIndex = -1,
                NavRightFallbackIndex = -1,
                NavDownFallbackIndex = -1
            };
            newArray[original.Length] = slot;

            Plugin.Log.LogInfo($"[BetterBeastCrest]: Added Rank 2 {type} Slot to Beast Crest");
            _slotsField.SetValue(Gameplay.WarriorCrest, newArray);
        }

        public static void AddWarrior3ToolSlot()
        {
            var original = Gameplay.WarriorCrest.Slots;
            var newArray = new ToolCrest.SlotInfo[original.Length + 1];
            Array.Copy(original, newArray, original.Length);
            
            // Grab our config color, but ensure that it is blue or yellow.
            var type = Plugin.Crest3.SlotColor;
            if (type != ToolItemType.Yellow && type != ToolItemType.Blue)
                type = ToolItemType.Yellow;

            // Find the rightmost slot to base our position on
            var rightMostSlot = original.Where(x => x.Type == ToolItemType.Yellow).OrderByDescending(s => s.Position.x).First();
            var slot = new ToolCrest.SlotInfo
            {
                Type = type,
                IsLocked = true,
                Position = new Vector2(rightMostSlot.Position.x + 0.45f, rightMostSlot.Position.y + _verticalGapDistance),
                NavUpIndex = rightMostSlot.NavUpIndex,
                NavLeftIndex = rightMostSlot.NavLeftIndex + 2,
                NavRightIndex = 5,

                // These appear to always be -1 for all other tool slots so we will do the same
                NavUpFallbackIndex = -1,
                NavLeftFallbackIndex = -1,
                NavRightFallbackIndex = -1,
                NavDownFallbackIndex = -1
            };
            newArray[original.Length] = slot;

            Plugin.Log.LogInfo($"[BetterBeastCrest]: Added Rank 3 {type} Slot to Beast Crest");
            _slotsField.SetValue(Gameplay.WarriorCrest, newArray);
        }

        private static string BuildDescription(string rankPrefix, CrestStats previous, CrestStats current)
        {
            var description = $" {rankPrefix.Trim()}: ";
            var parts = new List<string>();

            if (current.TotalHealing > previous.TotalHealing)
                parts.Add("Rage restores more health");
            else if (current.TotalHealing < previous.TotalHealing)
                parts.Add("Rage restores less health");

            if (current.RageDuration > previous.RageDuration)
                parts.Add("Rage lasts longer");
            else if (current.RageDuration < previous.RageDuration)
                parts.Add("Rage expires quicker");

            if (current.ToolSlotEnabled && !previous.ToolSlotEnabled)
                parts.Add("your tools have expanded");
            else if (current.ToolSlotEnabled && previous.ToolSlotEnabled)
                parts.Add("your tools have expanded further");

            if (parts.Count == 0)
                return "";

            description += string.Join(", ", parts);
            description += ".";
            return description;
        }
        
        private static Gameplay GetGameplayInstance() => (Gameplay) _getGameplayMethod.Invoke(null, new object[] {"Global Gameplay Settings"});
    }
}
