using HHG.Common.Runtime;
using System;
using UnityEngine;

namespace HHG.StatSystem.Runtime
{
    [Serializable]
    public class StatsMod : StatMod
    {
        public string Stat => stat == null ? _stat : stat;

        [SerializeField, Dropdown] private StatAsset stat;

        private string _stat;

        public StatsMod(string name, float value) : base(value) => _stat = name;
        public StatsMod(string name, float value, StatModType type) : base(value, type) => _stat = name;
        public StatsMod(string name, float value, StatModType type, object source) : base(value, type, source) => _stat = name;
        public StatsMod(string name, float value, StatModType type, int order) : base(value, type, order) => _stat = name;
        public StatsMod(string name, float value, StatModType type, object source, int order) : base(value, type, source, order) => _stat = name;

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
            if (Stat == null)
            {
                Debug.LogError($"Stat cannot be null.");
            }
            else
            {
                Stat s = stats[Stat];
                if (s == null)
                {
                    Debug.LogError($"Stat not found: {Stat}");
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