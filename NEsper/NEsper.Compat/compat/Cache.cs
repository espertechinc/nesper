///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.compat
{
    public sealed class Cache<K, V> : ICache<K, V> where K : class
    {
        private Entry _entry;

        public Cache()
        {
            Invalidate();
        }


        public bool TryGet(K key, out V value)
        {
            Entry e = _entry;
            if (key == e.Key)
            {
                value = e.Value;
                return true;
            }

            value = default(V);
            return false;
        }

        public V Get(K key)
        {
            var e = _entry;
            return e.Key == key ? e.Value : default(V);
        }

        public V Put(K key, V value)
        {
            _entry = new Entry(key, value);
            return value;
        }

        public void Invalidate()
        {
            _entry = new Entry(null, default(V));
        }

        class Entry
        {
            public readonly K Key;
            public readonly V Value;

            public Entry(K key, V value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
