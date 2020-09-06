using System;

namespace com.espertech.esper.compat.collections
{
    public class SmartCompare
    {
        public static int Compare<T>(
            T o1,
            T o2) where T : class, IComparable
        {
            if (o1 == o2)
                return 0;
            if (o1 == null)
                return 1;
            if (o2 == null)
                return -1;
            return o1.CompareTo(o2);
        }
        
        public static int Compare<T>(
            Nullable<T> o1,
            Nullable<T> o2) where T : struct, IComparable
        {
            if (o1.HasValue && o2.HasValue) {
                return o1.Value.CompareTo(o2.Value);
            } else if (o1.HasValue) {
                return -1;
            } else if (o2.HasValue) {
                return 1;
            }
            else {
                return 0;
            }
        }

        public static int Compare<T>(
            Nullable<T> o1,
            object o2) where T : struct, IComparable
        {
            if (o1.HasValue && o2 != null) {
                return o1.Value.CompareTo((T) o2);
            } else if (o1.HasValue) {
                return -1;
            } else if (o2 != null) {
                return 1;
            }
            else {
                return 0;
            }
        }
    }
}