///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.compat
{
    public class NoCache<K,V> : ICache<K,V> where K : class
    {
        public bool TryGet(K key, out V value)
        {
            value = default(V);
            return false;
        }

        public V Get(K key)
        {
            return default(V);
        }

        public V Put(K key, V value)
        {
            return value;
        }

        public void Invalidate()
        {
        }
    }
}
