///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using NUnit.Framework;

namespace com.espertech.esper.type
{
    [TestFixture]
    public class TestWildcardParameter  {
        private WildcardParameter _wildcard;
    
        [SetUp]
        public void SetUp() {
            _wildcard = new WildcardParameter();
        }
    
        [Test]
        public void TestIsWildcard() {
            Assert.IsTrue(_wildcard.IsWildcard(1, 10));
        }
    
        [Test]
        public void TestGetValuesInRange() {
            ICollection<int> result = _wildcard.GetValuesInRange(1, 10);
            for (int i = 1; i <= 10; i++) {
                Assert.IsTrue(result.Contains(i));
            }
            Assert.AreEqual(10, result.Count);
        }
    
        [Test]
        public void TestContainsPoint() {
            Assert.IsTrue(_wildcard.ContainsPoint(3));
            Assert.IsTrue(_wildcard.ContainsPoint(2));
        }
    
        [Test]
        public void TestFormat() {
            Assert.AreEqual("*", _wildcard.Formatted());
        }
    }
}
