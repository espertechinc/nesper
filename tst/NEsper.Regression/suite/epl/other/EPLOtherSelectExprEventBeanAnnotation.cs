///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherSelectExprEventBeanAnnotation
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithSimple(execs);
            With(WSubquery)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithWSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSelectExprEventBeanAnnoWSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSelectExprEventBeanAnnoSimple());
            return execs;
        }

        private class EPLOtherSelectExprEventBeanAnnoSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    RunAssertionEventBeanAnnotation(env, rep);
                }
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class EPLOtherSelectExprEventBeanAnnoWSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test non-named-window
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public @buseventtype create objectarray schema MyEvent(col1 string, col2 string)",
                    path);

                var eplInsert = "@name('insert') @public insert into DStream select " +
                                "(select * from MyEvent#keepall) @eventbean as c0 " +
                                "from SupportBean";
                env.CompileDeploy(eplInsert, path);

                env.AssertStatement(
                    "insert",
                    statement => {
                        foreach (var prop in "c0".SplitCsv()) {
                            AssertFragment(prop, statement.EventType, "MyEvent", true);
                        }
                    });

                // test consuming statement
                var fields = "f0,f1".SplitCsv();
                env.CompileDeploy(
                        "@name('s0') select " +
                        "c0 as f0, " +
                        "c0.lastOf().col1 as f1 " +
                        "from DStream",
                        path)
                    .AddListener("s0");

                var eventOne = new object[] { "E1", null };
                env.SendEventObjectArray(eventOne, "MyEvent");
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", fields, new object[] { new object[] { eventOne }, "E1" });

                var eventTwo = new object[] { "E2", null };
                env.SendEventObjectArray(eventTwo, "MyEvent");
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", fields, new object[] { new object[] { eventOne, eventTwo }, "E2" });

                env.UndeployAll();
            }
        }

        private static void RunAssertionEventBeanAnnotation(
            RegressionEnvironment env,
            EventRepresentationChoice rep)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                rep.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyEvent)) +
                "@name('schema') @buseventtype @public create schema MyEvent(col1 string)",
                path);

            var eplInsert = "@name('insert') @public insert into DStream select " +
                            "last(*) @eventbean as c0, " +
                            "window(*) @eventbean as c1, " +
                            "prevwindow(s0) @eventbean as c2 " +
                            "from MyEvent#length(2) as s0";
            env.CompileDeploy(eplInsert, path).AddListener("insert");

            env.AssertStatement(
                "insert",
                statement => {
                    foreach (var prop in "c0,c1,c2".SplitCsv()) {
                        AssertFragment(prop, statement.EventType, "MyEvent", prop.Equals("c1") || prop.Equals("c2"));
                    }
                });

            // test consuming statement
            var fields = "f0,f1,f2,f3,f4,f5".SplitCsv();
            env.CompileDeploy(
                    "@name('s0') select " +
                    "c0 as f0, " +
                    "c0.col1 as f1, " +
                    "c1 as f2, " +
                    "c1.lastOf().col1 as f3, " +
                    "c1 as f4, " +
                    "c1.lastOf().col1 as f5 " +
                    "from DStream",
                    path)
                .AddListener("s0");
            env.CompileDeploy("@name('s1') select * from MyEvent", path).AddListener("s1");

            var eventOne = SendEvent(env, rep, "E1");
            if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
                eventOne = env.Listener("s1").AssertOneGetNewAndReset().Underlying;
            }

            ClassicAssert.IsTrue(
                ((IDictionary<string, object>)env.Listener("insert").AssertOneGetNewAndReset().Underlying).Get("c0") is
                EventBean);
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] { eventOne, "E1", new object[] { eventOne }, "E1", new object[] { eventOne }, "E1" });

            var eventTwo = SendEvent(env, rep, "E2");
            if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
                eventTwo = env.Listener("s1").AssertOneGetNewAndReset().Underlying;
            }

            env.AssertPropsNew(
                "s0",
                fields,
                new object[] {
                    eventTwo, "E2", new object[] { eventOne, eventTwo }, "E2", new object[] { eventOne, eventTwo }, "E2"
                });

            // test SODA
            env.EplToModelCompileDeploy(eplInsert, path);

            // test invalid
            env.TryInvalidCompile(
                path,
                "@name('s0') select last(*) @xxx from MyEvent",
                "Failed to recognize select-expression annotation 'xxx', expected 'eventbean' in text 'last(*) @xxx'");

            env.UndeployAll();
        }

        private static void AssertFragment(
            string prop,
            EventType eventType,
            string fragmentTypeName,
            bool indexed)
        {
            var desc = eventType.GetPropertyDescriptor(prop);
            ClassicAssert.AreEqual(true, desc.IsFragment);
            var fragment = eventType.GetFragmentType(prop);
            ClassicAssert.AreEqual(fragmentTypeName, fragment.FragmentType.Name);
            ClassicAssert.AreEqual(false, fragment.IsNative);
            ClassicAssert.AreEqual(indexed, fragment.IsIndexed);
        }

        private static object SendEvent(
            RegressionEnvironment env,
            EventRepresentationChoice rep,
            string value)
        {
            object eventOne;
            if (rep.IsMapEvent()) {
                var @event = Collections.SingletonDataMap("col1", value);
                env.SendEventMap(@event, "MyEvent");
                eventOne = @event;
            }
            else if (rep.IsObjectArrayEvent()) {
                var @event = new object[] { value };
                env.SendEventObjectArray(@event, "MyEvent");
                eventOne = @event;
            }
            else if (rep.IsAvroEvent()) {
                var schema = env.RuntimeAvroSchemaByDeployment("schema", "MyEvent");
                var @event = new GenericRecord(schema.AsRecordSchema());
                @event.Put("col1", value);
                env.SendEventAvro(@event, "MyEvent");
                eventOne = @event;
            }
            else if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
                var @object = new JObject(new JProperty("col1", value));
                env.SendEventJson(@object.ToString(), "MyEvent");
                eventOne = @object.ToString();
            }
            else {
                throw new IllegalStateException();
            }

            return eventOne;
        }

        public class MyLocalJsonProvidedMyEvent
        {
            public string col1;
        }
    }
} // end of namespace