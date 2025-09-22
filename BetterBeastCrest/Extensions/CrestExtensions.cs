using System;
using System.Collections.Generic;
using System.Linq;
using BetterBeastCrest.Domain;

namespace BetterBeastCrest.Extensions
{
    public static class CrestExtensions
    {
        // One centralized dictionary of aliases
        private static readonly Dictionary<CrestType, string[]> Aliases = new Dictionary<CrestType, string[]>
        {
            { CrestType.Naked, CrestAliases.Naked},
            { CrestType.Hunter, CrestAliases.Hunter},
            { CrestType.Reaper, CrestAliases.Reaper},
            { CrestType.Wanderer, CrestAliases.Wanderer},
            { CrestType.Beast, CrestAliases.Beast},
            { CrestType.Witch, CrestAliases.Witch},
            { CrestType.Architect, CrestAliases.Architect},
            { CrestType.Shaman, CrestAliases.Shaman}
        };
        
        public static bool Matches(this CrestType crest, string? name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return Aliases.TryGetValue(crest, out var aliases) && aliases.Any(alias => string.Equals(alias, name, StringComparison.OrdinalIgnoreCase));
        }
        
        public static HeroController.ConfigGroup? ForCrest(
            this IEnumerable<HeroController.ConfigGroup> groups,
            CrestType crest)
        {
            return groups.FirstOrDefault(g => g?.ActiveRoot != null && crest.Matches(g.ActiveRoot.name));
        }
    }
}
