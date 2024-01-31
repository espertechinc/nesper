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

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

// record
using NUnit.Framework;
using NUnit.Framework.Legacy;
using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoPopulateCreateStream : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                RunAssertionCreateStream(env, rep);
            }

            foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                RunAssertionCreateStreamTwo(env, rep);
            }

            foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                if (rep.IsAvroEvent()) {
                    env.AssertThat(() => RunAssertPopulateFromNamedWindow(env, rep));
                }
                else {
                    RunAssertPopulateFromNamedWindow(env, rep);
                }
            }

            RunAssertionObjectArrPropertyReorder(env);
        }

        private static void RunAssertionObjectArrPropertyReorder(RegressionEnvironment env)
        {
            var epl = "create objectarray schema MyInner (p_inner string);\n" +
                      "create objectarray schema MyOATarget (unfilled string, p0 string, p1 string, i0 MyInner);\n" +
                      "@Public @buseventtype create objectarray schema MyOASource (p0 string, p1 string, i0 MyInner);\n" +
                      "insert into MyOATarget select p0, p1, i0, null as unfilled from MyOASource;\n" +
                      "@name('s0') select * from MyOATarget;\n";
            env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

            env.SendEventObjectArray(new object[] { "p0value", "p1value", new object[] { "i" } }, "MyOASource");
            env.AssertPropsNew("s0", "p0,p1".SplitCsv(), new object[] { "p0value", "p1value" });

            env.UndeployAll();
        }

        private static void RunAssertPopulateFromNamedWindow(
            RegressionEnvironment env,
            EventRepresentationChoice type)
        {
            var path = new RegressionPath();
            var schemaEPL = type.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedNode)) +
                            "@Public @buseventtype create schema Node(nid string)";
            env.CompileDeploy(schemaEPL, path);

            env.CompileDeploy("@Public create window NodeWindow#unique(nid) as Node", path);
            env.CompileDeploy("insert into NodeWindow select * from Node", path);
            env.CompileDeploy(
                type.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedNodePlus)) +
                "create schema NodePlus(npid string, node Node)",
                path);
            env.CompileDeploy(
                    "@name('s0') insert into NodePlus select 'E1' as npid, n1 as node from NodeWindow n1",
                    path)
                .AddListener("s0");

            if (type.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] { "n1" }, "Node");
            }
            else if (type.IsMapEvent()) {
                env.SendEventMap(Collections.SingletonDataMap("nid", "n1"), "Node");
            }
            else if (type.IsAvroEvent()) {
                var genericRecord = new GenericRecord(SchemaBuilder.Record("name", RequiredString("nid")));
                genericRecord.Put("nid", "n1");
                env.SendEventAvro(genericRecord, "Node");
            }
            else if (type.IsJsonEvent() || type.IsJsonProvidedClassEvent()) {
                var @object = new JObject();
                @object.Add("nid", "n1");
                env.SendEventJson(@object.ToString(), "Node");
            }
            else {
                Assert.Fail();
            }

            env.AssertEventNew(
                "s0",
                @event => {
                    ClassicAssert.AreEqual("E1", @event.Get("npid"));
                    ClassicAssert.AreEqual("n1", @event.Get("node.nid"));
                    var fragment = (EventBean)@event.GetFragment("node");
                    ClassicAssert.AreEqual("Node", fragment.EventType.Name);
                });

            env.UndeployAll();
        }

        private static void RunAssertionCreateStream(
            RegressionEnvironment env,
            EventRepresentationChoice representation)
        {
            var epl = representation.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyEvent)) +
                      " @buseventtype @public create schema MyEvent(myId int);\n" +
                      representation.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedCompositeEvent)) +
                      " @buseventtype @public create schema CompositeEvent(c1 MyEvent, c2 MyEvent, rule string);\n" +
                      "insert into MyStream select c, 'additionalValue' as value from MyEvent c;\n" +
                      "insert into CompositeEvent select e1.c as c1, e2.c as c2, '4' as rule " +
                      "  from pattern [e1=MyStream -> e2=MyStream];\n" +
                      representation.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedCompositeEvent)) +
                      " @Name('Target') select * from CompositeEvent;\n";
            env.CompileDeploy(epl, new RegressionPath()).AddListener("Target");

            if (representation.IsObjectArrayEvent()) {
                env.SendEventObjectArray(MakeEvent(10).Values.ToArray(), "MyEvent");
                env.SendEventObjectArray(MakeEvent(11).Values.ToArray(), "MyEvent");
            }
            else if (representation.IsMapEvent()) {
                env.SendEventMap(MakeEvent(10), "MyEvent");
                env.SendEventMap(MakeEvent(11), "MyEvent");
            }
            else if (representation.IsAvroEvent()) {
                env.SendEventAvro(MakeEventAvro(10), "MyEvent");
                env.SendEventAvro(MakeEventAvro(11), "MyEvent");
            }
            else if (representation.IsJsonEvent() || representation.IsJsonProvidedClassEvent()) {
                env.SendEventJson("{\"myId\": 10}", "MyEvent");
                env.SendEventJson("{\"myId\": 11}", "MyEvent");
            }
            else {
                Assert.Fail();
            }

            env.AssertEventNew(
                "Target",
                theEvent => {
                    ClassicAssert.AreEqual(10, theEvent.Get("c1.myId"));
                    ClassicAssert.AreEqual(11, theEvent.Get("c2.myId"));
                    ClassicAssert.AreEqual("4", theEvent.Get("rule"));
                });

            env.UndeployAll();
        }

        private static void RunAssertionCreateStreamTwo(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var path = new RegressionPath();
            var epl = eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyEvent)) +
                      " @public @buseventtype create schema MyEvent(myId int)\n;" +
                      eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedAllMyEvent)) +
                      " @public @buseventtype create schema AllMyEvent as (myEvent MyEvent, clazz String, reverse boolean);\n" +
                      eventRepresentationEnum.GetAnnotationTextWJsonProvided(
                          typeof(MyLocalJsonProvidedSuspectMyEvent)) +
                      " @public @buseventtype create schema SuspectMyEvent as (myEvent MyEvent, clazz String);\n";
            env.CompileDeploy(epl, path);

            env.CompileDeploy(
                    "@name('s0') insert into AllMyEvent " +
                    "select c as myEvent, 'test' as clazz, false as reverse " +
                    "from MyEvent(myId=1) c",
                    path)
                .AddListener("s0");

            env.AssertStatement(
                "s0",
                statement => ClassicAssert.IsTrue(eventRepresentationEnum.MatchesClass(statement.EventType.UnderlyingType)));

            env.CompileDeploy(
                    "@name('s1') insert into SuspectMyEvent " +
                    "select c.myEvent as myEvent, clazz " +
                    "from AllMyEvent(not reverse) c",
                    path)
                .AddListener("s1");

            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(MakeEvent(1).Values.ToArray(), "MyEvent");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(MakeEvent(1), "MyEvent");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                env.SendEventAvro(MakeEventAvro(1), "MyEvent");
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                env.SendEventJson("{\"myId\": 1}", "MyEvent");
            }
            else {
                Assert.Fail();
            }

            env.AssertEventNew("s0", @event => AssertCreateStreamTwo(eventRepresentationEnum, @event));
            env.AssertEventNew("s1", @event => AssertCreateStreamTwo(eventRepresentationEnum, @event));

            env.UndeployAll();
        }

        private static void AssertCreateStreamTwo(
            EventRepresentationChoice eventRepresentationEnum,
            EventBean eventBean)
        {
            if (eventRepresentationEnum.IsAvroOrJsonEvent()) {
                ClassicAssert.AreEqual(1, eventBean.Get("myEvent.myId"));
            }
            else {
                ClassicAssert.IsTrue(eventBean.Get("myEvent") is EventBean);
                ClassicAssert.AreEqual(1, ((EventBean)eventBean.Get("myEvent")).Get("myId"));
            }

            ClassicAssert.IsNotNull(eventBean.EventType.GetFragmentType("myEvent"));
        }

        private static IDictionary<string, object> MakeEvent(int myId)
        {
            return Collections.SingletonDataMap("myId", myId);
        }

        private static GenericRecord MakeEventAvro(int myId)
        {
            var schema = SchemaBuilder.Record("schema", Field("myId", RequiredInt("myId")));
            var record = new GenericRecord(schema);
            record.Put("myId", myId);
            return record;
        }

        public class MyLocalJsonProvidedMyEvent
        {
            public int? myId;
        }

        public class MyLocalJsonProvidedCompositeEvent
        {
            public MyLocalJsonProvidedMyEvent c1;
            public MyLocalJsonProvidedMyEvent c2;
            public string rule;
        }

        public class MyLocalJsonProvidedAllMyEvent
        {
            public MyLocalJsonProvidedMyEvent myEvent;
            public string clazz;
            public bool reverse;
        }

        public class MyLocalJsonProvidedSuspectMyEvent
        {
            public MyLocalJsonProvidedMyEvent myEvent;
            public string clazz;
        }

        public class MyLocalJsonProvidedNode
        {
            public string nid;
        }

        public class MyLocalJsonProvidedNodePlus
        {
            public string npid;
            public MyLocalJsonProvidedNode node;
        }
    }
} // end of namespace