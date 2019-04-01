///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.time;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprTimePeriodEvalDeltaConstMsec
    {
        [Test]
        public void TestComputeDelta()
        {
            ExprTimePeriodEvalDeltaConstGivenDelta delta500 = new ExprTimePeriodEvalDeltaConstGivenDelta(500);
            Assert.AreEqual(500, delta500.DeltaAdd(0));
            Assert.AreEqual(500, delta500.DeltaSubtract(0));

            ExprTimePeriodEvalDeltaConstGivenDelta delta10k = new ExprTimePeriodEvalDeltaConstGivenDelta(10000);
            Assert.AreEqual(10000, delta10k.DeltaAdd(0));
            Assert.AreEqual(10000, delta10k.DeltaSubtract(0));
    
            // With current=2300, ref=1000, and interval=500, expect 2500 as next interval and 200 as solution
            // the reference will stay the same since the computation is cheap without updated reference
            ExprTimePeriodEvalDeltaResult result = delta500.DeltaAddWReference(2300, 1000);
            Assert.AreEqual(200, result.Delta);
            Assert.AreEqual(1000, result.LastReference);
    
            result = delta500.DeltaAddWReference(2300, 4200);
            Assert.AreEqual(400, result.Delta);
            Assert.AreEqual(4200, result.LastReference);
    
            result = delta500.DeltaAddWReference(2200, 4200);
            Assert.AreEqual(500, result.Delta);
            Assert.AreEqual(4200, result.LastReference);
    
            result = delta500.DeltaAddWReference(2200, 2200);
            Assert.AreEqual(500, result.Delta);
            Assert.AreEqual(2200, result.LastReference);
    
            result = delta500.DeltaAddWReference(2201, 2200);
            Assert.AreEqual(499, result.Delta);
            Assert.AreEqual(2200, result.LastReference);
    
            result = delta500.DeltaAddWReference(2600, 2200);
            Assert.AreEqual(100, result.Delta);
            Assert.AreEqual(2200, result.LastReference);
    
            result = delta500.DeltaAddWReference(2699, 2200);
            Assert.AreEqual(1, result.Delta);
            Assert.AreEqual(2200, result.LastReference);
    
            result = delta500.DeltaAddWReference(2699, 2700);
            Assert.AreEqual(1, result.Delta);
            Assert.AreEqual(2700, result.LastReference);
    
            result = delta10k.DeltaAddWReference(2699, 2700);
            Assert.AreEqual(1, result.Delta);
            Assert.AreEqual(2700, result.LastReference);
    
            result = delta10k.DeltaAddWReference(2700, 2700);
            Assert.AreEqual(10000, result.Delta);
            Assert.AreEqual(2700, result.LastReference);
    
            result = delta10k.DeltaAddWReference(2700, 6800);
            Assert.AreEqual(4100, result.Delta);
            Assert.AreEqual(6800, result.LastReference);
    
            result = delta10k.DeltaAddWReference(23050, 16800);
            Assert.AreEqual(3750, result.Delta);
            Assert.AreEqual(16800, result.LastReference);
        }
    }
}
