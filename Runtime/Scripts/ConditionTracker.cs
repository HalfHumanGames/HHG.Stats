using HHG.Common.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace HHG.StatSystem.Runtime
{
    public class ConditionTracker : MonoBehaviour
    {
        private const float infinite = -1;

        private IStats stats;
        private List<string> tags = new List<string>();
        private Dictionary<string, ConditionAsset> current = new Dictionary<string, ConditionAsset>();
        private Dictionary<string, MetaBehaviour[]> behaviours = new Dictionary<string, MetaBehaviour[]>();
        private Dictionary<string, float> timers = new Dictionary<string, float>();

        private void Awake()
        {
            stats = GetComponentInChildren<IStats>();

            if (stats == null)
            {
                Destroy(this);
            }
        }

        public void Apply(params ConditionAsset[] conditions)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                string tag = conditions[i].Tag;

                if (HasCondition(conditions[i]))
                {
                    if (conditions[i].Priority > current[tag].Priority)
                    {
                        Remove(current[tag]);
                        Apply(conditions[i]);
                    }
                    else if (conditions[i].Priority == current[tag].Priority)
                    {
                        timers[tag] = conditions[i].Duration;
                    }
                }
                else
                {
                    tags.Add(tag);
                    current[tag] = conditions[i];
                    behaviours[tag] = gameObject.AddMetaBehaviours(conditions[i].Behaviours);
                    timers[tag] = conditions[i].Duration;
                    conditions[i].Apply(stats);
                    
                }
            }
        }

        private void Update()
        {
            for (int i = 0; i < tags.Count; i++)
            {
                string tag = tags[i];

                if (timers[tag] == infinite)
                {
                    continue;
                }

                timers[tag] -= Time.deltaTime;

                if (timers[tag] <= 0)
                {
                    Remove(current[tag]);
                    i--;
                }
            }
        }

        public void Remove(params ConditionAsset[] conditions)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                if (HasCondition(conditions[i]))
                {
                    string tag = conditions[i].Tag;
                    tags.Remove(tag);
                    current.Remove(tag);
                    behaviours[tag].Destroy();
                    behaviours.Remove(tag);
                    timers.Remove(tag);
                    conditions[i].Remove(stats);
                }
            }
        }

        public bool HasCondition(ConditionAsset condition)
        {
            return current.ContainsKey(condition.Tag);
        }
    }
}