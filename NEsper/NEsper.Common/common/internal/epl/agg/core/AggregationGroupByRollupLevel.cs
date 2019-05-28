///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationGroupByRollupLevel
    {
        private readonly int levelOffset;

        public AggregationGroupByRollupLevel(
            int levelNumber,
            int levelOffset,
            int[] rollupKeys)
        {
            LevelNumber = levelNumber;
            this.levelOffset = levelOffset;
            RollupKeys = rollupKeys;
        }

        public int LevelNumber { get; }

        public bool IsAggregationTop => levelOffset == -1;

        public int[] RollupKeys { get; }

        public int AggregationOffset {
            get {
                if (IsAggregationTop) {
                    throw new ArgumentException();
                }

                return levelOffset;
            }
        }

        public CodegenExpression ToExpression()
        {
            return NewInstance<AggregationGroupByRollupLevel>(
                Constant(LevelNumber), Constant(levelOffset),
                Constant(RollupKeys));
        }

        public object ComputeSubkey(object groupKey)
        {
            if (IsAggregationTop) {
                return null;
            }

            if (groupKey is HashableMultiKey) {
                var mk = (HashableMultiKey) groupKey;
                var keys = mk.Keys;
                if (RollupKeys.Length == keys.Length) {
                    return mk;
                }

                if (RollupKeys.Length == 1) {
                    return keys[RollupKeys[0]];
                }

                var subkeys = new object[RollupKeys.Length];
                var count = 0;
                foreach (var rollupKey in RollupKeys) {
                    subkeys[count++] = keys[rollupKey];
                }

                return new HashableMultiKey(subkeys);
            }

            return groupKey;
        }

        public override string ToString()
        {
            return "GroupByRollupLevel{" +
                   "levelOffset=" + levelOffset +
                   ", rollupKeys=" + RollupKeys.RenderAny() +
                   '}';
        }

        public HashableMultiKey ComputeMultiKey(
            object subkey,
            int numExpected)
        {
            if (subkey is HashableMultiKey) {
                var mk = (HashableMultiKey) subkey;
                if (mk.Keys.Length == numExpected) {
                    return mk;
                }

                object[] keysInner = {numExpected};
                for (var i = 0; i < RollupKeys.Length; i++) {
                    keysInner[RollupKeys[i]] = mk.Keys[i];
                }

                return new HashableMultiKey(keysInner);
            }

            var keys = new object[numExpected];
            if (subkey == null) {
                return new HashableMultiKey(keys);
            }

            keys[RollupKeys[0]] = subkey;
            return new HashableMultiKey(keys);
        }
    }
} // end of namespace