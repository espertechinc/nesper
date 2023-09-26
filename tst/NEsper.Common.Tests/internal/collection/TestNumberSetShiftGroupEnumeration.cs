///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestNumberSetShiftGroupEnumeration : AbstractCommonTest
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TestGen()
        {
            Assert.AreEqual(29, CountEnumeration(new int[] { 1, 2, 3, 4, 5, 6 }));
            Assert.AreEqual(31, CountEnumeration(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }));

            int[] set = new int[] { 1, 2, 3, 4, 5, 6, 7 };

            int[][] expectedValues = new int[][]{
                    new int[] {1, 2, 3, 4, 5, 6, 7},
                    new int[] {2, 3, 4, 5, 6, 7, 1},
                    new int[] {3, 4, 5, 6, 7, 1, 2},
                    new int[] {4, 5, 6, 7, 1, 2, 3},
                    new int[] {5, 6, 7, 1, 2, 3, 4},
                    new int[] {6, 7, 1, 2, 3, 4, 5},
                    new int[] {7, 1, 2, 3, 4, 5, 6},
                    new int[] {1, 5, 2, 6, 4, 3, 7},
                    new int[] {1, 5, 3, 7, 2, 6, 4},
                    new int[] {1, 5, 3, 7, 4, 2, 6},
                    new int[] {1, 5, 4, 2, 6, 3, 7},
                    new int[] {1, 5, 4, 3, 7, 2, 6},
                    new int[] {2, 6, 1, 5, 3, 7, 4},
                    new int[] {2, 6, 1, 5, 4, 3, 7},
                    new int[] {2, 6, 3, 7, 1, 5, 4},
                    new int[] {2, 6, 3, 7, 4, 1, 5},
                    new int[] {2, 6, 4, 1, 5, 3, 7},
                    new int[] {2, 6, 4, 3, 7, 1, 5},
                    new int[] {3, 7, 1, 5, 2, 6, 4},
                    new int[] {3, 7, 1, 5, 4, 2, 6},
                    new int[] {3, 7, 2, 6, 1, 5, 4},
                    new int[] {3, 7, 2, 6, 4, 1, 5},
                    new int[] {3, 7, 4, 1, 5, 2, 6},
                    new int[] {3, 7, 4, 2, 6, 1, 5},
                    new int[] {4, 1, 5, 2, 6, 3, 7},
                    new int[] {4, 1, 5, 3, 7, 2, 6},
                    new int[] {4, 2, 6, 1, 5, 3, 7},
                    new int[] {4, 2, 6, 3, 7, 1, 5},
                    new int[] {4, 3, 7, 1, 5, 2, 6},
                    new int[] {4, 3, 7, 2, 6, 1, 5},
            };

            // NumberSetShiftGroupEnumeration enumeration = new NumberSetShiftGroupEnumeration(set);
            // while(enumeration.hasMoreElements()) {
            // Console.WriteLine(Arrays.toString(enumeration.nextElement()));
            // }

            TryPermutation(set, expectedValues);
        }

        private int CountEnumeration(int[] numberSet)
        {
            var enumeration = NumberSetShiftGroupEnumeration.New(numberSet).GetEnumerator();
            int count = 0;
            while (enumeration.MoveNext())
            {
                int[] result = enumeration.Current;
                AssertSet(numberSet, result);
                count++;
            }
            return count;
        }

        private void TryPermutation(int[] numberSet, int[][] expectedValues)
        {
            var enumeration = NumberSetShiftGroupEnumeration.New(numberSet).GetEnumerator();

            int count = 0;
            while (enumeration.MoveNext())
            {
                Log.Debug(".tryPermutation count=" + count);

                int[] result = enumeration.Current;
                int[] expected = expectedValues[count];

                Log.Debug(".tryPermutation result=" + result.RenderAny());
                Log.Debug(".tryPermutation expected=" + expected.Render());

                AssertSet(expected, result);

                count++;
                Assert.IsTrue(Arrays.AreEqual(result, expected), "Mismatch in count=" + count);
            }
            Assert.AreEqual(count, expectedValues.Length);
            Assert.That(enumeration.MoveNext(), Is.False);
        }

        private void AssertSet(int[] expected, int[] result)
        {
            var treeExp = GetTreeSet(expected);
            var treeRes = GetTreeSet(result);
            EPAssertionUtil.AssertEqualsExactOrder(GetArr(treeRes), GetArr(treeExp));
        }

        private int[] GetArr(ICollection<int> set)
        {
            int[] arr = new int[set.Count];
            int count = 0;
            foreach (int val in set)
            {
                arr[count++] = val;
            }
            return arr;
        }

        private ICollection<int> GetTreeSet(int[] set)
        {
            var treeSet = new SortedSet<int>();
            foreach (int aSet in set)
            {
                treeSet.Add(aSet);
            }
            return treeSet;
        }
    }
} // end of namespace
