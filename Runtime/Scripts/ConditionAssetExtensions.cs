using HHG.Common.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HHG.StatSystem.Runtime
{
    public static class ConditionAssetExtensions
    {
        public static void Apply(this IEnumerable<ConditionAsset> conditions, object owner, GameObject target)
        {
            foreach (ConditionAsset condition in conditions)
            {
                condition.Apply(owner, target);
            }
        }

        public static void Remove(this IEnumerable<ConditionAsset> conditions, object owner, GameObject target)
        {
            foreach (ConditionAsset condition in conditions)
            {
                condition.Remove(owner, target);
            }
        }

        public static void AggregateTo(this IEnumerable<ConditionAsset> source, List<ConditionAsset> aggregated)
        {
            ObjectUtil.Destroy(aggregated);

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