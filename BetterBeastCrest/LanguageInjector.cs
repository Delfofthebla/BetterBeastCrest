using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TeamCherry.Localization;

namespace BetterBeastCrest
{
    public static class LocalizationInjector
    {
        private static readonly FieldInfo CurrentEntrySheetsField = AccessTools.Field(typeof(Language), "_currentEntrySheets");

        private static Dictionary<string, Dictionary<string, string>> CurrentEntrySheets => (Dictionary<string, Dictionary<string, string>>)CurrentEntrySheetsField.GetValue(null);

        public static void Inject(string sheet, string key, string value)
        {
            var sheets = CurrentEntrySheets;
            if (!sheets.ContainsKey(sheet))
                sheets[sheet] = new Dictionary<string, string>();

            sheets[sheet][key] = value;
        }

        public static LocalisedString Append(LocalisedString original, string newKey, string appendix)
        {
            if (string.IsNullOrEmpty(original.Sheet) || string.IsNullOrEmpty(original.Key))
                throw new ArgumentException("Original LocalisedString is not valid");

            var baseText = Language.Get(original.Key, original.Sheet);
            var newText = baseText + "\n\n" + appendix;

            // Inject into the current language dictionary
            var sheets = CurrentEntrySheets;
            if (!sheets.ContainsKey(original.Sheet))
                sheets[original.Sheet] = new Dictionary<string, string>();

            sheets[original.Sheet][newKey] = newText;

            // Return a new LocalisedString pointing to our injected entry
            return new LocalisedString(original.Sheet, newKey);
        }
    }

}
