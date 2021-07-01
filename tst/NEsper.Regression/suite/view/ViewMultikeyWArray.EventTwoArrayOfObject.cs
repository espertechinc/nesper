using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.suite.view
{
    public partial class ViewMultikeyWArray
    {
        [Serializable]
        public class EventTwoArrayOfObject
        {
            private readonly string id;
            private readonly object[] one;
            private readonly object[] two;

            public EventTwoArrayOfObject(
                string id,
                object[] one,
                object[] two)
            {
                this.id = id;
                this.one = one;
                this.two = two;
            }

            public string Id => id;

            public object[] One => one;

            public object[] Two => two;

            protected bool Equals(EventTwoArrayOfObject other)
            {
                return id == other.id && Arrays.DeepEquals(one, other.one) && Arrays.DeepEquals(two, other.two);
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

                return Equals((EventTwoArrayOfObject) obj);
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