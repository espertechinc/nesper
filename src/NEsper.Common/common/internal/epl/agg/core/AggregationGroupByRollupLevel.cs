///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public abstract class AggregationGroupByRollupLevel
    {
        public AggregationGroupByRollupLevel(
            int levelNumber,
            int levelOffset,
            int[] rollupKeys,
            DataInputOutputSerde subkeySerde)
        {
            LevelNumber = levelNumber;
            LevelOffset = levelOffset;
            RollupKeys = rollupKeys;
            SubkeySerde = subkeySerde;
        }

        public int LevelNumber { get; }

        public bool IsAggregationTop => LevelOffset == -1;

        public int AggregationOffset {
            get {
                if (IsAggregationTop) {
                    throw new ArgumentException();
                }

                return LevelOffset;
            }
        }

        public int LevelOffset { get; }

        public int[] RollupKeys { get; }

        public DataInputOutputSerde SubkeySerde { get; }

        public abstract object ComputeSubkey(object groupKey);

        public override string ToString()
        {
            return "GroupByRollupLevel{" +
                   "levelOffset=" +
                   LevelOffset +
                   ", rollupKeys=" +
                   RollupKeys.RenderAny() +
                   '}';
        }

        public object[] ComputeMultiKey(
            object subkey,
            int numExpected)
        {
            object[] keys;

            if (subkey is MultiKey mk) {
                if (mk.NumKeys == numExpected) {
                    return mk.ToObjectArray();
                }

                keys = new object[] { numExpected };
                for (var i = 0; i < RollupKeys.Length; i++) {
                    keys[RollupKeys[i]] = mk.GetKey(i);
                }

                return keys;
            }

            keys = new object[numExpected];
            if (subkey == null) {
                return keys;
            }

            keys[RollupKeys[0]] = subkey;
            return keys;
        }
    }

    public class ProxyAggregationGroupByRollupLevel : AggregationGroupByRollupLevel
    {
        private Func<object, object> ProcComputeSubkey { get; set; }

        public ProxyAggregationGroupByRollupLevel(
            int levelNumber,
            int levelOffset,
            int[] rollupKeys,
            DataInputOutputSerde subkeySerde,
            Func<object, object> procComputeSubkey)
            : base(levelNumber, levelOffset, rollupKeys, subkeySerde)
        {
            ProcComputeSubkey = procComputeSubkey;
        }

        public override object ComputeSubkey(object groupKey)
        {
            return ProcComputeSubkey?.Invoke(groupKey);
        }
    }
} // end of namespace