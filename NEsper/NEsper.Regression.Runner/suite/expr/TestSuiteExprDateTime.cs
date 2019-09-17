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
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprDateTime
    {
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

        private RegressionSession session;

        private static void Configure(Configuration configuration)
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
                new [] { "Id", "Sts", "Ets" },
                new object[] {typeof(string), typeof(long), typeof(long)},
                oa);
        }

        [Test]
        public void TestExprDTBetween()
        {
            RegressionRunner.Run(session, ExprDTBetween.Executions());
        }

        [Test]
        public void TestExprDTDataSources()
        {
            RegressionRunner.Run(session, ExprDTDataSources.Executions());
        }

        [Test]
        public void TestExprDTDocSamples()
        {
            RegressionRunner.Run(session, new ExprDTDocSamples());
        }

        [Test]
        public void TestExprDTFormat()
        {
            RegressionRunner.Run(session, ExprDTFormat.Executions());
        }

        [Test]
        public void TestExprDTGet()
        {
            RegressionRunner.Run(session, ExprDTGet.Executions());
        }

        [Test]
        public void TestExprDTIntervalOps()
        {
            RegressionRunner.Run(session, ExprDTIntervalOps.Executions());
        }

        [Test]
        public void TestExprDTIntervalOpsCreateSchema()
        {
            RegressionRunner.Run(session, new ExprDTIntervalOpsCreateSchema());
        }

        [Test]
        public void TestExprDTInvalid()
        {
            RegressionRunner.Run(session, new ExprDTInvalid());
        }

        [Test]
        public void TestExprDTNested()
        {
            RegressionRunner.Run(session, new ExprDTNested());
        }

        [Test]
        public void TestExprDTPerfBetween()
        {
            RegressionRunner.Run(session, new ExprDTPerfBetween());
        }

        [Test]
        public void TestExprDTPerfIntervalOps()
        {
            RegressionRunner.Run(session, new ExprDTPerfIntervalOps());
        }

        [Test]
        public void TestExprDTPlusMinus()
        {
            RegressionRunner.Run(session, ExprDTPlusMinus.Executions());
        }

        [Test]
        public void TestExprDTResolution()
        {
            RegressionRunner.Run(session, ExprDTResolution.Executions(false));
        }

        [Test]
        public void TestExprDTRound()
        {
            RegressionRunner.Run(session, ExprDTRound.Executions());
        }

        [Test]
        public void TestExprDTSet()
        {
            RegressionRunner.Run(session, ExprDTSet.Executions());
        }

        [Test]
        public void TestExprDTToDateCalMSec()
        {
            RegressionRunner.Run(session, new ExprDTToDateCalMSec());
        }

        [Test]
        public void TestExprDTWithDate()
        {
            RegressionRunner.Run(session, new ExprDTWithDate());
        }

        [Test]
        public void TestExprDTWithMax()
        {
            RegressionRunner.Run(session, ExprDTWithMax.Executions());
        }

        [Test]
        public void TestExprDTWithMin()
        {
            RegressionRunner.Run(session, ExprDTWithMin.Executions());
        }

        [Test]
        public void TestExprDTWithTime()
        {
            RegressionRunner.Run(session, new ExprDTWithTime());
        }
    }
} // end of namespace