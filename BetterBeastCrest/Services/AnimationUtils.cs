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

        // Clips that are allowed to overwrite
        private static readonly HashSet<string> WhitelistClips = new HashSet<string>
        {
            "V3 Down Slash Antic",  // Reaper
            "V3 Down Slash",        // Reaper
            "DownSlash",            // Wanderer
            "Downspike Charge",     // Architect
            "DownSpike Charge"      // Architect
        };

        public static void MergeDownAttackClips(tk2dSpriteAnimation newAnimsLib, tk2dSpriteAnimation targetAnimsLib)
        {
            if (targetAnimsLib == null || newAnimsLib == null)
                return;

            foreach (var clip in newAnimsLib.clips)
            {
                if (clip == null || string.IsNullOrEmpty(clip.name))
                    continue;

                var existingIndex = Array.FindIndex(targetAnimsLib.clips, c => c != null && c.name == clip.name);
                if (existingIndex >= 0)
                {
                    if (WhitelistClips.Contains(clip.name))
                    {
                        targetAnimsLib.clips[existingIndex] = clip;
                        Plugin.LogInfo($"Overwrote whitelisted animation clip: {clip.name}");
                    }
                    else
                    {
                        Plugin.LogInfo($"Skipped existing non-whitelisted clip: {clip.name}");
                    }
                }
                else
                {
                    // Append new clip
                    var newArray = new tk2dSpriteAnimationClip[targetAnimsLib.clips.Length + 1];
                    targetAnimsLib.clips.CopyTo(newArray, 0);
                    newArray[^1] = clip;
                    targetAnimsLib.clips = newArray;
                    Plugin.LogInfo($"Added new animation clip: {clip.name}");
                }
            }

            ReloadAnimationLookup(targetAnimsLib);
        }

        private static void ReloadAnimationLookup(tk2dSpriteAnimation targetAnimsLib)
        {
            if (targetAnimsLib == null || _lookupField == null || _animationInfoType == null)
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
