using System;
using System.Linq;
using System.Reflection;
using GlobalSettings;
using HarmonyLib;
using TeamCherry.Localization;
using UnityEngine;

namespace BetterBeastCrest.Patches
{
    [HarmonyPatch(typeof(Gameplay), "PreWarm")]
    public class Patch_AddUpgradeCrests
    {
        private static readonly FieldInfo descriptionField = AccessTools.Field(typeof(ToolCrest), "description");
        private static readonly FieldInfo slotsField = AccessTools.Field(typeof(ToolCrest), "slots");
        private static readonly FieldInfo oldPreviousVersionField = AccessTools.Field(typeof(ToolCrest), "oldPreviousVersion");
        private static readonly FieldInfo previousVersionField = AccessTools.Field(typeof(ToolCrest), "previousVersion");
        private static readonly FieldInfo upgradedVersionField  = AccessTools.Field(typeof(ToolCrest), "upgradedVersion");
        private static readonly MethodInfo onValidateMethod = AccessTools.Method(typeof(ToolCrest), "OnValidate");
        
        private static void Postfix(Gameplay __instance)
        {
            Plugin.Log.LogInfo("[BetterBeastCrest]: Preparing to create warrior crest upgrades");

            Plugin.WarriorCrest1 = Gameplay.WarriorCrest;
            
            foreach (var warriorSlot in Plugin.WarriorCrest1.Slots)
                Plugin.Log.LogInfo("Slot: " + warriorSlot.Type + ", At Position: " + warriorSlot.Position + ", is locked: " + warriorSlot.IsLocked);

            CreateWarrior2(Plugin.WarriorCrest1);
            CreateWarrior3(Plugin.WarriorCrest2);
            
            // Sync Progression Chain - warriorCrest1
            upgradedVersionField.SetValue(Plugin.WarriorCrest1, Plugin.WarriorCrest2);
            
            // Sync Progression Chain - warriorCrest2
            oldPreviousVersionField.SetValue(Plugin.WarriorCrest2, Plugin.WarriorCrest1);
            previousVersionField.SetValue(Plugin.WarriorCrest2, Plugin.WarriorCrest1);
            upgradedVersionField.SetValue(Plugin.WarriorCrest2, Plugin.WarriorCrest3);
            
            // Sync Progression Chain - warriorCrest3
            oldPreviousVersionField.SetValue(Plugin.WarriorCrest3, Plugin.WarriorCrest2);
            previousVersionField.SetValue(Plugin.WarriorCrest3, Plugin.WarriorCrest2);
            
            DebugCrestChain(Gameplay.HunterCrest);
            DebugCrestChain(Plugin.WarriorCrest1);
        }

        private static void CreateWarrior2(ToolCrest warrior1)
        {
            var warrior2 = UnityEngine.Object.Instantiate(warrior1);
            warrior2.name = "warriorCrest2";
            descriptionField.SetValue(warrior2, LocalizationInjector.Append(
                warrior1.Description,
                "WarriorCrest2_ModDesc",
                "Binding restores more health and your tools have expanded."
            ));
            slotsField.SetValue(warrior2, AddExtraBlueSlot(warrior1.Slots, 1));
            
            onValidateMethod.Invoke(warrior2, null);
            
            Plugin.WarriorCrest2 = warrior2;
            Plugin.Log.LogInfo("[BetterBeastCrest]: Instantiated warriorCrest2 (Beast Crest 2)");
        }

        private static void CreateWarrior3(ToolCrest warrior2)
        {
            var warrior3 = UnityEngine.Object.Instantiate(warrior2);
            warrior3.name = "warriorCrest3";
            descriptionField.SetValue(warrior3, LocalizationInjector.Append(
                Plugin.WarriorCrest1.Description,
                "WarriorCrest2_ModDesc",
                "Binding restores more health and your tools have expanded."
            ));
            onValidateMethod.Invoke(warrior3, null);
            
            Plugin.WarriorCrest3 = warrior3;
            Plugin.Log.LogInfo("[BetterBeastCrest]: Instantiated warriorCrest3 (Beast Crest 3)");
        }

        private static ToolCrest.SlotInfo[] AddExtraBlueSlot(ToolCrest.SlotInfo[] original, int extra)
        {
            var newArray = new ToolCrest.SlotInfo[original.Length + extra];
            Array.Copy(original, newArray, original.Length);

            // Find the topmost slot to base our position on
            var topSlot = original
                .OrderByDescending(s => s.Position.y)  // assuming Position is Vector2
                .First();

            for (var i = 0; i < extra; i++)
            {
                var slot = new ToolCrest.SlotInfo
                {
                    Type = ToolItemType.Blue,
                    IsLocked = true,
                    Position = new Vector2(topSlot.Position.x, 3.39f)
                };
                newArray[original.Length + i] = slot;
            }

            return newArray;
        }

        private static void DebugCrestChain(ToolCrest baseCrest)
        {
            var current = baseCrest;
            int index = 1;

            while (current != null)
            {
                var oldPrev = previousVersionField.GetValue(current) as ToolCrest;
                var prev = previousVersionField.GetValue(current) as ToolCrest;
                var upg  = upgradedVersionField.GetValue(current) as ToolCrest;

                Plugin.Log.LogInfo(
                    $"Crest {index}: " +
                    $"Name={current.name}, " +
                    $"OldPrev={(oldPrev ? oldPrev.name : "null")}, " +
                    $"Prev={(prev ? prev.name : "null")}, " +
                    $"Upg={(upg ? upg.name : "null")}, " +
                    $"IsBase={current.IsBaseVersion}, " +
                    $"IsUnlocked={current.IsUnlocked}, " +
                    $"IsUpgradedUnlocked={current.IsUpgradedVersionUnlocked}"
                );

                current = upg;
                index++;
            }
        }
    }
}
