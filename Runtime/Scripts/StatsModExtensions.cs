using System.Collections.Generic;

namespace HHG.StatSystem.Runtime
{
    public static class StatsModExtensions
    {
        public static void Apply(this IEnumerable<StatsMod> mods, IStats stats)
        {
            foreach (StatsMod mod in mods)
            {
                mod.Add(stats);
            }
        }

        public static void Remove(this IEnumerable<StatsMod> mods, IStats stats)
        {
            foreach (StatsMod mod in mods)
            {
                mod.Remove(stats);
            }
        }
    }
}