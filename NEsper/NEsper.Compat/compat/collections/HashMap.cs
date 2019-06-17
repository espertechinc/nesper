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
    /// <summary>
    /// An extended dictionary based upon a closed hashing
    /// algorithm.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>

    [Serializable]
	public class HashMap<K,V> : BaseMap<K,V>
        where K : class
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="HashMap{K,V}"/> class.
        /// </summary>
		public HashMap()
			: base( new Dictionary<K,V>() )
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="HashMap{K,V}"/> class.
        /// </summary>
        public HashMap(int initialCapacity)
            : base(new Dictionary<K, V>(initialCapacity))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashMap{K,V}"/> class.
        /// </summary>
		
		public HashMap(IEqualityComparer<K> eqComparer)
			: base( new Dictionary<K,V>( eqComparer ) )
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="HashMap&lt;K, V&gt;"/> class.
        /// </summary>
        /// <param name="subDictionary">The sub dictionary.</param>
        public HashMap(IDictionary<K, V> subDictionary) : base(subDictionary)
        {
        }
	}
}
