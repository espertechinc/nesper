///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.expression.time.abacus
{
    [TestFixture]
    public class TestTimeAbacusMilliseconds : AbstractCommonTest
    {
        private TimeAbacus abacus = TimeAbacusMilliseconds.INSTANCE;

        [Test]
        public void TestDeltaFor()
        {
            ClassicAssert.AreEqual(0, abacus.DeltaForSecondsNumber(0));
            ClassicAssert.AreEqual(1000, abacus.DeltaForSecondsNumber(1));
            ClassicAssert.AreEqual(5000, abacus.DeltaForSecondsNumber(5));
            ClassicAssert.AreEqual(123, abacus.DeltaForSecondsNumber(0.123));
            ClassicAssert.AreEqual(1, abacus.DeltaForSecondsNumber(0.001));
            ClassicAssert.AreEqual(10, abacus.DeltaForSecondsNumber(0.01));

            ClassicAssert.AreEqual(0, abacus.DeltaForSecondsNumber(0.0001));
            ClassicAssert.AreEqual(1, abacus.DeltaForSecondsNumber(0.000999999));
            ClassicAssert.AreEqual(5000, abacus.DeltaForSecondsNumber(5.0001));
            ClassicAssert.AreEqual(5001, abacus.DeltaForSecondsNumber(5.000999999));

            for (int i = 1; i < 1000; i++)
            {
                double d = ((double) i) / 1000;
                ClassicAssert.AreEqual((long) i, abacus.DeltaForSecondsNumber(d));
                ClassicAssert.AreEqual((long) i, abacus.DeltaForSecondsDouble(d));
            }
        }
    }
} // end of namespace
