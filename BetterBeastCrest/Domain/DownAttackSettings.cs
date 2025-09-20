using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterBeastCrest.Domain
{
    public class DownAttackSettings
    {
        public static DownAttackSettings Hunter = new DownAttackSettings(CrestType.Hunter, HeroControllerConfig.DownSlashTypes.DownSpike, null, true);
        public static DownAttackSettings Reaper = new DownAttackSettings(CrestType.Reaper, HeroControllerConfig.DownSlashTypes.Custom, "RPR DOWNSLASH", false);
        public static DownAttackSettings Wanderer = new DownAttackSettings(CrestType.Wanderer, HeroControllerConfig.DownSlashTypes.Slash, null, true);
        public static DownAttackSettings Beast = new DownAttackSettings(CrestType.Beast, HeroControllerConfig.DownSlashTypes.Custom, "WARRIOR DOWNSLASH", true);
        public static DownAttackSettings Witch = new DownAttackSettings(CrestType.Witch, HeroControllerConfig.DownSlashTypes.Custom, "WITCH DOWNSLASH", true);
        public static DownAttackSettings Architect = new DownAttackSettings(CrestType.Architect, HeroControllerConfig.DownSlashTypes.Custom, "TOOLMASTER DOWNSLASH", false);
        public static DownAttackSettings Shaman = new DownAttackSettings(CrestType.Shaman, HeroControllerConfig.DownSlashTypes.Custom, "SHAMAN DOWNSLASH", true);
        
        public CrestType CrestType { get; }
        public HeroControllerConfig.DownSlashTypes SlashType { get; }
        public string? DownAttackEvent { get; }
        public bool DownSpikeThrusts { get; }

        private DownAttackSettings(CrestType crestType, HeroControllerConfig.DownSlashTypes slashType, string? downAttackEvent, bool downSpikeThrusts)
        {
            CrestType = crestType;
            SlashType = slashType;
            DownAttackEvent = downAttackEvent;
            DownSpikeThrusts = downSpikeThrusts;
        }
        
        private static readonly Dictionary<CrestType, DownAttackSettings> _lookup = typeof(DownAttackSettings)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(DownAttackSettings))
            .Select(f => (DownAttackSettings)f.GetValue(null)!)
            .ToDictionary(s => s.CrestType);

        public static DownAttackSettings? GetByCrestType(CrestType crestType) => _lookup.GetValueOrDefault(crestType);
    }
}
