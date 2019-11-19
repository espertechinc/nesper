///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public class SetUtil
    {
        /// <summary>
        /// Creates the union of two sets.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set1">The set1.</param>
        /// <param name="set2">The set2.</param>
        /// <returns></returns>
        public static ICollection<T> Union<T>(ICollection<T> set1, ICollection<T> set2)
        {
            ICollection<T> iset = new HashSet<T>();

            iset.AddAll(set1);
            iset.AddAll(set2);

            return iset;
        }

        /// <summary>
        /// Creates the intersection of two sets.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set1">The set1.</param>
        /// <param name="set2">The set2.</param>
        /// <returns></returns>
        public static ICollection<T> Intersect<T>( ICollection<T> set1, ICollection<T> set2 )
        {
            ICollection<T> iset = new HashSet<T>();

            if ((set1 != null) && (set2 != null)) {
                // Reversed the sets if set1 is larger than set2
                if (set1.Count > set2.Count) {
                    ICollection<T> temp = set1;
                    set2 = set1;
                    set1 = temp;
                }

                foreach (T item in set2) {
                    if (set1.Contains(item)) {
                        iset.Add(item);
                    }
                }
            }

            return iset;
        }
    }
}
