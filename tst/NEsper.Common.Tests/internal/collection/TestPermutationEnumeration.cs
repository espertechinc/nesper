///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.collection
{
    public class TestPermutationEnumeration : AbstractCommonTest
    {
        [Test]
        public void TestInvalid()
        {
            Assert.That(() => PermutationEnumerator.Create(0), Throws.ArgumentException);
        }

        [Test]
        public void TestNext()
        {
            int[][] expectedValues4 = new[] {
                new[] {0, 1, 2, 3}, // 0
                new[] {0, 1, 3, 2},
                new[] {0, 2, 1, 3},
                new[] {0, 2, 3, 1},
                new[] {0, 3, 1, 2},
                new[] {0, 3, 2, 1}, // 5

                new[] {1, 0, 2, 3}, // 6
                new[] {1, 0, 3, 2}, // 7
                new[] {1, 2, 0, 3}, // 8
                new[] {1, 2, 3, 0},
                new[] {1, 3, 0, 2},
                new[] {1, 3, 2, 0}, // 11

                new[] {2, 0, 1, 3}, // 12
                new[] {2, 0, 3, 1},
                new[] {2, 1, 0, 3},
                new[] {2, 1, 3, 0},
                new[] {2, 3, 0, 1},
                new[] {2, 3, 1, 0}, // 17

                new[] {3, 0, 1, 2}, // 18
                new[] {3, 0, 2, 1},
                new[] {3, 1, 0, 2},
                new[] {3, 1, 2, 0}, // 21
                new[] {3, 2, 0, 1},
                new[] {3, 2, 1, 0}
            };
            TryPermutation(4, expectedValues4);

            int[][] expectedValues3 = new[] {
                new[] {0, 1, 2},
                new[] {0, 2, 1},
                new[] {1, 0, 2},
                new[] {1, 2, 0},
                new[] {2, 0, 1},
                new[] {2, 1, 0}
            };
            TryPermutation(3, expectedValues3);

            int[][] expectedValues2 = new[] {
                new[] {0, 1},
                new[] {1, 0}
            };
            TryPermutation(2, expectedValues2);

            int[][] expectedValues1 = new[] {
                new[]{0}
            };
            TryPermutation(1, expectedValues1);
        }

        private void TryPermutation(
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
                ClassicAssert.IsTrue(Arrays.AreEqual(result, expected),
                    "Mismatch in count=" + count);
            }

            ClassicAssert.AreEqual(count, expectedValues.Length);
            Assert.That(enumeration.MoveNext(), Is.False);
        }

        [Test]
        public void TestGetPermutation()
        {
            int[] factors = PermutationEnumerator.GetFactors(4);
            int[] result = PermutationEnumerator.GetPermutation(4, 21, factors);

            Log.Debug(".testGetPermutation result=" + result.RenderAny());
            ClassicAssert.IsTrue(Arrays.AreEqual(result, new[] { 3, 1, 2, 0 }));
        }

        [Test]
        public void TestGetFactors()
        {
            int[] factors = PermutationEnumerator.GetFactors(5);
            ClassicAssert.IsTrue(Arrays.AreEqual(factors, new[] { 24, 6, 2, 1, 0 }));

            factors = PermutationEnumerator.GetFactors(4);
            ClassicAssert.IsTrue(Arrays.AreEqual(factors, new[] { 6, 2, 1, 0 }));

            factors = PermutationEnumerator.GetFactors(3);
            ClassicAssert.IsTrue(Arrays.AreEqual(factors, new[] { 2, 1, 0 }));

            factors = PermutationEnumerator.GetFactors(2);
            ClassicAssert.IsTrue(Arrays.AreEqual(factors, new[] { 1, 0 }));

            factors = PermutationEnumerator.GetFactors(1);
            ClassicAssert.IsTrue(Arrays.AreEqual(factors, new[] { 0 }));

            //Log.debug(".testGetFactors " + Arrays.toString(factors));
        }

        [Test]
        public void TestFaculty()
        {
            ClassicAssert.AreEqual(0, PermutationEnumerator.Faculty(0));
            ClassicAssert.AreEqual(1, PermutationEnumerator.Faculty(1));
            ClassicAssert.AreEqual(2, PermutationEnumerator.Faculty(2));
            ClassicAssert.AreEqual(6, PermutationEnumerator.Faculty(3));
            ClassicAssert.AreEqual(24, PermutationEnumerator.Faculty(4));
            ClassicAssert.AreEqual(120, PermutationEnumerator.Faculty(5));
            ClassicAssert.AreEqual(720, PermutationEnumerator.Faculty(6));
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
