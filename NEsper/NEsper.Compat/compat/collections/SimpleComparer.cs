///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.collections
{
    public class SimpleComparer<T> : StandardComparer<T>
    {
        public static SimpleComparer<T> Forward { get; private set; }
        public static SimpleComparer<T> Reverse { get; private set; } 

        static SimpleComparer()
        {
            Forward = new SimpleComparer<T>(true);
            Reverse = new SimpleComparer<T>(false);
        } 

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleComparer{T}"/> class.
        /// </summary>
        public SimpleComparer(bool ascending)
            : base(GetComparisonFunction(ascending))
        {
        }

        /// <summary>
        /// Gets the simple comparison function.
        /// </summary>
        /// <param name="ascending">if set to <c>true</c> [ascending].</param>
        /// <returns></returns>
        public static Func<T, T, int> GetComparisonFunction(bool ascending)
        {
            if (ascending)
            {
                return (x, y) => ((IComparable) x).CompareTo(y);
            }

            return (x, y) => -((IComparable)x).CompareTo(y);
        }
    }
}
