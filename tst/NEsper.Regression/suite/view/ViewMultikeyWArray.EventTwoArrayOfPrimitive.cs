using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.suite.view
{
    public partial class ViewMultikeyWArray
    {
        [Serializable]
        public class EventTwoArrayOfPrimitive
        {
            private readonly string id;
            private readonly int[] one;
            private readonly int[] two;

            public EventTwoArrayOfPrimitive(
                string id,
                int[] one,
                int[] two)
            {
                this.id = id;
                this.one = one;
                this.two = two;
            }

            public string Id => id;

            public int[] One => one;

            public int[] Two => two;

            protected bool Equals(EventTwoArrayOfPrimitive other)
            {
                return id == other.id && Arrays.AreEqual(one, other.one) && Arrays.AreEqual(two, other.two);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }

                if (ReferenceEquals(this, obj)) {
                    return true;
                }

                if (obj.GetType() != this.GetType()) {
                    return false;
                }

                return Equals((EventTwoArrayOfPrimitive) obj);
            }

            public override int GetHashCode()
            {
                unchecked {
                    var hashCode = (id != null ? id.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (one != null ? one.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (two != null ? two.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
    }
}