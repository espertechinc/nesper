using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.index.hash
{
    /// <summary>
    ///     AsymmetricEqualityComparer is an equality comparer that tests for cases where
    ///     the equality method is implemented asymmetrically, meaning a.equals(b) returns
    ///     false, but b.equals(a) returns true.  This is needed since data structures
    ///     provide no guarantees about which value will be passed to the equality comparer
    ///     first.
    ///     <para>
    ///         Note: We consider this behavior a broken behavior in Esper.
    ///     </para>
    /// </summary>
    public class AsymmetricEqualityComparer : IEqualityComparer<object>
    {
        public static readonly AsymmetricEqualityComparer INSTANCE = new AsymmetricEqualityComparer();

        /// <summary>
        ///     Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(
            object x,
            object y)
        {
            if (x == y) {
                return true;
            }

            if (x == null || y == null) {
                return false;
            }

            return x.Equals(y) || y.Equals(x);
        }

        /// <summary>
        ///     Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(object obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }
}