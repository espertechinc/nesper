///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.epl.other;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBean_N = com.espertech.esper.regressionlib.support.bean.SupportBean_N;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;
using SupportBeanSimple = com.espertech.esper.regressionlib.support.bean.SupportBeanSimple;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLOther
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
            configuration.Common.AddEventType("ObjectEvent", typeof(object));

            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_S2),
                typeof(SupportBeanSourceEvent),
                typeof(OrderBean),
                typeof(SupportBeanReadOnly),
                typeof(SupportBeanErrorTestingOne),
                typeof(OrderBean),
                typeof(SupportCollection),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportBean_N),
                typeof(SupportChainTop),
                typeof(SupportTemperatureBean),
                typeof(SupportBeanKeywords),
                typeof(SupportBeanSimple),
                typeof(SupportBeanStaticOuter),
                typeof(SupportMarketDataBean),
                typeof(SupportBeanComplexProps),
                typeof(SupportBeanCombinedProps)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            IDictionary<string, object> myMapTypeInv = new Dictionary<string, object>();
            myMapTypeInv.Put("P0", typeof(long));
            myMapTypeInv.Put("P1", typeof(long));
            myMapTypeInv.Put("P2", typeof(long));
            myMapTypeInv.Put("p3", typeof(string));
            configuration.Common.AddEventType("MyMapTypeInv", myMapTypeInv);

            IDictionary<string, object> myMapTypeII = new Dictionary<string, object>();
            myMapTypeII.Put("P0", typeof(long));
            myMapTypeII.Put("P1", typeof(long));
            myMapTypeII.Put("P2", typeof(long));
            configuration.Common.AddEventType("MyMapTypeII", myMapTypeII);

            IDictionary<string, object> myMapTypeIDB = new Dictionary<string, object>();
            myMapTypeIDB.Put("P0", typeof(string));
            myMapTypeIDB.Put("P1", typeof(string));
            configuration.Common.AddEventType("MyMapTypeIDB", myMapTypeIDB);

            IDictionary<string, object> myMapTypeNW = new Dictionary<string, object>();
            myMapTypeNW.Put("P0", typeof(string));
            myMapTypeNW.Put("P1", typeof(string));
            configuration.Common.AddEventType("MyMapTypeNW", myMapTypeNW);

            IDictionary<string, object> myMapTypeSR = new Dictionary<string, object>();
            myMapTypeSR.Put("P0", typeof(string));
            myMapTypeSR.Put("P1", typeof(string));
            configuration.Common.AddEventType("MyMapTypeSR", myMapTypeSR);

            IDictionary<string, object> myMapTypeSODA = new Dictionary<string, object>();
            myMapTypeSODA.Put("P0", typeof(string));
            myMapTypeSODA.Put("P1", typeof(string));
            configuration.Common.AddEventType("MyMapTypeSODA", myMapTypeSODA);

            var configXML = new ConfigurationCommonEventTypeXMLDOM();
            configXML.RootElementName = "MyXMLEvent";
            configuration.Common.AddEventType("MyXmlEvent", configXML);

            var config = new ConfigurationCommonEventTypeXMLDOM();
            config.RootElementName = "simpleEvent";
            configuration.Common.AddEventType("MyXMLEvent", config);

            IDictionary<string, object> myMapTypeSelect = new Dictionary<string, object>();
            myMapTypeSelect.Put("s0", typeof(string));
            myMapTypeSelect.Put("s1", typeof(int));
            configuration.Common.AddEventType("MyMapTypeSelect", myMapTypeSelect);

            IDictionary<string, object> myMapTypeWhere = new Dictionary<string, object>();
            myMapTypeWhere.Put("w0", typeof(int));
            configuration.Common.AddEventType("MyMapTypeWhere", myMapTypeWhere);

            IDictionary<string, object> myMapTypeUO = new Dictionary<string, object>();
            myMapTypeUO.Put("s0", typeof(string));
            myMapTypeUO.Put("s1", typeof(int));
            configuration.Common.AddEventType("MyMapTypeUO", myMapTypeUO);

            var legacy = new ConfigurationCommonEventTypeBean();
            legacy.CopyMethod = "myCopyMethod";
            configuration.Common.AddEventType("SupportBeanCopyMethod", typeof(SupportBeanCopyMethod), legacy);

            IDictionary<string, object> defMapTypeKVDistinct = new Dictionary<string, object>();
            defMapTypeKVDistinct.Put("k1", typeof(string));
            defMapTypeKVDistinct.Put("v1", typeof(int));
            configuration.Common.AddEventType("MyMapTypeKVDistinct", defMapTypeKVDistinct);

            IDictionary<string, object> typeMap = new Dictionary<string, object>();
            typeMap.Put("int", typeof(int?));
            typeMap.Put("TheString", typeof(string));
            configuration.Common.AddEventType("MyMapEventIntString", typeMap);

            configuration.Common.AddEventType("MapTypeEmpty", new Dictionary<string, object>());

            var testXMLNoSchemaType = new ConfigurationCommonEventTypeXMLDOM();
            testXMLNoSchemaType.RootElementName = "myevent";
            configuration.Common.AddEventType("TestXMLNoSchemaType", testXMLNoSchemaType);

            IDictionary<string, object> myConfiguredMape = new Dictionary<string, object>();
            myConfiguredMape.Put("bean", "SupportBean");
            myConfiguredMape.Put("beanarray", "SupportBean_S0[]");
            configuration.Common.AddEventType("MyConfiguredMap", myConfiguredMape);

            configuration.Common.Logging.IsEnableQueryPlan = true;
            configuration.Runtime.Execution.IsPrioritized = true;

            configuration.Common.AddVariable("myvar", typeof(int?), 10);

            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));
            configuration.Common.AddImportType(typeof(EPLOtherStaticFunctions.LevelZero));
            configuration.Common.AddImportType(typeof(SupportChainTop));
            configuration.Common.AddImportType(typeof(EPLOtherStaticFunctions.NullPrimitive));
            configuration.Common.AddImportType(typeof(EPLOtherStaticFunctions.PrimitiveConversionLib));

            configuration.Compiler.ByteCode.AllowSubscriber = true;
            configuration.Compiler.AddPlugInSingleRowFunction(
                "sleepme",
                typeof(SupportStaticMethodLib),
                "Sleep",
                ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.ENABLED);
        }

        [Test]
        public void TestEPLOtherAsKeywordBacktick()
        {
            RegressionRunner.Run(session, EPLOtherAsKeywordBacktick.Executions());
        }

        [Test]
        public void TestEPLOtherComments()
        {
            RegressionRunner.Run(session, new EPLOtherComments());
        }

        [Test]
        public void TestEPLOtherCreateExpression()
        {
            RegressionRunner.Run(session, EPLOtherCreateExpression.Executions());
        }

        [Test]
        public void TestEPLOtherCreateIndex()
        {
            RegressionRunner.Run(session, EPLOtherCreateIndex.Executions());
        }

        [Test]
        public void TestEPLOtherCreateSchema()
        {
            RegressionRunner.Run(session, EPLOtherCreateSchema.Executions());
        }

        [Test]
        public void TestEPLOtherDistinct()
        {
            RegressionRunner.Run(session, EPLOtherDistinct.Executions());
        }

        [Test]
        public void TestEPLOtherForGroupDelivery()
        {
            RegressionRunner.Run(session, EPLOtherForGroupDelivery.Executions());
        }

        [Test]
        public void TestEPLOtherInvalid()
        {
            RegressionRunner.Run(session, EPLOtherInvalid.Executions());
        }

        [Test]
        public void TestEPLOtherIStreamRStreamKeywords()
        {
            RegressionRunner.Run(session, EPLOtherIStreamRStreamKeywords.Executions());
        }

        [Test]
        public void TestEPLOtherLiteralConstants()
        {
            RegressionRunner.Run(session, new EPLOtherLiteralConstants());
        }

        [Test]
        public void TestEPLOtherPatternEventProperties()
        {
            RegressionRunner.Run(session, EPLOtherPatternEventProperties.Executions());
        }

        [Test]
        public void TestEPLOtherPatternQueries()
        {
            RegressionRunner.Run(session, EPLOtherPatternQueries.Executions());
        }

        [Test]
        public void TestEPLOtherPlanExcludeHint()
        {
            RegressionRunner.Run(session, EPLOtherPlanExcludeHint.Executions());
        }

        [Test]
        public void TestEPLOtherPlanInKeywordQuery()
        {
            RegressionRunner.Run(session, EPLOtherPlanInKeywordQuery.Executions());
        }

        [Test]
        public void TestEPLOtherSelectExpr()
        {
            RegressionRunner.Run(session, EPLOtherSelectExpr.Executions());
        }

        [Test]
        public void TestEPLOtherSelectExprEventBeanAnnotation()
        {
            RegressionRunner.Run(session, EPLOtherSelectExprEventBeanAnnotation.Executions());
        }

        [Test]
        public void TestEPLOtherSelectExprSQLCompat()
        {
            RegressionRunner.Run(session, EPLOtherSelectExprSQLCompat.Executions());
        }

        [Test]
        public void TestEPLOtherSelectExprStreamSelector()
        {
            RegressionRunner.Run(session, EPLOtherSelectExprStreamSelector.Executions());
        }

        [Test]
        public void TestEPLOtherSelectJoin()
        {
            RegressionRunner.Run(session, EPLOtherSelectJoin.Executions());
        }

        [Test]
        public void TestEPLOtherSelectWildcardWAdditional()
        {
            RegressionRunner.Run(session, EPLOtherSelectWildcardWAdditional.Executions());
        }

        [Test]
        public void TestEPLOtherSplitStream()
        {
            RegressionRunner.Run(session, EPLOtherSplitStream.Executions());
        }

        [Test]
        public void TestEPLOtherStaticFunctions()
        {
            RegressionRunner.Run(session, EPLOtherStaticFunctions.Executions());
        }

        [Test]
        public void TestEPLOtherStreamExpr()
        {
            RegressionRunner.Run(session, EPLOtherStreamExpr.Executions());
        }

        [Test]
        public void TestEPLOtherUnaryMinus()
        {
            RegressionRunner.Run(session, new EPLOtherUnaryMinus());
        }

        [Test]
        public void TestEPLOtherUpdateIStream()
        {
            RegressionRunner.Run(session, EPLOtherUpdateIStream.Executions());
        }
    }
} // end of namespace