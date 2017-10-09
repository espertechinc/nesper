///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestNumberSetShiftGroupEnumeration 
    {
        [Test]
        public void TestGen() {
    
            Assert.AreEqual(29, CountEnumeration(new [] {1, 2, 3, 4, 5, 6}));
            Assert.AreEqual(31, CountEnumeration(new [] {1, 2, 3, 4, 5, 6, 7, 8}));
    
            int[] set = new [] {1, 2, 3, 4, 5, 6, 7};
    
            int[][] expectedValues = new [] {
                    new []{1, 2, 3, 4, 5, 6, 7},
                    new []{2, 3, 4, 5, 6, 7, 1},
                    new []{3, 4, 5, 6, 7, 1, 2},
                    new []{4, 5, 6, 7, 1, 2, 3},
                    new []{5, 6, 7, 1, 2, 3, 4},
                    new []{6, 7, 1, 2, 3, 4, 5},
                    new []{7, 1, 2, 3, 4, 5, 6},
                    new []{1, 5, 2, 6, 4, 3, 7},
                    new []{1, 5, 3, 7, 2, 6, 4},
                    new []{1, 5, 3, 7, 4, 2, 6},
                    new []{1, 5, 4, 2, 6, 3, 7},
                    new []{1, 5, 4, 3, 7, 2, 6},
                    new []{2, 6, 1, 5, 3, 7, 4},
                    new []{2, 6, 1, 5, 4, 3, 7},
                    new []{2, 6, 3, 7, 1, 5, 4},
                    new []{2, 6, 3, 7, 4, 1, 5},
                    new []{2, 6, 4, 1, 5, 3, 7},
                    new []{2, 6, 4, 3, 7, 1, 5},
                    new []{3, 7, 1, 5, 2, 6, 4},
                    new []{3, 7, 1, 5, 4, 2, 6},
                    new []{3, 7, 2, 6, 1, 5, 4},
                    new []{3, 7, 2, 6, 4, 1, 5},
                    new []{3, 7, 4, 1, 5, 2, 6},
                    new []{3, 7, 4, 2, 6, 1, 5},
                    new []{4, 1, 5, 2, 6, 3, 7},
                    new []{4, 1, 5, 3, 7, 2, 6},
                    new []{4, 2, 6, 1, 5, 3, 7},
                    new []{4, 2, 6, 3, 7, 1, 5},
                    new []{4, 3, 7, 1, 5, 2, 6},
                    new []{4, 3, 7, 2, 6, 1, 5},
            };
    
            /** Comment in here to print */
#if false
            var enumeration = NumberSetShiftGroupEnumeration.Create(set).GetEnumerator();
            while(enumeration.MoveNext()) {
                Console.WriteLine(enumeration.Current.RenderAny());
            }
#endif
    
            TryPermutation(set, expectedValues);
        }
    
        private int CountEnumeration(int[] numberSet) {
            var enumeration = NumberSetShiftGroupEnumeration.New(numberSet).GetEnumerator();
            int count = 0;
            while(enumeration.MoveNext()) {
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
            while(enumeration.MoveNext())
            {
                Log.Debug(".tryPermutation count=" + count);
    
                int[] result = enumeration.Current;
                int[] expected = expectedValues[count];

                Log.Debug(".tryPermutation result=" + result.Render());
                Log.Debug(".tryPermutation expected=" + expected.Render());
    
                AssertSet(expected, result);
    
                count++;
                Assert.IsTrue(Collections.AreEqual(result, expected), "Mismatch in count=" + count);
            }
            Assert.AreEqual(count, expectedValues.Length);

            Assert.IsFalse(enumeration.MoveNext());
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
            foreach (int val in set) {
                arr[count++] = val;
            }
            return arr;
        }
    
        private ICollection<int> GetTreeSet(int[] set) {

            IDictionary<int, int> treeSet = new SortedDictionary<int, int>();
            foreach (int aSet in set) {
                treeSet.Add(aSet, aSet);
            }
            return treeSet.Keys;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
