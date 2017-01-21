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
    public class TestListParameter  {
        private ListParameter _listParam;
    
        [SetUp]
        public void SetUp() {
            _listParam = new ListParameter();
            _listParam.Add(new IntParameter(5));
            _listParam.Add(new FrequencyParameter(3));
        }
    
        [Test]
        public void TestIsWildcard() {
            // Wildcard is expected to make only a best-guess effort, not be perfect
            Assert.IsTrue(_listParam.IsWildcard(5, 5));
            Assert.IsFalse(_listParam.IsWildcard(6, 10));
        }
    
        [Test]
        public void TestGetValues() {
            ICollection<int> result = _listParam.GetValuesInRange(1, 8);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{3, 5, 6}, result);
        }
    
        [Test]
        public void TestContainsPoint() {
            Assert.IsTrue(_listParam.ContainsPoint(0));
            Assert.IsFalse(_listParam.ContainsPoint(1));
            Assert.IsFalse(_listParam.ContainsPoint(2));
            Assert.IsTrue(_listParam.ContainsPoint(3));
            Assert.IsFalse(_listParam.ContainsPoint(4));
            Assert.IsTrue(_listParam.ContainsPoint(5));
        }
    
        [Test]
        public void TestFormat() {
            Assert.AreEqual("5, */3", _listParam.Formatted());
        }
    }
}
