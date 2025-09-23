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
        public static bool ShouldRevertPatchWhenAble = false;
        
        private static bool HasPatchedDownAir;
        private static HeroControllerConfig? BackupConfig;
        private static HeroController.ConfigGroup? BackupConfigGroup;
        private static HeroController.ConfigGroup? SwapCrestConfigGroup;
        
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

        public static void RevertDownAttackPatch(HeroController heroController)
        {
            HasPatchedDownAir = false;
            ShouldRevertPatchWhenAble = false;
            
            if (SwapCrestConfigGroup != null && (bool) (UnityEngine.Object) SwapCrestConfigGroup.ActiveRoot)
                SwapCrestConfigGroup.ActiveRoot.SetActive(false);

            if (BackupConfigGroup != null && BackupConfigGroup.Config != null)
            {
                var allConfigGroups = _hc_configs.GetValue(heroController) as HeroController.ConfigGroup[] ?? Array.Empty<HeroController.ConfigGroup>();
                var beastConfigGroup = allConfigGroups.ForCrest(CrestType.Beast);
                OverwriteDownAttackMoveset(BackupConfigGroup, beastConfigGroup);
            }
            
            Plugin.Log.LogInfo("Down Attack Patch disabled.");
        }

        public static void PatchDownAttackIfNecessary(HeroController heroController)
        {
            if (ShouldRevertPatchWhenAble)
                RevertDownAttackPatch(heroController);
            
            if (HasPatchedDownAir)
                return;

            Plugin.Log.LogInfo("Attempting to modify the Beast Crest Down Attack.");
            HasPatchedDownAir = true;
            
            var allConfigGroups = _hc_configs.GetValue(heroController) as HeroController.ConfigGroup[] ?? Array.Empty<HeroController.ConfigGroup>();

            var otherConfigGroup = allConfigGroups.ForCrest(Plugin.Config.DownAttackType);
            if (otherConfigGroup == null)
            {
                Plugin.Log.LogError($"Unable to find crest config group for down attack type: {Plugin.Config.DownAttackType}.");
                return;
            }
            SwapCrestConfigGroup = otherConfigGroup;

            var beastConfigGroup = allConfigGroups.ForCrest(CrestType.Beast);
            if (beastConfigGroup == null)
            {
                Plugin.Log.LogError("No normal config group found for Beast Crest.");
                return;
            }
            
            BackupBeastCrestDefaultsIfNecessary(beastConfigGroup);
            otherConfigGroup.ActiveRoot.SetActive(true);    // Forcefully load the prefabs (I guess? Good god I have no fucking idea what I am doing)
            OverwriteDownAttackMoveset(otherConfigGroup, beastConfigGroup);

            // Cause the HeroController to reprocess everything about the config
            _hc_updateConfig.Invoke(heroController, new object [] { });
            
            Plugin.Log.LogInfo($"Beast Crest down attack overridden with {Plugin.Config.DownAttackType} down attack.");
        }

        private static void BackupBeastCrestDefaultsIfNecessary(HeroController.ConfigGroup beastConfigGroup)
        {
            if (BackupConfig != null && BackupConfigGroup != null)
                return;

            BackupConfig = ScriptableObject.CreateInstance<HeroControllerConfig>();
            BackupConfigGroup = new HeroController.ConfigGroup { Config = BackupConfig };
            _heroAnimOverrideLibField.SetValue(BackupConfig, _heroAnimOverrideLibField.GetValue(beastConfigGroup.Config));
            CopyAndInitializeConfigGroup(beastConfigGroup, BackupConfigGroup);
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
