///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.rowrecog;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.rowrecog;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.rowrecog
{
    [TestFixture]
    public class TestSuiteRowRecog
    {
        private RegressionSession session;

        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        [Test]
        public void TestRowRecogOps()
        {
            RegressionRunner.Run(session, RowRecogOps.Executions());
        }

        [Test]
        public void TestRowRecogAfter()
        {
            RegressionRunner.Run(session, RowRecogAfter.Executions());
        }

        [Test]
        public void TestRowRecogInvalid()
        {
            RegressionRunner.Run(session, new RowRecogInvalid());
        }

        [Test]
        public void TestRowRecogClausePresence()
        {
            RegressionRunner.Run(session, new RowRecogClausePresence());
        }

        [Test]
        public void TestRowRecogGreedyness()
        {
            RegressionRunner.Run(session, RowRecogGreedyness.Executions());
        }

        [Test]
        public void TestRowRecogEmptyPartition()
        {
            RegressionRunner.Run(session, new RowRecogEmptyPartition());
        }

        [Test]
        public void TestRowRecogEnumMethod()
        {
            RegressionRunner.Run(session, new RowRecogEnumMethod());
        }

        [Test]
        public void TestRowRecogIntervalResolution()
        {
            RegressionRunner.Run(session, new RowRecogIntervalResolution(10000));
        }

        [Test]
        public void TestRowRecogIterateOnly()
        {
            RegressionRunner.Run(session, RowRecogIterateOnly.Executions());
        }

        [Test]
        public void TestRowRecogPerf()
        {
            RegressionRunner.Run(session, new RowRecogPerf());
        }

        [Test]
        public void TestRowRecogPermute()
        {
            RegressionRunner.Run(session, new RowRecogPermute());
        }

        [Test]
        public void TestRowRecogRegex()
        {
            RegressionRunner.Run(session, new RowRecogRegex());
        }

        [Test]
        public void TestRowRecogPrev()
        {
            RegressionRunner.Run(session, RowRecogPrev.Executions());
        }

        [Test]
        public void TestRowRecogDataWin()
        {
            RegressionRunner.Run(session, RowRecogDataWin.Executions());
        }

        [Test]
        public void TestRowRecogDelete()
        {
            RegressionRunner.Run(session, RowRecogDelete.Executions());
        }

        [Test]
        public void TestRowRecogRepetition()
        {
            RegressionRunner.Run(session, new RowRecogRepetition());
        }

        [Test]
        public void TestRowRecogAggregation()
        {
            RegressionRunner.Run(session, RowRecogAggregation.Executions());
        }

        [Test]
        public void TestRowRecogInterval()
        {
            RegressionRunner.Run(session, RowRecogInterval.Executions());
        }

        [Test]
        public void TestRowRecogIntervalOrTerminated()
        {
            RegressionRunner.Run(session, new RowRecogIntervalOrTerminated());
        }

        [Test]
        public void TestRowRecogVariantStream()
        {
            RegressionRunner.Run(session, new RowRecogVariantStream());
        }

        [Test]
        public void TestRowRecogArrayAccess()
        {
            RegressionRunner.Run(session, RowRecogArrayAccess.Executions());
        }

        [Test]
        public void TestRowRecogDataSet()
        {
            RegressionRunner.Run(session, RowRecogDataSet.Executions());
        }

        private void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportRecogBean),
                typeof(SupportBean_A),
                typeof(SupportBean_B)})
            {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddEventType("TemperatureSensorEvent",
                "id,device,temp".SplitCsv(), new object[] { typeof(string), typeof(int), typeof(double) });

            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));
            configuration.Common.AddVariable("mySleepDuration", typeof(long), 100);    // msec
        }
    }
} // end of namespace