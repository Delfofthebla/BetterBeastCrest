using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BetterBeastCrest.Domain;
using GlobalSettings;
using HarmonyLib;
using UnityEngine;

namespace BetterBeastCrest.Services
{
    public static class BeastCrestModifier
    {
        // General stuff
        private static Gameplay _gameplayInstance;
        private static readonly MethodInfo _getGameplayMethod = typeof(GlobalSettingsBase<Gameplay>).GetMethod("Get", BindingFlags.Static | BindingFlags.NonPublic)!;
        private static readonly FieldInfo _rageDurationField = AccessTools.Field(typeof(Gameplay), "warriorRageDuration");
        private static readonly FieldInfo _slotsField = AccessTools.Field(typeof(ToolCrest), "slots");
        
        private static readonly float _verticalGapDistance = 1.72f;
        private static ToolCrest.SlotInfo[]? _originalWarriorCrestSlots;
        private static bool _hasPatchedRageDuration;
        private static float _originalRageDuration;

        public static bool CacheOriginalValuesIfNecessary()
        {
            if (_originalWarriorCrestSlots != null)
                return false;

            Plugin.Log.LogInfo("Caching Original Beast Crest Slot Values.");
            _originalWarriorCrestSlots = Gameplay.WarriorCrest.Slots.ToArray();
            _originalRageDuration = Gameplay.WarriorRageDuration;
            return true;
        }

        public static void ResetModStateIfAble()
        {
            if (_originalWarriorCrestSlots != null)
            {
                Plugin.Log.LogInfo("Reverting to Original Beast Crest Slot Values");
                _slotsField.SetValue(Gameplay.WarriorCrest, _originalWarriorCrestSlots.ToArray());
            }

            if (!Mathf.Approximately(_originalRageDuration, Gameplay.WarriorRageDuration))
            {
                Plugin.Log.LogInfo("Reverting Rage Duration to Default Value of: " + _originalRageDuration + " from: " + Gameplay.WarriorRageDuration);
                _rageDurationField.SetValue(GetGameplayInstance(), _originalRageDuration);
            }
        }

        public static void MakeGlobalChanges()
        {
            AdjustRageDuration();
        }

        public static void ApplyWarrior1Changes()
        {
            Plugin.Log.LogInfo("Making Beast Crest Rank 1 Changes");

            ModifyCenterToolSlotIfNecessary();

            var description = BuildDescription("Rank 1", Plugin.Config.CrestDefault, Plugin.Config.Crest1);
            if (string.IsNullOrWhiteSpace(description))
                return;

            LocalizationInjector.Append(Gameplay.WarriorCrest.Description,
                description);
        }

        public static void UnlockWarrior2()
        {
            Plugin.Log.LogInfo("Unlocking Beast Crest Rank 2");
            foreach (var slot in Plugin.Config.Crest2.ExtraToolSlots)
                AddToolSlot(slot);

            var description = BuildDescription("Rank 2", Plugin.Config.Crest1, Plugin.Config.Crest2);
            if (string.IsNullOrWhiteSpace(description))
                return;

            LocalizationInjector.Append(Gameplay.WarriorCrest.Description, description);
        }

        public static void UnlockWarrior3()
        {
            Plugin.Log.LogInfo("Unlocking Beast Crest Rank 3");
            foreach (var slot in Plugin.Config.Crest3.ExtraToolSlots)
                AddToolSlot(slot);

            var description = BuildDescription("Rank 3", Plugin.Config.Crest2, Plugin.Config.Crest3);
            if (string.IsNullOrWhiteSpace(description))
                return;

            LocalizationInjector.Append(Gameplay.WarriorCrest.Description, description);
        }

        public static void AdjustRageDuration()
        {
            if (!_hasPatchedRageDuration)
            {
                _originalRageDuration = Gameplay.WarriorRageDuration;
                _hasPatchedRageDuration = true;
            }

            var gameplayInstance = GetGameplayInstance();
            if (Helpers.IsBeastCrest3Unlocked && Plugin.Config.Crest3.RageDuration != 0)
                _rageDurationField.SetValue(gameplayInstance, _originalRageDuration * (1f + (Plugin.Config.Crest3.RageDuration / 100f)));
            else if (Helpers.IsBeastCrest2Unlocked && Plugin.Config.Crest2.RageDuration != 0)
                _rageDurationField.SetValue(gameplayInstance, _originalRageDuration * (1f + (Plugin.Config.Crest2.RageDuration / 100f)));
            else if (Helpers.IsBeastCrest1Unlocked && Plugin.Config.Crest1.RageDuration != 0)
                _rageDurationField.SetValue(gameplayInstance, _originalRageDuration * (1f + (Plugin.Config.Crest1.RageDuration / 100f)));
        }

        private static void ModifyCenterToolSlotIfNecessary()
        {
            var type = Plugin.Config.CenterToolSlotColor;
            if (type == ToolItemType.Skill)
                return;

            var original = Gameplay.WarriorCrest.Slots;
            var newArray = new ToolCrest.SlotInfo[original.Length];
            Array.Copy(original, newArray, original.Length);

            for (var i = 0; i < original.Length; i++)
            {
                var slot = original[i];
                if (slot.Type != ToolItemType.Skill)
                    continue;

                var replacementSlot = new ToolCrest.SlotInfo
                {
                    Type = type,
                    AttackBinding = slot.AttackBinding,
                    IsLocked = false,
                    Position = slot.Position,
                    NavUpIndex = slot.NavUpIndex,
                    NavDownIndex = slot.NavDownIndex,
                    NavLeftIndex = slot.NavLeftIndex,
                    NavRightIndex = slot.NavRightIndex,

                    NavUpFallbackIndex = slot.NavUpFallbackIndex,
                    NavDownFallbackIndex = slot.NavDownFallbackIndex,
                    NavLeftFallbackIndex = slot.NavLeftFallbackIndex,
                    NavRightFallbackIndex = slot.NavRightFallbackIndex,
                };
                newArray[i] = replacementSlot;
            }

            Plugin.Log.LogInfo($"Modified Beast Crest Rank 1 Center tool slot to {type}");
            _slotsField.SetValue(Gameplay.WarriorCrest, newArray);
        }

        private static void AddToolSlot(ExtraToolSlot extraToolSlot)
        {
            if (extraToolSlot.Position == ExtraToolSlotPosition.TopLeft)
                AddTopLeftToolSlot(extraToolSlot);
            else if (extraToolSlot.Position == ExtraToolSlotPosition.TopRight)
                AddTopRightToolSlot(extraToolSlot);
        }

        private static void AddTopLeftToolSlot(ExtraToolSlot extraSlot)
        {
            var original = Gameplay.WarriorCrest.Slots;
            var newArray = new ToolCrest.SlotInfo[original.Length + 1];
            Array.Copy(original, newArray, original.Length);

            // Grab our config color, but ensure that it is blue or yellow.
            var type = extraSlot.SlotColor;
            if (type != ToolItemType.Yellow && type != ToolItemType.Blue)
                type = ToolItemType.Blue;

            // Find the topmost slot to base our position on
            var leftMostSlot = newArray.Where(x => x.Type == ToolItemType.Yellow).OrderByDescending(s => s.Position.x).Last();
            var slot = new ToolCrest.SlotInfo
            {
                Type = type,
                IsLocked = extraSlot.RequiresUnlocking,
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

            Plugin.Log.LogInfo($"Added Rank 2 {type} Slot to Beast Crest");
            _slotsField.SetValue(Gameplay.WarriorCrest, newArray);
        }

        private static void AddTopRightToolSlot(ExtraToolSlot extraSlot)
        {
            var original = Gameplay.WarriorCrest.Slots;
            var newArray = new ToolCrest.SlotInfo[original.Length + 1];
            Array.Copy(original, newArray, original.Length);

            // Grab our config color, but ensure that it is blue or yellow.
            var type = extraSlot.SlotColor;
            if (type != ToolItemType.Yellow && type != ToolItemType.Blue)
                type = ToolItemType.Yellow;

            // Find the rightmost slot to base our position on
            var rightMostSlot = original.Where(x => x.Type == ToolItemType.Yellow).OrderByDescending(s => s.Position.x).First();
            var slot = new ToolCrest.SlotInfo
            {
                Type = type,
                IsLocked = extraSlot.RequiresUnlocking,
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

            Plugin.Log.LogInfo($"Added Rank 3 {type} Slot to Beast Crest");
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

            var prevSlots = previous.ExtraToolSlots.Count;
            var currSlots = current.ExtraToolSlots.Count;
            
            if (currSlots > prevSlots)
                parts.Add("your tools have expanded");
            else if (currSlots >= prevSlots && prevSlots > 0)
                parts.Add("your tools have expanded further");
            
            if (parts.Count == 0)
                return "";

            description += string.Join(", ", parts);
            description += ".";
            return description;
        }

        private static Gameplay GetGameplayInstance()
        {
            if (_gameplayInstance == null)
                _gameplayInstance = (Gameplay) _getGameplayMethod.Invoke(null, new object[] {"Global Gameplay Settings"});
            return _gameplayInstance;
        }
    }
}
