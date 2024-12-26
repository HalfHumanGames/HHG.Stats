using HHG.Common.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HHG.StatSystem.Runtime
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
                    CalculateValue();
                }
                return value;
            }
        }

        public float BaseValue
        {
            get => baseValue;
            set
            {
                baseValue = value;
                CalculateValue();
            }
        }

        public Rounding Rounding
        {
            get => rounding;
            set
            {
                rounding = value;
                CalculateValue();
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
            CalculateValue();
        }

        public void AddMod(StatMod add)
        {
            mods.Add(add);
            mods.Sort();
            CalculateValue();
        }

        public void AddMods(IEnumerable<StatMod> add)
        {
            mods.AddRange(add);
            mods.Sort();
            CalculateValue();
        }

        public bool RemoveMod(StatMod remove)
        {
            if (mods.Remove(remove))
            {
                CalculateValue();
                return true;
            }

            return false;
        }

        public void RemoveMods(IEnumerable<StatMod> remove)
        {
            mods.RemoveRange(remove);
            CalculateValue();
        }

        public void ClearMods()
        {
            mods.Clear();
            CalculateValue();
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

            switch (rounding)
            {
                case Rounding.Round:
                    value = Mathf.Round(value);
                    break;
                case Rounding.Ceil:
                    value = Mathf.Ceil(value);
                    break;
                case Rounding.Floor:
                    value = Mathf.Floor(value);
                    break;
            }

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
            CalculateValue();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator float(Stat stat)
        {
            return stat.Value;
        }
    }
}
