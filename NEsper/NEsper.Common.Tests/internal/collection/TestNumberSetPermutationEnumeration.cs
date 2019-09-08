///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestNumberSetPermutationEnumeration : AbstractCommonTest
    {
        private void TryPermutation(
            int[] numberSet,
            int[][] expectedValues)
        {
            var enumeration = NumberSetPermutationEnumeration.New(numberSet).GetEnumerator();

            var count = 0;
            while (enumeration.MoveNext())
            {
                var result = enumeration.Current;
                var expected = expectedValues[count];

                Log.Debug(".tryPermutation result=" + result.RenderAny());
                Log.Debug(".tryPermutation expected=" + expected.RenderAny());

                count++;
                Assert.That(result, Is.EqualTo(expected), "Mismatch in count=" + count);
            }

            Assert.AreEqual(count, expectedValues.Length);
            Assert.That(enumeration.MoveNext(), Is.False);

            // Enumerators exposed via yield do not throw exceptions
            //Assert.That(() => enumeration.Current, Throws.InstanceOf<InvalidOperationException>());
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TestInvalid()
        {
            Assert.That(() => PermutationEnumerator.Create(0), Throws.ArgumentException);
        }

        [Test]
        public void TestNext()
        {
            int[] numberSet = { 10, 11, 12 };
            int[][] expectedValues = {
                new[] {10, 11, 12},
                new[] {10, 12, 11},
                new[] {11, 10, 12},
                new[] {11, 12, 10},
                new[] {12, 10, 11},
                new[] {12, 11, 10}
            };
            TryPermutation(numberSet, expectedValues);
        }
    }
} // end of namespace
