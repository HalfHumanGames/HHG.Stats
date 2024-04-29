using HHG.Common.Runtime;
using System;
using UnityEngine;

namespace HHG.StatSystem.Runtime
{
    [Serializable]
    public class StatsMod : StatMod
    {
        [SerializeField, Dropdown] private StatAsset stat;

        public StatsMod(StatAsset name, float value) : base(value) => stat = name;
        public StatsMod(StatAsset name, float value, StatModType type) : base(value, type) => stat = name;
        public StatsMod(StatAsset name, float value, StatModType type, object source) : base(value, type, source) => stat = name;
        public StatsMod(StatAsset name, float value, StatModType type, int order) : base(value, type, order) => stat = name;
        public StatsMod(StatAsset name, float value, StatModType type, object source, int order) : base(value, type, source, order) => stat = name;

        private enum Action
        {
            Add,
            Remove
        }

        public void Add(IStats stats)
        {
            Modify(stats, Action.Add);
        }

        public void Remove(IStats stats)
        {
            Modify(stats, Action.Remove);
        }

        private void Modify(IStats stats, Action action)
        {
            if (stat == null)
            {
                Debug.LogError($"Stat cannot be null.");
            }
            else
            {
                Stat s = stats[stat.name];
                if (s == null)
                {
                    Debug.LogError($"Stat not found: {stat.name}");
                }
                else
                {
                    switch (action)
                    {
                        case Action.Add: 
                            s.AddMod(this); 
                            break;
                        case Action.Remove: 
                            s.RemoveMod(this); 
                            break;
                    }
                }
            }
        }
    }


}