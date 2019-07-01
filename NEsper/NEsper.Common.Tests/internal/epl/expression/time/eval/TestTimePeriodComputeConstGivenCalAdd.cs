///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.adder;
using com.espertech.esper.compat.datetime;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
    [TestFixture]
    public class TestTimePeriodComputeConstGivenCalAdd : AbstractTestBase
    {
        [Test]
        public void TestComputeDelta()
        {
            TimePeriodComputeConstGivenCalAddEval addMonth = new TimePeriodComputeConstGivenCalAddEval();
            addMonth.TimeZone = TimeZoneInfo.Local;
            addMonth.TimeAbacus = TimeAbacusMilliseconds.INSTANCE;
            TimePeriodAdder[] adders = new TimePeriodAdder[1];
            adders[0] = new TimePeriodAdderMonth();
            addMonth.Adders = adders;
            addMonth.Added = new int[] { 1 };
            addMonth.IndexMicroseconds = -1;

            Assert.AreEqual(28 * 24 * 60 * 60 * 1000L, addMonth.DeltaAdd(Parse("2002-02-15T09:00:00.000"), null, true, null));
            Assert.AreEqual(28 * 24 * 60 * 60 * 1000L, addMonth.DeltaSubtract(Parse("2002-03-15T09:00:00.000"), null, true, null));

            TimePeriodDeltaResult result = addMonth.DeltaAddWReference(
                    Parse("2002-02-15T09:00:00.000"), Parse("2002-02-15T09:00:00.000"), null, true, null);
            Assert.AreEqual(Parse("2002-03-15T09:00:00.000") - Parse("2002-02-15T09:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-02-15T09:00:00.000"), result.LastReference);

            result = addMonth.DeltaAddWReference(
                    Parse("2002-03-15T09:00:00.000"), Parse("2002-02-15T09:00:00.000"), null, true, null);
            Assert.AreEqual(Parse("2002-04-15T09:00:00.000") - Parse("2002-03-15T09:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-03-15T09:00:00.000"), result.LastReference);

            result = addMonth.DeltaAddWReference(
                    Parse("2002-04-15T09:00:00.000"), Parse("2002-03-15T09:00:00.000"), null, true, null);
            Assert.AreEqual(Parse("2002-05-15T09:00:00.000") - Parse("2002-04-15T09:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-04-15T09:00:00.000"), result.LastReference);

            // try future reference
            result = addMonth.DeltaAddWReference(
                    Parse("2002-02-15T09:00:00.000"), Parse("2900-03-15T09:00:00.000"), null, true, null);
            Assert.AreEqual(Parse("2002-03-15T09:00:00.000") - Parse("2002-02-15T09:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-02-15T09:00:00.000"), result.LastReference);

            // try old reference
            result = addMonth.DeltaAddWReference(
                    Parse("2002-02-15T09:00:00.000"), Parse("1980-03-15T09:00:00.000"), null, true, null);
            Assert.AreEqual(Parse("2002-03-15T09:00:00.000") - Parse("2002-02-15T09:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-02-15T09:00:00.000"), result.LastReference);

            // try different-dates
            result = addMonth.DeltaAddWReference(
                    Parse("2002-02-18T09:00:00.000"), Parse("1980-03-15T09:00:00.000"), null, true, null);
            Assert.AreEqual(Parse("2002-03-15T09:00:00.000") - Parse("2002-02-18T09:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-02-15T09:00:00.000"), result.LastReference);

            result = addMonth.DeltaAddWReference(
                    Parse("2002-02-11T09:00:00.000"), Parse("2980-03-15T09:00:00.000"), null, true, null);
            Assert.AreEqual(Parse("2002-02-15T09:00:00.000") - Parse("2002-02-11T09:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-01-15T09:00:00.000"), result.LastReference);

            result = addMonth.DeltaAddWReference(
                    Parse("2002-04-05T09:00:00.000"), Parse("2002-02-11T09:01:02.003"), null, true, null);
            Assert.AreEqual(Parse("2002-04-11T09:01:02.003") - Parse("2002-04-05T09:00:00.000"), result.Delta);
            Assert.AreEqual(Parse("2002-03-11T09:01:02.003"), result.LastReference);
        }

        private long Parse(string date)
        {
            return DateTimeParsingFunctions.ParseDefaultMSec(date);
        }
    }
} // end of namespace
