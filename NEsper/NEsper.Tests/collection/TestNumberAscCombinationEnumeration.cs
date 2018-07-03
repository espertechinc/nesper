///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestNumberAscCombinationEnumeration
    {
        [Test]
        public void TestNumberAscCombinationEnumerationExec()
        {
            Compare(new int[][] {new int[] {0}}, 1);
            Compare(new int[][] { new int[] { 0, 1 }, new int[] { 0 }, new int[] { 1 } }, 2);
            Compare(new int[][] { new int[] { 0, 1, 2 }, new int[] { 0, 1 }, new int[] { 0, 2 }, new int[] { 1, 2 }, new int[] { 0 }, new int[] { 1 }, new int[] { 2 } }, 3);
            Compare(new int[][] {new int[]{0, 1, 2, 3},
                    new int[]{0, 1, 2}, new int[]{0, 1, 3}, new int[]{0, 2, 3}, new int[]{1, 2, 3},
                    new int[]{0, 1}, new int[]{0, 2}, new int[]{0, 3}, new int[]{1, 2}, new int[]{1, 3}, new int[]{2, 3},
                    new int[]{0}, new int[]{1}, new int[]{2}, new int[]{3}}, 4);
            Compare(new int[][] {new int[]{0, 1, 2, 3, 4},
                    new int[]{0, 1, 2, 3}, new int[]{0, 1, 2, 4}, new int[]{0, 1, 3, 4}, new int[]{0, 2, 3, 4}, new int[]{1, 2, 3, 4},
                    new int[]{0, 1, 2}, new int[]{0, 1, 3}, new int[]{0, 1, 4}, new int[]{0, 2, 3}, new int[]{0, 2, 4}, new int[]{0, 3, 4},
                    new int[]{1, 2, 3}, new int[]{1, 2, 4}, new int[]{1, 3, 4},
                    new int[]{2, 3, 4},
                    new int[]{0, 1}, new int[]{0, 2}, new int[]{0, 3}, new int[]{0, 4}, new int[]{1, 2}, new int[]{1, 3}, new int[]{1, 4}, new int[]{2, 3}, new int[]{2, 4}, new int[]{3, 4},
                    new int[]{0}, new int[]{1}, new int[]{2}, new int[]{3}, new int[]{4}}, 5);
    
            try {
                new NumberAscCombinationEnumeration(0);
                Assert.Fail();
            }
            catch (ArgumentException) {
                // expected
            }
        }
    
        private static void Compare(int[][] expected, int n)
        {
            var e = new NumberAscCombinationEnumeration(n);
            int count = 0;
            while(count < expected.Length) {
                Assert.IsTrue(e.MoveNext());
                int[] next = e.Current;
                int[] expectedArr = expected[count];
                if (!Collections.AreEqual(expectedArr, next)) {
                    Assert.Fail("Expected " + expectedArr.Render() + " Received " + next.Render() + " at index " + count);
                }
                count++;
            }
    
            Assert.IsFalse(e.MoveNext());
            try
            {
                Assert.That(e.Current, Is.Null); // should throw exception
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                // expected
            }
        }
    }
}
