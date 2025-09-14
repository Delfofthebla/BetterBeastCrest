using HarmonyLib;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(ToolItemManager), "Awake")]
    public class Patch_IncludeNewCrestsInToolManager
    {
        private static void Postfix(ToolItemManager __instance)
        {
            // Get private field "crestList"
            var crestListField = AccessTools.Field(typeof(ToolItemManager), "crestList");
            var crestList = crestListField.GetValue(__instance) as ToolCrestList;
            if (crestList == null)
            {
                Plugin.Log.LogError("[BetterBeastCrest]: Could not access ToolItemManager.crestList!");
                return;
            }
            
            if (Plugin.WarriorCrest2 == null || Plugin.WarriorCrest3 == null)
            {
                Plugin.Log.LogWarning("[BetterBeastCrest]: Beast crests not initialized yet. Delaying injection.");
                return;
            }

            // Add our custom crests if they’re not already present
            TryAddCrest(crestList, Plugin.WarriorCrest2);
            TryAddCrest(crestList, Plugin.WarriorCrest3);

            Plugin.Log.LogInfo("[BetterBeastCrest]: Injected warrior crest upgrades into ToolItemManager.crestList.");
        }

        private static void TryAddCrest(ToolCrestList list, ToolCrest crest)
        {
            if (crest == null)
                return;

            // Avoid duplicates
            if (list.Contains(crest))
                return;

            list.Add(crest);
        }
    }
}
