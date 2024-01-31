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
    public class TestTimeAbacusMicroseconds : AbstractCommonTest
    {
        private TimeAbacus abacus = TimeAbacusMicroseconds.INSTANCE;

        [Test]
        public void TestDeltaFor()
        {
            ClassicAssert.AreEqual(0, abacus.DeltaForSecondsNumber(0));
            ClassicAssert.AreEqual(1000000, abacus.DeltaForSecondsNumber(1));
            ClassicAssert.AreEqual(5000000, abacus.DeltaForSecondsNumber(5));
            ClassicAssert.AreEqual(123000, abacus.DeltaForSecondsNumber(0.123));
            ClassicAssert.AreEqual(1, abacus.DeltaForSecondsNumber(0.000001));
            ClassicAssert.AreEqual(10, abacus.DeltaForSecondsNumber(0.000010));

            ClassicAssert.AreEqual(0, abacus.DeltaForSecondsNumber(0.0000001));
            ClassicAssert.AreEqual(1, abacus.DeltaForSecondsNumber(0.000000999999));
            ClassicAssert.AreEqual(5000000, abacus.DeltaForSecondsNumber(5.0000001));
            ClassicAssert.AreEqual(5000001, abacus.DeltaForSecondsNumber(5.000000999999));

            for (int i = 1; i < 1000000; i++)
            {
                double d = ((double) i) / 1000000;
                ClassicAssert.AreEqual((long) i, abacus.DeltaForSecondsNumber(d));
                ClassicAssert.AreEqual((long) i, abacus.DeltaForSecondsDouble(d));
            }
        }
    }
} // end of namespace
