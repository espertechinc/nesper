///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.filterspec
{
    [TestFixture]
    public class TestDoubleRange : AbstractCommonTest
    {
        [Test]
        public void TestNew()
        {
            var range = new DoubleRange(10d, 20d);
            ClassicAssert.AreEqual(20d, range.Max);
            ClassicAssert.AreEqual(10d, range.Min);

            range = new DoubleRange(20d, 10d);
            ClassicAssert.AreEqual(20d, range.Max);
            ClassicAssert.AreEqual(10d, range.Min);
        }

        [Test]
        public void TestEquals()
        {
            var rangeOne = new DoubleRange(10d, 20d);
            var rangeTwo = new DoubleRange(20d, 10d);
            var rangeThree = new DoubleRange(20d, 11d);
            var rangeFour = new DoubleRange(21d, 10d);

            ClassicAssert.AreEqual(rangeOne, rangeTwo);
            ClassicAssert.AreEqual(rangeTwo, rangeOne);
            ClassicAssert.IsFalse(rangeOne.Equals(rangeThree));
            ClassicAssert.IsFalse(rangeOne.Equals(rangeFour));
            ClassicAssert.IsFalse(rangeThree.Equals(rangeFour));
        }

        [Test]
        public void TestHash()
        {
            var range = new DoubleRange(10d, 20d);
            var hashCode = 7;
            hashCode *= 31;
            hashCode ^= 10.0d.GetHashCode();
            hashCode *= 31;
            hashCode ^= 20.0d.GetHashCode();

            ClassicAssert.AreEqual(hashCode, range.GetHashCode());
        }
    }
} // end of namespace
