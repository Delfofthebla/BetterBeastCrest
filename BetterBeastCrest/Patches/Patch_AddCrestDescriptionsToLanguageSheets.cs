// using HarmonyLib;
//
// namespace BetterBeastCrest.Patches
// {
//     [HarmonyPatch(typeof(GameManager), "Awake")]
//     public class Patch_AddCrestDescriptionsToLanguageSheets
//     {
//         public static void PostFix(GameManager __instance)
//         {
//             Plugin.Log.LogInfo("GameManager is awake, now safe to set up BetterBeastCrest");
//             
//             LocalizationInjector.Inject("BetterBeastCrest", "WarriorCrest2_Desc", "Upgraded Beast Crest: Now holds more power.");
//             LocalizationInjector.Inject("BetterBeastCrest", "WarriorCrest3_Desc", "Final form of the Beast Crest.");
//
//
//             
//             LanguageInjector.AddEntry("Crests", "WarriorCrest2_Description", "A stronger beastly form with stronger healing and a new tool slot.");
//             LanguageInjector.AddEntry("Crests", "WarriorCrest3_Description", "The beast crest reaches its peak, unlocking further power.");
//         }
//     }
// }
