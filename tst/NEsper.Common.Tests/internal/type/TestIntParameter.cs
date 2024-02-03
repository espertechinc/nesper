///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.type
{
    [TestFixture]
    public class TestIntParameter : AbstractCommonTest
    {
        [Test]
        public void TestIsWildcard()
        {
            IntParameter intParam = new IntParameter(5);
            ClassicAssert.IsTrue(intParam.IsWildcard(5, 5));
            ClassicAssert.IsFalse(intParam.IsWildcard(4, 5));
            ClassicAssert.IsFalse(intParam.IsWildcard(5, 6));
            ClassicAssert.IsFalse(intParam.IsWildcard(4, 6));
        }

        [Test]
        public void TestGetValues()
        {
            IntParameter intParam = new IntParameter(3);
            ICollection<int> result = intParam.GetValuesInRange(1, 8);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] { 3 }, result);

            result = intParam.GetValuesInRange(1, 2);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] { }, result);

            result = intParam.GetValuesInRange(4, 10);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] { }, result);

            result = intParam.GetValuesInRange(1, 3);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] { 3 }, result);

            result = intParam.GetValuesInRange(3, 5);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] { 3 }, result);
        }

        [Test]
        public void TestContainsPoint()
        {
            IntParameter intParam = new IntParameter(3);
            ClassicAssert.IsTrue(intParam.ContainsPoint(3));
            ClassicAssert.IsFalse(intParam.ContainsPoint(2));
        }

        [Test]
        public void TestFormat()
        {
            IntParameter intParam = new IntParameter(3);
            ClassicAssert.AreEqual("3", intParam.Formatted());
        }
    }
} // end of namespace
