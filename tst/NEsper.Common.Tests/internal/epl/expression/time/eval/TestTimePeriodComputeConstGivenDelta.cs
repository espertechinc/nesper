///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
    [TestFixture]
    public class TestTimePeriodComputeConstGivenDelta : AbstractCommonTest
    {
        [Test]
        public void TestComputeDelta()
        {
            TimePeriodComputeConstGivenDeltaEval delta500 = new TimePeriodComputeConstGivenDeltaEval(500);
            ClassicAssert.AreEqual(500, delta500.DeltaAdd(0, null, true, null));
            ClassicAssert.AreEqual(500, delta500.DeltaSubtract(0, null, true, null));

            TimePeriodComputeConstGivenDeltaEval delta10k = new TimePeriodComputeConstGivenDeltaEval(10000);
            ClassicAssert.AreEqual(10000, delta10k.DeltaAdd(0, null, true, null));
            ClassicAssert.AreEqual(10000, delta10k.DeltaSubtract(0, null, true, null));

            // With current=2300, ref=1000, and interval=500, expect 2500 as next interval and 200 as solution
            // the reference will stay the same since the computation is cheap without updated reference
            TimePeriodDeltaResult result = delta500.DeltaAddWReference(2300, 1000, null, true, null);
            ClassicAssert.AreEqual(200, result.Delta);
            ClassicAssert.AreEqual(1000, result.LastReference);

            result = delta500.DeltaAddWReference(2300, 4200, null, true, null);
            ClassicAssert.AreEqual(400, result.Delta);
            ClassicAssert.AreEqual(4200, result.LastReference);

            result = delta500.DeltaAddWReference(2200, 4200, null, true, null);
            ClassicAssert.AreEqual(500, result.Delta);
            ClassicAssert.AreEqual(4200, result.LastReference);

            result = delta500.DeltaAddWReference(2200, 2200, null, true, null);
            ClassicAssert.AreEqual(500, result.Delta);
            ClassicAssert.AreEqual(2200, result.LastReference);

            result = delta500.DeltaAddWReference(2201, 2200, null, true, null);
            ClassicAssert.AreEqual(499, result.Delta);
            ClassicAssert.AreEqual(2200, result.LastReference);

            result = delta500.DeltaAddWReference(2600, 2200, null, true, null);
            ClassicAssert.AreEqual(100, result.Delta);
            ClassicAssert.AreEqual(2200, result.LastReference);

            result = delta500.DeltaAddWReference(2699, 2200, null, true, null);
            ClassicAssert.AreEqual(1, result.Delta);
            ClassicAssert.AreEqual(2200, result.LastReference);

            result = delta500.DeltaAddWReference(2699, 2700, null, true, null);
            ClassicAssert.AreEqual(1, result.Delta);
            ClassicAssert.AreEqual(2700, result.LastReference);

            result = delta10k.DeltaAddWReference(2699, 2700, null, true, null);
            ClassicAssert.AreEqual(1, result.Delta);
            ClassicAssert.AreEqual(2700, result.LastReference);

            result = delta10k.DeltaAddWReference(2700, 2700, null, true, null);
            ClassicAssert.AreEqual(10000, result.Delta);
            ClassicAssert.AreEqual(2700, result.LastReference);

            result = delta10k.DeltaAddWReference(2700, 6800, null, true, null);
            ClassicAssert.AreEqual(4100, result.Delta);
            ClassicAssert.AreEqual(6800, result.LastReference);

            result = delta10k.DeltaAddWReference(23050, 16800, null, true, null);
            ClassicAssert.AreEqual(3750, result.Delta);
            ClassicAssert.AreEqual(16800, result.LastReference);
        }
    }
} // end of namespace
