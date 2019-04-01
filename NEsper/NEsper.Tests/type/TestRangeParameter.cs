///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.type
{
    [TestFixture]
    public class TestRangeParameter  {
        [Test]
        public void TestIsWildcard() {
            RangeParameter rangeParameter = new RangeParameter(10, 20);
            Assert.IsTrue(rangeParameter.IsWildcard(10, 20));
            Assert.IsTrue(rangeParameter.IsWildcard(11, 20));
            Assert.IsTrue(rangeParameter.IsWildcard(10, 19));
            Assert.IsFalse(rangeParameter.IsWildcard(9, 21));
            Assert.IsFalse(rangeParameter.IsWildcard(10, 21));
            Assert.IsFalse(rangeParameter.IsWildcard(9, 20));
            Assert.IsTrue(rangeParameter.IsWildcard(11, 19));
        }
    
        [Test]
        public void TestGetValues() {
            RangeParameter rangeParameter = new RangeParameter(0, 5);
            ICollection<int> values = rangeParameter.GetValuesInRange(1, 3);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{1, 2, 3}, values);
    
            values = rangeParameter.GetValuesInRange(-2, 3);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{0, 1, 2, 3}, values);
    
            values = rangeParameter.GetValuesInRange(4, 6);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{4, 5}, values);
    
            values = rangeParameter.GetValuesInRange(10, 20);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{}, values);
    
            values = rangeParameter.GetValuesInRange(-7, -1);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{}, values);
        }
    
        [Test]
        public void TestContainsPoint() {
            RangeParameter rangeParameter = new RangeParameter(10, 20);
            Assert.IsTrue(rangeParameter.ContainsPoint(10));
            Assert.IsTrue(rangeParameter.ContainsPoint(11));
            Assert.IsTrue(rangeParameter.ContainsPoint(20));
            Assert.IsFalse(rangeParameter.ContainsPoint(9));
            Assert.IsFalse(rangeParameter.ContainsPoint(21));
        }
    
        [Test]
        public void TestFormat() {
            RangeParameter rangeParameter = new RangeParameter(10, 20);
            Assert.AreEqual("10-20", rangeParameter.Formatted());
        }
    }
}
