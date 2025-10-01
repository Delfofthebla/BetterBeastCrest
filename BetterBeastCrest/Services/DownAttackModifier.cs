using System;
using System.Reflection;
using BetterBeastCrest.Domain;
using BetterBeastCrest.Extensions;
using HarmonyLib;
using UnityEngine;

namespace BetterBeastCrest.Services
{
    public static class DownAttackModifier
    {
        private static bool _hasPatchedDownAir;
        private static HeroControllerConfig? _backupConfig;
        private static HeroController.ConfigGroup? _backupConfigGroup;
        private static HeroController.ConfigGroup? _swapCrestConfigGroup;
        
        private static readonly FieldInfo _heroAnimOverrideLibField = AccessTools.Field(typeof(HeroControllerConfig), "heroAnimOverrideLib");
        private static readonly string[] _configFields =
        {
            "downSlashType",
            "downSlashEvent",
            "downspikeAnticTime",
            "downspikeTime",
            "downspikeSpeed",
            "downspikeRecoveryTime",
            "downspikeBurstEffect",
            "downspikeThrusts",
        };
        private static readonly string[] _groupFields = {"DownSlashObject", "AltDownSlashObject"};

        private static readonly FieldInfo _hc_configs = AccessTools.Field(typeof(HeroController), "configs");
        private static readonly MethodInfo _hc_updateConfig = AccessTools.Method(typeof(HeroController), "UpdateConfig");

        private static readonly PropertyInfo _cg_downspike = AccessTools.Property(typeof(HeroController.ConfigGroup), "Downspike");
        private static readonly PropertyInfo _cg_altDownspike = AccessTools.Property(typeof(HeroController.ConfigGroup), "AltDownspike");
        private static readonly PropertyInfo _cg_downSlash = AccessTools.Property(typeof(HeroController.ConfigGroup), "DownSlash");
        private static readonly PropertyInfo _cg_altDownSlash = AccessTools.Property(typeof(HeroController.ConfigGroup), "AltDownSlash");
        private static readonly PropertyInfo _cg_downSlashDamager = AccessTools.Property(typeof(HeroController.ConfigGroup), "DownSlashDamager");
        private static readonly PropertyInfo _cg_altDownSlashDamager = AccessTools.Property(typeof(HeroController.ConfigGroup), "AltDownSlashDamager");
        
        public static bool ShouldRevertPatchWhenAble = false;

        public static void RevertDownAttackPatch(HeroController heroController)
        {
            _hasPatchedDownAir = false;
            ShouldRevertPatchWhenAble = false;
            
            if (_swapCrestConfigGroup != null && (bool) (UnityEngine.Object) _swapCrestConfigGroup.ActiveRoot)
                _swapCrestConfigGroup.ActiveRoot.SetActive(false);

            if (_backupConfigGroup != null && _backupConfigGroup.Config != null)
            {
                var allConfigGroups = _hc_configs.GetValue(heroController) as HeroController.ConfigGroup[] ?? Array.Empty<HeroController.ConfigGroup>();
                var beastConfigGroup = allConfigGroups.ForCrest(CrestType.Beast);
                OverwriteDownAttackMoveset(_backupConfigGroup, beastConfigGroup);
            }
            
            Plugin.Log.LogInfo("Down Attack Patch disabled.");
        }

        public static void PatchDownAttackIfNecessary(HeroController heroController)
        {
            if (ShouldRevertPatchWhenAble)
                RevertDownAttackPatch(heroController);

            // var beastConfig = beastConfigGroup.Config as HeroControllerConfigWarrior;
            // var otherBeastConfig = Gameplay.WarriorCrest.HeroConfig as HeroControllerConfigWarrior;
            // if (beastConfig == null || otherBeastConfig == null)
            // {
            //     Plugin.Log.LogError("Couldn't cast it as a warrior config. Cringe.");
            //     return;
            // }
            //
            // Plugin.Log.LogInfo("====STATS====");
            // Plugin.Log.LogInfo("WarriorDamageMultiplier: " + Gameplay.WarriorDamageMultiplier);
            // Plugin.Log.LogInfo($"rageAttackDuration: {AccessTools.Field(typeof(HeroControllerConfigWarrior), "rageAttackDuration").GetValue(beastConfig)}, AttackDuration: {beastConfig.AttackDuration}");
            // Plugin.Log.LogInfo($"rageAttackRecoveryTime: {AccessTools.Field(typeof(HeroControllerConfigWarrior), "rageAttackRecoveryTime").GetValue(beastConfig)}, AttackRecoveryTime: {beastConfig.AttackRecoveryTime}");
            // Plugin.Log.LogInfo($"HeroController rageAttackCooldownTime: {AccessTools.Field(typeof(HeroControllerConfigWarrior), "rageAttackCooldownTime").GetValue(beastConfig)}, AttackCooldownTime: {beastConfig.AttackCooldownTime}");
            // Plugin.Log.LogInfo($"Gameplay rageAttackCooldownTime: {AccessTools.Field(typeof(HeroControllerConfigWarrior), "rageAttackCooldownTime").GetValue(otherBeastConfig)}, AttackCooldownTime: {otherBeastConfig.AttackCooldownTime}");
            // Plugin.Log.LogInfo($"rageQuickAttackCooldownTime: {AccessTools.Field(typeof(HeroControllerConfigWarrior), "rageQuickAttackCooldownTime").GetValue(beastConfig)}, QuickAttackCooldownTime: {beastConfig.QuickAttackCooldownTime}");
            
            if (_hasPatchedDownAir)
                return;

            Plugin.Log.LogInfo("Attempting to modify the Beast Crest Down Attack.");
            _hasPatchedDownAir = true;
            
            var allConfigGroups = _hc_configs.GetValue(heroController) as HeroController.ConfigGroup[] ?? Array.Empty<HeroController.ConfigGroup>();
            var beastConfigGroup = allConfigGroups.ForCrest(CrestType.Beast);
            if (beastConfigGroup == null)
            {
                Plugin.Log.LogError("No config group found for Beast Crest.");
                return;
            }
            
            var otherConfigGroup = allConfigGroups.ForCrest(Plugin.ModConfig.DownAttackType);
            if (otherConfigGroup == null)
            {
                Plugin.Log.LogError($"Unable to find crest config group for down attack type: {Plugin.ModConfig.DownAttackType}.");
                return;
            }
            _swapCrestConfigGroup = otherConfigGroup;
            
            BackupBeastCrestDefaultsIfNecessary(beastConfigGroup);
            otherConfigGroup.ActiveRoot.SetActive(true);    // Forcefully load the prefabs (I guess? Good god I have no fucking idea what I am doing)
            OverwriteDownAttackMoveset(otherConfigGroup, beastConfigGroup);

            // Cause the HeroController to reprocess everything about the config
            _hc_updateConfig.Invoke(heroController, new object [] { });
            
            Plugin.Log.LogInfo($"Beast Crest down attack overridden with {Plugin.ModConfig.DownAttackType} down attack.");
        }

        private static void BackupBeastCrestDefaultsIfNecessary(HeroController.ConfigGroup beastConfigGroup)
        {
            if (_backupConfig != null && _backupConfigGroup != null)
                return;

            _backupConfig = ScriptableObject.CreateInstance<HeroControllerConfig>();
            _backupConfigGroup = new HeroController.ConfigGroup { Config = _backupConfig };
            _heroAnimOverrideLibField.SetValue(_backupConfig, _heroAnimOverrideLibField.GetValue(beastConfigGroup.Config));
            CopyAndInitializeConfigGroup(beastConfigGroup, _backupConfigGroup);
        }

        private static void OverwriteDownAttackMoveset(HeroController.ConfigGroup? movesetGroup, HeroController.ConfigGroup? modifyGroup)
        {
            if (movesetGroup == null || modifyGroup == null)
            {
                Plugin.Log.LogError("Cannot modify the HeroController ConfigGroup due to one of the config groups being null");
                return;
            }
            
            // Hackily merge the down attack properties, values, and animations from the otherConfig
            CopyAndInitializeConfigGroup(movesetGroup, modifyGroup);
            AnimationUtils.MergeDownAttackClips(
                _heroAnimOverrideLibField.GetValue(movesetGroup.Config) as tk2dSpriteAnimation,
                _heroAnimOverrideLibField.GetValue(modifyGroup.Config) as tk2dSpriteAnimation
            );
        }

        private static void CopyAndInitializeConfigGroup(HeroController.ConfigGroup movesetGroup, HeroController.ConfigGroup modifyGroup)
        {
            ReflectionUtils.CopyFields(movesetGroup, modifyGroup, _groupFields);
            if (movesetGroup.Config == null || modifyGroup.Config == null)
            {
                Plugin.Log.LogError("Cannot copy the Config Fields to the configs on the groups due to one of the configs being null");
                return;
            }
            
            ReflectionUtils.CopyFields(movesetGroup.Config, modifyGroup.Config, _configFields);
            
            // Emulate the Setup() method on the config group, but only for down slash / down spike and with the desired crest
            switch (movesetGroup.Config.DownSlashType)
            {
                case HeroControllerConfig.DownSlashTypes.DownSpike:
                    if ((bool)(UnityEngine.Object) movesetGroup.DownSlashObject)
                        _cg_downspike.SetValue(modifyGroup, GetOrAddComponent<Downspike>(movesetGroup.DownSlashObject));
                    if ((bool)(UnityEngine.Object) movesetGroup.AltDownSlashObject)
                        _cg_altDownspike.SetValue(modifyGroup, GetOrAddComponent<Downspike>(movesetGroup.AltDownSlashObject));
                    
                    goto case HeroControllerConfig.DownSlashTypes.Custom;
                case HeroControllerConfig.DownSlashTypes.Slash:
                    if ((bool)(UnityEngine.Object) movesetGroup.DownSlashObject)
                        _cg_downSlash.SetValue(modifyGroup, GetOrAddComponent<NailSlash>(movesetGroup.DownSlashObject));
                    if ((bool)(UnityEngine.Object) movesetGroup.AltDownSlashObject)
                        _cg_altDownSlash.SetValue(modifyGroup, GetOrAddComponent<NailSlash>(movesetGroup.AltDownSlashObject));
            
                    goto case HeroControllerConfig.DownSlashTypes.Custom;
                case HeroControllerConfig.DownSlashTypes.Custom:
                    if ((bool)(UnityEngine.Object) movesetGroup.DownSlashObject)
                        _cg_downSlashDamager.SetValue(modifyGroup, GetOrAddComponent<DamageEnemies>(movesetGroup.DownSlashObject));
                    if ((bool)(UnityEngine.Object) movesetGroup.AltDownSlashObject)
                        _cg_altDownSlashDamager.SetValue(modifyGroup, GetOrAddComponent<DamageEnemies>(movesetGroup.AltDownSlashObject));
                    break;
                default:
                    break;
            }
        }
        
        private static T GetOrAddComponent<T>(GameObject gameObj)
            where T : Component
        {
            var component = gameObj.GetComponent<T>();
            component ??= gameObj.AddComponent<T>();
            return component;
        }
    }
}
