using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;

namespace BetterBeastCrest.Domain
{
    public class ModConfig
    {
        private const int CURRENT_CONFIG_VERSION = 2;
        private ConfigEntry<int> _configVersion;
        
        private readonly ConfigFile _config;

        // Global settings
        private ConfigEntry<CrestType> _downAttackCrestType;
        
        private ConfigEntry<ToolItemType> _centerToolSlotColor;
        
        private ConfigEntry<int> _topLeftToolSlotRequiredRank;
        private ConfigEntry<int> _topRightToolSlotRequiredRank;

        private ConfigEntry<bool> _topLeftToolSlotRequiresUnlocking;
        private ConfigEntry<bool> _topRightToolSlotRequiresUnlocking;

        private ConfigEntry<ToolItemType> _topLeftToolSlotColor;
        private ConfigEntry<ToolItemType> _topRightToolSlotColor;

        // Healing / Rage
        private ConfigEntry<int> _immediateHealthOnBind1;
        private ConfigEntry<int> _maximumLifeLeech1;
        private ConfigEntry<int> _rageDurationIncrease1;

        private ConfigEntry<int> _immediateHealthOnBind2;
        private ConfigEntry<int> _maximumLifeLeech2;
        private ConfigEntry<int> _rageDurationIncrease2;

        private ConfigEntry<int> _immediateHealthOnBind3;
        private ConfigEntry<int> _maximumLifeLeech3;
        private ConfigEntry<int> _rageDurationIncrease3;

        public ToolItemType CenterToolSlotColor => _centerToolSlotColor.Value;
        public CrestType DownAttackType => _downAttackCrestType.Value;

        public CrestStats CrestDefault { get; } = new CrestStats(0, 3, 0);
        public CrestStats Crest1 { get; private set; }
        public CrestStats Crest2 { get; private set; }
        public CrestStats Crest3 { get; private set; }

        public ModConfig()
        {
            var configPath = Path.Combine(Paths.ConfigPath, "BetterBeastCrest.cfg");
            var isNewConfig = !File.Exists(configPath);
            
            _config = new ConfigFile(configPath, true);
            LoadConfig();
            
            if (!isNewConfig)
                MigrateIfNeeded();
            else
            {
                Plugin.Log.LogInfo("Detected No Config, Generating new one.");
                _configVersion = _config.Bind("Global", "ConfigVersion", CURRENT_CONFIG_VERSION, "Internal config version for migration purposes");
            }
            
            BuildCrestStats();
        }

        public void ReloadConfig()
        {
            _config.Reload();
            LoadConfig();
            BuildCrestStats();
            Plugin.Log.LogInfo("Config reloaded and CrestStats rebuilt.");
        }

        private void LoadConfig()
        {
            _centerToolSlotColor = _config.Bind("Global", "CenterToolSlotColor", ToolItemType.Skill, "Specify the type of the center most tool slot. It is Skill in the base game, but you can change that here.");
            _downAttackCrestType = _config.Bind("Global", "DownAttackType", CrestType.Beast, "Specify the down attack to be used with the beast crest.");
            
            _topLeftToolSlotRequiredRank = _config.Bind("Global", "TopLeftToolSlot_RequiredRank", 2, "Rank at which the top-left tool slot is available (-1 to disable).");
            _topLeftToolSlotRequiresUnlocking = _config.Bind("Global", "TopLeftToolSlot_RequiresUnlocking", true, "Whether or not to require spending a memory locket to unluck this slot.");
            _topLeftToolSlotColor = _config.Bind("Global", "TopLeftToolSlot_Color", ToolItemType.Blue, "The tool slot color for this rank. ==Only Blue and Yellow are supported==");
            
            _topRightToolSlotRequiredRank = _config.Bind("Global", "TopRightToolSlot_RequiredRank", 3, "Rank at which the top-left tool slot is available (-1 to disable).");
            _topRightToolSlotRequiresUnlocking = _config.Bind("Global", "TopRightToolSlot_RequiresUnlocking", true, "Whether or not to require spending a memory locket to unluck this slot.");
            _topRightToolSlotColor = _config.Bind("Global", "TopRightToolSlot_Color", ToolItemType.Yellow, "The tool slot color for this rank. ==Only Blue and Yellow are supported==");
            
            _immediateHealthOnBind1 = _config.Bind("BeastCrestStage1", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with Beast Crest Rank 1. (Separate from the rage lifesteal)");
            _maximumLifeLeech1 = _config.Bind("BeastCrestStage1", "MaximumLifeLeech", 2, "The maximum amount of masks you can restore by attacking after binding for Rank 1.");
            _rageDurationIncrease1 = _config.Bind("BeastCrestStage1", "RageDurationIncrease", 0, "The percentage increase in rage duration for Rank 1. (You can also use negative numbers if you wish to decrease it)");
            
            _immediateHealthOnBind2 = _config.Bind("BeastCrestStage2", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with Beast Crest Rank 2. (Separate from the rage lifesteal)");
            _maximumLifeLeech2 = _config.Bind("BeastCrestStage2", "MaximumLifeLeech", 2, "The maximum amount of masks you can restore by attacking after binding for Rank 2.");
            _rageDurationIncrease2 = _config.Bind("BeastCrestStage2", "RageDurationIncrease", 20, "The percentage increase in rage duration for Rank 2. (You can also use negative numbers if you wish to decrease it)");
            
            _immediateHealthOnBind3 = _config.Bind("BeastCrestStage3", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with Beast Crest Rank 3. (Separate from the rage lifesteal)");
            _maximumLifeLeech3 = _config.Bind("BeastCrestStage3", "MaximumLifeLeech", 3, "The maximum amount of masks you can restore by attacking after binding for Rank 3.");
            _rageDurationIncrease3 = _config.Bind("BeastCrestStage3", "RageDurationIncrease", 20, "The percentage increase in rage duration for Rank 3. (You can also use negative numbers if you wish to decrease it)");
        }

        private void MigrateIfNeeded()
        {
            _configVersion = _config.Bind("Global", "ConfigVersion", 1, "Internal config version for migration purposes");
            var existingVersion = _configVersion.Value;
            Plugin.Log.LogInfo("Detected Config Version: " + existingVersion);
            
            if (existingVersion < CURRENT_CONFIG_VERSION)
            {
                Plugin.Log.LogInfo($"Migrating config from version {existingVersion} to {CURRENT_CONFIG_VERSION}");
                               
                // Bind all the legacy values to load them into our config. Very messy but I just can't be assed anymore.
                _config.Bind("BeastCrestStage2", "EnableNewToolSlot", true, "");
                _config.Bind("BeastCrestStage2", "ToolSlotRequiresUnlocking", true, "");
                _config.Bind("BeastCrestStage2", "ToolSlotColor", ToolItemType.Blue, "");
                _config.Bind("BeastCrestStage3", "EnableNewToolSlot", true, "");
                _config.Bind("BeastCrestStage3", "ToolSlotRequiresUnlocking", true, "");
                _config.Bind("BeastCrestStage3", "ToolSlotColor", ToolItemType.Yellow, "");
                
                MigrateToolSlot("BeastCrestStage2", ExtraToolSlotPosition.TopLeft);
                MigrateToolSlot("BeastCrestStage3", ExtraToolSlotPosition.TopRight);
            }

            _configVersion.Value = CURRENT_CONFIG_VERSION;
            
            _config.Save();
        }

        private void MigrateToolSlot(string oldSection, ExtraToolSlotPosition position)
        {
            var enableKey = new ConfigDefinition(oldSection, "EnableNewToolSlot");
            var requireKey = new ConfigDefinition(oldSection, "ToolSlotRequiresUnlocking");
            var colorKey = new ConfigDefinition(oldSection, "ToolSlotColor");

            if (_config.TryGetEntry(enableKey, out ConfigEntry<bool> enableEntry) && enableEntry.Value)
            {
                if (position == ExtraToolSlotPosition.TopLeft)
                    _topLeftToolSlotRequiredRank.Value = 2;
                else if (position == ExtraToolSlotPosition.TopRight)
                    _topRightToolSlotRequiredRank.Value = 3;
            }
            else
            {
                if (position == ExtraToolSlotPosition.TopLeft)
                    _topLeftToolSlotRequiredRank.Value = -1;
                else if (position == ExtraToolSlotPosition.TopRight)
                    _topRightToolSlotRequiredRank.Value = -1;
            }

            if (_config.TryGetEntry<bool>(requireKey, out var reqEntry))
            {
                if (position == ExtraToolSlotPosition.TopLeft)
                    _topLeftToolSlotRequiresUnlocking.Value = reqEntry.Value;
                else
                    _topRightToolSlotRequiresUnlocking.Value = reqEntry.Value;
            }

            if (_config.TryGetEntry<ToolItemType>(colorKey, out var colorEntry))
            {
                if (position == ExtraToolSlotPosition.TopLeft)
                    _topLeftToolSlotColor.Value = colorEntry.Value;
                else
                    _topRightToolSlotColor.Value = colorEntry.Value;
            }

            // Remove old keys
            _config.Remove(enableKey);
            _config.Remove(requireKey);
            _config.Remove(colorKey);
            
            _config.Save();
        }

        private void BuildCrestStats()
        {
            Crest1 = new CrestStats(
                _immediateHealthOnBind1.Value,
                _maximumLifeLeech1.Value,
                _rageDurationIncrease1.Value,
                BuildExtraToolSlots(1)
            );

            Crest2 = new CrestStats(
                _immediateHealthOnBind2.Value,
                _maximumLifeLeech2.Value,
                _rageDurationIncrease2.Value,
                BuildExtraToolSlots(2)
            );

            Crest3 = new CrestStats(
                _immediateHealthOnBind3.Value,
                _maximumLifeLeech3.Value,
                _rageDurationIncrease3.Value,
                BuildExtraToolSlots(3)
            );
        }

        private List<ExtraToolSlot> BuildExtraToolSlots(int rank)
        {
            var slots = new List<ExtraToolSlot>();

            if (_topLeftToolSlotRequiredRank.Value != -1 && rank == _topLeftToolSlotRequiredRank.Value)
            {
                slots.Add(new ExtraToolSlot(
                    ExtraToolSlotPosition.TopLeft,
                    _topLeftToolSlotRequiresUnlocking.Value,
                    _topLeftToolSlotColor.Value
                ));
            }

            if (_topRightToolSlotRequiredRank.Value != -1 && rank == _topRightToolSlotRequiredRank.Value)
            {
                slots.Add(new ExtraToolSlot(
                    ExtraToolSlotPosition.TopRight,
                    _topRightToolSlotRequiresUnlocking.Value,
                    _topRightToolSlotColor.Value
                ));
            }

            return slots;
        }
    }
}
