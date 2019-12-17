///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     LRU cache settings.
    /// </summary>
    [Serializable]
    public class ConfigurationCommonCacheLRU : ConfigurationCommonCache
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="size">is the maximum cache size</param>
        public ConfigurationCommonCacheLRU(int size)
        {
            Size = size;
        }

        /// <summary>
        ///     Returns the maximum cache size.
        /// </summary>
        /// <returns>max cache size</returns>
        public int Size { get; }

        public override string ToString()
        {
            return "LRUCacheDesc size=" + Size;
        }
    }
} // end of namespace