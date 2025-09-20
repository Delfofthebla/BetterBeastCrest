// using System.Reflection;
// using BetterBeastCrest.Domain;
// using BetterBeastCrest.Services;
// using GlobalSettings;
// using HarmonyLib;
//
// namespace BetterBeastCrest
// {
//     [HarmonyPatch(typeof(HeroController), "DownAttack")]
//     public static class Patch_DownAttack
//     {
//         private static readonly FieldInfo _heroAnimOverrideLibField = AccessTools.Field(typeof(HeroControllerConfig), "heroAnimOverrideLib");
//
//         [HarmonyPrefix]
//         public static bool Prefix(HeroController __instance, ref bool isSlashing)
//         {
//             // Copy over the down attack animations without nuking the rest
//             var sourceLib = __instance.Config.GetType().GetField("heroAnimOverrideLib", AccessTools.all)?.GetValue(__instance.Config) as tk2dSpriteAnimation;
//             foreach (var clip in sourceLib.clips)
//             {
//                 if (clip != null)
//                     Plugin.LogInfo($"SourceLib clip: {clip.name} (frames: {clip.frames.Length})");
//             }
//
//             return isSlashing;
//         }
//     }
// }
