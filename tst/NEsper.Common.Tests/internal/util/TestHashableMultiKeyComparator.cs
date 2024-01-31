///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestHashableMultiKeyComparator : AbstractCommonTest
    {
        private IComparer<HashableMultiKey> comparator;
        private HashableMultiKey firstValues;
        private HashableMultiKey secondValues;

        [Test]
        public void TestCompareSingleProperty()
        {
            comparator = new ComparatorHashableMultiKey(new bool[] { false });

            firstValues = new HashableMultiKey(new object[] { 3d });
            secondValues = new HashableMultiKey(new object[] { 4d });
            ClassicAssert.IsTrue(comparator.Compare(firstValues, secondValues) < 0);

            comparator = new ComparatorHashableMultiKey(new bool[] { true });

            ClassicAssert.IsTrue(comparator.Compare(firstValues, secondValues) > 0);
            ClassicAssert.IsTrue(comparator.Compare(firstValues, firstValues) == 0);
        }

        [Test]
        public void TestCompareTwoProperties()
        {
            comparator = new ComparatorHashableMultiKey(new bool[] { false, false });

            firstValues = new HashableMultiKey(new object[] { 3d, 3L });
            secondValues = new HashableMultiKey(new object[] { 3d, 4L });
            ClassicAssert.IsTrue(comparator.Compare(firstValues, secondValues) < 0);

            comparator = new ComparatorHashableMultiKey(new bool[] { false, true });

            ClassicAssert.IsTrue(comparator.Compare(firstValues, secondValues) > 0);
            ClassicAssert.IsTrue(comparator.Compare(firstValues, firstValues) == 0);
        }

        [Test]
        public void TestInvalid()
        {
            comparator = new ComparatorHashableMultiKey(new bool[] { false, false });

            firstValues = new HashableMultiKey(new object[] { 3d });
            secondValues = new HashableMultiKey(new object[] { 3d, 4L });
            try
            {
                comparator.Compare(firstValues, secondValues);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // Expected
            }

            firstValues = new HashableMultiKey(new object[] { 3d });
            secondValues = new HashableMultiKey(new object[] { 3d });
            try
            {
                comparator.Compare(firstValues, secondValues);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }
    }
} // end of namespace
