///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

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

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoPopulateUndStreamSelect
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithNamedWindowInheritsMap(execs);
            WithNamedWindowRep(execs);
            WithStreamInsertWWidenOA(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithStreamInsertWWidenOA(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoStreamInsertWWidenOA());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowRep(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNamedWindowRep());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowInheritsMap(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNamedWindowInheritsMap());
            return execs;
        }

        internal class EPLInsertIntoNamedWindowInheritsMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "create objectarray schema Event();\n" +
                             "create objectarray schema ChildEvent(id string, action string) inherits Event;\n" +
                             "create objectarray schema Incident(name string, event Event);\n" +
                             "@Name('window') create window IncidentWindow#keepall as Incident;\n" +
                             "\n" +
                             "on ChildEvent e\n" +
                             "    merge IncidentWindow w\n" +
                             "    where e.id = cast(w.event.id? as string)\n" +
                             "    when not matched\n" +
                             "        then insert (name, event) select 'ChildIncident', e \n" +
                             "            where e.action = 'INSERT'\n" +
                             "    when matched\n" +
                             "        then update set w.event = e \n" +
                             "            where e.action = 'INSERT'\n" +
                             "        then delete\n" +
                             "            where e.action = 'CLEAR';";
                env.CompileDeployWBusPublicType(epl, new RegressionPath());

                env.SendEventObjectArray(new object[] {"ID1", "INSERT"}, "ChildEvent");
                EventBean @event = env.Statement("window").First();
                object[] underlying = (object[]) @event.Underlying;
                Assert.AreEqual("ChildIncident", underlying[0]);
                object[] underlyingInner = (object[]) ((EventBean) underlying[1]).Underlying;
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"ID1", "INSERT"}, underlyingInner);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoNamedWindowRep : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (EventRepresentationChoice rep in EventRepresentationChoiceExtensions.Values()) {
                    if (rep.IsJsonProvidedClassEvent()) { // assertion uses inheritance of types
                        continue;
                    }

                    TryAssertionNamedWindow(env, rep);
                }
            }
        }

        internal class EPLInsertIntoStreamInsertWWidenOA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (EventRepresentationChoice rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionStreamInsertWWidenMap(env, rep);
                }
            }
        }

        internal class EPLInsertIntoInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (EventRepresentationChoice rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionInvalid(env, rep);
                }
            }
        }

        private static void TryAssertionNamedWindow(
            RegressionEnvironment env,
            EventRepresentationChoice rep)
        {
            RegressionPath path = new RegressionPath();
            string schema = rep.GetAnnotationText() +
                            "@Name('schema') create schema A as (myint int, mystr string);\n" +
                            rep.GetAnnotationText() +
                            "create schema C as (addprop int) inherits A;\n";
            env.CompileDeployWBusPublicType(schema, path);

            env.CompileDeploy("create window MyWindow#time(5 days) as C", path);
            env.CompileDeploy("@Name('s0') select * from MyWindow", path).AddListener("s0");

            // select underlying
            env.CompileDeploy("@Name('insert') insert into MyWindow select mya.* from A as mya", path);
            if (rep.IsMapEvent()) {
                env.SendEventMap(MakeMap(123, "abc"), "A");
            }
            else if (rep.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {123, "abc"}, "A");
            }
            else if (rep.IsAvroEvent()) {
                env.SendEventAvro(MakeAvro(env, 123, "abc"), "A");
            }
            else if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
                var @object = new JObject();
                @object.Add("myint", 123);
                @object.Add("mystr", "abc");
                env.SendEventJson(@object.ToString(), "A");
            }
            else {
                Assert.Fail();
            }

            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "myint,mystr,addprop".SplitCsv(), new object[] {123, "abc", null});
            env.UndeployModuleContaining("insert");

            // select underlying plus property
            env.CompileDeploy("insert into MyWindow select mya.*, 1 as addprop from A as mya", path);
            if (rep.IsMapEvent()) {
                env.SendEventMap(MakeMap(456, "def"), "A");
            }
            else if (rep.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {456, "def"}, "A");
            }
            else if (rep.IsAvroEvent()) {
                env.SendEventAvro(MakeAvro(env, 456, "def"), "A");
            }
            else if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
                var @object = new JObject();
                @object.Add("myint", 456);
                @object.Add("mystr", "def");
                env.SendEventJson(@object.ToString(), "A");
            }
            else {
                Assert.Fail();
            }

            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "myint,mystr,addprop".SplitCsv(), new object[] {456, "def", 1});

            env.UndeployAll();
        }

        private static void TryAssertionStreamInsertWWidenMap(
            RegressionEnvironment env,
            EventRepresentationChoice rep)
        {
            RegressionPath path = new RegressionPath();
            string schemaSrc = rep.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedSrc>() +
                               "@Name('schema') create schema Src as (myint int, mystr string)";
            env.CompileDeployWBusPublicType(schemaSrc, path);

            env.CompileDeploy(
                rep.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedD1>() + "create schema D1 as (myint int, mystr string, addprop long)",
                path);
            string eplOne = "insert into D1 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(env, path, rep, eplOne, "myint,mystr,addprop", new object[] {123, "abc", 1L});

            env.CompileDeploy(
                rep.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedD2>() + "create schema D2 as (mystr string, myint int, addprop double)",
                path);
            string eplTwo = "insert into D2 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(env, path, rep, eplTwo, "myint,mystr,addprop", new object[] {123, "abc", 1d});

            env.CompileDeploy(rep.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedD3>() + "create schema D3 as (mystr string, addprop int)", path);
            string eplThree = "insert into D3 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(env, path, rep, eplThree, "mystr,addprop", new object[] {"abc", 1});

            env.CompileDeploy(rep.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedD4>() + "create schema D4 as (myint int, mystr string)", path);
            string eplFour = "insert into D4 select mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(env, path, rep, eplFour, "myint,mystr", new object[] {123, "abc"});

            string eplFive = "insert into D4 select mysrc.*, 999 as myint, 'xxx' as mystr from Src as mysrc";
            RunStreamInsertAssertion(env, path, rep, eplFive, "myint,mystr", new object[] {999, "xxx"});
            string eplSix = "insert into D4 select 999 as myint, 'xxx' as mystr, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(env, path, rep, eplSix, "myint,mystr", new object[] {999, "xxx"});

            env.UndeployAll();
        }

        private static void TryAssertionInvalid(
            RegressionEnvironment env,
            EventRepresentationChoice rep)
        {
            RegressionPath path = new RegressionPath();
            env.CompileDeploy(rep.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedSrc>() + "create schema Src as (myint int, mystr string)", path);

            // mismatch in type
            env.CompileDeploy(rep.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedE1>() + "create schema E1 as (myint long)", path);
            string message = !rep.IsAvroEvent()
                ? "Type by name 'E1' in property 'myint' expected System.Nullable<System.Int32> but receives System.Nullable<System.Int64>"
                : "Type by name 'E1' in property 'myint' expected schema '{\"type\":\"long\"}' but received schema '{\"type\":\"int\"}'";
            SupportMessageAssertUtil.TryInvalidCompile(env, path, "insert into E1 select mysrc.* from Src as mysrc", message);

            // mismatch in column name
            env.CompileDeploy(rep.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedE2>() + "create schema E2 as (someprop long)", path);
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                path,
                "insert into E2 select mysrc.*, 1 as otherprop from Src as mysrc",
                "Failed to find column 'otherprop' in target type 'E2' [insert into E2 select mysrc.*, 1 as otherprop from Src as mysrc]");

            env.UndeployAll();
        }

        private static void RunStreamInsertAssertion(
            RegressionEnvironment env,
            RegressionPath path,
            EventRepresentationChoice rep,
            string epl,
            string fields,
            object[] expected)
        {
            env.CompileDeploy("@Name('s0') " + epl, path).AddListener("s0");

            if (rep.IsMapEvent()) {
                env.SendEventMap(MakeMap(123, "abc"), "Src");
            }
            else if (rep.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {123, "abc"}, "Src");
            }
            else if (rep.IsAvroEvent()) {
                var eventType = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("schema"), "Src");
                var @event = new GenericRecord(SupportAvroUtil.GetAvroSchema(eventType).AsRecordSchema());
                @event.Put("myint", 123);
                @event.Put("mystr", "abc");
                env.SendEventAvro(@event, "Src");
            }
            else if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
                JObject @object = new JObject();
                @object.Add("myint", 123);
                @object.Add("mystr", "abc");
                env.SendEventJson(@object.ToString(), "Src");
            }
            else {
                Assert.Fail();
            }

            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields.SplitCsv(), expected);
            env.UndeployModuleContaining("s0");
        }

        private static IDictionary<string, object> MakeMap(
            int myint,
            string mystr)
        {
            IDictionary<string, object> @event = new Dictionary<string, object>();
            @event.Put("myint", myint);
            @event.Put("mystr", mystr);
            return @event;
        }

        private static GenericRecord MakeAvro(
            RegressionEnvironment env,
            int myint,
            string mystr)
        {
            EventType eventType = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("schema"), "A");
            var record = new GenericRecord(SupportAvroUtil.GetAvroSchema(eventType).AsRecordSchema());
            record.Put("myint", myint);
            record.Put("mystr", mystr);
            return record;
        }

        [Serializable]
        public class MyLocalJsonProvidedSrc
        {
            public int myint;
            public string mystr;
        }

        [Serializable]
        public class MyLocalJsonProvidedD1
        {
            public int myint;
            public string mystr;
            public long addprop;
        }

        [Serializable]
        public class MyLocalJsonProvidedD2
        {
            public int myint;
            public string mystr;
            public double addprop;
        }

        [Serializable]
        public class MyLocalJsonProvidedD3
        {
            public string mystr;
            public int addprop;
        }

        [Serializable]
        public class MyLocalJsonProvidedD4
        {
            public int myint;
            public string mystr;
        }

        [Serializable]
        public class MyLocalJsonProvidedE1
        {
            public long myint;
        }

        [Serializable]
        public class MyLocalJsonProvidedE2
        {
            public long someprop;
        }
    }
} // end of namespace