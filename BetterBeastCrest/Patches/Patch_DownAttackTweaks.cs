using System;
using System.Linq;
using System.Reflection;
using BetterBeastCrest.Domain;
using BetterBeastCrest.Extensions;
using BetterBeastCrest.Services;
using GlobalSettings;
using HarmonyLib;
using UnityEngine;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch]
    public static class Patch_DownAttackTweaks
    {
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "DownAttack")]
        public static void DownAttackPrefix(HeroController __instance, ref bool isSlashing)
        {
            if (!Gameplay.WarriorCrest.IsEquipped || Plugin.HasPatchedDownAir)
                return;

            Plugin.HasPatchedDownAir = true;
            
            var allConfigGroups = _hc_configs.GetValue(__instance) as HeroController.ConfigGroup[] ?? Array.Empty<HeroController.ConfigGroup>();

            var otherConfigGroup = allConfigGroups.ForCrest(Plugin.Config.DownAttackType);
            if (otherConfigGroup == null)
            {
                Plugin.Log.LogError($"Unable to find crest config group for down attack type: {Plugin.Config.DownAttackType}.");
                return;
            }

            var beastConfigGroup = allConfigGroups.ForCrest(CrestType.Beast);
            if (beastConfigGroup == null)
            {
                Plugin.Log.LogError("No normal config group found for Beast Crest.");
                return;
            }
            
            //BeastDiagnostics.LogDownAttackWiring("Prior to any HeroController or ConfigGroup Changes", __instance, null, "N/A");
            
            otherConfigGroup.ActiveRoot.SetActive(true);    // Forcefully load the prefabs (I guess? Good god I have no fucking idea what I am doing)
            
            // Hackily merge the down attack properties, values, and animations from the otherConfig
            CopyAndInitializeConfigGroup(otherConfigGroup, beastConfigGroup);
            AnimationUtils.MergeDownAttackClips(
                _heroAnimOverrideLibField.GetValue(otherConfigGroup.Config) as tk2dSpriteAnimation,
                _heroAnimOverrideLibField.GetValue(beastConfigGroup.Config) as tk2dSpriteAnimation
            );
            
            // Cause the HeroController to reprocess everything about the config
            _hc_updateConfig.Invoke(__instance, new object [] { });
            
            //BeastDiagnostics.LogDownAttackWiring("Post ConfigGroup Changes", __instance, otherConfigGroup, Plugin.Config.DownAttackType.ToString());
            //BeastDiagnostics.LogDownAttackWiring("Post ConfigGroup Changes", __instance, beastConfigGroup, "Beast");
            
            Plugin.Log.LogInfo($"Beast Crest down attack overridden with {Plugin.Config.DownAttackType} down attack.");
        }

        private static void CopyAndInitializeConfigGroup(HeroController.ConfigGroup? otherGroup, HeroController.ConfigGroup? beastGroup)
        {
            if (otherGroup == null || beastGroup == null)
                return;

            ReflectionUtils.CopyFields(otherGroup, beastGroup, _groupFields);
            if (otherGroup.Config == null || beastGroup.Config == null)
            {
                Plugin.Log.LogError("Cannot copy the Config Fields to the configs on the groups due to one of the configs being null");
                return;
            }
            
            ReflectionUtils.CopyFields(otherGroup.Config, beastGroup.Config, _configFields);
            
            // Emulate the Setup() method on the config group, but only for down slash / down spike and with the desired crest
            switch (otherGroup.Config.DownSlashType)
            {
                case HeroControllerConfig.DownSlashTypes.DownSpike:
                    if ((bool)(UnityEngine.Object) otherGroup.DownSlashObject)
                        _cg_downspike.SetValue(beastGroup, GetOrAddComponent<Downspike>(otherGroup.DownSlashObject));
                    if ((bool)(UnityEngine.Object) otherGroup.AltDownSlashObject)
                        _cg_altDownspike.SetValue(beastGroup, GetOrAddComponent<Downspike>(otherGroup.AltDownSlashObject));
                    
                    goto case HeroControllerConfig.DownSlashTypes.Custom;
                case HeroControllerConfig.DownSlashTypes.Slash:
                    if ((bool)(UnityEngine.Object) otherGroup.DownSlashObject)
                        _cg_downSlash.SetValue(beastGroup, GetOrAddComponent<NailSlash>(otherGroup.DownSlashObject));
                    if ((bool)(UnityEngine.Object) otherGroup.AltDownSlashObject)
                        _cg_altDownSlash.SetValue(beastGroup, GetOrAddComponent<NailSlash>(otherGroup.AltDownSlashObject));
            
                    goto case HeroControllerConfig.DownSlashTypes.Custom;
                case HeroControllerConfig.DownSlashTypes.Custom:
                    if ((bool)(UnityEngine.Object) otherGroup.DownSlashObject)
                        _cg_downSlashDamager.SetValue(beastGroup, GetOrAddComponent<DamageEnemies>(otherGroup.DownSlashObject));
                    if ((bool)(UnityEngine.Object) otherGroup.AltDownSlashObject)
                        _cg_altDownSlashDamager.SetValue(beastGroup, GetOrAddComponent<DamageEnemies>(otherGroup.AltDownSlashObject));
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
