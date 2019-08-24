///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.type
{
    [TestFixture]
    public class TestWildcardParameter : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            wildcard = WildcardParameter.Instance;
        }

        private WildcardParameter wildcard;

        [Test]
        public void TestContainsPoint()
        {
            Assert.IsTrue(wildcard.ContainsPoint(3));
            Assert.IsTrue(wildcard.ContainsPoint(2));
        }

        [Test]
        public void TestFormat()
        {
            Assert.AreEqual("*", wildcard.Formatted());
        }

        [Test]
        public void TestGetValuesInRange()
        {
            var result = wildcard.GetValuesInRange(1, 10);
            for (var i = 1; i <= 10; i++)
            {
                Assert.IsTrue(result.Contains(i));
            }

            Assert.AreEqual(10, result.Count);
        }

        [Test]
        public void TestIsWildcard()
        {
            Assert.IsTrue(wildcard.IsWildcard(1, 10));
        }
    }
} // end of namespace
