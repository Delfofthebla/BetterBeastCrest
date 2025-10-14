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
        public static bool ModInitialized = false;
        
        private static Gameplay? _gameplayInstance;
        private static readonly MethodInfo _getGameplayMethod = typeof(GlobalSettingsBase<Gameplay>).GetMethod("Get", BindingFlags.Static | BindingFlags.NonPublic)!;
        private static readonly FieldInfo _rageDurationField = AccessTools.Field(typeof(Gameplay), "warriorRageDuration");
        private static readonly FieldInfo _rageDamageMultField = AccessTools.Field(typeof(Gameplay), "warriorDamageMultiplier");
        private static readonly FieldInfo _rageAttackCooldownField = AccessTools.Field(typeof(HeroControllerConfigWarrior), "rageAttackCooldownTime");
        private static readonly FieldInfo _slotsField = AccessTools.Field(typeof(ToolCrest), "slots");
        
        private static readonly float _verticalGapDistance = 1.72f;
        private static ToolCrest.SlotInfo[]? _originalWarriorCrestSlots;

        public static bool CacheOriginalValuesIfNecessary()
        {
            if (_originalWarriorCrestSlots != null)
                return false;

            Plugin.Log.LogInfo("Caching Original Beast Crest Slot Values.");
            _originalWarriorCrestSlots = Gameplay.WarriorCrest.Slots.ToArray();
            return true;
        }

        public static void ResetModStateIfAble()
        {
            if (_originalWarriorCrestSlots != null)
            {
                Plugin.Log.LogInfo("Reverting to Original Beast Crest Slot Values");
                _slotsField.SetValue(Gameplay.WarriorCrest, _originalWarriorCrestSlots.ToArray());
            }

            if (!Mathf.Approximately(Plugin.ModConfig.CrestDefault.RageDuration, Gameplay.WarriorRageDuration))
            {
                Plugin.Log.LogInfo("Reverting Rage Duration to Default Value of: " + Plugin.ModConfig.CrestDefault.RageDuration + " from: " + Gameplay.WarriorRageDuration);
                _rageDurationField.SetValue(GetGameplayInstance(), Plugin.ModConfig.CrestDefault.RageDuration);
            }
        }

        public static void MakeGlobalChanges()
        {
            AdjustRageStats();
        }

        public static void ApplyRank1Changes()
        {
            Plugin.Log.LogInfo("Making Beast Crest Rank 1 Changes");

            ModifyCenterToolSlotsIfNecessary();
            AddExtraToolSlotsIfNecessary(Plugin.ModConfig.Crest1.ExtraToolSlots);

            var description = BuildDescription("Rank 1", Plugin.ModConfig.CrestDefault, Plugin.ModConfig.Crest1);
            if (string.IsNullOrWhiteSpace(description))
                return;

            LocalizationInjector.Append(Gameplay.WarriorCrest.Description, description);
        }

        public static void UnlockRank2()
        {
            Plugin.Log.LogInfo("Unlocking Beast Crest Rank 2");
            AddExtraToolSlotsIfNecessary(Plugin.ModConfig.Crest2.ExtraToolSlots);

            var description = BuildDescription("Rank 2", Plugin.ModConfig.Crest1, Plugin.ModConfig.Crest2);
            if (string.IsNullOrWhiteSpace(description))
                return;

            LocalizationInjector.Append(Gameplay.WarriorCrest.Description, description);
        }

        public static void UnlockRank3()
        {
            Plugin.Log.LogInfo("Unlocking Beast Crest Rank 3");
            AddExtraToolSlotsIfNecessary(Plugin.ModConfig.Crest3.ExtraToolSlots);

            var description = BuildDescription("Rank 3", Plugin.ModConfig.Crest2, Plugin.ModConfig.Crest3);
            if (string.IsNullOrWhiteSpace(description))
                return;

            LocalizationInjector.Append(Gameplay.WarriorCrest.Description, description);
        }

        public static void AdjustRageStats()
        {
            var gameplayInstance = GetGameplayInstance();
            var beastConfig = Gameplay.WarriorCrest.HeroConfig as HeroControllerConfigWarrior;
            if (Helpers.IsBeastCrest3Unlocked)
            {
                _rageDurationField.SetValue(gameplayInstance, Plugin.ModConfig.Crest3.RageDuration);
                _rageDamageMultField.SetValue(gameplayInstance, Plugin.ModConfig.Crest3.RageDamageMultiplier);
                _rageAttackCooldownField.SetValue(beastConfig, Plugin.ModConfig.Crest3.RageAttackCooldown);
            }
            else if (Helpers.IsBeastCrest2Unlocked)
            {
                _rageDurationField.SetValue(gameplayInstance, Plugin.ModConfig.Crest2.RageDuration);
                _rageDamageMultField.SetValue(gameplayInstance, Plugin.ModConfig.Crest2.RageDamageMultiplier);
                _rageAttackCooldownField.SetValue(beastConfig, Plugin.ModConfig.Crest2.RageAttackCooldown);
            }
            else if (Helpers.IsBeastCrest1Unlocked)
            {
                _rageDurationField.SetValue(gameplayInstance, Plugin.ModConfig.Crest1.RageDuration);
                _rageDamageMultField.SetValue(gameplayInstance, Plugin.ModConfig.Crest1.RageDamageMultiplier);
                _rageAttackCooldownField.SetValue(beastConfig, Plugin.ModConfig.Crest1.RageAttackCooldown);
            }
        }

        private static void ModifyCenterToolSlotsIfNecessary()
        {
            var original = Gameplay.WarriorCrest.Slots;
            var newArray = new ToolCrest.SlotInfo[original.Length];
            Array.Copy(original, newArray, original.Length);

            if (Plugin.ModConfig.TopCenterToolSlotColor != ToolItemType.Red)
            {
                var replaceIndex = SlotUtils.GetTopCenterIndex(original, ToolItemType.Red);
                newArray[replaceIndex] = CloneToSlotWithType(original[replaceIndex], Plugin.ModConfig.TopCenterToolSlotColor);
                Plugin.Log.LogInfo($"Set Beast Crest Top Center tool slot to {Plugin.ModConfig.TopCenterToolSlotColor}");
            }
            
            if (Plugin.ModConfig.MiddleCenterToolSlotColor != ToolItemType.Skill)
            {
                var replaceIndex = SlotUtils.GetMiddleCenterIndex(original, ToolItemType.Skill);
                newArray[replaceIndex] = CloneToSlotWithType(original[replaceIndex], Plugin.ModConfig.MiddleCenterToolSlotColor);
                Plugin.Log.LogInfo($"Set Beast Crest Top Center tool slot to {Plugin.ModConfig.MiddleCenterToolSlotColor}");
            }
            
            if (Plugin.ModConfig.BottomCenterToolSlotColor != ToolItemType.Red)
            {
                var replaceIndex = SlotUtils.GetBottomCenterIndex(original, ToolItemType.Red);
                newArray[replaceIndex] = CloneToSlotWithType(original[replaceIndex], Plugin.ModConfig.BottomCenterToolSlotColor);
                Plugin.Log.LogInfo($"Set Beast Crest Top Center tool slot to {Plugin.ModConfig.BottomCenterToolSlotColor}");
            }
            
            _slotsField.SetValue(Gameplay.WarriorCrest, newArray);
        }

        private static ToolCrest.SlotInfo CloneToSlotWithType(ToolCrest.SlotInfo existingSlot, ToolItemType type) => new ToolCrest.SlotInfo
        {
            Type = type,
            AttackBinding = existingSlot.AttackBinding,
            IsLocked = existingSlot.IsLocked,
            Position = existingSlot.Position,
            NavUpIndex = existingSlot.NavUpIndex,
            NavDownIndex = existingSlot.NavDownIndex,
            NavLeftIndex = existingSlot.NavLeftIndex,
            NavRightIndex = existingSlot.NavRightIndex,

            NavUpFallbackIndex = existingSlot.NavUpFallbackIndex,
            NavDownFallbackIndex = existingSlot.NavDownFallbackIndex,
            NavLeftFallbackIndex = existingSlot.NavLeftFallbackIndex,
            NavRightFallbackIndex = existingSlot.NavRightFallbackIndex
        };

        private static void AddExtraToolSlotsIfNecessary(IReadOnlyList<ExtraToolSlot> extraSlots)
        {
            if (!extraSlots.Any())
                return;

            foreach (var slot in extraSlots)
                AddToolSlot(slot);

            var fixedSlots = ToolSlotNavigationHelper.ToFixedNavigation(Gameplay.WarriorCrest.Slots);
            _slotsField.SetValue(Gameplay.WarriorCrest, fixedSlots);
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
                Position = new Vector2(leftMostSlot.Position.x - 0.45f, leftMostSlot.Position.y + _verticalGapDistance)
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
                Position = new Vector2(rightMostSlot.Position.x + 0.45f, rightMostSlot.Position.y + _verticalGapDistance)
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
