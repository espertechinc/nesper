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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.infra.namedwindow;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.util.CollectionUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.infra
{
    // see INFRA suite for additional Named Window tests
    [TestFixture]
    public class TestSuiteInfraNamedWindow
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
            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(OrderBean),
                typeof(OrderWithItems),
                typeof(SupportBeanAtoFBase),
                typeof(SupportBean_A),
                typeof(SupportMarketDataBean),
                typeof(SupportSimpleBeanTwo),
                typeof(SupportSimpleBeanOne),
                typeof(SupportVariableSetEvent),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBeanRange),
                typeof(SupportBean_B),
                typeof(SupportOverrideOneA),
                typeof(SupportOverrideOne),
                typeof(SupportOverrideBase),
                typeof(SupportQueueEnter),
                typeof(SupportQueueLeave),
                typeof(SupportBeanAtoFBase),
                typeof(SupportBeanAbstractSub),
                typeof(SupportBean_ST0),
                typeof(SupportBeanTwo),
                typeof(SupportCountAccessEvent),
                typeof(BookDesc),
                typeof(SupportBean_Container)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            IDictionary<string, object> outerMapInnerType = new Dictionary<string, object>();
            outerMapInnerType.Put("key", typeof(string));
            configuration.Common.AddEventType("InnerMap", outerMapInnerType);
            IDictionary<string, object> outerMap = new Dictionary<string, object>();
            outerMap.Put("innermap", "InnerMap");
            configuration.Common.AddEventType("OuterMap", outerMap);

            IDictionary<string, object> typesSimpleKeyValue = new Dictionary<string, object>();
            typesSimpleKeyValue.Put("key", typeof(string));
            typesSimpleKeyValue.Put("value", typeof(long));
            configuration.Common.AddEventType("MySimpleKeyValueMap", typesSimpleKeyValue);

            IDictionary<string, object> innerTypeOne = new Dictionary<string, object>();
            innerTypeOne.Put("i1", typeof(int));
            IDictionary<string, object> innerTypeTwo = new Dictionary<string, object>();
            innerTypeTwo.Put("i2", typeof(int));
            IDictionary<string, object> outerType = new Dictionary<string, object>();
            outerType.Put("one", "T1");
            outerType.Put("two", "T2");
            configuration.Common.AddEventType("T1", innerTypeOne);
            configuration.Common.AddEventType("T2", innerTypeTwo);
            configuration.Common.AddEventType("OuterType", outerType);

            IDictionary<string, object> types = new Dictionary<string, object>();
            types.Put("key", typeof(string));
            types.Put("primitive", typeof(long));
            types.Put("boxed", typeof(long?));
            configuration.Common.AddEventType("MyMapWithKeyPrimitiveBoxed", types);

            var dataType = BuildMap(
                new object[][] {
                    new object[] {"a", typeof(string)},
                    new object[] {"b", typeof(int)}
                });
            configuration.Common.AddEventType("MyMapAB", dataType);

            var legacy = new ConfigurationCommonEventTypeBean();
            legacy.CopyMethod = "MyCopyMethod";
            configuration.Common.AddEventType("SupportBeanCopyMethod", typeof(SupportBeanCopyMethod), legacy);

            configuration.Compiler.AddPlugInSingleRowFunction(
                "setBeanLongPrimitive999",
                typeof(InfraNamedWindowOnUpdate),
                "SetBeanLongPrimitive999");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "increaseIntCopyDouble",
                typeof(InfraNamedWindowOnMerge),
                "IncreaseIntCopyDouble");

            var config = new ConfigurationCommonVariantStream();
            config.AddEventTypeName("SupportBean_A");
            config.AddEventTypeName("SupportBean_B");
            configuration.Common.AddVariantStream("VarStream", config);

            configuration.Common.Logging.IsEnableQueryPlan = true;
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowConsumer()
        {
            RegressionRunner.Run(session, InfraNamedWindowConsumer.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowContainedEvent()
        {
            RegressionRunner.Run(session, new InfraNamedWindowContainedEvent());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowIndex()
        {
            RegressionRunner.Run(session, new InfraNamedWindowIndex());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowInsertFrom()
        {
            RegressionRunner.Run(session, InfraNamedWindowInsertFrom.Executions());
        }

        [Test]
        public void TestInfraNamedWindowJoin()
        {
            RegressionRunner.Run(session, InfraNamedWindowJoin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowLateStartIndex()
        {
            RegressionRunner.Run(session, new InfraNamedWindowLateStartIndex());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowOM()
        {
            RegressionRunner.Run(session, InfraNamedWindowOM.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowOnDelete()
        {
            RegressionRunner.Run(session, InfraNamedWindowOnDelete.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowOnMerge()
        {
            RegressionRunner.Run(session, InfraNamedWindowOnMerge.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowOnSelect()
        {
            RegressionRunner.Run(session, InfraNamedWindowOnSelect.Executions());
        }

        [Test]
        public void TestInfraNamedWindowOnUpdate()
        {
            RegressionRunner.Run(session, InfraNamedWindowOnUpdate.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowOutputrate()
        {
            RegressionRunner.Run(session, new InfraNamedWindowOutputrate());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowProcessingOrder()
        {
            RegressionRunner.Run(session, InfraNamedWindowProcessingOrder.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowRemoveStream()
        {
            RegressionRunner.Run(session, new InfraNamedWindowRemoveStream());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNamedWindowSubquery()
        {
            RegressionRunner.Run(session, InfraNamedWindowSubquery.Executions());
        }

        [Test]
        public void TestInfraNamedWindowTypes()
        {
            RegressionRunner.Run(session, InfraNamedWindowTypes.Executions());
        }

        [Test]
        public void TestInfraNamedWindowViews()
        {
            RegressionRunner.Run(session, InfraNamedWindowViews.Executions());
        }
    }
} // end of namespace