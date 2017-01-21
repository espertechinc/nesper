///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
    /// <summary>LRU cache settings. </summary>
    [Serializable]
    public class ConfigurationLRUCache : ConfigurationDataCache
    {
        /// <summary>Ctor. </summary>
        /// <param name="size">is the maximum cache size</param>
        public ConfigurationLRUCache(int size)
        {
            Size = size;
        }

        /// <summary>
        /// Gets the maximum cache size.
        /// </summary>
        /// <value>The size.</value>
        public int Size { get; private set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override String ToString()
        {
            return "LRUCacheDesc size=" + Size;
        }
    }
}
