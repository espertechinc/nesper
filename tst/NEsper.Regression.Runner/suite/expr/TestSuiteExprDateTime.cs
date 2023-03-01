///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.suite.expr.datetime;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.schedule;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprDateTime : AbstractTestBase
    {
        public static void Configure(Configuration configuration)
        {
            configuration.Common.AddImportType(typeof(DateTimeParsingFunctions));

            foreach (var clazz in new[] {
                typeof(SupportDateTime),
                typeof(SupportTimeStartEndA),
                typeof(SupportBean),
                typeof(SupportEventWithJustGet),
                typeof(SupportBean_ST0_Container)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            IDictionary<string, object> meta = new Dictionary<string, object>();
            meta.Put("timeTaken", typeof(DateTimeEx));
            configuration.Common.AddEventType("RFIDEvent", meta);

            var common = configuration.Common;
            common.AddVariable("V_START", typeof(long), -1);
            common.AddVariable("V_END", typeof(long), -1);

            var leg = new ConfigurationCommonEventTypeBean();
            leg.StartTimestampPropertyName = "LongdateStart";
            configuration.Common.AddEventType("A", typeof(SupportTimeStartEndA), leg);
            configuration.Common.AddEventType("B", typeof(SupportTimeStartEndB), leg);

            var configBean = new ConfigurationCommonEventTypeBean();
            configBean.StartTimestampPropertyName = "LongdateStart";
            configBean.EndTimestampPropertyName = "LongdateEnd";
            configuration.Common.AddEventType("SupportTimeStartEndA", typeof(SupportTimeStartEndA), configBean);
            configuration.Common.AddEventType("SupportTimeStartEndB", typeof(SupportTimeStartEndB), configBean);

            configuration.Common.AddImportType(typeof(DateTime));
            configuration.Common.AddImportType(typeof(SupportBean_ST0_Container));
            configuration.Compiler.AddPlugInSingleRowFunction(
                "makeTest",
                typeof(SupportBean_ST0_Container),
                "MakeTest");

            foreach (var fieldType in EnumHelper.GetValues<SupportDateTimeFieldType>()) {
                var oa = new ConfigurationCommonEventTypeObjectArray();
                oa.StartTimestampPropertyName = "startTS";
                oa.EndTimestampPropertyName = "endTS";
                configuration.Common.AddEventType(
                    "A_" + fieldType.GetName(),
                    new[] {"startTS", "endTS"},
                    new object[] {
                        fieldType.GetFieldType(),
                        fieldType.GetFieldType()
                    },
                    oa);
                configuration.Common.AddEventType(
                    "B_" + fieldType.GetName(),
                    new[] {"startTS", "endTS"},
                    new object[] {
                        fieldType.GetFieldType(),
                        fieldType.GetFieldType()
                    },
                    oa);
            }

            AddIdStsEtsEvent(configuration);
        }

        internal static void AddIdStsEtsEvent(Configuration configuration)
        {
            var oa = new ConfigurationCommonEventTypeObjectArray();
            oa.StartTimestampPropertyName = "Sts";
            oa.EndTimestampPropertyName = "Ets";
            configuration.Common.AddEventType(
                "MyEvent",
                new[] {"Id", "Sts", "Ets"},
                new object[] {typeof(string), typeof(long), typeof(long)},
                oa);
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTBetween()
        {
            RegressionRunner.Run(_session, ExprDTBetween.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTDataSources()
        {
            RegressionRunner.Run(_session, ExprDTDataSources.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTDocSamples()
        {
            RegressionRunner.Run(_session, new ExprDTDocSamples());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTFormat()
        {
            RegressionRunner.Run(_session, ExprDTFormat.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTGet()
        {
            RegressionRunner.Run(_session, ExprDTGet.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTIntervalOps()
        {
            RegressionRunner.Run(_session, ExprDTIntervalOps.Executions());
        }

        [Test]
        public void TestExprDTIntervalOpsCreateSchema()
        {
            RegressionRunner.Run(_session, new ExprDTIntervalOpsCreateSchema());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTInvalid()
        {
            RegressionRunner.Run(_session, new ExprDTInvalid());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTNested()
        {
            RegressionRunner.Run(_session, new ExprDTNested());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTPerfBetween()
        {
            using (new PerformanceContext()) {
                RegressionRunner.Run(_session, new ExprDTPerfBetween());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTPerfIntervalOps()
        {
            using (new PerformanceContext()) {
                RegressionRunner.Run(_session, new ExprDTPerfIntervalOps());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTPlusMinus()
        {
            RegressionRunner.Run(_session, ExprDTPlusMinus.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTResolution()
        {
            RegressionRunner.Run(_session, ExprDTResolution.Executions(false));
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTRound()
        {
            RegressionRunner.Run(_session, ExprDTRound.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTSet()
        {
            RegressionRunner.Run(_session, ExprDTSet.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTToDateMSec()
        {
            RegressionRunner.Run(_session, ExprDTToDateMSec.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTWithDate()
        {
            RegressionRunner.Run(_session, new ExprDTWithDate());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTWithMax()
        {
            RegressionRunner.Run(_session, ExprDTWithMax.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTWithMin()
        {
            RegressionRunner.Run(_session, ExprDTWithMin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTWithTime()
        {
            RegressionRunner.Run(_session, new ExprDTWithTime());
        }
    }
} // end of namespace