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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.epl.join;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.Runner;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLJoin
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

        [Test, RunInApplicationDomain]
        public void TestEPLJoin2StreamSimple()
        {
            RegressionRunner.Run(session, new EPLJoin2StreamSimple());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinSelectClause()
        {
            RegressionRunner.Run(session, new EPLJoinSelectClause());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinSingleOp3Stream()
        {
            RegressionRunner.Run(session, EPLJoinSingleOp3Stream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoin2StreamAndPropertyPerformance()
        {
            RegressionRunner.Run(session, EPLJoin2StreamAndPropertyPerformance.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoin2StreamSimplePerformance()
        {
            RegressionRunner.Run(session, EPLJoin2StreamSimplePerformance.Executions());
        }

        [Test]
        public void TestEPLJoin2StreamRangePerformance()
        {
            RegressionRunner.Run(session, EPLJoin2StreamRangePerformance.Executions());
        }

        [Test]
        public void TestEPLJoin2StreamSimpleCoercionPerformance()
        {
            RegressionRunner.Run(session, EPLJoin2StreamSimpleCoercionPerformance.Executions());
        }

        [Test]
        public void TestEPLJoin3StreamRangePerformance()
        {
            RegressionRunner.Run(session, EPLJoin3StreamRangePerformance.Executions());
        }

        [Test]
        public void TestEPLJoin5StreamPerformance()
        {
            RegressionRunner.Run(session, new EPLJoin5StreamPerformance());
        }

        [Test]
        public void TestEPLJoin2StreamExprPerformance()
        {
            RegressionRunner.Run(session, new EPLJoin2StreamExprPerformance());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinCoercion()
        {
            RegressionRunner.Run(session, EPLJoinCoercion.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinMultiKeyAndRange()
        {
            RegressionRunner.Run(session, EPLJoinMultiKeyAndRange.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinStartStop()
        {
            RegressionRunner.Run(session, EPLJoinStartStop.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinDerivedValueViews()
        {
            RegressionRunner.Run(session, new EPLJoinDerivedValueViews());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinNoTableName()
        {
            RegressionRunner.Run(session, new EPLJoinNoTableName());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinNoWhereClause()
        {
            RegressionRunner.Run(session, EPLJoinNoWhereClause.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinEventRepresentation()
        {
            RegressionRunner.Run(session, EPLJoinEventRepresentation.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinInheritAndInterface()
        {
            RegressionRunner.Run(session, new EPLJoinInheritAndInterface());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinPatterns()
        {
            RegressionRunner.Run(session, EPLJoinPatterns.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoin2StreamInKeywordPerformance()
        {
            RegressionRunner.Run(session, EPLJoin2StreamInKeywordPerformance.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterJoin2Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoin2Stream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinUniqueIndex()
        {
            RegressionRunner.Run(session, new EPLJoinUniqueIndex());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterFullJoin3Stream()
        {
            RegressionRunner.Run(session, EPLOuterFullJoin3Stream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterInnerJoin3Stream()
        {
            RegressionRunner.Run(session, EPLOuterInnerJoin3Stream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterInnerJoin4Stream()
        {
            RegressionRunner.Run(session, EPLOuterInnerJoin4Stream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterJoin6Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoin6Stream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterJoin7Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoin7Stream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterJoinCart4Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoinCart4Stream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterJoinCart5Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoinCart5Stream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterJoinChain4Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoinChain4Stream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterJoinUnidirectional()
        {
            RegressionRunner.Run(session, EPLOuterJoinUnidirectional.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterJoinVarA3Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoinVarA3Stream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterJoinVarB3Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoinVarB3Stream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterJoinVarC3Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoinVarC3Stream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOuterJoinLeftWWhere()
        {
            RegressionRunner.Run(session, EPLOuterJoinLeftWWhere.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinUnidirectionalStream()
        {
            RegressionRunner.Run(session, EPLJoinUnidirectionalStream.Executions());
        }

        [Test]
        public void TestEPLJoin3StreamAndPropertyPerformance()
        {
            RegressionRunner.Run(session, EPLJoin3StreamAndPropertyPerformance.Executions());
        }

        [Test]
        public void TestEPLJoin3StreamCoercionPerformance()
        {
            RegressionRunner.Run(session, EPLJoin3StreamCoercionPerformance.Executions());
        }

        [Test]
        public void TestEPLJoin3StreamOuterJoinCoercionPerformance()
        {
            RegressionRunner.Run(session, EPLJoin3StreamOuterJoinCoercionPerformance.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoin20Stream()
        {
            RegressionRunner.Run(session, new EPLJoin20Stream());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoin3StreamInKeywordPerformance()
        {
            RegressionRunner.Run(session, new EPLJoin3StreamInKeywordPerformance());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinPropertyAccess()
        {
            RegressionRunner.Run(session, EPLJoinPropertyAccess.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{
                typeof(SupportBean),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportBean_C),
                typeof(SupportBean_D),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_S2),
                typeof(SupportBean_S3),
                typeof(SupportBean_S4),
                typeof(SupportBean_S5),
                typeof(SupportBean_S6),
                typeof(SupportBeanComplexProps),
                typeof(ISupportA),
                typeof(ISupportB),
                typeof(ISupportAImpl),
                typeof(ISupportBImpl),
                typeof(SupportBeanCombinedProps),
                typeof(SupportSimpleBeanOne),
                typeof(SupportSimpleBeanTwo),
                typeof(SupportMarketDataBean),
                typeof(SupportBean_ST0),
                typeof(SupportBean_ST1),
                typeof(SupportBeanRange)
            })
            {
                configuration.Common.AddEventType(clazz);
            }

            IDictionary<string, object> typeInfo = new Dictionary<string, object>();
            typeInfo.Put("Id", typeof(string));
            typeInfo.Put("P00", typeof(int));
            configuration.Common.AddEventType("MapS0", typeInfo);
            configuration.Common.AddEventType("MapS1", typeInfo);

            IDictionary<string, object> mapType = new Dictionary<string, object>();
            mapType.Put("col1", typeof(string));
            mapType.Put("col2", typeof(string));
            configuration.Common.AddEventType("Type1", mapType);
            configuration.Common.AddEventType("Type2", mapType);
            configuration.Common.AddEventType("Type3", mapType);

            IDictionary<string, object> typeInfoS0S0 = new Dictionary<string, object>();
            typeInfoS0S0.Put("Id", typeof(string));
            typeInfoS0S0.Put("P00", typeof(int));
            configuration.Common.AddEventType("S0_" + EventUnderlyingType.MAP.GetName(), typeInfoS0S0);
            configuration.Common.AddEventType("S1_" + EventUnderlyingType.MAP.GetName(), typeInfoS0S0);

            string[] names = new [] { "Id","P00" };
            object[] types = new object[] { typeof(string), typeof(int) };
            configuration.Common.AddEventType("S0_" + EventUnderlyingType.OBJECTARRAY.GetName(), names, types);
            configuration.Common.AddEventType("S1_" + EventUnderlyingType.OBJECTARRAY.GetName(), names, types);

            var schema = SchemaBuilder.Record("name",
                    Field("Id", StringType(AvroConstant.PROP_STRING)),
                    RequiredInt("P00"));
            configuration.Common.AddEventTypeAvro("S0_" + EventUnderlyingType.AVRO.GetName(), new ConfigurationCommonEventTypeAvro().SetAvroSchema(schema));
            configuration.Common.AddEventTypeAvro("S1_" + EventUnderlyingType.AVRO.GetName(), new ConfigurationCommonEventTypeAvro().SetAvroSchema(schema));

            configuration.Compiler.AddPlugInSingleRowFunction(
                "myStaticEvaluator", typeof(EPLJoin2StreamAndPropertyPerformance.MyStaticEval), "MyStaticEvaluator");

            configuration.Common.Logging.IsEnableQueryPlan = true;
        }
    }
} // end of namespace