using System;
using UnityEngine;

namespace HHG.StatSystem.Runtime
{
    [Serializable]
    public class StatMod : IComparable<StatMod>
    {
        public float Value => value;
        public StatModType Type => type;
        public object Source => source;
        public int Order => order;

        [SerializeField] private float value;
        [SerializeField] private StatModType type;
        [SerializeField] private int order;

        private object source;

        public StatMod(float value, StatModType type, object source, int order)
        {
            this.value = value;
            this.type = type;
            this.source = source;
            this.order = order;
        }

        public StatMod(float value) : this(value, StatModType.Flat, null, (int)StatModType.Flat) { }
        public StatMod(float value, StatModType type) : this(value, type, null, (int)type) { }
        public StatMod(float value, StatModType type, object source) : this(value, type, source, (int)type) { }
        public StatMod(float value, StatModType type, int order) : this(value, type, null, order) { }

        public int CompareTo(StatMod other) => Order.CompareTo(other.Order);
    }
}
