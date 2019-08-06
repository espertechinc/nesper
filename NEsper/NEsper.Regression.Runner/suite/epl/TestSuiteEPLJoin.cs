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
using com.espertech.esper.regressionrun.runner;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

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

        [Test]
        public void TestEPLJoin2StreamSimple()
        {
            RegressionRunner.Run(session, new EPLJoin2StreamSimple());
        }

        [Test]
        public void TestEPLJoinSelectClause()
        {
            RegressionRunner.Run(session, new EPLJoinSelectClause());
        }

        [Test]
        public void TestEPLJoinSingleOp3Stream()
        {
            RegressionRunner.Run(session, EPLJoinSingleOp3Stream.Executions());
        }

        [Test]
        public void TestEPLJoin2StreamAndPropertyPerformance()
        {
            RegressionRunner.Run(session, EPLJoin2StreamAndPropertyPerformance.Executions());
        }

        [Test]
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

        [Test]
        public void TestEPLJoinCoercion()
        {
            RegressionRunner.Run(session, EPLJoinCoercion.Executions());
        }

        [Test]
        public void TestEPLJoinMultiKeyAndRange()
        {
            RegressionRunner.Run(session, EPLJoinMultiKeyAndRange.Executions());
        }

        [Test]
        public void TestEPLJoinStartStop()
        {
            RegressionRunner.Run(session, EPLJoinStartStop.Executions());
        }

        [Test]
        public void TestEPLJoinDerivedValueViews()
        {
            RegressionRunner.Run(session, new EPLJoinDerivedValueViews());
        }

        [Test]
        public void TestEPLJoinNoTableName()
        {
            RegressionRunner.Run(session, new EPLJoinNoTableName());
        }

        [Test]
        public void TestEPLJoinNoWhereClause()
        {
            RegressionRunner.Run(session, EPLJoinNoWhereClause.Executions());
        }

        [Test]
        public void TestEPLJoinEventRepresentation()
        {
            RegressionRunner.Run(session, EPLJoinEventRepresentation.Executions());
        }

        [Test]
        public void TestEPLJoinInheritAndInterface()
        {
            RegressionRunner.Run(session, new EPLJoinInheritAndInterface());
        }

        [Test]
        public void TestEPLJoinPatterns()
        {
            RegressionRunner.Run(session, EPLJoinPatterns.Executions());
        }

        [Test]
        public void TestEPLJoin2StreamInKeywordPerformance()
        {
            RegressionRunner.Run(session, EPLJoin2StreamInKeywordPerformance.Executions());
        }

        [Test]
        public void TestEPLOuterJoin2Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoin2Stream.Executions());
        }

        [Test]
        public void TestEPLJoinUniqueIndex()
        {
            RegressionRunner.Run(session, new EPLJoinUniqueIndex());
        }

        [Test]
        public void TestEPLOuterFullJoin3Stream()
        {
            RegressionRunner.Run(session, EPLOuterFullJoin3Stream.Executions());
        }

        [Test]
        public void TestEPLOuterInnerJoin3Stream()
        {
            RegressionRunner.Run(session, EPLOuterInnerJoin3Stream.Executions());
        }

        [Test]
        public void TestEPLOuterInnerJoin4Stream()
        {
            RegressionRunner.Run(session, EPLOuterInnerJoin4Stream.Executions());
        }

        [Test]
        public void TestEPLOuterJoin6Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoin6Stream.Executions());
        }

        [Test]
        public void TestEPLOuterJoin7Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoin7Stream.Executions());
        }

        [Test]
        public void TestEPLOuterJoinCart4Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoinCart4Stream.Executions());
        }

        [Test]
        public void TestEPLOuterJoinCart5Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoinCart5Stream.Executions());
        }

        [Test]
        public void TestEPLOuterJoinChain4Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoinChain4Stream.Executions());
        }

        [Test]
        public void TestEPLOuterJoinUnidirectional()
        {
            RegressionRunner.Run(session, EPLOuterJoinUnidirectional.Executions());
        }

        [Test]
        public void TestEPLOuterJoinVarA3Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoinVarA3Stream.Executions());
        }

        [Test]
        public void TestEPLOuterJoinVarB3Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoinVarB3Stream.Executions());
        }

        [Test]
        public void TestEPLOuterJoinVarC3Stream()
        {
            RegressionRunner.Run(session, EPLOuterJoinVarC3Stream.Executions());
        }

        [Test]
        public void TestEPLOuterJoinLeftWWhere()
        {
            RegressionRunner.Run(session, EPLOuterJoinLeftWWhere.Executions());
        }

        [Test]
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

        [Test]
        public void TestEPLJoin20Stream()
        {
            RegressionRunner.Run(session, new EPLJoin20Stream());
        }

        [Test]
        public void TestEPLJoin3StreamInKeywordPerformance()
        {
            RegressionRunner.Run(session, new EPLJoin3StreamInKeywordPerformance());
        }

        [Test]
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
            typeInfo.Put("id", typeof(string));
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
            typeInfoS0S0.Put("id", typeof(string));
            typeInfoS0S0.Put("P00", typeof(int));
            configuration.Common.AddEventType("S0_" + EventUnderlyingType.MAP.GetName(), typeInfoS0S0);
            configuration.Common.AddEventType("S1_" + EventUnderlyingType.MAP.GetName(), typeInfoS0S0);

            string[] names = "id,P00".SplitCsv();
            object[] types = new object[] { typeof(string), typeof(int) };
            configuration.Common.AddEventType("S0_" + EventUnderlyingType.OBJECTARRAY.GetName(), names, types);
            configuration.Common.AddEventType("S1_" + EventUnderlyingType.OBJECTARRAY.GetName(), names, types);

            var schema = SchemaBuilder.Record("name",
                    TypeBuilder.Field("id", TypeBuilder.StringType(
                            TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE))),
                    TypeBuilder.RequiredInt("P00"));
            configuration.Common.AddEventTypeAvro("S0_" + EventUnderlyingType.AVRO.GetName(), new ConfigurationCommonEventTypeAvro().SetAvroSchema(schema));
            configuration.Common.AddEventTypeAvro("S1_" + EventUnderlyingType.AVRO.GetName(), new ConfigurationCommonEventTypeAvro().SetAvroSchema(schema));

            configuration.Compiler.AddPlugInSingleRowFunction("myStaticEvaluator", typeof(EPLJoin2StreamAndPropertyPerformance.MyStaticEval), "myStaticEvaluator");

            configuration.Common.Logging.IsEnableQueryPlan = true;
        }
    }
} // end of namespace