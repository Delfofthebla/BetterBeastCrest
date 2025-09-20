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
        
        // Down attack config stuff
        private static readonly FieldInfo _heroAnimOverrideLibField = AccessTools.Field(typeof(HeroControllerConfig), "heroAnimOverrideLib");
        private static readonly FieldInfo _downSlashTypeField = AccessTools.Field(typeof(HeroControllerConfig), "downSlashType");
        private static readonly FieldInfo _downSlashEventField = AccessTools.Field(typeof(HeroControllerConfig), "downSlashEvent");
        private static readonly FieldInfo _downspikeAnticTimeField = AccessTools.Field(typeof(HeroControllerConfig), "downspikeAnticTime");
        private static readonly FieldInfo _downspikeTimeField = AccessTools.Field(typeof(HeroControllerConfig), "downspikeTime");
        private static readonly FieldInfo _downspikeSpeedField = AccessTools.Field(typeof(HeroControllerConfig), "downspikeSpeed");
        private static readonly FieldInfo _downspikeRecoveryTimeField = AccessTools.Field(typeof(HeroControllerConfig), "downspikeRecoveryTime");
        private static readonly FieldInfo _downspikeBurstEffectField = AccessTools.Field(typeof(HeroControllerConfig), "downspikeBurstEffect");
        private static readonly FieldInfo _downspikeThrustsField = AccessTools.Field(typeof(HeroControllerConfig), "downspikeThrusts");
        
        private static readonly float _verticalGapDistance = 1.72f;
        
        private static ToolCrest.SlotInfo[]? _originalWarriorCrestSlots;
        private static bool _hasPatchedRageDuration;
        private static float _originalRageDuration;

        public static bool CacheOriginalValuesIfNecessary()
        {
            if (_originalWarriorCrestSlots != null)
                return false;

            Plugin.LogInfo("Caching Original Beast Crest Slot Values.");
            _originalWarriorCrestSlots = Gameplay.WarriorCrest.Slots.ToArray();
            _originalRageDuration = Gameplay.WarriorRageDuration;
            return true;
        }

        public static void ResetModStateIfAble()
        {
            if (_originalWarriorCrestSlots != null)
            {
                Plugin.LogInfo("Reverting to Original Beast Crest Slot Values"); 
                _slotsField.SetValue(Gameplay.WarriorCrest, _originalWarriorCrestSlots.ToArray());
            }

            if (!Mathf.Approximately(_originalRageDuration, Gameplay.WarriorRageDuration))
            {
                Plugin.LogInfo("Reverting Rage Duration to Default Value of: " + _originalRageDuration + " from: " + Gameplay.WarriorRageDuration);
                _rageDurationField.SetValue(GetGameplayInstance(), _originalRageDuration);
            }
        }

        public static void MakeGlobalChanges()
        {
            ModifyDownAttackConfig();
            AdjustRageDuration();
        }

        public static void ApplyWarrior1Changes()
        {
            Plugin.LogInfo("Making Beast Crest Rank 1 Changes");
            
            ModifyCenterToolSlotIfNecessary();
            
            var description = BuildDescription("Rank 1", Plugin.Config.CrestDefault, Plugin.Config.Crest1);
            if (string.IsNullOrWhiteSpace(description))
                return;
            
            LocalizationInjector.Append(
                Gameplay.WarriorCrest.Description,
                description
            );
        }
    
        public static void UnlockWarrior2()
        {
            Plugin.LogInfo("Unlocking Beast Crest Rank 2");
            if (Plugin.Config.Crest2.ToolSlotEnabled)
                AddWarrior2ToolSlot();
            
            var description = BuildDescription("Rank 2", Plugin.Config.Crest1, Plugin.Config.Crest2);
            if (string.IsNullOrWhiteSpace(description))
                return;
            
            LocalizationInjector.Append(
                Gameplay.WarriorCrest.Description,
                description
            );
        }

        public static void UnlockWarrior3()
        {
            Plugin.LogInfo("Unlocking Beast Crest Rank 3");
            
            if (Plugin.Config.Crest3.ToolSlotEnabled)
                AddWarrior3ToolSlot();
            
            var description = BuildDescription("Rank 3", Plugin.Config.Crest2, Plugin.Config.Crest3);
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
            if (Helpers.IsBeastCrest3Unlocked && Plugin.Config.Crest3.RageDuration != 0)
                _rageDurationField.SetValue(gameplayInstance, _originalRageDuration * (1f + (Plugin.Config.Crest3.RageDuration / 100f)));
            else if (Helpers.IsBeastCrest2Unlocked && Plugin.Config.Crest2.RageDuration != 0)
                _rageDurationField.SetValue(gameplayInstance, _originalRageDuration * (1f + (Plugin.Config.Crest2.RageDuration / 100f)));
            else if (Helpers.IsBeastCrest1Unlocked && Plugin.Config.Crest1.RageDuration != 0)
                _rageDurationField.SetValue(gameplayInstance, _originalRageDuration * (1f + (Plugin.Config.Crest1.RageDuration / 100f)));
        }
        
        private static void ModifyDownAttackConfig()
        {
            if (Plugin.Config.DownAttackType == CrestType.Beast)
                return;
            
            var desiredCrestConfig = Plugin.Config.DownAttackType switch
            {
                CrestType.Hunter => Gameplay.HunterCrest.HeroConfig,
                CrestType.Reaper => Gameplay.ReaperCrest.HeroConfig,
                CrestType.Wanderer => Gameplay.WandererCrest.HeroConfig,
                CrestType.Beast => Gameplay.WarriorCrest.HeroConfig,
                CrestType.Witch => Gameplay.WitchCrest.HeroConfig,
                CrestType.Architect => Gameplay.ToolmasterCrest.HeroConfig,
                CrestType.Shaman => Gameplay.SpellCrest.HeroConfig,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            if (Plugin.Config.DownAttackType == CrestType.Hunter)
            {
                if (Gameplay.HunterCrest3.IsUnlocked && Gameplay.HunterCrest3.HeroConfig != null)
                    desiredCrestConfig = Gameplay.HunterCrest3.HeroConfig;
                else if (Gameplay.HunterCrest2.IsUnlocked && Gameplay.HunterCrest2.HeroConfig != null)
                    desiredCrestConfig = Gameplay.HunterCrest2.HeroConfig;
                else if (desiredCrestConfig == null) // I have no fucking idea dude
                    desiredCrestConfig = Gameplay.HunterCrest.HeroConfig ?? Gameplay.HunterCrest2.HeroConfig ?? Gameplay.HunterCrest3.HeroConfig;
            }
            
            Plugin.LogInfo("Desired Config: " + desiredCrestConfig);
        
            var beastCrestConfig = Gameplay.WarriorCrest.HeroConfig;
            
            // Copy over the down attack animations without nuking the rest
            
            var desiredAnimLib = _heroAnimOverrideLibField.GetValue(desiredCrestConfig) as tk2dSpriteAnimation;
            var beastAnimLib = _heroAnimOverrideLibField.GetValue(beastCrestConfig) as tk2dSpriteAnimation;
            if (desiredAnimLib != null && beastAnimLib == null)
            {
                Plugin.LogInfo("Desired Anim Swap is not null but Beast Crest Anims ARE null");
                _heroAnimOverrideLibField.SetValue(beastCrestConfig, desiredAnimLib);
            }
            else if (desiredAnimLib == null && beastAnimLib != null)
            {
                Plugin.LogInfo("Desired Anim Swap IS null but Beast Crest Anims are NOT null");
                _heroAnimOverrideLibField.SetValue(beastCrestConfig, null);
            }
            else
            {
                Plugin.LogInfo("Neither the desired Anim Swap or the Beast Crest Anims are null");
            }

            Plugin.LogInfo("Modifying anims and down attack variables");
            AnimationUtils.MergeDownAttackClips(desiredAnimLib!, beastAnimLib!);
    
            // Modify down attack values
            _downSlashTypeField.SetValue(beastCrestConfig, desiredCrestConfig.DownSlashType);
            _downSlashEventField.SetValue(beastCrestConfig, desiredCrestConfig.DownSlashEvent);
            _downspikeAnticTimeField.SetValue(beastCrestConfig, desiredCrestConfig.DownSpikeAnticTime);
            _downspikeTimeField.SetValue(beastCrestConfig, desiredCrestConfig.DownSpikeTime);
            _downspikeSpeedField.SetValue(beastCrestConfig, desiredCrestConfig.DownspikeSpeed);
            _downspikeRecoveryTimeField.SetValue(beastCrestConfig, desiredCrestConfig.DownspikeRecoveryTime);
            _downspikeBurstEffectField.SetValue(beastCrestConfig, desiredCrestConfig.DownspikeBurstEffect);
            _downspikeThrustsField.SetValue(beastCrestConfig, desiredCrestConfig.DownspikeThrusts);
        }
        
        private static void ModifyCenterToolSlotIfNecessary()
        {
            if (!Plugin.Config.Crest1.ToolSlotEnabled)
                return;
            
            var original = Gameplay.WarriorCrest.Slots;
            var newArray = new ToolCrest.SlotInfo[original.Length];
            Array.Copy(original, newArray, original.Length);

            // Grab our config color
            var type = Plugin.Config.Crest1.SlotColor;

            for (var i = 0; i < original.Length; i++)
            {
                var slot = original[i];
                if (slot.Type != ToolItemType.Skill)
                    continue;

                var attackBinding = slot.AttackBinding;
                // if (type != ToolItemType.Red)
                //     attackBinding = null;
                
                var replacementSlot = new ToolCrest.SlotInfo
                {
                    Type = type,
                    AttackBinding = attackBinding,
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
            
            Plugin.LogInfo($"Modified Beast Crest Rank 1 Center tool slot to {type}");
            _slotsField.SetValue(Gameplay.WarriorCrest, newArray);
        }

        private static void AddWarrior2ToolSlot()
        {
            var original = Gameplay.WarriorCrest.Slots;
            var newArray = new ToolCrest.SlotInfo[original.Length + 1];
            Array.Copy(original, newArray, original.Length);

            // Grab our config color, but ensure that it is blue or yellow.
            var type = Plugin.Config.Crest2.SlotColor;
            if (type != ToolItemType.Yellow && type != ToolItemType.Blue)
                type = ToolItemType.Blue;
            
            // Find the topmost slot to base our position on
            var leftMostSlot = newArray.Where(x => x.Type == ToolItemType.Yellow).OrderByDescending(s => s.Position.x).Last();
            var slot = new ToolCrest.SlotInfo
            {
                Type = type,
                IsLocked = Plugin.Config.Crest2.ToolSlotRequiresUnlocking,
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

            Plugin.LogInfo($"Added Rank 2 {type} Slot to Beast Crest");
            _slotsField.SetValue(Gameplay.WarriorCrest, newArray);
        }

        private static void AddWarrior3ToolSlot()
        {
            var original = Gameplay.WarriorCrest.Slots;
            var newArray = new ToolCrest.SlotInfo[original.Length + 1];
            Array.Copy(original, newArray, original.Length);
            
            // Grab our config color, but ensure that it is blue or yellow.
            var type = Plugin.Config.Crest3.SlotColor;
            if (type != ToolItemType.Yellow && type != ToolItemType.Blue)
                type = ToolItemType.Yellow;

            // Find the rightmost slot to base our position on
            var rightMostSlot = original.Where(x => x.Type == ToolItemType.Yellow).OrderByDescending(s => s.Position.x).First();
            var slot = new ToolCrest.SlotInfo
            {
                Type = type,
                IsLocked = Plugin.Config.Crest3.ToolSlotRequiresUnlocking,
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

            Plugin.LogInfo($"Added Rank 3 {type} Slot to Beast Crest");
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

        private static Gameplay GetGameplayInstance()
        {
            if (_gameplayInstance == null)
                _gameplayInstance = (Gameplay) _getGameplayMethod.Invoke(null, new object[] {"Global Gameplay Settings"});
            return _gameplayInstance;
        }
    }
}
