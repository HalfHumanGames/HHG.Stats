using HHG.Common.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HHG.StatSystem.Runtime
{
    [Serializable]
    public class Stat : ICloneable<Stat>
    {
        [SerializeField] private float baseValue;

        private bool isCalculated;

        private float value;
        // Serialize mods so get copied over with Object.Instantiate
        [SerializeField, HideInInspector] private List<StatMod> mods = new List<StatMod>();

        public float Value
        {
            get
            {
                if (!isCalculated)
                {
                    isCalculated = true;
                    CalculateValue();
                }
                return value;
            }
        }

        public event Action<float> Updated;

        public Stat()
        {
            mods = new List<StatMod>();
        }

        public Stat(float val) : this()
        {
            baseValue = val;
        }

        public void AddMod(StatMod mod)
        {
            mods.Add(mod);
            mods.Sort();
            CalculateValue();
        }

        public bool RemoveMod(StatMod mod)
        {
            if (mods.Remove(mod))
            {
                CalculateValue();
                return true;
            }

            return false;
        }

        public bool RemoveModsFromSource(object source)
        {
            int removed = mods.RemoveAll(mod => mod.Source == source);

            if (removed > 0)
            {
                CalculateValue();
                return true;
            }

            return false;
        }

        private void CalculateValue()
        {
            value = baseValue;

            float percAdd = 0;

            for (int i = 0; i < mods.Count; i++)
            {
                StatMod mod = mods[i];

                switch (mod.Type)
                {
                    case StatModType.FlatAdd:
                        value += mod.Value;
                        break;
                    case StatModType.PercAdd:
                        percAdd += mod.Value;
                        if (i == mods.Count - 1 || mods[i + 1].Type != StatModType.PercAdd)
                        {
                            value *= 1 + percAdd;
                            percAdd = 0;
                        }
                        break;
                    case StatModType.PercMult:
                        value *= 1 + mod.Value;
                        break;
                }
            }

            Updated?.Invoke(value);
        }

        public Stat Clone()
        {
            Stat clone = (Stat)MemberwiseClone();
            clone.mods = new List<StatMod>(mods);
            return clone;
        }

        public static implicit operator float(Stat stat)
        {
            return stat.Value;
        }
    }
}
