using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace BetterBeastCrest.Services
{
    public static class AnimationUtils
    {
        private static readonly FieldInfo _isValidField = AccessTools.Field(typeof(tk2dSpriteAnimation), "isValid");
        private static readonly FieldInfo _lookupField = AccessTools.Field(typeof(tk2dSpriteAnimation), "lookup");
        private static readonly Type _animationInfoType = typeof(tk2dSpriteAnimation).GetNestedType("AnimationInfo", BindingFlags.NonPublic);

        // Clips that are allowed to overwrite. Incomplete and I decided on an easier approach but I'm leaving it here in case I run into issues.
        private static readonly HashSet<string> DownAttackClips = new HashSet<string>
        {
            "DownSlash",            // Everybody?
            "DownSlashAlt",         // Wanderer
            "DownSlashEffect",      // Everybody?
            
            "DownSpike Antic Old",  // Reaper
            "DownSpike Old",        // Reaper
            "DownSpike Recovery",   // Reaper, Shaman
            "DownSlashEffect Old",  // Reaper
            "DownSpike Antic",      // Reaper
            "DownSpike",            // Reaper
            "V3 Down Slash Antic",  // Reaper
            "V3 Down Slash",        // Reaper
            "V3 Down Slash Effect", // Reaper
            
            "Downspike Charge",     // Architect
            "DownSpike Charge"      // Architect
        };

        private static readonly HashSet<string> ExtraClips = new HashSet<string>
        {
            "Drill Grind Charged",
        };

        public static void MergeDownAttackClips(tk2dSpriteAnimation? newAnimsLib, tk2dSpriteAnimation? targetAnimsLib)
        {
            if (targetAnimsLib == null || newAnimsLib == null)
                return;

            foreach (var clip in newAnimsLib.clips)
            {
                if (clip == null || string.IsNullOrEmpty(clip.name))
                    continue;

                if (!clip.name.Contains("down", StringComparison.InvariantCultureIgnoreCase) && !ExtraClips.Contains(clip.name))
                {
                    //Plugin.Log.LogInfo($"Skipped non-Down Attack clip: {clip.name}");
                    continue;
                }

                var existingIndex = Array.FindIndex(targetAnimsLib.clips, c => c != null && c.name == clip.name);
                if (existingIndex >= 0)
                {
                    targetAnimsLib.clips[existingIndex] = clip;
                    Plugin.Log.LogInfo($"Overwrote whitelisted animation clip: {clip.name}");
                }
                else
                {
                    // Append new clip
                    var newArray = new tk2dSpriteAnimationClip[targetAnimsLib.clips.Length + 1];
                    targetAnimsLib.clips.CopyTo(newArray, 0);
                    newArray[^1] = clip;
                    targetAnimsLib.clips = newArray;
                    Plugin.Log.LogInfo($"Added new animation clip: {clip.name}");
                }
            }

            ReloadAnimationLookup(targetAnimsLib);
        }

        private static void ReloadAnimationLookup(tk2dSpriteAnimation targetAnimsLib)
        {
            if (targetAnimsLib == null)
                return;

            // Create a new empty dictionary<string, AnimationInfo>
            var dictType = typeof(Dictionary<,>).MakeGenericType(typeof(string), _animationInfoType);
            var newLookup = Activator.CreateInstance(dictType);

            // Assign and reset
            _lookupField.SetValue(targetAnimsLib, newLookup);
            _isValidField.SetValue(targetAnimsLib, false);
            targetAnimsLib.ValidateLookup();
        }
    }
}
