using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TeamCherry.Localization;

namespace BetterBeastCrest.Services
{
public static class LocalizationInjector
{
    private static readonly FieldInfo CurrentEntrySheetsField = AccessTools.Field(typeof(Language), "_currentEntrySheets");
    private static Dictionary<string, Dictionary<string, string>> CurrentEntrySheets => (Dictionary<string, Dictionary<string, string>>)CurrentEntrySheetsField.GetValue(null);
    private static readonly Dictionary<(string Sheet, string Key), string> OriginalTexts = new Dictionary<(string, string), string>();

    public static void Append(LocalisedString original, string appendix)
    {
        if (string.IsNullOrEmpty(original.Sheet) || string.IsNullOrEmpty(original.Key))
            throw new ArgumentException("Original LocalisedString is not valid");

        var baseText = Language.Get(original.Key, original.Sheet);

        // Cache original if we haven't yet, revert if we have.
        var key = (original.Sheet, original.Key);
        if (!OriginalTexts.TryAdd(key, baseText))
            Revert(original);

        var newText = baseText + appendix;

        if (!CurrentEntrySheets.ContainsKey(original.Sheet))
            CurrentEntrySheets[original.Sheet] = new Dictionary<string, string>();

        CurrentEntrySheets[original.Sheet][original.Key] = newText;
    }

    public static void Revert(LocalisedString original)
    {
        var key = (original.Sheet, original.Key);
        if (OriginalTexts.TryGetValue(key, out var baseText))
        {
            if (!CurrentEntrySheets.ContainsKey(original.Sheet))
                CurrentEntrySheets[original.Sheet] = new Dictionary<string, string>();

            CurrentEntrySheets[original.Sheet][original.Key] = baseText;
        }
    }

    public static void RevertAll()
    {
        foreach (var kvp in OriginalTexts)
        {
            var (sheet, key) = kvp.Key;
            var baseText = kvp.Value;

            if (!CurrentEntrySheets.ContainsKey(sheet))
                CurrentEntrySheets[sheet] = new Dictionary<string, string>();

            CurrentEntrySheets[sheet][key] = baseText;
        }
    }
}

}
