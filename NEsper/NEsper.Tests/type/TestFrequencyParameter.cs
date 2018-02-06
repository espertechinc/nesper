///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.type
{
    [TestFixture]
    public class TestFrequencyParameter  {
        [Test]
        public void TestInvalid() {
            try {
                new FrequencyParameter(0);
                Assert.Fail();
            } catch (ArgumentException) {
                // Expected
            }
        }
    
        [Test]
        public void TestIsWildcard() {
            FrequencyParameter freq = new FrequencyParameter(1);
            Assert.IsTrue(freq.IsWildcard(1, 10));
    
            freq = new FrequencyParameter(2);
            Assert.IsFalse(freq.IsWildcard(1, 20));
        }
    
        [Test]
        public void TestGetValues() {
            FrequencyParameter freq = new FrequencyParameter(3);
            ICollection<int> result = freq.GetValuesInRange(1, 8);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{3, 6}, result);
    
            freq = new FrequencyParameter(4);
            result = freq.GetValuesInRange(6, 16);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{8, 12, 16}, result);
    
            freq = new FrequencyParameter(4);
            result = freq.GetValuesInRange(0, 14);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{0, 4, 8, 12}, result);
    
            freq = new FrequencyParameter(1);
            result = freq.GetValuesInRange(2, 5);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[]{2, 3, 4, 5}, result);
        }
    
        [Test]
        public void TestContainsPoint() {
            FrequencyParameter freqThree = new FrequencyParameter(3);
            Assert.IsTrue(freqThree.ContainsPoint(0));
            Assert.IsTrue(freqThree.ContainsPoint(3));
            Assert.IsTrue(freqThree.ContainsPoint(6));
            Assert.IsFalse(freqThree.ContainsPoint(1));
            Assert.IsFalse(freqThree.ContainsPoint(2));
            Assert.IsFalse(freqThree.ContainsPoint(4));
    
            FrequencyParameter freqOne = new FrequencyParameter(1);
            Assert.IsTrue(freqOne.ContainsPoint(1));
            Assert.IsTrue(freqOne.ContainsPoint(2));
        }
    
        [Test]
        public void TestFormat() {
            FrequencyParameter freqThree = new FrequencyParameter(3);
            Assert.AreEqual("*/3", freqThree.Formatted());
        }
    }
}
