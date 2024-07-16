using System.Collections.Generic;

namespace HHG.StatSystem.Runtime
{
    public class ConditionAssetTagComparer : EqualityComparer<ConditionAsset>
    {
        public override bool Equals(ConditionAsset a, ConditionAsset b)
        {
            return a.Tag.Equals(b.Tag);
        }

        public override int GetHashCode(ConditionAsset a)
        {
            return a.Tag.GetHashCode();
        }
    }
}