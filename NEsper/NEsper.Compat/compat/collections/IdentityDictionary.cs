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
    /// An extended dictionary based upon a closed hashing
    /// algorithm.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>

	public class IdentityDictionary<K,V>
		: HashMap<K,V>
        where K : class
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityDictionary{K,V}"/> class.
        /// </summary>
		public IdentityDictionary()
			: base(new EqualityComparer())
		{
		}

        internal class EqualityComparer : IEqualityComparer<K>
        {
            /// <summary>
            /// Returns true if the two objects are equal.  In the case of the
            /// identity dictionary, equality is true only if the objects are
            /// the same reference.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>

            public bool Equals(K x, K y)
            {
                return x == y;
            }

            /// <summary>
            /// Returns a hashcode for the object.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>

            public int GetHashCode(K obj)
            {
                return obj.GetHashCode();
            }
        }
	}
}
