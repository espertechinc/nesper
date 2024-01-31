///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Collection that maps a string to a string.
    /// </summary>

    public class Properties : Dictionary<string,string>
	{
        public Properties() {}

        protected Properties(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public Properties Copy()
        {
            Properties clone = new Properties();
            clone.PutAll(this);
            return clone;
        }

        /// <summary>
        /// Compares one properties set to another.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(Properties other)
        {
            if (Count != other.Count)
                return false;

            return !(Keys.Any(k => !other.Keys.Contains(k) || !Equals(this[k], other[k])));
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Properties)) return false;
            return Equals((Properties) obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Keys.Aggregate(0, (current, key) => current*397 + key.GetHashCode());
        }
	}
}
