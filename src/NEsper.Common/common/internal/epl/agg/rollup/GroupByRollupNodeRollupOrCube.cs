///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.rollup
{
    public class GroupByRollupNodeRollupOrCube : GroupByRollupNodeBase
    {
        private readonly bool cube;

        public GroupByRollupNodeRollupOrCube(bool cube)
        {
            this.cube = cube;
        }

        public override IList<int[]> Evaluate(GroupByRollupEvalContext context)
        {
            var childIndexes = EvaluateChildNodes(context);

            // find duplicate entries among child expressions
            for (var i = 0; i < childIndexes.Length; i++) {
                for (var j = i + 1; j < childIndexes.Length; j++) {
                    ValidateCompare(childIndexes[i], childIndexes[j]);
                }
            }

            IList<int[]> rollup;
            if (cube) {
                rollup = HandleCube(childIndexes);
            }
            else {
                rollup = HandleRollup(childIndexes);
            }

            rollup.Add(Array.Empty<int>());
            return rollup;
        }

        private static void ValidateCompare(
            int[] one,
            int[] other)
        {
            if (one.AreEqual(other)) {
                throw new GroupByRollupDuplicateException(one);
            }
        }

        private IList<int[]> HandleCube(int[][] childIndexes)
        {
            IList<int[]> enumerationSorted = new List<int[]>();
            var size = ChildNodes.Count;
            var e = new NumberAscCombinationEnumeration(size);
            while (e.MoveNext()) {
                enumerationSorted.Add(e.Current);
            }

            enumerationSorted.SortInPlace(
                new ProxyComparer<int[]> {
                    ProcCompare = (
                        o1,
                        o2) => {
                        var shared = Math.Min(o1.Length, o2.Length);
                        for (var i = 0; i < shared; i++) {
                            if (o1[i] < o2[i]) {
                                return -1;
                            }

                            if (o1[i] > o2[i]) {
                                return 1;
                            }
                        }

                        if (o1.Length > o2.Length) {
                            return -1;
                        }

                        if (o1.Length < o2.Length) {
                            return 1;
                        }

                        return 0;
                    }
                });

            IList<int[]> rollup = new List<int[]>(enumerationSorted.Count + 1);
            ISet<int> keys = new LinkedHashSet<int>();
            foreach (var item in enumerationSorted) {
                keys.Clear();
                foreach (var index in item) {
                    var childIndex = childIndexes[index];
                    foreach (var childIndexItem in childIndex) {
                        keys.Add(childIndexItem);
                    }
                }

                rollup.Add(CollectionUtil.IntArray(keys));
            }

            return rollup;
        }

        private IList<int[]> HandleRollup(int[][] childIndexes)
        {
            var size = ChildNodes.Count;
            IList<int[]> rollup = new List<int[]>(size + 1);
            ISet<int> keyset = new LinkedHashSet<int>();

            for (var i = 0; i < size; i++) {
                keyset.Clear();

                for (var j = 0; j < size - i; j++) {
                    var childIndex = childIndexes[j];
                    foreach (var aChildIndex in childIndex) {
                        keyset.Add(aChildIndex);
                    }
                }

                rollup.Add(CollectionUtil.IntArray(keyset));
            }

            return rollup;
        }

        private int[][] EvaluateChildNodes(GroupByRollupEvalContext context)
        {
            var size = ChildNodes.Count;
            var childIndexes = new int[size][];
            for (var i = 0; i < size; i++) {
                var childIndex = ChildNodes[i].Evaluate(context);
                if (childIndex.Count != 1) {
                    throw new IllegalStateException();
                }

                childIndexes[i] = childIndex[0];
            }

            return childIndexes;
        }
    }
} // end of namespace