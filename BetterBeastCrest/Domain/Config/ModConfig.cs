using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BetterBeastCrest.Services;

namespace BetterBeastCrest.Domain.Config
{
    public class ModConfig
    {
        private static readonly string _rank1Prefix = "Rank 1";
        private static readonly string _rank2Prefix = "Rank 2";
        private static readonly string _rank3Prefix = "Rank 3";
        private static readonly string _bindDesc = "Number of masks to restore immediately upon binding (Separate from rage lifesteal)";
        private static readonly string _lifestealDesc = "Maximum amount of masks you can restore by attacking after binding";
        private static readonly string _rageDurationDesc = "Duration (in seconds) of the Rage buff after binding. (Vanilla default value is 5 seconds)";
        private static readonly string _rageDamageMultDesc = "Damage multiplier during rage mode. (Vanilla default value is 25%)";
        private static readonly string _rageAttackSpeedDesc = "Attack Speed multiplier during rage mode. (Vanilla default value is 18%)";
        
        private static readonly FieldInfo? _defaultValueConfigField = typeof(ConfigEntryBase).GetField("<DefaultValue>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
        
        private const int CURRENT_CONFIG_VERSION = 4;
        private ConfigEntry<int> _configVersion;
        
        public readonly ConfigFile ConfigFile;

        // Global settings
        private ConfigEntry<CrestType> _downAttackCrestType;
        
        private ConfigEntry<ToolItemType> _topCenterToolSlotColor;
        private ConfigEntry<ToolItemType> _middleCenterToolSlotColor;
        private ConfigEntry<ToolItemType> _bottomCenterToolSlotColor;
        
        private ConfigEntry<int> _topLeftToolSlotRequiredRank;
        private ConfigEntry<int> _topRightToolSlotRequiredRank;

        private ConfigEntry<bool> _topLeftToolSlotRequiresUnlocking;
        private ConfigEntry<bool> _topRightToolSlotRequiresUnlocking;

        private ConfigEntry<ToolItemType> _topLeftToolSlotColor;
        private ConfigEntry<ToolItemType> _topRightToolSlotColor;

        // Healing / Rage
        private ConfigEntry<int> _immediateHealthOnBind1;
        private ConfigEntry<int> _maximumLifeLeech1;
        private ConfigEntry<float> _rageDuration1;
        private ConfigEntry<int> _rageDamageMultiplier1;
        private ConfigEntry<float> _rageAttackSpeedPercent1;

        private ConfigEntry<int> _immediateHealthOnBind2;
        private ConfigEntry<int> _maximumLifeLeech2;
        private ConfigEntry<float> _rageDuration2;
        private ConfigEntry<int> _rageDamageMultiplier2;
        private ConfigEntry<float> _rageAttackSpeedPercent2;

        private ConfigEntry<int> _immediateHealthOnBind3;
        private ConfigEntry<int> _maximumLifeLeech3;
        private ConfigEntry<float> _rageDuration3;
        private ConfigEntry<int> _rageDamageMultiplier3;
        private ConfigEntry<float> _rageAttackSpeedPercent3;

        public ToolItemType TopCenterToolSlotColor => _topCenterToolSlotColor.Value;
        public ToolItemType MiddleCenterToolSlotColor => _middleCenterToolSlotColor.Value;
        public ToolItemType BottomCenterToolSlotColor => _bottomCenterToolSlotColor.Value;
        public CrestType DownAttackType => _downAttackCrestType.Value;

        public CrestStats CrestDefault { get; } = new CrestStatsNoConfig(0, 3, 5.0f, 1.25f, 0.32f);
        public CrestStats Crest1 { get; private set; }
        public CrestStats Crest2 { get; private set; }
        public CrestStats Crest3 { get; private set; }

        public ModConfig()
        {
            var configPath = Path.Combine(Paths.ConfigPath, "BetterBeastCrest.cfg");
            var isNewConfig = !File.Exists(configPath);
            
            ConfigFile = new ConfigFile(configPath, true);
            LoadConfig();
            
            if (!isNewConfig)
                MigrateIfNeeded();
            else
            {
                Plugin.Log.LogInfo("Detected No Config, Generating new one.");
                _configVersion = ConfigFile.Bind("Global", "ConfigVersion", CURRENT_CONFIG_VERSION, "Internal config version for migration purposes");
            }
            
            BuildCrestStats();
        }

        public void ReloadConfig()
        {
            ConfigFile.Reload();
            LoadConfig();
            BuildCrestStats();
            Plugin.Log.LogInfo("Config reloaded and CrestStats rebuilt.");
        }

        private void LoadConfig()
        {
            _topCenterToolSlotColor = ConfigFile.Bind("Global", "TopCenterToolSlotColor", ToolItemType.Red, "Specify the type of the top center tool slot. It is Red in the base game, but you can change that here.");
            _middleCenterToolSlotColor = ConfigFile.Bind("Global", "MiddleCenterToolSlotColor", ToolItemType.Skill, "Specify the type of the centermost tool slot. It is Skill in the base game, but you can change that here.");
            _bottomCenterToolSlotColor = ConfigFile.Bind("Global", "BottomCenterToolSlotColor", ToolItemType.Red, "Specify the type of the bottom center tool slot. It is Red in the base game, but you can change that here.");
            _downAttackCrestType = ConfigFile.Bind("Global", "DownAttackType", CrestType.Beast, "Specify the down attack to be used with the beast crest.");
            
            _topLeftToolSlotRequiredRank = ConfigFile.Bind("Global", "TopLeftToolSlot_RequiredRank", 2,
                new ConfigDescription("Rank at which the new top-left tool slot is available (-1 to disable).", new AcceptableValueList<int>(-1,1,2,3)));
            _topLeftToolSlotRequiresUnlocking = ConfigFile.Bind("Global", "TopLeftToolSlot_RequiresUnlocking", true, "Whether or not to require spending a memory locket to unlock this slot.");
            _topLeftToolSlotColor = ConfigFile.Bind("Global", "TopLeftToolSlot_Color", ToolItemType.Blue, new ConfigDescription("The tool slot color for this rank. ==Only Blue and Yellow are supported==",
                new AcceptableEnumList<ToolItemType>(ToolItemType.Blue, ToolItemType.Yellow)));
            
            _topRightToolSlotRequiredRank = ConfigFile.Bind("Global", "TopRightToolSlot_RequiredRank", 3,
                new ConfigDescription("Rank at which the new top-right tool slot is available (-1 to disable).", new AcceptableValueList<int>(-1,1,2,3)));
            _topRightToolSlotRequiresUnlocking = ConfigFile.Bind("Global", "TopRightToolSlot_RequiresUnlocking", true, "Whether or not to require spending a memory locket to unlock this slot.");
            _topRightToolSlotColor = ConfigFile.Bind("Global", "TopRightToolSlot_Color", ToolItemType.Yellow,
                new ConfigDescription("The tool slot color for this rank. ==Only Blue and Yellow are supported==", new AcceptableEnumList<ToolItemType>(ToolItemType.Blue, ToolItemType.Yellow)));

            _immediateHealthOnBind1 = ConfigFile.Bind("BeastCrestStage1", "HealOnBind", 1, new ConfigDescription($"{_rank1Prefix} - {_bindDesc}", new AcceptableValueRange<int>(0, 10)));
            _maximumLifeLeech1 = ConfigFile.Bind("BeastCrestStage1", "MaximumLifeLeech", 2, new ConfigDescription($"{_rank1Prefix} - {_lifestealDesc}", new AcceptableValueRange<int>(0, 10)));
            _rageDuration1 = ConfigFile.Bind("BeastCrestStage1", "RageDuration", 5.0f, new ConfigDescription($"{_rank1Prefix} - {_rageDurationDesc}", new AcceptableValueRange<float>(0.0f, 15.0f)));
            _rageDuration1.SettingChanged += OnRageStatsChanged;
            _rageDamageMultiplier1 = ConfigFile.Bind("BeastCrestStage1", "RageDamageMultiplier", 25,
                new ConfigDescription($"{_rank1Prefix} - {_rageDamageMultDesc}", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { ShowRangeAsPercent = true }));
            _rageDamageMultiplier1.SettingChanged += OnRageStatsChanged;
            _rageAttackSpeedPercent1 = ConfigFile.Bind("BeastCrestStage1", "RageAttackSpeedPercent", 18f, new ConfigDescription($"{_rank1Prefix} - {_rageAttackSpeedDesc}", new AcceptableValueRange<float>(0.0f, 50.0f)));
            _rageAttackSpeedPercent1.SettingChanged += OnRageStatsChanged;
            
            _immediateHealthOnBind2 = ConfigFile.Bind("BeastCrestStage2", "HealOnBind", 1, new ConfigDescription($"{_rank2Prefix} - {_bindDesc}", new AcceptableValueRange<int>(0, 10)));
            _maximumLifeLeech2 = ConfigFile.Bind("BeastCrestStage2", "MaximumLifeLeech", 2, new ConfigDescription($"{_rank2Prefix} - {_lifestealDesc}", new AcceptableValueRange<int>(0, 10)));
            _rageDuration2 = ConfigFile.Bind("BeastCrestStage2", "RageDuration", 6.0f, new ConfigDescription($"{_rank2Prefix} - {_rageDurationDesc}", new AcceptableValueRange<float>(0.0f, 15.0f)));
            _rageDuration2.SettingChanged += OnRageStatsChanged;
            _rageDamageMultiplier2 = ConfigFile.Bind("BeastCrestStage2", "RageDamageMultiplier", 25,
                new ConfigDescription($"{_rank2Prefix} - {_rageDamageMultDesc}", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { ShowRangeAsPercent = true }));
            _rageDamageMultiplier2.SettingChanged += OnRageStatsChanged;
            _rageAttackSpeedPercent2 = ConfigFile.Bind("BeastCrestStage2", "RageAttackSpeedPercent", 18f, new ConfigDescription($"{_rank2Prefix} - {_rageAttackSpeedDesc}", new AcceptableValueRange<float>(0.0f, 50.0f)));
            _rageAttackSpeedPercent2.SettingChanged += OnRageStatsChanged;
            
            _immediateHealthOnBind3 = ConfigFile.Bind("BeastCrestStage3", "HealOnBind", 1, new ConfigDescription($"{_rank3Prefix} - {_bindDesc}", new AcceptableValueRange<int>(0, 10)));
            _maximumLifeLeech3 = ConfigFile.Bind("BeastCrestStage3", "MaximumLifeLeech", 3, new ConfigDescription($"{_rank3Prefix} - {_lifestealDesc}", new AcceptableValueRange<int>(0, 10)));
            _rageDuration3 = ConfigFile.Bind("BeastCrestStage3", "RageDuration", 6.0f, new ConfigDescription($"{_rank3Prefix} - {_rageDurationDesc}", new AcceptableValueRange<float>(0.0f, 15.0f)));
            _rageDuration3.SettingChanged += OnRageStatsChanged;
            _rageDamageMultiplier3 = ConfigFile.Bind("BeastCrestStage3", "RageDamageMultiplier", 25,
                new ConfigDescription($"{_rank3Prefix} - {_rageDamageMultDesc}", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { ShowRangeAsPercent = true }));
            _rageDamageMultiplier3.SettingChanged += OnRageStatsChanged;
            _rageAttackSpeedPercent3 = ConfigFile.Bind("BeastCrestStage3", "RageAttackSpeedPercent", 18f, new ConfigDescription($"{_rank3Prefix} - {_rageAttackSpeedDesc}", new AcceptableValueRange<float>(0.0f, 50.0f)));
            _rageAttackSpeedPercent3.SettingChanged += OnRageStatsChanged;
        }

        private static void OnRageStatsChanged(object sender, EventArgs e)
        {
            if (BeastCrestModifier.ModInitialized)
                BeastCrestModifier.AdjustRageStats();
        }

        private void BuildCrestStats()
        {
            Crest1 = new CrestStats(
                _immediateHealthOnBind1,
                _maximumLifeLeech1,
                _rageDuration1,
                _rageDamageMultiplier1,
                _rageAttackSpeedPercent1,
                BuildExtraToolSlots(1)
            );

            Crest2 = new CrestStats(
                _immediateHealthOnBind2,
                _maximumLifeLeech2,
                _rageDuration2,
                _rageDamageMultiplier2,
                _rageAttackSpeedPercent2,
                BuildExtraToolSlots(2)
            );

            Crest3 = new CrestStats(
                _immediateHealthOnBind3,
                _maximumLifeLeech3,
                _rageDuration3,
                _rageDamageMultiplier3,
                _rageAttackSpeedPercent3,
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
        
        private void MigrateIfNeeded()
        {
            _configVersion = ConfigFile.Bind("Global", "ConfigVersion", 1,
                new ConfigDescription("Internal config version for migration purposes", null, new ConfigurationManagerAttributes { Browsable = false, HideDefaultButton = true, HideSettingName = true }));
            
            var existingVersion = _configVersion.Value;
            Plugin.Log.LogInfo("Detected Config Version: " + existingVersion);
            
            if (existingVersion != CURRENT_CONFIG_VERSION)
                Plugin.Log.LogInfo($"Migrating config from version {existingVersion} to {CURRENT_CONFIG_VERSION}");
            
            if (existingVersion < 2)
                UpgradeToVersion2();

            if (existingVersion < 3)
                UpgradeToVersion3();

            if (existingVersion < 4)
                UpgradeToVersion4();
            
            _defaultValueConfigField?.SetValue(_configVersion, CURRENT_CONFIG_VERSION);
            _configVersion.Value = CURRENT_CONFIG_VERSION;
            
            ConfigFile.Save();
        }

        private void UpgradeToVersion2()
        {
            // Bind all the legacy values to load them into our config. Very messy but I just can't be assed anymore.
            ConfigFile.Bind("BeastCrestStage2", "EnableNewToolSlot", true, "");
            ConfigFile.Bind("BeastCrestStage2", "ToolSlotRequiresUnlocking", true, "");
            ConfigFile.Bind("BeastCrestStage2", "ToolSlotColor", ToolItemType.Blue, "");
            ConfigFile.Bind("BeastCrestStage3", "EnableNewToolSlot", true, "");
            ConfigFile.Bind("BeastCrestStage3", "ToolSlotRequiresUnlocking", true, "");
            ConfigFile.Bind("BeastCrestStage3", "ToolSlotColor", ToolItemType.Yellow, "");
                
            MigrateToolSlot("BeastCrestStage2", ExtraToolSlotPosition.TopLeft);
            MigrateToolSlot("BeastCrestStage3", ExtraToolSlotPosition.TopRight);
        }

        private void UpgradeToVersion3()
        {
            var oldDurationIncrease1 = ConfigFile.Bind("BeastCrestStage1", "RageDurationIncrease", 0, "");
            var oldDurationIncrease2 = ConfigFile.Bind("BeastCrestStage2", "RageDurationIncrease", 20, "");
            var oldDurationIncrease3 = ConfigFile.Bind("BeastCrestStage3", "RageDurationIncrease", 20, "");

            _rageDuration1.Value = CrestDefault.RageDuration * (1f + (oldDurationIncrease1.Value / 100f));
            ConfigFile.Remove(oldDurationIncrease1.Definition);
            _rageDuration2.Value = CrestDefault.RageDuration * (1f + (oldDurationIncrease2.Value / 100f));
            ConfigFile.Remove(oldDurationIncrease2.Definition);
            _rageDuration3.Value = CrestDefault.RageDuration * (1f + (oldDurationIncrease3.Value / 100f));
            ConfigFile.Remove(oldDurationIncrease3.Definition);
        }
        
        private void UpgradeToVersion4()
        {
            var oldCenterSlotColor = ConfigFile.Bind("Global", "CenterToolSlotColor", ToolItemType.Skill, "");
            _middleCenterToolSlotColor.Value = oldCenterSlotColor.Value;
            ConfigFile.Remove(oldCenterSlotColor.Definition);
        }

        private void MigrateToolSlot(string oldSection, ExtraToolSlotPosition position)
        {
            var enableKey = new ConfigDefinition(oldSection, "EnableNewToolSlot");
            var requireKey = new ConfigDefinition(oldSection, "ToolSlotRequiresUnlocking");
            var colorKey = new ConfigDefinition(oldSection, "ToolSlotColor");

            if (ConfigFile.TryGetEntry(enableKey, out ConfigEntry<bool> enableEntry) && enableEntry.Value)
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

            if (ConfigFile.TryGetEntry<bool>(requireKey, out var reqEntry))
            {
                if (position == ExtraToolSlotPosition.TopLeft)
                    _topLeftToolSlotRequiresUnlocking.Value = reqEntry.Value;
                else
                    _topRightToolSlotRequiresUnlocking.Value = reqEntry.Value;
            }

            if (ConfigFile.TryGetEntry<ToolItemType>(colorKey, out var colorEntry))
            {
                if (position == ExtraToolSlotPosition.TopLeft)
                    _topLeftToolSlotColor.Value = colorEntry.Value;
                else
                    _topRightToolSlotColor.Value = colorEntry.Value;
            }

            // Remove old keys
            ConfigFile.Remove(enableKey);
            ConfigFile.Remove(requireKey);
            ConfigFile.Remove(colorKey);
            
            ConfigFile.Save();
        }
    }
}
