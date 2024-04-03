using HHG.Common.Runtime;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HHG.StatSystem.Runtime
{
    [CreateAssetMenu(fileName = "Condition", menuName = "HHG/Stat System/Condition")]
    public class ConditionAsset : ScriptableObject
    {
        public string Tag
        {
            get
            {
                if (string.IsNullOrEmpty(tag))
                {
                    tag = Regex.Replace(name, @"[\(\[\{].*?[\)\]\}]|\s+|[^a-zA-Z\s]", "");
                }
                return tag;
            }
        }
        public float Priority => priority;
        public float Duration => duration;
        public MetaBehaviour[] Behaviours => behaviours;

        [SerializeField] private string tag;
        [SerializeField] private float priority;
        [SerializeField] private float duration;
        [SerializeField] private StatsMod[] mods;

        [SerializeReference, SerializeReferenceDropdown] private MetaBehaviour[] behaviours;

        public void Apply(GameObject target)
        {
            if (target.TryGetComponent(out ConditionTracker tracker))
            {
                tracker.Apply(this);
            }
        }

        public void Remove(GameObject target)
        {
            if (target.TryGetComponent(out ConditionTracker tracker))
            {
                tracker.Remove(this);
            }
        }

        private void OnValidate()
        {
            tag = null;
            _ = Tag;
        }

        internal void Apply(IStats stats) => mods.Apply(stats);
        internal void Remove(IStats stats) => mods.Remove(stats);
    }
}