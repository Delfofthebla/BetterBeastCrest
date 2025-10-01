using System.Collections.Generic;

namespace BetterBeastCrest.Domain
{
    public class CrestStatsNoConfig : CrestStats
    {
        public override int ImmediateHeal { get; }
        public override int MaxLifeLeech { get; }
        public override float RageDuration { get; }
        public override float RageDamageMultiplier { get; }
        public override float RageAttackCooldown { get; }

        public CrestStatsNoConfig(int immediateHeal, int maxLifeLeech, float rageDuration, float rageDamageMultiplier, float rageAttackCooldown, List<ExtraToolSlot>? modToolSlots = null)
            : base(modToolSlots)
        {
            ImmediateHeal = immediateHeal;
            MaxLifeLeech = maxLifeLeech;
            RageDuration = rageDuration;
            RageDamageMultiplier = rageDamageMultiplier;
            RageAttackCooldown = rageAttackCooldown;
        }
    }
}
