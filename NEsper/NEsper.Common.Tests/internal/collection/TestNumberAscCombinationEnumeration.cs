///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestNumberAscCombinationEnumeration : AbstractTestBase
    {
        private static void Compare(
            int[][] expected,
            int n)
        {
            var e = new NumberAscCombinationEnumeration(n);
            var count = 0;
            while (count < expected.Length)
            {
                Assert.IsTrue(e.MoveNext());
                int[] next = e.Current;
                var expectedArr = expected[count];
                if (!Equals(expectedArr, next))
                {
                    Assert.Fail("Expected " + expectedArr.RenderAny() + " Received " + next.RenderAny() + " at index " + count);
                }

                count++;
            }

            Assert.IsFalse(e.MoveNext());
            Assert.That(() => e.Current, Throws.InstanceOf<NoSuchElementException>());
        }

        [Test]
        public void BasicTestNumberAscCombinationEnumeration()
        {
            Compare(new[] { new[] { 0 } }, 1);
            Compare(new[] { new[] { 0, 1 }, new[] { 0 }, new[] { 1 } }, 2);
            Compare(new[] { new[] { 0, 1, 2 }, new[] { 0, 1 }, new[] { 0, 2 }, new[] { 1, 2 }, new[] { 0 }, new[] { 1 }, new[] { 2 } }, 3);
            Compare(
                new int[][] {
                    new[] {0, 1, 2, 3},
                    new[] {0, 1, 2}, new[] {0, 1, 3}, new[] {0, 2, 3}, new []{1, 2, 3},
                    new []{0, 1}, new []{0, 2}, new []{0, 3}, new []{1, 2}, new []{1, 3}, new []{2, 3},
                    new []{0}, new []{1}, new []{2}, new []{3}
                },
                4);
            Compare(
                new int[][] {
                    new[] {0, 1, 2, 3, 4},
                    new[] {0, 1, 2, 3}, new[] {0, 1, 2, 4}, new[] {0, 1, 3, 4}, new[] {0, 2, 3, 4}, new[] {1, 2, 3, 4},
                    new[] {0, 1, 2}, new[] {0, 1, 3}, new[] {0, 1, 4}, new[] {0, 2, 3}, new []{0, 2, 4}, new []{0, 3, 4},
                    new []{1, 2, 3}, new []{1, 2, 4}, new []{1, 3, 4},
                    new []{2, 3, 4},
                    new []{0, 1}, new []{0, 2}, new []{0, 3}, new []{0, 4}, new []{1, 2}, new []{1, 3}, new []{1, 4}, new []{2, 3}, new []{2, 4}, new []{3, 4},
                    new []{0}, new []{1}, new []{2}, new []{3}, new[] {4}
                },
                5);

            try
            {
                new NumberAscCombinationEnumeration(0);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                // expected
            }
        }
    }
} // end of namespace