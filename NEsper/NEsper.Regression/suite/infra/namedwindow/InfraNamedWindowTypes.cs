///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using Avro.Generic;

using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowTypes
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraMapTranspose());
            execs.Add(new InfraNoWildcardWithAs());
            execs.Add(new InfraNoWildcardNoAs());
            execs.Add(new InfraConstantsAs());
            execs.Add(new InfraCreateTableSyntax());
            execs.Add(new InfraWildcardNoFieldsNoAs());
            execs.Add(new InfraModelAfterMap());
            execs.Add(new InfraWildcardInheritance());
            execs.Add(new InfraNoSpecificationBean());
            execs.Add(new InfraWildcardWithFields());
            execs.Add(new InfraCreateTableArray());
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                execs.Add(new InfraEventTypeColumnDef(rep));
            }

            execs.Add(new InfraCreateSchemaModelAfter());
            return execs;
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            long longPrimitive,
            long? longBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongPrimitive = longPrimitive;
            bean.LongBoxed = longBoxed;
            env.SendEventBean(bean);
        }

        private static void SendMarketBean(
            RegressionEnvironment env,
            string symbol,
            long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            env.SendEventBean(bean);
        }

        private static void SendMap(
            RegressionEnvironment env,
            string key,
            long primitive,
            long? boxed)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("key", key);
            map.Put("primitive", primitive);
            map.Put("boxed", boxed);
            env.SendEventMap(map, "MyMapWithKeyPrimitiveBoxed");
        }

        internal class InfraEventTypeColumnDef : RegressionExecution
        {
            private readonly EventRepresentationChoice eventRepresentationEnum;

            public InfraEventTypeColumnDef(EventRepresentationChoice eventRepresentationEnum)
            {
                this.eventRepresentationEnum = eventRepresentationEnum;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = eventRepresentationEnum.GetAnnotationText() +
                          " @name('schema') create schema SchemaOne(col1 int, col2 int);\n";
                epl += eventRepresentationEnum.GetAnnotationText() +
                       " @name('create') create window SchemaWindow#lastevent as (s1 SchemaOne);\n";
                epl += "insert into SchemaWindow (s1) select sone from SchemaOne as sone;\n";
                env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("create");

                Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("schema").EventType.UnderlyingType));
                Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("create").EventType.UnderlyingType));

                if (eventRepresentationEnum.IsObjectArrayEvent()) {
                    env.SendEventObjectArray(new object[] {10, 11}, "SchemaOne");
                }
                else if (eventRepresentationEnum.IsMapEvent()) {
                    IDictionary<string, object> theEvent = new Dictionary<string, object>();
                    theEvent.Put("col1", 10);
                    theEvent.Put("col2", 11);
                    env.SendEventMap(theEvent, "SchemaOne");
                }
                else if (eventRepresentationEnum.IsAvroEvent()) {
                    var theEvent = new GenericRecord(
                        SupportAvroUtil
                            .GetAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured("SchemaOne"))
                            .AsRecordSchema());
                    theEvent.Put("col1", 10);
                    theEvent.Put("col2", 11);
                    env.EventService.SendEventAvro(theEvent, "SchemaOne");
                }
                else {
                    Assert.Fail();
                }

                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    "s1.col1,s1.col2".SplitCsv(),
                    new object[] {10, 11});

                env.UndeployAll();
            }
        }

        internal class InfraMapTranspose : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionMapTranspose(env, EventRepresentationChoice.ARRAY);
                TryAssertionMapTranspose(env, EventRepresentationChoice.MAP);
                TryAssertionMapTranspose(env, EventRepresentationChoice.DEFAULT);
            }

            private void TryAssertionMapTranspose(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum)
            {
                // create window
                var epl = eventRepresentationEnum.GetAnnotationText() +
                          " @name('create') create window MyWindowMT#keepall as select one, two from OuterType;\n" +
                          "insert into MyWindowMT select one, two from OuterType;\n";
                env.CompileDeploy(epl).AddListener("create");

                var eventType = env.Statement("create").EventType;
                Assert.IsTrue(eventRepresentationEnum.MatchesClass(eventType.UnderlyingType));
                EPAssertionUtil.AssertEqualsAnyOrder(eventType.PropertyNames, new[] {"one", "two"});
                Assert.AreEqual("T1", eventType.GetFragmentType("one").FragmentType.Name);
                Assert.AreEqual("T2", eventType.GetFragmentType("two").FragmentType.Name);

                IDictionary<string, object> innerDataOne = new Dictionary<string, object>();
                innerDataOne.Put("i1", 1);
                IDictionary<string, object> innerDataTwo = new Dictionary<string, object>();
                innerDataTwo.Put("i2", 2);
                IDictionary<string, object> outerData = new Dictionary<string, object>();
                outerData.Put("one", innerDataOne);
                outerData.Put("two", innerDataTwo);

                env.SendEventMap(outerData, "OuterType");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    "one.i1,two.i2".SplitCsv(),
                    new object[] {1, 2});

                env.UndeployAll();
            }
        }

        internal class InfraNoWildcardWithAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('create') create window MyWindowNW#keepall as select TheString as a, LongPrimitive as b, LongBoxed as c from SupportBean;\n" +
                    "insert into MyWindowNW select TheString as a, LongPrimitive as b, LongBoxed as c from SupportBean;\n" +
                    "insert into MyWindowNW select Symbol as a, Volume as b, Volume as c from SupportMarketDataBean;\n" +
                    "insert into MyWindowNW select key as a, boxed as b, primitive as c from MyMapWithKeyPrimitiveBoxed;\n" +
                    "@Name('s1') select a, b, c from MyWindowNW;\n" +
                    "@Name('delete') on SupportMarketDataBean as s0 delete from MyWindowNW as s1 where s0.Symbol = s1.a;\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("s1").AddListener("delete");

                var eventType = env.Statement("create").EventType;
                EPAssertionUtil.AssertEqualsAnyOrder(eventType.PropertyNames, new[] {"a", "b", "c"});
                Assert.AreEqual(typeof(string), eventType.GetPropertyType("a"));
                Assert.AreEqual(typeof(long?), eventType.GetPropertyType("b"));
                Assert.AreEqual(typeof(long?), eventType.GetPropertyType("c"));

                // assert type metadata
                var type = env.Deployment.GetStatement(env.DeploymentId("create"), "create").EventType;
                Assert.AreEqual(EventTypeTypeClass.NAMED_WINDOW, type.Metadata.TypeClass);
                Assert.AreEqual("MyWindowNW", type.Metadata.Name);
                Assert.AreEqual(EventTypeApplicationType.MAP, type.Metadata.ApplicationType);

                eventType = env.Statement("s1").EventType;
                EPAssertionUtil.AssertEqualsAnyOrder(eventType.PropertyNames, new[] {"a", "b", "c"});
                Assert.AreEqual(typeof(string), eventType.GetPropertyType("a"));
                Assert.AreEqual(typeof(long?), eventType.GetPropertyType("b"));
                Assert.AreEqual(typeof(long?), eventType.GetPropertyType("c"));

                SendSupportBean(env, "E1", 1L, 10L);
                string[] fields = {"a", "b", "c"};
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L, 10L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L, 10L});

                SendMarketBean(env, "S1", 99L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", 99L, 99L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", 99L, 99L});

                SendMap(env, "M1", 100L, 101L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"M1", 101L, 100L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"M1", 101L, 100L});

                env.UndeployAll();
            }
        }

        internal class InfraNoWildcardNoAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('create') create window MyWindowNWNA#keepall as select TheString, LongPrimitive, LongBoxed from SupportBean;\n" +
                    "insert into MyWindowNWNA select TheString, LongPrimitive, LongBoxed from SupportBean;\n" +
                    "insert into MyWindowNWNA select Symbol as TheString, Volume as LongPrimitive, Volume as LongBoxed from SupportMarketDataBean;\n" +
                    "insert into MyWindowNWNA select key as TheString, boxed as LongPrimitive, primitive as LongBoxed from MyMapWithKeyPrimitiveBoxed;\n" +
                    "@Name('select') select TheString, LongPrimitive, LongBoxed from MyWindowNWNA;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                SendSupportBean(env, "E1", 1L, 10L);
                string[] fields = {"TheString", "LongPrimitive", "LongBoxed"};
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L, 10L});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L, 10L});

                SendMarketBean(env, "S1", 99L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", 99L, 99L});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", 99L, 99L});

                SendMap(env, "M1", 100L, 101L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"M1", 101L, 100L});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"M1", 101L, 100L});

                env.UndeployAll();
            }
        }

        internal class InfraConstantsAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('create') create window MyWindowCA#keepall as select '' as TheString, 0L as LongPrimitive, 0L as LongBoxed from MyMapWithKeyPrimitiveBoxed;\n" +
                    "insert into MyWindowCA select TheString, LongPrimitive, LongBoxed from SupportBean;\n" +
                    "insert into MyWindowCA select Symbol as TheString, Volume as LongPrimitive, Volume as LongBoxed from SupportMarketDataBean;\n" +
                    "@Name('select') select TheString, LongPrimitive, LongBoxed from MyWindowCA;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                SendSupportBean(env, "E1", 1L, 10L);
                string[] fields = {"TheString", "LongPrimitive", "LongBoxed"};
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L, 10L});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L, 10L});

                SendMarketBean(env, "S1", 99L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", 99L, 99L});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", 99L, 99L});

                env.UndeployAll();
            }
        }

        internal class InfraCreateSchemaModelAfter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionCreateSchemaModelAfter(env, rep);
                }

                // test model-after for POJO with inheritance
                var path = new RegressionPath();
                env.CompileDeploy("create window ParentWindow#keepall as select * from SupportBeanAtoFBase", path);
                env.CompileDeploy("insert into ParentWindow select * from SupportBeanAtoFBase", path);
                env.CompileDeploy("create window ChildWindow#keepall as select * from SupportBean_A", path);
                env.CompileDeploy("insert into ChildWindow select * from SupportBean_A", path);

                var parentQuery = "@Name('s0') select parent from ParentWindow as parent";
                env.CompileDeploy(parentQuery, path).AddListener("s0");

                env.SendEventBean(new SupportBean_A("E1"));
                Assert.AreEqual(1, env.Listener("s0").NewDataListFlattened.Length);

                env.UndeployAll();
            }

            private void TryAssertionCreateSchemaModelAfter(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum)
            {
                var epl = eventRepresentationEnum.GetAnnotationText() +
                          " create schema EventTypeOne (hsi int);\n" +
                          eventRepresentationEnum.GetAnnotationText() +
                          " create schema EventTypeTwo (event EventTypeOne);\n" +
                          eventRepresentationEnum.GetAnnotationText() +
                          " @name('create') create window NamedWindow#unique(event.hsi) as EventTypeTwo;\n" +
                          "on EventTypeOne as ev insert into NamedWindow select ev as event;\n";
                env.CompileDeployWBusPublicType(epl, new RegressionPath());

                if (eventRepresentationEnum.IsObjectArrayEvent()) {
                    env.SendEventObjectArray(new object[] {10}, "EventTypeOne");
                }
                else if (eventRepresentationEnum.IsMapEvent()) {
                    env.SendEventMap(Collections.SingletonDataMap("hsi", 10), "EventTypeOne");
                }
                else if (eventRepresentationEnum.IsAvroEvent()) {
                    var theEvent = new GenericRecord(
                        SupportAvroUtil
                            .GetAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured("EventTypeOne"))
                            .AsRecordSchema());
                    theEvent.Put("hsi", 10);
                    env.EventService.SendEventAvro(theEvent, "EventTypeOne");
                }
                else {
                    Assert.Fail();
                }

                var result = env.Statement("create").First();
                var getter = result.EventType.GetGetter("event.hsi");
                Assert.AreEqual(10, getter.Get(result));

                env.UndeployAll();
            }
        }

        public class InfraCreateTableArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create schema SecurityData (name String, roles String[]);\n" +
                          "create window SecurityEvent#time(30 sec) (ipAddress string, userId String, secData SecurityData, historySecData SecurityData[]);\n" +
                          "@Name('create') create window MyWindowCTA#keepall (myvalue string[]);\n" +
                          "insert into MyWindowCTA select {'a','b'} as myvalue from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("create");

                SendSupportBean(env, "E1", 1L, 10L);
                var values = (string[]) env.Listener("create").AssertOneGetNewAndReset().Get("myvalue");
                EPAssertionUtil.AssertEqualsExactOrder(values, new[] {"a", "b"});

                env.UndeployAll();
            }
        }

        internal class InfraCreateTableSyntax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('create') create window MyWindowCTS#keepall (stringValOne varchar, stringValTwo string, intVal int, longVal long);\n" +
                    "insert into MyWindowCTS select TheString as stringValOne, TheString as stringValTwo, cast(LongPrimitive, int) as intVal, LongBoxed as longVal from SupportBean;\n" +
                    "@Name('select') select stringValOne, stringValTwo, intVal, longVal from MyWindowCTS;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                SendSupportBean(env, "E1", 1L, 10L);
                var fields = "stringValOne,stringValTwo,intVal,longVal".SplitCsv();
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1", 1, 10L});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1", 1, 10L});

                env.UndeployAll();

                // create window with two views
                epl =
                    "create window MyWindowCTSTwo#unique(stringValOne)#keepall (stringValOne varchar, stringValTwo string, intVal int, longVal long)";
                env.CompileDeploy(epl).UndeployAll();

                //create window with statement object model
                var text = "@Name('create') create window MyWindowCTSThree#keepall as (a string, b integer, c integer)";
                env.EplToModelCompileDeploy(text);
                Assert.AreEqual(typeof(string), env.Statement("create").EventType.GetPropertyType("a"));
                Assert.AreEqual(typeof(int?), env.Statement("create").EventType.GetPropertyType("b"));
                Assert.AreEqual(typeof(int?), env.Statement("create").EventType.GetPropertyType("c"));
                env.UndeployAll();

                text =
                    "create window MyWindowCTSFour#unique(a)#unique(b) retain-union as (a string, b integer, c integer)";
                env.EplToModelCompileDeploy(text);

                env.UndeployAll();
            }
        }

        internal class InfraWildcardNoFieldsNoAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('create') create window MyWindowWNF#keepall select * from SupportBean_A;\n" +
                          "insert into MyWindowWNF select * from SupportBean_A;" +
                          "@Name('select') select Id from MyWindowWNF;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                env.SendEventBean(new SupportBean_A("E1"));
                string[] fields = {"Id"};
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.UndeployAll();
            }
        }

        internal class InfraModelAfterMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('create') create window MyWindowMAM#keepall select * from MyMapWithKeyPrimitiveBoxed;\n" +
                    "@Name('insert') insert into MyWindowMAM select * from MyMapWithKeyPrimitiveBoxed;\n";
                env.CompileDeploy(epl).AddListener("create");
                Assert.IsTrue(env.Statement("create").EventType is MapEventType);

                SendMap(env, "k1", 100L, 200L);
                var theEvent = env.Listener("create").AssertOneGetNewAndReset();
                Assert.IsTrue(theEvent is MappedEventBean);
                EPAssertionUtil.AssertProps(
                    theEvent,
                    "key,primitive".SplitCsv(),
                    new object[] {"k1", 100L});

                env.UndeployAll();
            }
        }

        internal class InfraWildcardInheritance : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('create') create window MyWindowWI#keepall as select * from SupportBeanAtoFBase;\n" +
                          "insert into MyWindowWI select * from SupportBean_A;\n" +
                          "insert into MyWindowWI select * from SupportBean_B;\n" +
                          "@Name('select') select Id from MyWindowWI;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                env.SendEventBean(new SupportBean_A("E1"));
                string[] fields = {"Id"};
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.SendEventBean(new SupportBean_B("E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.UndeployAll();
            }
        }

        internal class InfraNoSpecificationBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('create') create window MyWindowNSB#keepall as SupportBean_A;\n" +
                          "insert into MyWindowNSB select * from SupportBean_A;\n" +
                          "@Name('select') select Id from MyWindowNSB;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                env.SendEventBean(new SupportBean_A("E1"));
                string[] fields = {"Id"};
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.UndeployAll();
            }
        }

        internal class InfraWildcardWithFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('create') create window MyWindowWWF#keepall as select *, Id as myId from SupportBean_A;\n" +
                    "insert into MyWindowWWF select *, Id || 'A' as myId from SupportBean_A;\n" +
                    "@Name('select') select Id, myId from MyWindowWWF;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                env.SendEventBean(new SupportBean_A("E1"));
                string[] fields = {"Id", "myId"};
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1A"});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1A"});

                env.UndeployAll();
            }
        }

        public class NWTypesParentClass
        {
        }

        public class NWTypesChildClass : NWTypesParentClass
        {
        }
    }
} // end of namespace