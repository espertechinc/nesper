using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public class ProxyComparer<T> : IComparer<T>
    {
        public Func<T, T, int> ProcCompare { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyComparer&lt;T&gt;"/> class.
        /// </summary>
        public ProxyComparer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyComparer&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="procCompare">The procCompare.</param>
        public ProxyComparer(Func<T, T, int> procCompare)
        {
            ProcCompare = procCompare;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// Value 
        ///                     Condition 
        ///                     Less than zero
        ///                 <paramref name="x"/> is less than <paramref name="y"/>.
        ///                     Zero
        ///                 <paramref name="x"/> equals <paramref name="y"/>.
        ///                     Greater than zero
        ///                 <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.
        ///                 </param><param name="y">The second object to compare.
        ///                 </param>
        public int Compare(T x, T y)
        {
            return ProcCompare.Invoke(x, y);
        }
    }
}
