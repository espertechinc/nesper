///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.agg.rollup
{
    public class GroupByRollupNodeRollupOrCube : GroupByRollupNodeBase
    {
        private readonly bool _cube;

        public GroupByRollupNodeRollupOrCube(bool cube)
        {
            _cube = cube;
        }

        public override IList<int[]> Evaluate(GroupByRollupEvalContext context)
        {
            int[][] childIndexes = EvaluateChildNodes(context);

            // find duplicate entries among child expressions
            for (int i = 0; i < childIndexes.Length; i++)
            {
                for (int j = i + 1; j < childIndexes.Length; j++)
                {
                    ValidateCompare(childIndexes[i], childIndexes[j]);
                }
            }

            IList<int[]> rollup;
            if (_cube)
            {
                rollup = HandleCube(childIndexes);
            }
            else
            {
                rollup = HandleRollup(childIndexes);
            }
            rollup.Add(new int[0]);
            return rollup;
        }

        private static void ValidateCompare(int[] one, int[] other)
        {
            if (Collections.AreEqual(one, other))
            {
                throw new GroupByRollupDuplicateException(one);
            }
        }

        private IList<int[]> HandleCube(int[][] childIndexes)
        {
            var enumerationSorted = new List<int[]>();
            var size = ChildNodes.Count;
            var e = new NumberAscCombinationEnumeration(size);
            while (e.MoveNext())
            {
                enumerationSorted.Add(e.Current);
            }


            enumerationSorted.SortInPlace(
                (o1, o2) =>
                {
                    int shared = Math.Min(o1.Length, o2.Length);
                    for (int i = 0; i < shared; i++)
                    {
                        if (o1[i] < o2[i])
                        {
                            return -1;
                        }
                        if (o1[i] > o2[i])
                        {
                            return 1;
                        }
                    }
                    if (o1.Length > o2.Length)
                    {
                        return -1;
                    }
                    if (o1.Length < o2.Length)
                    {
                        return 1;
                    }
                    return 0;
                });

            var rollup = new List<int[]>(enumerationSorted.Count + 1);
            var keys = new LinkedHashSet<int>();
            foreach (var item in enumerationSorted)
            {
                keys.Clear();
                foreach (int index in item)
                {
                    int[] childIndex = childIndexes[index];
                    foreach (int childIndexItem in childIndex)
                    {
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
            var rollup = new List<int[]>(size + 1);
            var keyset = new LinkedHashSet<int>();

            for (int i = 0; i < size; i++)
            {
                keyset.Clear();

                for (int j = 0; j < size - i; j++)
                {
                    int[] childIndex = childIndexes[j];
                    foreach (int aChildIndex in childIndex)
                    {
                        keyset.Add(aChildIndex);
                    }
                }
                rollup.Add(CollectionUtil.IntArray(keyset));
            }
            return rollup;
        }

        private int[][] EvaluateChildNodes(GroupByRollupEvalContext context)
        {
            int size = ChildNodes.Count;
            var childIndexes = new int[size][];
            for (int i = 0; i < size; i++)
            {
                var childIndex = ChildNodes[i].Evaluate(context);
                if (childIndex.Count != 1)
                {
                    throw new IllegalStateException();
                }
                childIndexes[i] = childIndex[0];
            }
            return childIndexes;
        }
    }
}