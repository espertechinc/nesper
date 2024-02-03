///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

            configuration.Common.EventMeta.AvroSettings.IsEnableAvro = true;
            
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
                    new[] { "startTS", "endTS" },
                    new object[] {
                        fieldType.GetFieldType(),
                        fieldType.GetFieldType()
                    },
                    oa);
                configuration.Common.AddEventType(
                    "B_" + fieldType.GetName(),
                    new[] { "startTS", "endTS" },
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
                new[] { "Id", "Sts", "Ets" },
                new object[] { typeof(string), typeof(long), typeof(long) },
                oa);
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTDocSamples()
        {
            RegressionRunner.Run(_session, new ExprDTDocSamples());
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
        public void TestExprDTWithDate()
        {
            RegressionRunner.Run(_session, new ExprDTWithDate());
        }

        [Test, RunInApplicationDomain]
        public void TestExprDTWithTime()
        {
            RegressionRunner.Run(_session, new ExprDTWithTime());
        }

        /// <summary>
        /// Auto-test(s): ExprDTIntervalOps
        /// <code>
        /// RegressionRunner.Run(_session, ExprDTIntervalOps.Executions());
        /// </code>
        /// </summary>

        [Parallelizable(ParallelScope.All)]
        public class TestExprDTIntervalOps : AbstractTestBase
        {
            public TestExprDTIntervalOps() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTimePeriodWYearNonConst() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithTimePeriodWYearNonConst());

            [Test, RunInApplicationDomain]
            public void WithBeforeWVariable() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithBeforeWVariable());

            [Test, RunInApplicationDomain]
            public void WithPointInTimeWCalendarOps() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithPointInTimeWCalendarOps());

            [Test, RunInApplicationDomain]
            public void WithStartedByWhereClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithStartedByWhereClause());

            [Test, RunInApplicationDomain]
            public void WithStartsWhereClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithStartsWhereClause());

            [Test, RunInApplicationDomain]
            public void WithOverlappedByWhereClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithOverlappedByWhereClause());

            [Test, RunInApplicationDomain]
            public void WithOverlapsWhereClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithOverlapsWhereClause());

            [Test, RunInApplicationDomain]
            public void WithMetByWhereClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithMetByWhereClause());

            [Test, RunInApplicationDomain]
            public void WithMeetsWhereClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithMeetsWhereClause());

            [Test, RunInApplicationDomain]
            public void WithIncludesByWhereClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithIncludesByWhereClause());

            [Test, RunInApplicationDomain]
            public void WithFinishedByWhereClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithFinishedByWhereClause());

            [Test, RunInApplicationDomain]
            public void WithFinishesWhereClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithFinishesWhereClause());

            [Test, RunInApplicationDomain]
            public void WithDuringWhereClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithDuringWhereClause());

            [Test, RunInApplicationDomain]
            public void WithCoincidesWhereClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithCoincidesWhereClause());

            [Test, RunInApplicationDomain]
            public void WithAfterWhereClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithAfterWhereClause());

            [Test, RunInApplicationDomain]
            public void WithBeforeWhereClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithBeforeWhereClause());

            [Test, RunInApplicationDomain]
            public void WithBeforeWhereClauseWithBean() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithBeforeWhereClauseWithBean());

            [Test, RunInApplicationDomain]
            public void WithBeforeInSelectClause() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithBeforeInSelectClause());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithCalendarOps() => RegressionRunner.Run(_session, ExprDTIntervalOps.WithCalendarOps());
        }

        /// <summary>
        /// Auto-test(s): ExprDTGet
        /// <code>
        /// RegressionRunner.Run(_session, ExprDTGet.Executions());
        /// </code>
        /// </summary>

        public class TestExprDTGet : AbstractTestBase
        {
            public TestExprDTGet() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInput() => RegressionRunner.Run(_session, ExprDTGet.WithInput());

            [Test, RunInApplicationDomain]
            public void WithFields() => RegressionRunner.Run(_session, ExprDTGet.WithFields());
        }

        /// <summary>
        /// Auto-test(s): ExprDTFormat
        /// <code>
        /// RegressionRunner.Run(_session, ExprDTFormat.Executions());
        /// </code>
        /// </summary>

        public class TestExprDTFormat : AbstractTestBase
        {
            public TestExprDTFormat() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWString() => RegressionRunner.Run(_session, ExprDTFormat.WithWString());

            [Test, RunInApplicationDomain]
            public void WithSimple() => RegressionRunner.Run(_session, ExprDTFormat.WithSimple());
        }

        /// <summary>
        /// Auto-test(s): ExprDTDataSources
        /// <code>
        /// RegressionRunner.Run(_session, ExprDTDataSources.Executions());
        /// </code>
        /// </summary>

        public class TestExprDTDataSources : AbstractTestBase
        {
            public TestExprDTDataSources() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMinMax() => RegressionRunner.Run(_session, ExprDTDataSources.WithMinMax());

            [Test, RunInApplicationDomain]
            public void WithAllCombinations() => RegressionRunner.Run(_session, ExprDTDataSources.WithAllCombinations());

            [Test, RunInApplicationDomain]
            public void WithFieldWValue() => RegressionRunner.Run(_session, ExprDTDataSources.WithFieldWValue());

            [Test, RunInApplicationDomain]
            public void WithStartEndTS() => RegressionRunner.Run(_session, ExprDTDataSources.WithStartEndTS());
        }

        /// <summary>
        /// Auto-test(s): ExprDTBetween
        /// <code>
        /// RegressionRunner.Run(_session, ExprDTBetween.Executions());
        /// </code>
        /// </summary>

        public class TestExprDTBetween : AbstractTestBase
        {
            public TestExprDTBetween() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTypes() => RegressionRunner.Run(_session, ExprDTBetween.WithTypes());

            [Test, RunInApplicationDomain]
            public void WithExcludeEndpoints() => RegressionRunner.Run(_session, ExprDTBetween.WithExcludeEndpoints());

            [Test, RunInApplicationDomain]
            public void WithIncludeEndpoints() => RegressionRunner.Run(_session, ExprDTBetween.WithIncludeEndpoints());
        }

        /// <summary>
        /// Auto-test(s): ExprDTPlusMinus
        /// <code>
        /// RegressionRunner.Run(_session, ExprDTPlusMinus.Executions());
        /// </code>
        /// </summary>

        public class TestExprDTPlusMinus : AbstractTestBase
        {
            public TestExprDTPlusMinus() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTimePeriod() => RegressionRunner.Run(_session, ExprDTPlusMinus.WithTimePeriod());

            [Test, RunInApplicationDomain]
            public void WithSimple() => RegressionRunner.Run(_session, ExprDTPlusMinus.WithSimple());
        }

        /// <summary>
        /// Auto-test(s): ExprDTResolution
        /// <code>
        /// RegressionRunner.Run(_session, ExprDTResolution.Executions());
        /// </code>
        /// </summary>

        public class TestExprDTResolution : AbstractTestBase
        {
            public TestExprDTResolution() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithLongProperty() => RegressionRunner.Run(_session, ExprDTResolution.WithLongProperty(false));

            [Test, RunInApplicationDomain]
            public void WithResolutionEventTime() => RegressionRunner.Run(_session, ExprDTResolution.WithResolutionEventTime(false));
        }

        /// <summary>
        /// Auto-test(s): ExprDTRound
        /// <code>
        /// RegressionRunner.Run(_session, ExprDTRound.Executions());
        /// </code>
        /// </summary>

        public class TestExprDTRound : AbstractTestBase
        {
            public TestExprDTRound() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithHalf() => RegressionRunner.Run(_session, ExprDTRound.WithHalf());

            [Test, RunInApplicationDomain]
            public void WithFloor() => RegressionRunner.Run(_session, ExprDTRound.WithFloor());

            [Test, RunInApplicationDomain]
            public void WithCeil() => RegressionRunner.Run(_session, ExprDTRound.WithCeil());

            [Test, RunInApplicationDomain]
            public void WithInput() => RegressionRunner.Run(_session, ExprDTRound.WithInput());
        }

        /// <summary>
        /// Auto-test(s): ExprDTSet
        /// <code>
        /// RegressionRunner.Run(_session, ExprDTSet.Executions());
        /// </code>
        /// </summary>

        public class TestExprDTSet : AbstractTestBase
        {
            public TestExprDTSet() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFields() => RegressionRunner.Run(_session, ExprDTSet.WithFields());

            [Test, RunInApplicationDomain]
            public void WithInput() => RegressionRunner.Run(_session, ExprDTSet.WithInput());
        }

        /// <summary>
        /// Auto-test(s): ExprDTToDateMSec
        /// <code>
        /// RegressionRunner.Run(_session, ExprDTToDateMSec.Executions());
        /// </code>
        /// </summary>

        public class TestExprDTToDateMSec : AbstractTestBase
        {
            public TestExprDTToDateMSec() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDTToDateTimeExMSecValue() => RegressionRunner.Run(_session, ExprDTToDateMSec.WithDTToDateTimeExMSecValue());

            [Test, RunInApplicationDomain]
            public void WithToDateTimeExChain() => RegressionRunner.Run(_session, ExprDTToDateMSec.WithToDateTimeExChain());
        }

        /// <summary>
        /// Auto-test(s): ExprDTWithMax
        /// <code>
        /// RegressionRunner.Run(_session, ExprDTWithMax.Executions());
        /// </code>
        /// </summary>

        public class TestExprDTWithMax : AbstractTestBase
        {
            public TestExprDTWithMax() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFields() => RegressionRunner.Run(_session, ExprDTWithMax.WithFields());

            [Test, RunInApplicationDomain]
            public void WithInput() => RegressionRunner.Run(_session, ExprDTWithMax.WithInput());
        }

        /// <summary>
        /// Auto-test(s): ExprDTWithMin
        /// <code>
        /// RegressionRunner.Run(_session, ExprDTWithMin.Executions());
        /// </code>
        /// </summary>

        public class TestExprDTWithMin : AbstractTestBase
        {
            public TestExprDTWithMin() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFields() => RegressionRunner.Run(_session, ExprDTWithMin.WithFields());

            [Test, RunInApplicationDomain]
            public void WithInput() => RegressionRunner.Run(_session, ExprDTWithMin.WithInput());
        }
    }
} // end of namespace