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
        private List<object> owners = new List<object>();
        private List<ConditionAsset> current = new List<ConditionAsset>();
        private List<MetaBehaviour[]> behaviours = new List<MetaBehaviour[]>();
        private List<float> timers = new List<float>();

        private void Awake()
        {
            stats = GetComponentInChildren<IStats>();
        }

        public bool HasCondition(string tag)
        {
            return current.Any(c => c.Tag == tag);
        }

        public void Apply(object owner, params ConditionAsset[] conditions)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                // Removing current conditions with the same tag
                for (int j = 0; j < current.Count; j++)
                {
                    if (current[i].Tag == conditions[i].Tag)
                    {
                        RemoveAt(i);
                    }
                }

                // Disable game object while setting up to make sure
                // no issues occur since MetaBehaviours may reference
                // Conditiontracker in Awake to get owner and whatnot
                bool wasActive = gameObject.activeSelf;
                gameObject.SetActive(false);

                owners.Add(owner);
                current.Add(conditions[i]);
                behaviours.Add(gameObject.AddMetaBehaviours(conditions[i].Behaviours));
                timers.Add(conditions[i].Duration);

                if (stats != null)
                {
                    conditions[i].Apply(stats);
                }

                // Enable game object if it was previously active
                gameObject.SetActive(wasActive);
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


        public void Remove(object owner, ConditionAsset condition)
        {
            int index = -1;

            for (int i = 0; i < current.Count; i++)
            {
                if (owners[i] == owner && current[i] == condition)
                {
                    index = i;
                    break;
                }
            }

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
            owners.RemoveAt(index);

            if (stats != null)
            {
                condition.Remove(stats);
            }
        }

        public GameObject GetOwner(MetaBehaviour metaBehaviour)
        {
            return GetOwner<GameObject>(metaBehaviour);
        }

        public T GetOwner<T>(MetaBehaviour metaBehaviour)
        {
            T owner;
            TryGetOwner(metaBehaviour, out owner);
            return owner;
        }

        public bool TryGetOwner(MetaBehaviour metaBehaviour, out GameObject owner)
        {
            return TryGetOwner<GameObject>(metaBehaviour, out owner);
        }

        public bool TryGetOwner<T>(MetaBehaviour metaBehaviour, out T owner)
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours[i].Contains(metaBehaviour) && owners[i] is T val)
                {
                    owner = val;
                    return true;
                }
            }

            owner = default;
            return false;
        }

        public ConditionAsset GetCondition(MetaBehaviour metaBehaviour)
        {
            ConditionAsset condition;
            TryGetCondition(metaBehaviour, out condition);
            return condition;
        }

        public bool TryGetCondition(MetaBehaviour metaBehaviour, out ConditionAsset condition)
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours[i].Contains(metaBehaviour))
                {
                    condition = current[i];
                    return true;
                }
            }

            condition = default;
            return false;
        }

        public bool RemoveConditionOf(MetaBehaviour metaBehaviour)
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours[i].Contains(metaBehaviour))
                {
                    RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
    }
}