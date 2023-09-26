///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.collection
{
    public sealed class MultiKeyInt
    {
        private readonly int[] _keys;

        public MultiKeyInt(int[] keys)
        {
            _keys = keys;
        }

        public int[] Keys => _keys;

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (MultiKeyInt)o;
            return Collections.AreEqual(_keys, that._keys);
        }

        public override int GetHashCode()
        {
            if (_keys.Length == 0) {
                return 0;
            }

            return System.Linq.Enumerable.Aggregate(
                _keys,
                0,
                (
                    a,
                    b) => a ^ b.GetHashCode());
        }
    }
}