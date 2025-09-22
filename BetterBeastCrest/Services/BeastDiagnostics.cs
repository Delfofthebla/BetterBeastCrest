using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace BetterBeastCrest.Services
{
    public static class BeastDiagnostics
    {
        private static readonly BindingFlags PF = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static void LogDownAttackWiring(string patchState, HeroController hero, HeroController.ConfigGroup? group, string configName)
        {
            try
            {
                Plugin.Log.LogInfo("=== DownAttack wiring diagnostics ===");
                Plugin.Log.LogInfo($"=== Patch State For Log: {patchState} ===");

                if (hero == null)
                {
                    Plugin.Log.LogError("HeroController is null");
                    return;
                }

                Plugin.Log.LogInfo($"Hero object: {hero.gameObject.name} (activeInHierarchy={hero.gameObject.activeInHierarchy})");

                // Reflectively read hero private fields we patched earlier
                var fDown = AccessTools.Field(typeof(HeroController), "downSlash");
                var fAltDown = AccessTools.Field(typeof(HeroController), "altDownSlash");
                var fDownDam = AccessTools.Field(typeof(HeroController), "downSlashDamager");
                var fAltDownDam = AccessTools.Field(typeof(HeroController), "altDownSlashDamager");
                var fSlashComp = AccessTools.Field(typeof(HeroController), "_slashComponent");

                var heroDown = fDown?.GetValue(hero);
                var heroAltDown = fAltDown?.GetValue(hero);
                var heroDownDam = fDownDam?.GetValue(hero);
                var heroAltDownDam = fAltDownDam?.GetValue(hero);

                Plugin.Log.LogInfo($"hero.downSlash = {(heroDown != null ? heroDown.GetType().Name + "@" + heroDown.GetHashCode() : "null")}");
                Plugin.Log.LogInfo($"hero.altDownSlash = {(heroAltDown != null ? heroAltDown.GetType().Name + "@" + heroAltDown.GetHashCode() : "null")}");
                Plugin.Log.LogInfo($"hero.downSlashDamager = {(heroDownDam != null ? heroDownDam.GetType().Name + "@" + heroDownDam.GetHashCode() : "null")}");
                Plugin.Log.LogInfo($"hero.altDownSlashDamager = {(heroAltDownDam != null ? heroAltDownDam.GetType().Name + "@" + heroAltDownDam.GetHashCode() : "null")}");

                if (group == null)
                {
                    Plugin.Log.LogWarning("group is null or was not provided.");
                    return;
                }

                Plugin.Log.LogInfo($"Group.Config: {configName}");
                Plugin.Log.LogInfo($"Group.DownSlashObject: {(group.DownSlashObject != null ? group.DownSlashObject.name : "null")}");
                Plugin.Log.LogInfo($"Group.AltDownSlashObject: {(group.AltDownSlashObject != null ? group.AltDownSlashObject.name : "null")}");
                Plugin.Log.LogInfo($"Group.ActiveRoot: {(group.ActiveRoot != null ? group.ActiveRoot.name + " (active=" + group.ActiveRoot.activeInHierarchy + ")" : "null")}");

                // The Setup() should have populated these properties
                var downSlash = group.DownSlash;
                var altDownSlash = group.AltDownSlash;
                var downDamager = group.DownSlashDamager;
                var altDownDamager = group.AltDownSlashDamager;

                Plugin.Log.LogInfo($"group.DownSlash (NailSlash) = {(downSlash != null ? "present" : "null")}");
                Plugin.Log.LogInfo($"group.AltDownSlash (NailSlash) = {(altDownSlash != null ? "present" : "null")}");
                Plugin.Log.LogInfo($"group.DownSlashDamager = {(downDamager != null ? "present" : "null")}");
                Plugin.Log.LogInfo($"group.AltDownSlashDamager = {(altDownDamager != null ? "present" : "null")}");

                void DumpNs(object? o)
                {
                    if (o == null)
                    {
                        Plugin.Log.LogInfo("  <null>");
                        return;
                    }

                    var ns = o as NailSlash;
                    if (ns == null)
                    {
                        Plugin.Log.LogInfo("  not a NailSlash instance, type=" + o.GetType().FullName);
                        return;
                    }

                    Plugin.Log.LogInfo("  NailSlash.gameObject = " + ns.gameObject.name + " (active=" + ns.gameObject.activeInHierarchy + ")");

                    // use reflection to reach private fields if needed:
                    var animField = typeof(NailSlash).GetField("anim", PF);
                    tk2dSpriteAnimator? anim = animField?.GetValue(ns) as tk2dSpriteAnimator;
                    var animNameField = typeof(NailSlash).GetField("animName", PF) ?? typeof(NailSlash).GetField("animName", BindingFlags.Public | BindingFlags.Instance);
                    string? animName = animNameField?.GetValue(ns) as string;
                    Plugin.Log.LogInfo($"    animName = {animName ?? "<null>"}");
                    if (anim == null)
                    {
                        // try component lookup as last resort
                        anim = ns.GetComponent<tk2dSpriteAnimator>();
                    }

                    Plugin.Log.LogInfo($"    tk2dSpriteAnimator = {(anim != null ? anim.name + " (Library=" + (anim.Library != null ? anim.Library.name : "null") + ")" : "null")}");
                    if (anim != null && !string.IsNullOrEmpty(animName))
                    {
                        var clip = anim.GetClipByName(animName);
                        Plugin.Log.LogInfo($"    animator.GetClipByName(animName) => {(clip != null ? "FOUND clip '" + clip.name + "'" : "null (missing)")}");
                    }

                    // collider / mesh / damager
                    var polyField = typeof(NailSlash).GetField("poly", PF);
                    var meshField = typeof(NailSlash).GetField("mesh", PF);
                    var damagerField = typeof(NailSlash).GetField("enemyDamager", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy) ?? typeof(NailAttackBase).GetField("enemyDamager", PF);
                    var poly = polyField?.GetValue(ns); // keep as object or cast to the actual type
                    var mesh = meshField?.GetValue(ns) as Renderer;
                    var damager = damagerField?.GetValue(ns) as DamageEnemies;

                    Plugin.Log.LogInfo($"    poly present: {(poly != null ? "yes" : "no")}");
                    Plugin.Log.LogInfo($"    mesh present: {(mesh != null ? mesh.name : "no")}");
                    Plugin.Log.LogInfo($"    enemyDamager present: {(damager != null ? damager.name : "no")}");
                    if (damager != null)
                    {
                        // print a few properties
                        var manualTriggerField = damager.GetType().GetField("manualTrigger", PF);
                        if (manualTriggerField != null)
                        {
                            var manual = manualTriggerField.GetValue(damager);
                            Plugin.Log.LogInfo($"      enemyDamager.manualTrigger = {manual}");
                        }

                        // print whether damager will currently start
                    }
                }

                Plugin.Log.LogInfo("--- group.DownSlash details ---");
                DumpNs(downSlash);

                Plugin.Log.LogInfo("--- group.AltDownSlash details ---");
                DumpNs(altDownSlash);

                Plugin.Log.LogInfo("--- hero object equality checks ---");
                // check that hero fields point to these same instances
                if (downSlash != null && heroDown != null)
                    Plugin.Log.LogInfo($"hero.downSlash == group.DownSlash ? {ReferenceEquals(heroDown, downSlash)}");
                if (downDamager != null && heroDownDam != null)
                    Plugin.Log.LogInfo($"hero.downSlashDamager == group.DownSlashDamager ? {ReferenceEquals(heroDownDam, downDamager)}");

                Plugin.Log.LogInfo("=== end diagnostics ===");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Diagnostics failed: " + ex);
            }
        }
    }
}
