using HHG.Common.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HHG.Stats.Runtime
{
    [Serializable]
    public class Stat : ICloneable<Stat>, ISerializationCallbackReceiver
    {
        public float Value
        {
            get
            {
                if (!isCalculated)
                {
                    isCalculated = true;
                    UpdateValue();
                }
                return value;
            }
        }

        public float BaseValue
        {
            get => baseValue;
            set
            {
                baseValue = rounding.Round(value);
                UpdateValue();
            }
        }

        public Rounding Rounding
        {
            get => rounding;
            set
            {
                rounding = value;
                UpdateValue();
            }
        }

        public event Action<float> Updated;

        [SerializeField] private float baseValue;

        private bool isCalculated;

        private float value;

        // Serialize so gets copied over with Object.Instantiate
        [SerializeField, HideInInspector] private Rounding rounding;
        [SerializeField, HideInInspector] private List<StatMod> mods = new List<StatMod>();


        public Stat(Rounding round = Rounding.None)
        {
            rounding = round;
            mods = new List<StatMod>();
        }

        public Stat(float val, Rounding round = Rounding.None) : this(round)
        {
            baseValue = val;
        }

        public void Flatten()
        {
            baseValue = value;
            mods.Clear();
            UpdateValue();
        }

        public void AddMod(StatMod add)
        {
            mods.Add(add);
            mods.Sort();
            UpdateValue();
        }

        public void AddMods(IEnumerable<StatMod> add)
        {
            mods.AddRange(add);
            mods.Sort();
            UpdateValue();
        }

        public bool RemoveMod(StatMod remove)
        {
            if (mods.Remove(remove))
            {
                UpdateValue();
                return true;
            }

            return false;
        }

        public void RemoveMods(IEnumerable<StatMod> remove)
        {
            mods.RemoveRange(remove);
            UpdateValue();
        }

        public void ClearMods()
        {
            mods.Clear();
            UpdateValue();
        }

        public bool RemoveModsFromSource(object source)
        {
            int removed = mods.RemoveAll(mod => mod.Source == source);

            if (removed > 0)
            {
                UpdateValue();
                return true;
            }

            return false;
        }

        // Public so can calculate value from a subset of stat mods
        // For example, to get value from just buffs/debuffs: 62 (↑20)
        public float CalculateValue(Func<StatMod, bool> filter = null)
        {
            float value = baseValue;
            float percAdd = 0;

            for (int i = 0; i < mods.Count; i++)
            {
                StatMod mod = mods[i];

                if (filter?.Invoke(mod) == false) continue;

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

            return rounding.Round(value);
        }

        private void UpdateValue()
        {
            value = CalculateValue();
            Updated?.Invoke(value);
        }

        public Stat Clone()
        {
            Stat clone = (Stat)MemberwiseClone();
            clone.mods = new List<StatMod>(mods.Select(m => m.Clone()));
            return clone;
        }

        public void OnBeforeSerialize()
        {
            // Do nothing
        }

        // In case part of an object cloned with JsonUtility
        public void OnAfterDeserialize()
        {
            mods ??= new List<StatMod>();
            UpdateValue();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public string ToString(string format)
        {
            return Value.ToString(format);
        }

        public static implicit operator float(Stat stat)
        {
            return stat.Value;
        }

        public static implicit operator int(Stat stat)
        {
            return (int)stat.Value;
        }
    }
}
