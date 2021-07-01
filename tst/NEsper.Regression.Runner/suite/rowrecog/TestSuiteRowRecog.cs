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
            session.Dispose();
            session = null;
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogOps()
        {
            RegressionRunner.Run(session, RowRecogOps.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogAfter()
        {
            RegressionRunner.Run(session, RowRecogAfter.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogInvalid()
        {
            RegressionRunner.Run(session, new RowRecogInvalid());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogClausePresence()
        {
            RegressionRunner.Run(session, new RowRecogClausePresence());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogGreedyness()
        {
            RegressionRunner.Run(session, RowRecogGreedyness.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogEmptyPartition()
        {
            RegressionRunner.Run(session, new RowRecogEmptyPartition());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogEnumMethod()
        {
            RegressionRunner.Run(session, new RowRecogEnumMethod());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogIntervalResolution()
        {
            RegressionRunner.Run(session, new RowRecogIntervalResolution(10000));
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogIterateOnly()
        {
            RegressionRunner.Run(session, RowRecogIterateOnly.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogPerf()
        {
            RegressionRunner.Run(session, new RowRecogPerf());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogPermute()
        {
            RegressionRunner.Run(session, new RowRecogPermute());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogRegex()
        {
            RegressionRunner.Run(session, new RowRecogRegex());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogPrev()
        {
            RegressionRunner.Run(session, RowRecogPrev.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogDataWin()
        {
            RegressionRunner.Run(session, RowRecogDataWin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogDelete()
        {
            RegressionRunner.Run(session, RowRecogDelete.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogRepetition()
        {
            RegressionRunner.Run(session, new RowRecogRepetition());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogAggregation()
        {
            RegressionRunner.Run(session, RowRecogAggregation.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogInterval()
        {
            RegressionRunner.Run(session, RowRecogInterval.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogIntervalOrTerminated()
        {
            RegressionRunner.Run(session, new RowRecogIntervalOrTerminated());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogVariantStream()
        {
            RegressionRunner.Run(session, new RowRecogVariantStream());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogArrayAccess()
        {
            RegressionRunner.Run(session, RowRecogArrayAccess.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogDataSet()
        {
            RegressionRunner.Run(session, RowRecogDataSet.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogMultikeyWArray()
        {
            RegressionRunner.Run(session, RowRecogMultikeyWArray.Executions());
        }
        
        private void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportRecogBean),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportEventWithIntArray)
            })
            {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddEventType("TemperatureSensorEvent",
                new [] { "Id","Device","Temp" }, new object[] { typeof(string), typeof(int), typeof(double) });

            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));
            configuration.Common.AddVariable("mySleepDuration", typeof(long), 100);    // msec
        }
    }
} // end of namespace