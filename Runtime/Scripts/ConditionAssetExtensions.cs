using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HHG.StatSystem.Runtime
{
    public static class ConditionAssetExtensions
    {
        public static void Apply(this IEnumerable<ConditionAsset> conditions, GameObject target)
        {
            foreach (ConditionAsset condition in conditions)
            {
                condition.Apply(target);
            }
        }

        public static void Remove(this IEnumerable<ConditionAsset> conditions, GameObject target)
        {
            foreach (ConditionAsset condition in conditions)
            {
                condition.Remove(target);
            }
        }

        public static void AggregateTo(this IEnumerable<ConditionAsset> source, List<ConditionAsset> aggregated)
        {
            foreach (ConditionAsset item in aggregated)
            {
                Object.Destroy(item);
            }

            aggregated.Clear();

            if (source.Any())
            {
                Dictionary<string, ConditionAsset> dict = new Dictionary<string, ConditionAsset>();

                foreach (ConditionAsset condition in source)
                {
                    string tag = condition.Tag;
                    dict[tag] = dict.ContainsKey(tag) ? dict[tag].Aggregate(condition) : Object.Instantiate(condition);
                }

                aggregated.AddRange(dict.Values);
            }
        }
    }
}