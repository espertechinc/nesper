///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.collection
{
    /// <summary>
    /// General-purpose pair of values of any type. The pair only equals another pair if
    /// the objects that form the pair equal, ie. first pair first object equals (.equals) the second pair first object,
    /// and the first pair second object equals the second pair second object.
    /// </summary>
    [Serializable]
    public sealed class Pair<TFirst, TSecond>
    {
        /// <summary>
        /// Gets or sets the first value within pair.
        /// </summary>
        /// <value>The first.</value>
        public TFirst First;

        /// <summary>
        /// Gets or sets the second value within pair.
        /// </summary>
        /// <value>The second.</value>
        public TSecond Second;

        /// <summary>
        /// Construct pair of values.
        /// </summary>
        /// <param name="first">is the first value</param>
        /// <param name="second">is the second value</param>

        public Pair(TFirst first, TSecond second)
        {
            First = first;
            Second = second;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pair&lt;TFirst, TSecond&gt;"/> class.
        /// </summary>
        /// <param name="keyValuePair">The key value pair.</param>
        public Pair(KeyValuePair<TFirst, TSecond> keyValuePair)
        {
            First = keyValuePair.Key;
            Second = keyValuePair.Value;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        public override bool Equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }

            var other = obj as Pair<TFirst, TSecond>;
            if (other == null)
            {
                return false;
            }

            return
                Equals(First, other.First) &&
                Equals(Second, other.Second);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
            Object o1 = First;
            Object o2 = Second;

            return
                (o1 != null ? o1.GetHashCode() * 397 : 0) ^
                (o2 != null ? o2.GetHashCode() : 0);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            return "Pair [" + First + ':' + Second + ']';
        }
    }
}
