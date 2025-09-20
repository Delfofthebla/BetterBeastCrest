using System.IO;
using BepInEx;
using BepInEx.Configuration;

namespace BetterBeastCrest.Domain
{
    public class ModConfig
    {
        private readonly ConfigFile _config;
        
        private ConfigEntry<ToolItemType> _centerToolSlotColor;
        private ConfigEntry<CrestType> _downAttackCrestType;

        private ConfigEntry<int> _immediateHealthOnBind1;
        private ConfigEntry<int> _maximumLifeLeech1;
        private ConfigEntry<int> _rageDurationIncrease1;

        private ConfigEntry<int> _immediateHealthOnBind2;
        private ConfigEntry<int> _maximumLifeLeech2;
        private ConfigEntry<int> _rageDurationIncrease2;
        private ConfigEntry<ToolItemType> _toolSlotColor2;
        private ConfigEntry<bool> _enableNewToolSlot2;
        private ConfigEntry<bool> _toolSlotRequireUnlocking2;

        private ConfigEntry<int> _immediateHealthOnBind3;
        private ConfigEntry<int> _maximumLifeLeech3;
        private ConfigEntry<int> _rageDurationIncrease3;
        private ConfigEntry<ToolItemType> _toolSlotColor3;
        private ConfigEntry<bool> _enableNewToolSlot3;
        private ConfigEntry<bool> _toolSlotRequireUnlocking3;
        
        public CrestStats CrestDefault { get; } = new CrestStats(0, 3, 0, false, false, ToolItemType.Skill);
        public CrestStats Crest1 { get; private set; }
        public CrestStats Crest2 { get; private set; }
        public CrestStats Crest3 { get; private set; }
        public CrestType DownAttackType => _downAttackCrestType.Value;
        
        public ModConfig()
        {
            _config = new ConfigFile(Path.Combine(Paths.ConfigPath, "BetterBeastCrest.cfg"), true);
            LoadConfig();
        }
        
        public void ReloadConfig()
        {
            _config.Reload();
            LoadConfig();
            Plugin.LogInfo("Config reloaded and CrestStats rebuilt.");
        }

        private void LoadConfig()
        {
            _centerToolSlotColor = _config.Bind("Global", "CenterToolSlotColor", ToolItemType.Skill, "Specify the type of the center most tool slot. It is Skill in the base game, but you can change that here.");
            _downAttackCrestType = _config.Bind("Global", "DownAttackType", CrestType.Beast, "Specify the down attack to be used with the beast crest.");
            _immediateHealthOnBind1 = _config.Bind("BeastCrestStage1", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with Beast Crest Rank 1. (Separate from the rage lifesteal)");
            _maximumLifeLeech1 = _config.Bind("BeastCrestStage1", "MaximumLifeLeech", 2, "The maximum amount of masks you can restore by attacking after binding for Rank 1. (You can also use negative numbers if you wish to decrease it)");
            _rageDurationIncrease1 = _config.Bind("BeastCrestStage1", "RageDurationIncrease", 0, "The percentage increase in rage duration for Rank 1. (You can also use negative numbers if you wish to decrease it)");
            
            _immediateHealthOnBind2 = _config.Bind("BeastCrestStage2", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with Beast Crest Rank 2. (Separate from the rage lifesteal)");
            _maximumLifeLeech2 = _config.Bind("BeastCrestStage2", "MaximumLifeLeech", 2, "The maximum amount of masks you can restore by attacking after binding for Rank 2. (You can also use negative numbers if you wish to decrease it)");
            _rageDurationIncrease2 = _config.Bind("BeastCrestStage2", "RageDurationIncrease", 20, "The percentage increase in rage duration for Rank 2. (You can also use negative numbers if you wish to decrease it)");
            _enableNewToolSlot2 = _config.Bind("BeastCrestStage2", "EnableNewToolSlot", true, "Enable the tool slot for this rank.");
            _toolSlotRequireUnlocking2 = _config.Bind("BeastCrestStage2", "ToolSlotRequiresUnlocking", true, "Whether or not to require spending a memory locket ot unluck the slot.");
            _toolSlotColor2 = _config.Bind("BeastCrestStage2", "ToolSlotColor", ToolItemType.Blue, "The tool slot color for this rank. ==Only Blue and Yellow are supported==");
            
            _immediateHealthOnBind3 = _config.Bind("BeastCrestStage3", "HealOnBind", 1, "Specify the amount of masks you want to restore upon binding with Beast Crest Rank 3. (Separate from the rage lifesteal)");
            _maximumLifeLeech3 = _config.Bind("BeastCrestStage3", "MaximumLifeLeech", 3, "The maximum amount of masks you can restore by attacking after binding for Rank 3. (You can also use negative numbers if you wish to decrease it)");
            _rageDurationIncrease3 = _config.Bind("BeastCrestStage3", "RageDurationIncrease", 20, "The percentage increase in rage duration for Rank 3. (You can also use negative numbers if you wish to decrease it)");
            _enableNewToolSlot3 = _config.Bind("BeastCrestStage3", "EnableNewToolSlot", true, "Enable the tool slot for this rank.");
            _toolSlotRequireUnlocking3 = _config.Bind("BeastCrestStage3", "ToolSlotRequiresUnlocking", true, "Whether or not to require spending a memory locket ot unluck the slot.");
            _toolSlotColor3 = _config.Bind("BeastCrestStage3", "ToolSlotColor", ToolItemType.Yellow, "The tool slot color for this rank. ==Only Blue and Yellow are supported== It's Yellow by default because 2 blues is VERY strong :)");

            Crest1 = new CrestStats(
                _immediateHealthOnBind1.Value,
                _maximumLifeLeech1.Value,
                _rageDurationIncrease1.Value,
                _centerToolSlotColor.Value != ToolItemType.Skill,
                false,
                _centerToolSlotColor.Value
            );
            Crest2 = new CrestStats(
                _immediateHealthOnBind2.Value,
                _maximumLifeLeech2.Value,
                _rageDurationIncrease2.Value,
                _enableNewToolSlot2.Value,
                _toolSlotRequireUnlocking2.Value,
                _toolSlotColor2.Value
            );
            Crest3 = new CrestStats(
                _immediateHealthOnBind3.Value,
                _maximumLifeLeech3.Value,
                _rageDurationIncrease3.Value,
                _enableNewToolSlot3.Value,
                _toolSlotRequireUnlocking2.Value,
                _toolSlotColor3.Value
            );
        }
    }
}
