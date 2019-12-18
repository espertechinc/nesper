///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.compat
{
    public sealed class Cache2D<K, V> : ICache<K, V> where K : class
    {
        private Entry _entry1;
        private Entry _entry2;

        public Cache2D()
        {
            Invalidate();
        }

        public bool TryGet(K key, out V value)
        {
            Entry e = _entry1;
            if (key == e.Key)
            {
                value = e.Value;
                return true;
            }

            e = _entry2;
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
            Entry e = _entry1;
            if (key == e.Key) return e.Value;
            e = _entry2; if (key == e.Key) return e.Value;
            return default(V);
        }

        public V Put(K key, V value)
        {
            _entry1 = _entry2;
            _entry2 = new Entry(key, value);
            return value;
        }

        public void Invalidate()
        {
            _entry1 = _entry2 = new Entry(null, default(V));
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
