///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestNumberSetPermutationEnumeration 
    {
        [Test]
        public void TestInvalid()
        {
            try
            {
                PermutationEnumerator.Create(0);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }
    
        [Test]
        public void TestNext()
        {
            int[] numberSet = new int[] {10, 11, 12};
            int[][] expectedValues = new int[][] {
                new[] { 10, 11, 12 },
                new[] { 10, 12, 11 },
                new[] { 11, 10, 12 },
                new[] { 11, 12, 10 },
                new[] { 12, 10, 11 },
                new[] { 12, 11, 10 }};
            TryPermutation(numberSet, expectedValues);
        }
    
        private void TryPermutation(int[] numberSet, int[][] expectedValues)
        {
            var enumeration = NumberSetPermutationEnumeration.New(numberSet).GetEnumerator();
    
            int count = 0;
            while(enumeration.MoveNext())
            {
                int[] result = enumeration.Current;
                int[] expected = expectedValues[count];

                Log.Debug(".tryPermutation result=" + result.Render());
                Log.Debug(".tryPermutation expected=" + expected.Render());
    
                count++;
                Assert.IsTrue(Collections.AreEqual(result, expected), "Mismatch in count=" + count);
            }

            Assert.AreEqual(count, expectedValues.Length);
            Assert.IsFalse(enumeration.MoveNext());
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
