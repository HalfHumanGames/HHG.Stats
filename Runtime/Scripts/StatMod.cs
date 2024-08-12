using HHG.Common.Runtime;
using System;
using UnityEngine;

namespace HHG.StatSystem.Runtime
{
    [Serializable]
    public class StatMod : IComparable<StatMod>, ICloneable<StatMod>
    {
        private const int defaultSortOrder = -1;

        public float Value => value;
        public StatModType Type => type;
        public object Source => source;
        public int SortOrder => sortOrder;

        [SerializeField] private float value;
        [SerializeField] private StatModType type;
        [SerializeField, HideInInspector] private int sortOrder;

        private object source;

        public StatMod(float modValue = 0, StatModType modType = StatModType.FlatAdd, object modSource = null, int modSortOrder = defaultSortOrder)
        {
            value = modValue;
            type = modType;
            source = modSource;
            sortOrder = modSortOrder == defaultSortOrder ? (int)modType : modSortOrder;
        }

        public int CompareTo(StatMod other) => SortOrder.CompareTo(other.SortOrder);

        public StatMod Clone()
        {
            return (StatMod) MemberwiseClone();
        }
    }
}
