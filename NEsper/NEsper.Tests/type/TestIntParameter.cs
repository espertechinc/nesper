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
    public class TestIntParameter  {
        [Test]
        public void TestIsWildcard() {
            IntParameter intParam = new IntParameter(5);
            Assert.IsTrue(intParam.IsWildcard(5, 5));
            Assert.IsFalse(intParam.IsWildcard(4, 5));
            Assert.IsFalse(intParam.IsWildcard(5, 6));
            Assert.IsFalse(intParam.IsWildcard(4, 6));
        }
    
        [Test]
        public void TestGetValues() {
            IntParameter intParam = new IntParameter(3);
            ICollection<int> result = intParam.GetValuesInRange(1, 8);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{3}, result);
    
            result = intParam.GetValuesInRange(1, 2);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{}, result);
    
            result = intParam.GetValuesInRange(4, 10);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{}, result);
    
            result = intParam.GetValuesInRange(1, 3);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{3}, result);
    
            result = intParam.GetValuesInRange(3, 5);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{3}, result);
        }
    
        [Test]
        public void TestContainsPoint() {
            IntParameter intParam = new IntParameter(3);
            Assert.IsTrue(intParam.ContainsPoint(3));
            Assert.IsFalse(intParam.ContainsPoint(2));
        }
    
        [Test]
        public void TestFormat() {
            IntParameter intParam = new IntParameter(3);
            Assert.AreEqual("3", intParam.Formatted());
        }
    }
}
