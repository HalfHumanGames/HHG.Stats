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

        public IReadOnlyList<StatMod> Mods => mods;
        public IReadOnlyDictionary<StatMod, float> Contributions => contributions;

        public event Action<float> Updated;

        [SerializeField] private float baseValue;

        private bool isCalculated;

        private float value;

        // Serialize so gets copied over with Object.Instantiate
        [SerializeField, HideInInspector] private Rounding rounding;
        [SerializeField, HideInInspector] private List<StatMod> mods = new List<StatMod>();
        [SerializeField, HideInInspector] private SerializedDictionary<StatMod, float> contributions = new SerializedDictionary<StatMod, float>();

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

        public void Clear()
        {
            mods.Clear();
            baseValue = 0f;
            UpdateValue();
        }

        public bool RemoveModsFromSource(object source)
        {
            // Use Equals instead of == since the latter compares boxed structs as references
            // instead of as valuess, causing it to return false even if it had the same value
            int removed = mods.RemoveAll(mod => mod.Source.Equals(source));

            if (removed > 0)
            {
                UpdateValue();
                return true;
            }

            return false;
        }

        // Public so can calculate value from a subset of stat mods
        // For example, to get value from just buffs/debuffs: 62 (↑20)
        // Contributions is an optional param for external invocations
        // We don't want to overwrite the actual contributions dictionary
        public float CalculateValue(Func<StatMod, bool> filter = null, IDictionary<StatMod, float> contributions = null)
        {
            float before;
            float value = baseValue;
            float percAdd = 0;

            contributions?.Clear();

            using(Pool.GetList(out List<StatMod> percAddMods))
            {
                for (int i = 0, len = mods.Count; i < len; i++)
                {
                    StatMod mod = mods[i];

                    if (filter?.Invoke(mod) == false) continue;

                    switch (mod.Type)
                    {
                        case StatModType.FlatAdd:
                            value += mod.Value;
                            contributions?.Add(mod, mod.Value);
                            break;

                        case StatModType.PercAdd:
                            percAdd += mod.Value;
                            percAddMods.Add(mod);

                            StatMod next = GetNextMod(i, len);
                            if (next == null || next.Type != StatModType.PercAdd)
                            {
                                before = value;
                                value *= 1 + percAdd;

                                foreach (StatMod percAddMod in percAddMods)
                                {
                                    float contribution = rounding.Round(before * percAddMod.Value);
                                    contributions?.Add(percAddMod, contribution);
                                }

                                percAdd = 0;
                                percAddMods.Clear();
                            }

                            break;

                        case StatModType.PercMult:
                            before = value;
                            value *= 1 + mod.Value;
                            contributions?.Add(mod, value - before);
                            break;
                    }
                }
            }

            return rounding.Round(value);

            StatMod GetNextMod(int i, int len)
            {
                for (int j = i + 1; j < len; j++)
                {
                    StatMod next = mods[j];

                    if (filter?.Invoke(next) != false)
                    {
                        return next;
                    }
                }

                return null;
            }
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

        private void UpdateValue()
        {
            value = CalculateValue(null, contributions);
            Updated?.Invoke(value);
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
