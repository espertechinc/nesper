using System;

namespace com.espertech.esper.compat
{
    public static class NullableExtensions
    {
        public static int CompareTo<T>(
            this Nullable<T> a,
            Nullable<T> b)
            where T : struct, IComparable
        {
            return a.HasValue 
                ? b.HasValue 
                    ? a.Value.CompareTo(b.Value) 
                    : -1
                : b.HasValue 
                    ? 1 
                    : 0;
        }
    }
}