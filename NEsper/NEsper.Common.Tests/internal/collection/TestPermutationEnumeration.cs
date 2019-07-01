///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    public class TestPermutationEnumeration : AbstractTestBase
    {
        [Test]
        public void TestInvalid()
        {
            Assert.That(() => PermutationEnumerator.Create(0), Throws.ArgumentException);
        }

        [Test]
        public void TestNext()
        {
            int[][] expectedValues4 = new int[][] {
                new int[] {0, 1, 2, 3}, // 0
                new int[] {0, 1, 3, 2},
                new int[] {0, 2, 1, 3},
                new int[] {0, 2, 3, 1},
                new int[] {0, 3, 1, 2},
                new int[] {0, 3, 2, 1}, // 5

                new int[] {1, 0, 2, 3}, // 6
                new int[] {1, 0, 3, 2}, // 7
                new int[] {1, 2, 0, 3}, // 8
                new int[] {1, 2, 3, 0},
                new int[] {1, 3, 0, 2},
                new int[] {1, 3, 2, 0}, // 11

                new int[] {2, 0, 1, 3}, // 12
                new int[] {2, 0, 3, 1},
                new int[] {2, 1, 0, 3},
                new int[] {2, 1, 3, 0},
                new int[] {2, 3, 0, 1},
                new int[] {2, 3, 1, 0}, // 17

                new int[] {3, 0, 1, 2}, // 18
                new int[] {3, 0, 2, 1},
                new int[] {3, 1, 0, 2},
                new int[] {3, 1, 2, 0}, // 21
                new int[] {3, 2, 0, 1},
                new int[] {3, 2, 1, 0}
            };
            tryPermutation(4, expectedValues4);

            int[][] expectedValues3 = new int[][] {
                new int[] {0, 1, 2},
                new int[] {0, 2, 1},
                new int[] {1, 0, 2},
                new int[] {1, 2, 0},
                new int[] {2, 0, 1},
                new int[] {2, 1, 0}
            };
            tryPermutation(3, expectedValues3);

            int[][] expectedValues2 = new int[][] {
                new int[] {0, 1},
                new int[] {1, 0}
            };
            tryPermutation(2, expectedValues2);

            int[][] expectedValues1 = new int[][] {
                new int[]{0}
            };
            tryPermutation(1, expectedValues1);
        }

        private void tryPermutation(
            int numElements,
            int[][] expectedValues)
        {
            /*
            Total: 4 * 3 * 2 = 24 = 6!  (6 faculty)

            Example:8
            n / 6 = first number        == index 1, total {1}, remains {0, 2, 3}
            remainder 8 - 1 * 6         == 2
            n / 2 = second number       == index 1, total {1, 2}, remain {0, 3}
            remainder 2 - 1 * 2         == 0
                                        == total {1, 2, 0, 3}

            Example:21   out {0, 1, 2, 3}
            21 / 6                      == index 3 -> in {3}, out {0, 1, 2}
            remainder 21 - 3 * 6        == 3
            3 / 2 = second number       == index 1 -> in {3, 1}, remain {0, 2}
            remainder 3 - 1 * 2         == 1
                                        == index 1 -> in {3, 1, 2} out {0}
            */
            var enumeration = PermutationEnumerator.Create(numElements).GetEnumerator();
            int count = 0;
            while (enumeration.MoveNext())
            {
                int[] result = enumeration.Current;
                int[] expected = expectedValues[count];

                Log.Debug(".tryPermutation result=" + CompatExtensions.RenderAny(result));
                Log.Debug(".tryPermutation expected=" + CompatExtensions.RenderAny(result));

                count++;
                Assert.IsTrue(Arrays.AreEqual(result, expected),
                    "Mismatch in count=" + count);
            }

            Assert.AreEqual(count, expectedValues.Length);

            try
            {
                Assert.IsNotNull(enumeration.Current);
                Assert.Fail();
            }
            catch (NoSuchElementException ex)
            {
                // Expected
            }
        }

        [Test]
        public void TestGetPermutation()
        {
            int[] factors = PermutationEnumerator.GetFactors(4);
            int[] result = PermutationEnumerator.GetPermutation(4, 21, factors);

            Log.Debug(".testGetPermutation result=" + result.RenderAny());
            Assert.IsTrue(Arrays.Equals(result, new int[] { 3, 1, 2, 0 }));
        }

        [Test]
        public void TestGetFactors()
        {
            int[] factors = PermutationEnumerator.GetFactors(5);
            Assert.IsTrue(Arrays.Equals(factors, new int[] { 24, 6, 2, 1, 0 }));

            factors = PermutationEnumerator.GetFactors(4);
            Assert.IsTrue(Arrays.Equals(factors, new int[] { 6, 2, 1, 0 }));

            factors = PermutationEnumerator.GetFactors(3);
            Assert.IsTrue(Arrays.Equals(factors, new int[] { 2, 1, 0 }));

            factors = PermutationEnumerator.GetFactors(2);
            Assert.IsTrue(Arrays.Equals(factors, new int[] { 1, 0 }));

            factors = PermutationEnumerator.GetFactors(1);
            Assert.IsTrue(Arrays.Equals(factors, new int[] { 0 }));

            //Log.debug(".testGetFactors " + Arrays.toString(factors));
        }

        [Test]
        public void TestFaculty()
        {
            Assert.AreEqual(0, PermutationEnumerator.Faculty(0));
            Assert.AreEqual(1, PermutationEnumerator.Faculty(1));
            Assert.AreEqual(2, PermutationEnumerator.Faculty(2));
            Assert.AreEqual(6, PermutationEnumerator.Faculty(3));
            Assert.AreEqual(24, PermutationEnumerator.Faculty(4));
            Assert.AreEqual(120, PermutationEnumerator.Faculty(5));
            Assert.AreEqual(720, PermutationEnumerator.Faculty(6));
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace