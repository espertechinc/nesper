using System.Collections.Generic;

namespace com.espertech.esper.compat.collections.bound
{
    public class Bound<TV>
    {
        public TV Value;
        public bool IsInclusive;

        public Bound(
            TV value,
            bool isInclusive)
        {
            Value = value;
            IsInclusive = isInclusive;
        }
    }
}