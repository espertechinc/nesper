using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.suite.view
{
    public partial class ViewMultikeyWArray
    {
        public class EventTwoDimArray
        {
            private readonly string _id;
            private readonly int[][] _array;

            public EventTwoDimArray(
                string id,
                int[][] array)
            {
                _id = id;
                _array = array;
            }

            public string Id => _id;

            public int[][] Array => _array;

            protected bool Equals(EventTwoDimArray other)
            {
                return _id == other._id && Arrays.DeepEquals(_array, other._array);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }

                if (ReferenceEquals(this, obj)) {
                    return true;
                }

                if (obj.GetType() != GetType()) {
                    return false;
                }

                return Equals((EventTwoDimArray)obj);
            }

            public override int GetHashCode()
            {
                unchecked {
                    return ((_id != null ? _id.GetHashCode() : 0) * 397) ^ (_array != null ? _array.GetHashCode() : 0);
                }
            }
        }
    }
}