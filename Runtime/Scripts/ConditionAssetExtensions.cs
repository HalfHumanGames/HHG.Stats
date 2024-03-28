using System.Collections.Generic;
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
    }
}