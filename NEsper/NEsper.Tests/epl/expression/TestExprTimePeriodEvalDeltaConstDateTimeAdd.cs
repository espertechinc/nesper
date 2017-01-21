///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression
{
    [TestFixture]
    public class TestExprTimePeriodEvalDeltaConstDateTimeAdd
    {
        [Test]
        public void TestComputeDelta() 
        {
            ExprTimePeriod timePeriod = new ExprTimePeriodImpl(TimeZoneInfo.Local, false, true, false, false, false, false, false, false);
            timePeriod.AddChildNode(new ExprConstantNodeImpl(1));
            timePeriod.Validate(null);

            ExprTimePeriodEvalDeltaConstDateTimeAdd addMonth = (ExprTimePeriodEvalDeltaConstDateTimeAdd)timePeriod.ConstEvaluator(null);
            Assert.AreEqual(28*24*60*60*1000L, addMonth.DeltaMillisecondsAdd(Parse("2002-02-15T9:00:00.000")));
            Assert.AreEqual(28*24*60*60*1000L, addMonth.DeltaMillisecondsSubtract(Parse("2002-03-15T9:00:00.000")));
    
            ExprTimePeriodEvalDeltaResult result = addMonth.DeltaMillisecondsAddWReference(
                    Parse("2002-02-15T9:00:00.000"), Parse("2002-02-15T9:00:00.000"));
            Assert.AreEqual(Parse("2002-03-15T9:00:00.000") - Parse("2002-02-15T9:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-02-15T9:00:00.000"), result.LastReference);
    
            result = addMonth.DeltaMillisecondsAddWReference(
                    Parse("2002-03-15T9:00:00.000"), Parse("2002-02-15T9:00:00.000"));
            Assert.AreEqual(Parse("2002-04-15T9:00:00.000") - Parse("2002-03-15T9:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-03-15T9:00:00.000"), result.LastReference);
    
            result = addMonth.DeltaMillisecondsAddWReference(
                    Parse("2002-04-15T9:00:00.000"), Parse("2002-03-15T9:00:00.000"));
            Assert.AreEqual(Parse("2002-05-15T9:00:00.000") - Parse("2002-04-15T9:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-04-15T9:00:00.000"), result.LastReference);
    
            // try future reference
            result = addMonth.DeltaMillisecondsAddWReference(
                    Parse("2002-02-15T9:00:00.000"), Parse("2900-03-15T9:00:00.000"));
            Assert.AreEqual(Parse("2002-03-15T9:00:00.000") - Parse("2002-02-15T9:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-02-15T9:00:00.000"), result.LastReference);
    
            // try old reference
            result = addMonth.DeltaMillisecondsAddWReference(
                    Parse("2002-02-15T9:00:00.000"), Parse("1980-03-15T9:00:00.000"));
            Assert.AreEqual(Parse("2002-03-15T9:00:00.000") - Parse("2002-02-15T9:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-02-15T9:00:00.000"), result.LastReference);
    
            // try different-dates
            result = addMonth.DeltaMillisecondsAddWReference(
                    Parse("2002-02-18T9:00:00.000"), Parse("1980-03-15T9:00:00.000"));
            Assert.AreEqual(Parse("2002-03-15T9:00:00.000") - Parse("2002-02-18T9:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-02-15T9:00:00.000"), result.LastReference);
    
            result = addMonth.DeltaMillisecondsAddWReference(
                    Parse("2002-02-11T9:00:00.000"), Parse("2980-03-15T9:00:00.000"));
            Assert.AreEqual(Parse("2002-02-15T9:00:00.000") - Parse("2002-02-11T9:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-01-15T9:00:00.000"), result.LastReference);
    
            result = addMonth.DeltaMillisecondsAddWReference(
                    Parse("2002-04-05T9:00:00.000"), Parse("2002-02-11T9:01:02.003"));
            Assert.AreEqual(Parse("2002-04-11T9:01:02.003") - Parse("2002-04-05T9:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-03-11T9:01:02.003"), result.LastReference);
        }
    
        private long Parse(string date) {
            return DateTimeParser.ParseDefaultMSec(date);
        }
    }
}
