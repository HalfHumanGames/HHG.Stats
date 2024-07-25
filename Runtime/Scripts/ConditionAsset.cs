using HHG.Common.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    tag = TagUtil.GetTag(name);
                }
                return tag;
            }
        }
        public float Priority => priority;
        public float Duration => duration;
        public Color Color => color;
        public IReadOnlyList<MetaBehaviour> Behaviours => behaviours;

        [SerializeField] private string tag;
        [SerializeField] private float priority;
        [SerializeField] private float duration;
        [SerializeField] private Color color;
        [SerializeField] private List<StatsMod> mods = new List<StatsMod>();
        [SerializeReference, SerializeReferenceDropdown] private List<MetaBehaviour> behaviours = new List<MetaBehaviour>();
        [SerializeField] private AggregateFlags aggregateFlags;

        private bool seeded;

        [Flags]
        private enum AggregateFlags
        {
            Duration = 1 << 0,
            StatMods = 1 << 1,
            MetaBehaviours = 1 << 2,
        }

        public void Apply(object owner, GameObject target)
        {
            if (target.TryGetComponent(out ConditionTracker tracker))
            {
                tracker.Apply(owner, this);
            }
        }

        public void Remove(object owner, GameObject target)
        {
            if (target.TryGetComponent(out ConditionTracker tracker))
            {
                tracker.Remove(owner, this);
            }
        }

        public object Seed()
        {
            if (!seeded)
            {
                seeded = true;

                foreach (MetaBehaviour behaviour in behaviours)
                {
                    if (behaviour is IAggregatable aggregatable)
                    {
                        aggregatable.Seed();
                    }
                }
            }

            return this;
        }

        public ConditionAsset Aggregate(ConditionAsset other)
        {
            if (aggregateFlags.HasFlag(AggregateFlags.Duration))
            {
                duration += other.duration;
            }

            if (aggregateFlags.HasFlag(AggregateFlags.StatMods))
            {
                mods.AddRange(other.mods);
            }

            if (aggregateFlags.HasFlag(AggregateFlags.MetaBehaviours))
            {
                Dictionary<Type, MetaBehaviour> dict = new Dictionary<Type, MetaBehaviour>();

                // IEnumerables reference source collections, so convert to a list
                // to make sure that when we clear behaviours, it doesn't affect combined
                List<MetaBehaviour> combined = behaviours.Concat(other.behaviours).ToList();
                behaviours.Clear(); // Clear after get combined

                foreach (MetaBehaviour behaviour in combined)
                {
                    if (behaviour is IAggregatable aggregatable)
                    {
                        Type type = behaviour.GetType();
                        dict[type] = (MetaBehaviour)(dict.ContainsKey(type) ? ((IAggregatable)dict[type]).Aggregate(aggregatable) : aggregatable.Seed());
                    }
                    else
                    {
                        behaviours.Add(behaviour);
                    }
                }

                behaviours.AddRange(dict.Values);
            }

            return this;
        }

        private void OnValidate()
        {
            string tempTag = TagUtil.GetTag(name);
            if (tag != tempTag)
            {
                tag = tempTag;
            }
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