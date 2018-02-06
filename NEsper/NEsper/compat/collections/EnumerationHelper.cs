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
    /// <summary>
    /// Collection of utilities specifically to help with enumeration.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumerationHelper<T>
    {
        /// <summary>
        /// Creates the empty enumerator.
        /// </summary>
        /// <returns></returns>
        public static IEnumerator<T> Empty()
        {
            return new NullEnumerator<T>();
        }

        /// <summary>
        /// Creates the singleton enumerator.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static IEnumerator<T> Singleton(T item)
        {
            yield return item;
        }

        /// <summary>
        /// Creates an enumerator that skips a number of items in the
        /// subEnumerator.
        /// </summary>
        /// <param name="subEnumerator">The child enumerator.</param>
        /// <param name="numToAdvance">The num to advance.</param>
        /// <returns></returns>
        public static IEnumerable<T> AdvanceEnumerable( IEnumerator<T> subEnumerator, int numToAdvance )
        {
            bool hasMore = true;

            for( int ii = 0 ; ii < numToAdvance ; ii++ ) {
                if (!subEnumerator.MoveNext()) {
                    hasMore = false;
                    break;
                }
            }

            if ( hasMore ) {
                while( subEnumerator.MoveNext() ) {
                    yield return subEnumerator.Current;
                }
            }
        }
    }
}
