///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.collection
{

    /// <summary> General-purpose pair of values of any type. The pair equals another pair if
    /// the objects that form the pair equal in any order, ie. first pair first object equals (.equals)
    /// the second pair first object or second object, and the first pair second object equals the second pair first object
    /// or second object.
    /// </summary>
    public sealed class InterchangeablePair<FirstT, SecondT>
    {
        /// <summary>
        /// Gets or sets the first value within the pair.
        /// </summary>
        public FirstT First { get; set; }

        /// <summary>
        /// Gets or sets the second value within the pair.
        /// </summary>
        public SecondT Second { get; set; }

        /// <summary> Construct pair of values.</summary>
        /// <param name="first">is the first value</param>
        /// <param name="second">is the second value</param>

        public InterchangeablePair(FirstT first, SecondT second)
        {
            First = first;
            Second = second;
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

            var other = obj as InterchangeablePair<FirstT, SecondT>;
            if (other == null) {
                return false;
            }

            return
                (Equals(First, other.First) && Equals(Second, other.Second)) ||
                (Equals(First, other.Second) && Equals(Second, other.First));
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
            var o1 = First;
            var o2 = Second;

            var a1 = ReferenceEquals(o1, null);
            var a2 = ReferenceEquals(o2, null);

            if (a1 && a2) return 0;
            if (a1) return o2.GetHashCode();
            if (a2) return o1.GetHashCode();

            var h1 = o1.GetHashCode();
            var h2 = o2.GetHashCode();

            if (h1 > h2)
                return h1*397 ^ h2;
            else
                return h2*397 ^ h1;
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
