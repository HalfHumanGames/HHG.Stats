using HHG.Common.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HHG.StatSystem.Runtime
{
    public class ConditionTracker : MonoBehaviour
    {
        private const float infinite = -1;

        private IStats stats;
        private List<ConditionAsset> current = new List<ConditionAsset>();
        private List<MetaBehaviour[]> behaviours = new List<MetaBehaviour[]>();
        private List<float> timers = new List<float>();

        private void Awake()
        {
            stats = GetComponentInChildren<IStats>();
        }

        public void Apply(params ConditionAsset[] conditions)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                if (current.Any(c => c.Tag == conditions[i].Tag))
                {
                    continue;
                }

                current.Add(conditions[i]);
                behaviours.Add(gameObject.AddMetaBehaviours(conditions[i].Behaviours));
                timers.Add(conditions[i].Duration);

                if (stats != null)
                {
                    conditions[i].Apply(stats);
                }
            }
        }

        private void Update()
        {
            for (int i = 0; i < timers.Count; i++)
            {
                if (timers[i] == infinite)
                {
                    continue;
                }

                timers[i] -= Time.deltaTime;

                if (timers[i] <= 0)
                {
                    RemoveAt(i);
                    i--;
                }
            }
        }


        public void Remove(ConditionAsset condition)
        {
            int index = current.IndexOf(condition);

            if (index >= 0)
            {
                RemoveAt(index);
            }
        }

        public void RemoveAt(int index)
        {
            ConditionAsset condition = current[index];
            current.RemoveAt(index);
            behaviours[index].Destroy();
            behaviours.RemoveAt(index);
            timers.RemoveAt(index);

            if (stats != null)
            {
                condition.Remove(stats);
            }
        }
    }
}