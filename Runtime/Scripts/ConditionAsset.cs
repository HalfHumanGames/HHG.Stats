using HHG.Common.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HHG.StatSystem.Runtime
{
    [CreateAssetMenu(fileName = "Condition", menuName = "HHG/Stat System/Condition")]
    public class ConditionAsset : ScriptableObject, IAggregatable<ConditionAsset>
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
        public IReadOnlyList<MetaBehaviour> Behaviours => behaviours;

        [SerializeField] private string tag;
        [SerializeField] private float priority;
        [SerializeField] private float duration;
        [SerializeField] private List<StatsMod> mods = new List<StatsMod>();

        [SerializeReference, SerializeReferenceDropdown] private List<MetaBehaviour> behaviours = new List<MetaBehaviour>();

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

        public ConditionAsset Aggregate(ConditionAsset other)
        {
            Dictionary<Type, MetaBehaviour> dict = new Dictionary<Type, MetaBehaviour>();
            mods.AddRange(other.mods);
            // IEnumerables reference source collections, so convert to a list
            // to make sure that when we clear behaviours, it doesn't affect combined
            List<MetaBehaviour> combined = behaviours.Concat(other.behaviours).ToList();
            behaviours.Clear(); // Clear after get combined
            
            foreach (MetaBehaviour behaviour in combined)
            {
                if (behaviour is IAggregatable)
                {
                    Type type = behaviour.GetType();
                    dict[type] = dict.ContainsKey(type) ? (MetaBehaviour)((IAggregatable)dict[type]).Aggregate(behaviour) : behaviour;
                }
                else
                {
                    behaviours.Add(behaviour);
                }
            }

            behaviours.AddRange(dict.Values);
            return this;
        }

        private void OnValidate()
        {
            tag = null;
            _ = Tag;
        }

        internal void Apply(IStats stats) => mods.Apply(stats);
        internal void Remove(IStats stats) => mods.Remove(stats);

        public void OnDestroy()
        {
            foreach(MetaBehaviour behaviour in behaviours)
            {
                if (behaviour is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}