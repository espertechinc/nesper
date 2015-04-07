///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.agg.service
{
    [Serializable]
    public class AggregationGroupByRollupLevel
    {
        private readonly int _levelOffset;

        public AggregationGroupByRollupLevel(int levelNumber, int levelOffset, int[] rollupKeys)
        {
            LevelNumber = levelNumber;
            _levelOffset = levelOffset;
            RollupKeys = rollupKeys;
        }

        public int LevelNumber { get; private set; }

        public int AggregationOffset
        {
            get
            {
                if (IsAggregationTop)
                {
                    throw new ArgumentException();
                }
                return _levelOffset;
            }
        }

        public bool IsAggregationTop
        {
            get { return _levelOffset == -1; }
        }

        public int[] RollupKeys { get; private set; }

        public Object ComputeSubkey(Object groupKey)
        {
            if (IsAggregationTop)
            {
                return null;
            }
            if (groupKey is MultiKeyUntyped)
            {
                var mk = (MultiKeyUntyped) groupKey;
                var keys = mk.Keys;
                if (RollupKeys.Length == keys.Length)
                {
                    return mk;
                }
                else if (RollupKeys.Length == 1)
                {
                    return keys[RollupKeys[0]];
                }
                else
                {
                    var subkeys = new Object[RollupKeys.Length];
                    var count = 0;
                    foreach (var rollupKey in RollupKeys)
                    {
                        subkeys[count++] = keys[rollupKey];
                    }
                    return new MultiKeyUntyped(subkeys);
                }
            }
            else
            {
                return groupKey;
            }
        }

        public override String ToString()
        {
            return "GroupByRollupLevel{" +
                   "levelOffset=" + _levelOffset +
                   ", rollupKeys=" + RollupKeys.Render() +
                   '}';
        }

        public MultiKeyUntyped ComputeMultiKey(Object subkey, int numExpected)
        {
            var rollupKeys = RollupKeys;

            if (subkey is MultiKeyUntyped)
            {
                var mk = (MultiKeyUntyped) subkey;
                if (mk.Keys.Length == numExpected) {
                    return mk;
                }
                var keysInner = new Object[] {numExpected};
                for (var i = 0; i < rollupKeys.Length; i++) {
                    keysInner[rollupKeys[i]] = mk.Keys[i];
                }
                return new MultiKeyUntyped(keysInner);
            }
            var keys = new Object[numExpected];
            if (subkey == null) {
                return new MultiKeyUntyped(keys);
            }
            keys[rollupKeys[0]] = subkey;
            return new MultiKeyUntyped(keys);
        }
    }
}