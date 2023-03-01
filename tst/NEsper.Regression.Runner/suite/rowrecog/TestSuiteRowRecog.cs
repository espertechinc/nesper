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
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.rowrecog
{
    [TestFixture]
    public class TestSuiteRowRecog : AbstractTestBase
    {
        [Test, RunInApplicationDomain]
        public void TestRowRecogOps()
        {
            RegressionRunner.Run(_session, RowRecogOps.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogAfter()
        {
            RegressionRunner.Run(_session, RowRecogAfter.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogInvalid()
        {
            RegressionRunner.Run(_session, new RowRecogInvalid());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogClausePresence()
        {
            RegressionRunner.Run(_session, new RowRecogClausePresence());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogGreedyness()
        {
            RegressionRunner.Run(_session, RowRecogGreedyness.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogEmptyPartition()
        {
            RegressionRunner.Run(_session, new RowRecogEmptyPartition());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogEnumMethod()
        {
            RegressionRunner.Run(_session, new RowRecogEnumMethod());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogIntervalResolution()
        {
            RegressionRunner.Run(_session, new RowRecogIntervalResolution(10000));
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogIterateOnly()
        {
            RegressionRunner.Run(_session, RowRecogIterateOnly.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogPerf()
        {
            RegressionRunner.Run(_session, new RowRecogPerf());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogPermute()
        {
            RegressionRunner.Run(_session, new RowRecogPermute());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogRegex()
        {
            RegressionRunner.Run(_session, new RowRecogRegex());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogPrev()
        {
            RegressionRunner.Run(_session, RowRecogPrev.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogDataWin()
        {
            RegressionRunner.Run(_session, RowRecogDataWin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogDelete()
        {
            RegressionRunner.Run(_session, RowRecogDelete.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogRepetition()
        {
            RegressionRunner.Run(_session, new RowRecogRepetition());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogAggregation()
        {
            RegressionRunner.Run(_session, RowRecogAggregation.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogInterval()
        {
            RegressionRunner.Run(_session, RowRecogInterval.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogIntervalOrTerminated()
        {
            RegressionRunner.Run(_session, new RowRecogIntervalOrTerminated());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogVariantStream()
        {
            RegressionRunner.Run(_session, new RowRecogVariantStream());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogArrayAccess()
        {
            RegressionRunner.Run(_session, RowRecogArrayAccess.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogDataSet()
        {
            RegressionRunner.Run(_session, RowRecogDataSet.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogMultikeyWArray()
        {
            RegressionRunner.Run(_session, RowRecogMultikeyWArray.Executions());
        }
        
        public static void Configure(Configuration configuration)
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