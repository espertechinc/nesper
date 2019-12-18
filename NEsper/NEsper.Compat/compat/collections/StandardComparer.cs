///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public class StandardComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> _finalComparison;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardComparer&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="finalComparison">The final comparison.</param>
        public StandardComparer(Func<T, T, int> finalComparison)
        {
            _finalComparison = finalComparison;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to,
        /// or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value
        /// Condition
        /// Less than zero
        /// <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero
        /// <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero
        /// <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        public int Compare(T x, T y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (Equals(x, y))
                return 0;
            return _finalComparison(x, y);
        }
    }
}
