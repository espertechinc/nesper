///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherSelectExprEventBeanAnnotation
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSimple(execs);
            WithWSubquery(execs);
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

        internal class EPLOtherSelectExprEventBeanAnnoSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    RunAssertionEventBeanAnnotation(env, rep);
                }
            }
        }

        internal class EPLOtherSelectExprEventBeanAnnoWSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test non-named-window
                var path = new RegressionPath();
                env.CompileDeployWBusPublicType("create objectarray schema MyEvent(col1 string, col2 string)", path);

                var eplInsert = "@Name('insert') insert into DStream select " +
                                "(select * from MyEvent#keepall) @eventbean as c0 " +
                                "from SupportBean";
                env.CompileDeploy(eplInsert, path);

                foreach (var prop in "c0".SplitCsv()) {
                    AssertFragment(prop, env.Statement("insert").EventType, "MyEvent", true);
                }

                // test consuming statement
                var fields = "f0,f1".SplitCsv();
                env.CompileDeploy(
                        "@Name('s0') select " +
                        "c0 as f0, " +
                        "c0.lastOf().col1 as f1 " +
                        "from DStream",
                        path)
                    .AddListener("s0");

                var eventOne = new object[] {"E1", null};
                env.SendEventObjectArray(eventOne, "MyEvent");
                env.SendEventBean(new SupportBean());
                var @out = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(@out, fields, new object[] {new object[] {eventOne}, "E1"});

                var eventTwo = new object[] {"E2", null};
                env.SendEventObjectArray(eventTwo, "MyEvent");
                env.SendEventBean(new SupportBean());
                @out = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(@out, fields, new object[] {new object[] {eventOne, eventTwo}, "E2"});

                env.UndeployAll();
            }
        }

        private static void RunAssertionEventBeanAnnotation(
            RegressionEnvironment env,
            EventRepresentationChoice rep)
        {
            var path = new RegressionPath();
            env.CompileDeployWBusPublicType(
                rep.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMyEvent>() + "@Name('schema') create schema MyEvent(col1 string)",
                path);

            var eplInsert = "@Name('insert') insert into DStream select " +
                            "last(*) @eventbean as c0, " +
                            "window(*) @eventbean as c1, " +
                            "prevwindow(s0) @eventbean as c2 " +
                            "from MyEvent#length(2) as s0";
            env.CompileDeploy(eplInsert, path).AddListener("insert");

            foreach (var prop in "c0,c1,c2".SplitCsv()) {
                AssertFragment(prop, env.Statement("insert").EventType, "MyEvent", prop.Equals("c1") || prop.Equals("c2"));
            }

            // test consuming statement
            var fields = "f0,f1,f2,f3,f4,f5".SplitCsv();
            env.CompileDeploy(
                    "@Name('s0') select " +
                    "c0 as f0, " +
                    "c0.col1 as f1, " +
                    "c1 as f2, " +
                    "c1.lastOf().col1 as f3, " +
                    "c1 as f4, " +
                    "c1.lastOf().col1 as f5 " +
                    "from DStream",
                    path)
                .AddListener("s0");
            env.CompileDeploy("@Name('s1') select * from MyEvent", path).AddListener("s1");

            var eventOne = SendEvent(env, rep, "E1");
            if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
                eventOne = env.Listener("s1").AssertOneGetNewAndReset().Underlying;
            }

            var underlying = env.Listener("insert").AssertOneGetNewAndReset().Underlying.AsStringDictionary();
            Assert.IsTrue(underlying.Get("c0") is EventBean);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new[] {eventOne, "E1", new[] {eventOne}, "E1", new[] {eventOne}, "E1"});

            var eventTwo = SendEvent(env, rep, "E2");
            if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
                eventTwo = env.Listener("s1").AssertOneGetNewAndReset().Underlying;
            }

            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new[] {eventTwo, "E2", new[] {eventOne, eventTwo}, "E2", new[] {eventOne, eventTwo}, "E2"});

            // test SODA
            env.EplToModelCompileDeploy(eplInsert, path);

            // test invalid
            TryInvalidCompile(
                env,
                path,
                "@Name('s0') select last(*) @xxx from MyEvent",
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
            Assert.AreEqual(true, desc.IsFragment);
            var fragment = eventType.GetFragmentType(prop);
            Assert.AreEqual(fragmentTypeName, fragment.FragmentType.Name);
            Assert.AreEqual(false, fragment.IsNative);
            Assert.AreEqual(indexed, fragment.IsIndexed);
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
                var @event = new object[] {value};
                env.SendEventObjectArray(@event, "MyEvent");
                eventOne = @event;
            }
            else if (rep.IsAvroEvent()) {
                var schema = SupportAvroUtil.GetAvroSchema(env.Statement("schema").EventType).AsRecordSchema();
                var @event = new GenericRecord(schema);
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

        [Serializable]
        public class MyLocalJsonProvidedMyEvent
        {
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once UnusedMember.Global
            public string col1;
        }
    }
} // end of namespace