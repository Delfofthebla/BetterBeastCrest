using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using BetterBeastCrest.Services;

namespace BetterBeastCrest.Domain
{
    public class CrestStats
    {
        private readonly ConfigEntry<int> _immediateHealConfig;
        private readonly ConfigEntry<int> _maxLifeLeechConfig;
        private readonly ConfigEntry<float> _rageDurationConfig;
        private readonly ConfigEntry<int> _rageDamageMultiplierConfig;
        private readonly ConfigEntry<float> _rageAttackSpeedIncreaseConfig;

        public virtual int ImmediateHeal => _immediateHealConfig.Value;
        public virtual int MaxLifeLeech => _maxLifeLeechConfig.Value;
        public virtual float RageDuration => _rageDurationConfig.Value;
        public virtual float RageDamageMultiplier => 1f + (_rageDamageMultiplierConfig.Value / 100f);
        public virtual float RageAttackCooldown => MathF.Round(Helpers.DefaultAttackCooldown * (1f - (_rageAttackSpeedIncreaseConfig.Value / 100f)), 3);

        // Only additional mod slots (TopLeft/TopRight)
        public IReadOnlyList<ExtraToolSlot> ExtraToolSlots { get; }

        public CrestStats(
            ConfigEntry<int> immediateHealConfig,
            ConfigEntry<int> maxLifeLeechConfig,
            ConfigEntry<float> rageDurationConfig,
            ConfigEntry<int> rageDamageMultiplierConfig,
            ConfigEntry<float> rageAttackSpeedIncreaseConfig,
            List<ExtraToolSlot>? modToolSlots = null)
            : this(modToolSlots)
        {
            _immediateHealConfig = immediateHealConfig;
            _maxLifeLeechConfig = maxLifeLeechConfig;
            _rageDurationConfig = rageDurationConfig;
            _rageDamageMultiplierConfig = rageDamageMultiplierConfig;
            _rageAttackSpeedIncreaseConfig = rageAttackSpeedIncreaseConfig;
        }
        
        #pragma warning disable CS8618
        protected CrestStats(List<ExtraToolSlot>? modToolSlots = null)
        {
            ExtraToolSlots = modToolSlots ?? new List<ExtraToolSlot>();
        }
        #pragma warning restore CS8618

        public int TotalHealing => ImmediateHeal + MaxLifeLeech;
    }

    public class ExtraToolSlot
    {
        public ExtraToolSlotPosition Position { get; }
        public bool RequiresUnlocking { get; }
        public ToolItemType SlotColor { get; }

        public ExtraToolSlot(ExtraToolSlotPosition position, bool requiresUnlocking, ToolItemType slotColor)
        {
            Position = position;
            RequiresUnlocking = requiresUnlocking;
            SlotColor = slotColor;
        }
    }

    public enum ExtraToolSlotPosition
    {
        TopLeft,
        TopRight
    }
}
