///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

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

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    /// NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowTypes
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithMapTranspose(execs);
            WithNoWildcardWithAs(execs);
            WithNoWildcardNoAs(execs);
            WithConstantsAs(execs);
            WithCreateTableSyntax(execs);
            WithWildcardNoFieldsNoAs(execs);
            WithModelAfterMap(execs);
            WithWildcardInheritance(execs);
            WithNoSpecificationBean(execs);
            WithWildcardWithFields(execs);
            WithCreateTableArray(execs);

            foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                WithEventTypeColumnDef(rep, execs);
            }

            WithCreateSchemaModelAfter(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchemaModelAfter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraCreateSchemaModelAfter());
            return execs;
        }

        public static IList<RegressionExecution> WithEventTypeColumnDef(
            EventRepresentationChoice eventRepresentationChoice,
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraEventTypeColumnDef(eventRepresentationChoice));
            return execs;
        }

        public static IList<RegressionExecution> WithCreateTableArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraCreateTableArray());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcardWithFields(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraWildcardWithFields());
            return execs;
        }

        public static IList<RegressionExecution> WithNoSpecificationBean(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNoSpecificationBean());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcardInheritance(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraWildcardInheritance());
            return execs;
        }

        public static IList<RegressionExecution> WithModelAfterMap(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraModelAfterMap());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcardNoFieldsNoAs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraWildcardNoFieldsNoAs());
            return execs;
        }

        public static IList<RegressionExecution> WithCreateTableSyntax(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraCreateTableSyntax());
            return execs;
        }

        public static IList<RegressionExecution> WithConstantsAs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraConstantsAs());
            return execs;
        }

        public static IList<RegressionExecution> WithNoWildcardNoAs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNoWildcardNoAs());
            return execs;
        }

        public static IList<RegressionExecution> WithNoWildcardWithAs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNoWildcardWithAs());
            return execs;
        }

        public static IList<RegressionExecution> WithMapTranspose(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraMapTranspose());
            return execs;
        }

        private class InfraEventTypeColumnDef : RegressionExecution
        {
            private readonly EventRepresentationChoice eventRepresentationEnum;

            public InfraEventTypeColumnDef(EventRepresentationChoice eventRepresentationEnum)
            {
                this.eventRepresentationEnum = eventRepresentationEnum;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedSchemaOne)) +
                          " @name('schema') @buseventtype @public create schema SchemaOne(col1 int, col2 int);\n";
                epl += eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedSchemaWindow)) +
                       " @name('create') @public create window SchemaWindow#lastevent as (s1 SchemaOne);\n";
                epl += "insert into SchemaWindow (s1) select sone from SchemaOne as sone;\n";
                env.CompileDeploy(epl, new RegressionPath()).AddListener("create");

                foreach (var name in new string[] { "schema", "create" }) {
                    env.AssertStatement(
                        name,
                        statement => ClassicAssert.IsTrue(
                            eventRepresentationEnum.MatchesClass(statement.EventType.UnderlyingType)));
                }

                if (eventRepresentationEnum.IsObjectArrayEvent()) {
                    env.SendEventObjectArray(new object[] { 10, 11 }, "SchemaOne");
                }
                else if (eventRepresentationEnum.IsMapEvent()) {
                    IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
                    theEvent.Put("col1", 10);
                    theEvent.Put("col2", 11);
                    env.SendEventMap(theEvent, "SchemaOne");
                }
                else if (eventRepresentationEnum.IsAvroEvent()) {
                    var theEvent = new GenericRecord(env.RuntimeAvroSchemaPreconfigured("SchemaOne").AsRecordSchema());
                    theEvent.Put("col1", 10);
                    theEvent.Put("col2", 11);
                    env.SendEventAvro(theEvent, "SchemaOne");
                }
                else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                    env.SendEventJson("{\"col1\": 10, \"col2\": 11}", "SchemaOne");
                }
                else {
                    Assert.Fail();
                }

                env.AssertPropsNew("create", "s1.col1,s1.col2".SplitCsv(), new object[] { 10, 11 });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "eventRepresentationEnum=" +
                       eventRepresentationEnum +
                       '}';
            }
        }

        private class InfraMapTranspose : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionMapTranspose(env, EventRepresentationChoice.OBJECTARRAY);
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

                env.AssertStatement(
                    "create",
                    statement => {
                        var eventType = statement.EventType;
                        ClassicAssert.IsTrue(eventRepresentationEnum.MatchesClass(eventType.UnderlyingType));
                        EPAssertionUtil.AssertEqualsAnyOrder(eventType.PropertyNames, new string[] { "one", "two" });
                        ClassicAssert.AreEqual("T1", eventType.GetFragmentType("one").FragmentType.Name);
                        ClassicAssert.AreEqual("T2", eventType.GetFragmentType("two").FragmentType.Name);
                    });

                IDictionary<string, object> innerDataOne = new Dictionary<string, object>();
                innerDataOne.Put("i1", 1);
                IDictionary<string, object> innerDataTwo = new Dictionary<string, object>();
                innerDataTwo.Put("i2", 2);
                IDictionary<string, object> outerData = new Dictionary<string, object>();
                outerData.Put("one", innerDataOne);
                outerData.Put("two", innerDataTwo);

                env.SendEventMap(outerData, "OuterType");
                env.AssertPropsNew("create", "one.i1,two.i2".SplitCsv(), new object[] { 1, 2 });

                env.UndeployAll();
            }
        }

        private class InfraNoWildcardWithAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('create') create window MyWindowNW#keepall as select TheString as a, LongPrimitive as b, LongBoxed as c from SupportBean;\n" +
                    "insert into MyWindowNW select TheString as a, LongPrimitive as b, LongBoxed as c from SupportBean;\n" +
                    "insert into MyWindowNW select Symbol as a, Volume as b, Volume as c from SupportMarketDataBean;\n" +
                    "insert into MyWindowNW select key as a, boxed as b, primitive as c from MyMapWithKeyPrimitiveBoxed;\n" +
                    "@name('s1') select a, b, c from MyWindowNW;\n" +
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowNW as s1 where s0.Symbol = s1.a;\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("s1").AddListener("delete");

                env.AssertStatement(
                    "create",
                    statement => {
                        var eventType = statement.EventType;
                        EPAssertionUtil.AssertEqualsAnyOrder(eventType.PropertyNames, new string[] { "a", "b", "c" });
                        ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("a"));
                        ClassicAssert.AreEqual(typeof(long?), eventType.GetPropertyType("b"));
                        ClassicAssert.AreEqual(typeof(long?), eventType.GetPropertyType("c"));
                        ClassicAssert.AreEqual(EventTypeTypeClass.NAMED_WINDOW, eventType.Metadata.TypeClass);
                        ClassicAssert.AreEqual("MyWindowNW", eventType.Metadata.Name);
                        ClassicAssert.AreEqual(EventTypeApplicationType.MAP, eventType.Metadata.ApplicationType);
                    });

                env.AssertStatement(
                    "s1",
                    statement => {
                        var eventType = statement.EventType;
                        EPAssertionUtil.AssertEqualsAnyOrder(eventType.PropertyNames, new string[] { "a", "b", "c" });
                        ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("a"));
                        ClassicAssert.AreEqual(typeof(long?), eventType.GetPropertyType("b"));
                        ClassicAssert.AreEqual(typeof(long?), eventType.GetPropertyType("c"));
                    });

                SendSupportBean(env, "E1", 1L, 10L);
                var fields = new string[] { "a", "b", "c" };
                env.AssertPropsNew("create", fields, new object[] { "E1", 1L, 10L });
                env.AssertPropsNew("s1", fields, new object[] { "E1", 1L, 10L });

                SendMarketBean(env, "S1", 99L);
                env.AssertPropsNew("create", fields, new object[] { "S1", 99L, 99L });
                env.AssertPropsNew("s1", fields, new object[] { "S1", 99L, 99L });

                SendMap(env, "M1", 100L, 101L);
                env.AssertPropsNew("create", fields, new object[] { "M1", 101L, 100L });
                env.AssertPropsNew("s1", fields, new object[] { "M1", 101L, 100L });

                env.UndeployAll();
            }
        }

        private class InfraNoWildcardNoAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('create') create window MyWindowNWNA#keepall as select TheString, LongPrimitive, LongBoxed from SupportBean;\n" +
                    "insert into MyWindowNWNA select TheString, LongPrimitive, LongBoxed from SupportBean;\n" +
                    "insert into MyWindowNWNA select Symbol as TheString, Volume as LongPrimitive, Volume as LongBoxed from SupportMarketDataBean;\n" +
                    "insert into MyWindowNWNA select key as TheString, boxed as LongPrimitive, primitive as LongBoxed from MyMapWithKeyPrimitiveBoxed;\n" +
                    "@name('select') select TheString, LongPrimitive, LongBoxed from MyWindowNWNA;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                SendSupportBean(env, "E1", 1L, 10L);
                var fields = new string[] { "TheString", "LongPrimitive", "LongBoxed" };
                env.AssertPropsNew("create", fields, new object[] { "E1", 1L, 10L });
                env.AssertPropsNew("select", fields, new object[] { "E1", 1L, 10L });

                SendMarketBean(env, "S1", 99L);
                env.AssertPropsNew("create", fields, new object[] { "S1", 99L, 99L });
                env.AssertPropsNew("select", fields, new object[] { "S1", 99L, 99L });

                SendMap(env, "M1", 100L, 101L);
                env.AssertPropsNew("create", fields, new object[] { "M1", 101L, 100L });
                env.AssertPropsNew("select", fields, new object[] { "M1", 101L, 100L });

                env.UndeployAll();
            }
        }

        private class InfraConstantsAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('create') create window MyWindowCA#keepall as select '' as TheString, 0L as LongPrimitive, 0L as LongBoxed from MyMapWithKeyPrimitiveBoxed;\n" +
                    "insert into MyWindowCA select TheString, LongPrimitive, LongBoxed from SupportBean;\n" +
                    "insert into MyWindowCA select Symbol as TheString, Volume as LongPrimitive, Volume as LongBoxed from SupportMarketDataBean;\n" +
                    "@name('select') select TheString, LongPrimitive, LongBoxed from MyWindowCA;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                SendSupportBean(env, "E1", 1L, 10L);
                var fields = new string[] { "TheString", "LongPrimitive", "LongBoxed" };
                env.AssertPropsNew("create", fields, new object[] { "E1", 1L, 10L });
                env.AssertPropsNew("select", fields, new object[] { "E1", 1L, 10L });

                SendMarketBean(env, "S1", 99L);
                env.AssertPropsNew("create", fields, new object[] { "S1", 99L, 99L });
                env.AssertPropsNew("select", fields, new object[] { "S1", 99L, 99L });

                env.UndeployAll();
            }
        }

        private class InfraCreateSchemaModelAfter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionCreateSchemaModelAfter(env, rep);
                }

                // test model-after for PONO with inheritance
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create window ParentWindow#keepall as select * from SupportBeanAtoFBase",
                    path);
                env.CompileDeploy("insert into ParentWindow select * from SupportBeanAtoFBase", path);
                env.CompileDeploy("@public create window ChildWindow#keepall as select * from SupportBean_A", path);
                env.CompileDeploy("insert into ChildWindow select * from SupportBean_A", path);

                var parentQuery = "@name('s0') select parent from ParentWindow as parent";
                env.CompileDeploy(parentQuery, path).AddListener("s0");

                env.SendEventBean(new SupportBean_A("E1"));
                env.AssertListener("s0", listener => ClassicAssert.AreEqual(1, listener.NewDataListFlattened.Length));

                env.UndeployAll();
            }

            private void TryAssertionCreateSchemaModelAfter(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum)
            {
                var epl =
                    eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedEventTypeOne)) +
                    " @public @buseventtype create schema EventTypeOne (hsi int);\n" +
                    eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedEventTypeTwo)) +
                    " @public @buseventtype create schema EventTypeTwo (event EventTypeOne);\n" +
                    "@name('create') create window NamedWindow#unique(event.hsi) as EventTypeTwo;\n" +
                    "on EventTypeOne as ev insert into NamedWindow select ev as event;\n";
                env.CompileDeploy(epl, new RegressionPath());

                if (eventRepresentationEnum.IsObjectArrayEvent()) {
                    env.SendEventObjectArray(new object[] { 10 }, "EventTypeOne");
                }
                else if (eventRepresentationEnum.IsMapEvent()) {
                    env.SendEventMap(Collections.SingletonDataMap("hsi", 10), "EventTypeOne");
                }
                else if (eventRepresentationEnum.IsAvroEvent()) {
                    var theEvent = new GenericRecord(
                        env.RuntimeAvroSchemaPreconfigured("EventTypeOne").AsRecordSchema());
                    theEvent.Put("hsi", 10);
                    env.SendEventAvro(theEvent, "EventTypeOne");
                }
                else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                    env.SendEventJson("{\"hsi\": 10}", "EventTypeOne");
                }
                else {
                    Assert.Fail();
                }

                env.AssertIterator(
                    "create",
                    iterator => {
                        var result = iterator.Advance();
                        var getter = result.EventType.GetGetter("event.hsi");
                        ClassicAssert.AreEqual(10, getter.Get(result));
                    });

                env.UndeployAll();
            }
        }

        public class InfraCreateTableArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create schema SecurityData (name String, roles String[]);\n" +
                          "create window SecurityEvent#time(30 sec) (ipAddress string, userId String, secData SecurityData, historySecData SecurityData[]);\n" +
                          "@name('create') create window MyWindowCTA#keepall (myvalue string[]);\n" +
                          "insert into MyWindowCTA select {'a','b'} as myvalue from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("create");

                SendSupportBean(env, "E1", 1L, 10L);
                env.AssertListener(
                    "create",
                    listener => {
                        var values = (string[])listener.AssertOneGetNewAndReset().Get("myvalue");
                        EPAssertionUtil.AssertEqualsExactOrder(values, new string[] { "a", "b" });
                    });

                env.UndeployAll();
            }
        }

        private class InfraCreateTableSyntax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('create') create window MyWindowCTS#keepall (stringValOne varchar, stringValTwo string, intVal int, longVal long);\n" +
                    "insert into MyWindowCTS select TheString as stringValOne, TheString as stringValTwo, cast(LongPrimitive, int) as intVal, LongBoxed as longVal from SupportBean;\n" +
                    "@name('select') select stringValOne, stringValTwo, intVal, longVal from MyWindowCTS;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                SendSupportBean(env, "E1", 1L, 10L);
                var fields = "stringValOne,stringValTwo,intVal,longVal".SplitCsv();
                env.AssertPropsNew("create", fields, new object[] { "E1", "E1", 1, 10L });
                env.AssertPropsNew("select", fields, new object[] { "E1", "E1", 1, 10L });

                env.UndeployAll();

                // create window with two views
                epl =
                    "create window MyWindowCTSTwo#unique(stringValOne)#keepall (stringValOne varchar, stringValTwo string, intVal int, longVal long)";
                env.CompileDeploy(epl).UndeployAll();

                //create window with statement object model
                var text = "@name('create') create window MyWindowCTSThree#keepall as (a string, b integer, c integer)";
                env.EplToModelCompileDeploy(text);
                env.AssertStatement(
                    "create",
                    statement => {
                        ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("a"));
                        ClassicAssert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("b"));
                        ClassicAssert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("c"));
                    });
                env.UndeployAll();

                text =
                    "create window MyWindowCTSFour#unique(a)#unique(b) retain-union as (a string, b integer, c integer)";
                env.EplToModelCompileDeploy(text);

                env.UndeployAll();
            }
        }

        private class InfraWildcardNoFieldsNoAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('create') create window MyWindowWNF#keepall select * from SupportBean_A;\n" +
                          "insert into MyWindowWNF select * from SupportBean_A;" +
                          "@name('select') select Id from MyWindowWNF;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                env.SendEventBean(new SupportBean_A("E1"));
                var fields = new string[] { "Id" };
                env.AssertPropsNew("create", fields, new object[] { "E1" });
                env.AssertPropsNew("select", fields, new object[] { "E1" });

                env.UndeployAll();
            }
        }

        private class InfraModelAfterMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('create') create window MyWindowMAM#keepall select * from MyMapWithKeyPrimitiveBoxed;\n" +
                    "@name('insert') insert into MyWindowMAM select * from MyMapWithKeyPrimitiveBoxed;\n";
                env.CompileDeploy(epl).AddListener("create");
                env.AssertStatement("create", statement => ClassicAssert.IsTrue(statement.EventType is MapEventType));

                SendMap(env, "k1", 100L, 200L);
                env.AssertListener(
                    "create",
                    listener => {
                        var theEvent = listener.AssertOneGetNewAndReset();
                        ClassicAssert.IsTrue(theEvent is MappedEventBean);
                        EPAssertionUtil.AssertProps(theEvent, "key,primitive".SplitCsv(), new object[] { "k1", 100L });
                    });

                env.UndeployAll();
            }
        }

        private class InfraWildcardInheritance : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('create') create window MyWindowWI#keepall as select * from SupportBeanAtoFBase;\n" +
                          "insert into MyWindowWI select * from SupportBean_A;\n" +
                          "insert into MyWindowWI select * from SupportBean_B;\n" +
                          "@name('select') select Id from MyWindowWI;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                env.SendEventBean(new SupportBean_A("E1"));
                var fields = new string[] { "Id" };
                env.AssertPropsNew("create", fields, new object[] { "E1" });
                env.AssertPropsNew("select", fields, new object[] { "E1" });

                env.SendEventBean(new SupportBean_B("E2"));
                env.AssertPropsNew("create", fields, new object[] { "E2" });
                env.AssertPropsNew("select", fields, new object[] { "E2" });

                env.UndeployAll();
            }
        }

        private class InfraNoSpecificationBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('create') create window MyWindowNSB#keepall as SupportBean_A;\n" +
                          "insert into MyWindowNSB select * from SupportBean_A;\n" +
                          "@name('select') select Id from MyWindowNSB;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                env.SendEventBean(new SupportBean_A("E1"));
                var fields = new string[] { "Id" };
                env.AssertPropsNew("create", fields, new object[] { "E1" });
                env.AssertPropsNew("select", fields, new object[] { "E1" });

                env.UndeployAll();
            }
        }

        private class InfraWildcardWithFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('create') create window MyWindowWWF#keepall as select *, Id as myid from SupportBean_A;\n" +
                    "insert into MyWindowWWF select *, Id || 'A' as myid from SupportBean_A;\n" +
                    "@name('select') select Id, myid from MyWindowWWF;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                env.SendEventBean(new SupportBean_A("E1"));
                var fields = new string[] { "Id", "myid" };
                env.AssertPropsNew("create", fields, new object[] { "E1", "E1A" });
                env.AssertPropsNew("select", fields, new object[] { "E1", "E1A" });

                env.UndeployAll();
            }
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

        public class NWTypesParentClass
        {
        }

        public class NWTypesChildClass : NWTypesParentClass
        {
        }

        public class MyLocalJsonProvidedSchemaOne
        {
            public int col1;
            public int col2;
        }

        public class MyLocalJsonProvidedSchemaWindow
        {
            public MyLocalJsonProvidedSchemaOne s1;
        }

        public class MyLocalJsonProvidedEventTypeOne
        {
            public int hsi;
        }

        public class MyLocalJsonProvidedEventTypeTwo
        {
            public MyLocalJsonProvidedEventTypeOne @event;
        }
    }
} // end of namespace