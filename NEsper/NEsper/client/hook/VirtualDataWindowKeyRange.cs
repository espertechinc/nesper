///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// Provides a range as a start and end value, for use as a paramater to the lookup values passed to the
    /// <see cref="VirtualDataWindowLookup" /> lookup method. 
    /// <para/>
    /// Consult <see cref="VirtualDataWindowLookupOp" /> for information on the type of range represented (open, closed, inverted etc.) .
    /// </summary>
    public class VirtualDataWindowKeyRange
    {
        /// <summary>Ctor. </summary>
        /// <param name="start">range start</param>
        /// <param name="end">range end</param>
        public VirtualDataWindowKeyRange(Object start, Object end)
        {
            Start = start;
            End = end;
        }

        /// <summary>Returns the start value of the range. </summary>
        /// <value>start value</value>
        public object Start { get; private set; }

        /// <summary>Returns the end value of the range. </summary>
        /// <value>end value</value>
        public object End { get; private set; }

        public bool Equals(VirtualDataWindowKeyRange other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Start, Start) && Equals(other.End, End);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (VirtualDataWindowKeyRange)) return false;
            return Equals((VirtualDataWindowKeyRange) obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked {
                return ((Start != null ? Start.GetHashCode() : 0)*397) ^ (End != null ? End.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override String ToString() {
            return "VirtualDataWindowKeyRange{" +
                    "start=" + Start +
                    ", end=" + End +
                    '}';
        }
    }
}
