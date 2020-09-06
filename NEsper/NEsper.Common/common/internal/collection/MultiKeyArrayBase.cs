///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.collection
{
    public class MultiKeyArrayBase<T> : MultiKeyArrayWrap
    {
        private readonly T[] _keys;
        private readonly int _hash;
        
        public MultiKeyArrayBase(T[] keys)
        {
            _keys = keys;
            _hash = CompatExtensions.DeepHash(keys);
        }

        public T[] Keys => _keys;

        public object Array => _keys;

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (MultiKeyArrayBase<T>) o;
            return Arrays.AreEqual(Keys, that.Keys);
        }

        public override int GetHashCode() => _hash;

        public override string ToString()
        {
            var type = GetType().Name;
            var render = Keys.RenderAny();
            return $"{type}{{keys={render}{'}'}";
        }
    }
} // end of namespace