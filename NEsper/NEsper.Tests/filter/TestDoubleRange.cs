///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestDoubleRange 
    {
        [Test]
        public void TestNew()
        {
            var range = new DoubleRange(10d, 20d);
            Assert.AreEqual(20d, range.Max);
            Assert.AreEqual(10d, range.Min);
    
            range = new DoubleRange(20d, 10d);
            Assert.AreEqual(20d, range.Max);
            Assert.AreEqual(10d, range.Min);
        }
    
        [Test]
        public void TestEquals()
        {
            var rangeOne = new DoubleRange(10d, 20d);
            var rangeTwo = new DoubleRange(20d, 10d);
            var rangeThree = new DoubleRange(20d, 11d);
            var rangeFour = new DoubleRange(21d, 10d);
    
            Assert.AreEqual(rangeOne, rangeTwo);
            Assert.AreEqual(rangeTwo, rangeOne);
            Assert.IsFalse(rangeOne.Equals(rangeThree));
            Assert.IsFalse(rangeOne.Equals(rangeFour));
            Assert.IsFalse(rangeThree.Equals(rangeFour));
        }
    
        [Test]
        public void TestHash()
        {
            var range = new DoubleRange(10d, 20d);

            int hashCode = 10.0.GetHashCode();
            hashCode *= 31;
            hashCode ^= 20.0.GetHashCode();

            Assert.That(range.GetHashCode(), Is.EqualTo(hashCode));
        }
    }
}
