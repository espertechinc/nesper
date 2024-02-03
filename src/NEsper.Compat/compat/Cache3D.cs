///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compat
{
    public sealed class Cache3D<TK, TV> : ICache<TK, TV> where TK : class
    {
        private Entry _entry1;
        private Entry _entry2;
        private Entry _entry3;

        public Cache3D()
        {
            Invalidate();
        }

        public bool TryGet(TK key, out TV value)
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

            e = _entry3; 
            if (key == e.Key)
            {
                value = e.Value;
                return true;
            }

            value = default(TV);
            return false;
        }

        public TV Get(TK key)
        {
            Entry e = _entry1;
            if (key == e.Key) return e.Value;
            e = _entry2; if (key == e.Key) return e.Value;
            e = _entry3; if (key == e.Key) return e.Value;
            return default(TV);
        }

        public TV Put(TK key, TV value)
        {
            _entry1 = _entry2;
            _entry2 = _entry3;
            _entry3 = new Entry(key, value);
            return value;
        }

        public void Invalidate()
        {
            _entry1 = _entry2 = _entry3 = new Entry(null, default(TV));
        }

        class Entry
        {
            public TK Key;
            public TV Value;

            public Entry(TK key, TV value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
