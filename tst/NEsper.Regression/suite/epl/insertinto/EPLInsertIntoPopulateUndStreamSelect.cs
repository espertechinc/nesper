///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

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
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithStreamInsertWWidenOA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoStreamInsertWWidenOA());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowRep(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNamedWindowRep());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowInheritsMap(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNamedWindowInheritsMap());
            return execs;
        }

        private class EPLInsertIntoNamedWindowInheritsMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype @public create objectarray schema Event();\n" +
                          "@public @buseventtype create objectarray schema ChildEvent(Id string, action string) inherits Event;\n" +
                          "@public @buseventtype create objectarray schema Incident(name string, event Event);\n" +
                          "@name('window') create window IncidentWindow#keepall as Incident;\n" +
                          "\n" +
                          "on ChildEvent e\n" +
                          "    merge IncidentWindow w\n" +
                          "    where e.Id = cast(w.event.Id? as string)\n" +
                          "    when not matched\n" +
                          "        then insert (name, event) select 'ChildIncident', e \n" +
                          "            where e.action = 'INSERT'\n" +
                          "    when matched\n" +
                          "        then update set w.event = e \n" +
                          "            where e.action = 'INSERT'\n" +
                          "        then delete\n" +
                          "            where e.action = 'CLEAR';";
                env.CompileDeploy(epl, new RegressionPath());

                env.SendEventObjectArray(new object[] { "ID1", "INSERT" }, "ChildEvent");
                env.AssertIterator(
                    "window",
                    iterator => {
                        var @event = iterator.Advance();
                        var underlying = (object[])@event.Underlying;
                        Assert.AreEqual("ChildIncident", underlying[0]);
                        var underlyingInner = (object[])((EventBean)underlying[1]).Underlying;
                        EPAssertionUtil.AssertEqualsExactOrder(new object[] { "ID1", "INSERT" }, underlyingInner);
                    });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoNamedWindowRep : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    if (rep.IsJsonProvidedClassEvent()) { // assertion uses inheritance of types
                        continue;
                    }

                    TryAssertionNamedWindow(env, rep);
                }
            }
        }

        private class EPLInsertIntoStreamInsertWWidenOA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionStreamInsertWWidenMap(env, rep);
                }
            }
        }

        private class EPLInsertIntoInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionInvalid(env, rep);
                }
            }
        }

        private static void TryAssertionNamedWindow(
            RegressionEnvironment env,
            EventRepresentationChoice rep)
        {
            var path = new RegressionPath();
            var schema = rep.GetAnnotationText() +
                         "@name('schema') @public @buseventtype create schema A as (myint int, mystr string);\n" +
                         rep.GetAnnotationText() +
                         "@public @buseventtype create schema C as (addprop int) inherits A;\n";
            env.CompileDeploy(schema, path);

            env.CompileDeploy("@public create window MyWindow#time(5 days) as C", path);
            env.CompileDeploy("@name('s0') select * from MyWindow", path).AddListener("s0");

            // select underlying
            env.CompileDeploy("@name('insert') insert into MyWindow select mya.* from A as mya", path);
            if (rep.IsMapEvent()) {
                env.SendEventMap(MakeMap(123, "abc"), "A");
            }
            else if (rep.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] { 123, "abc" }, "A");
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

            env.AssertPropsNew("s0", "myint,mystr,addprop".SplitCsv(), new object[] { 123, "abc", null });
            env.UndeployModuleContaining("insert");

            // select underlying plus property
            env.CompileDeploy("insert into MyWindow select mya.*, 1 as addprop from A as mya", path);
            if (rep.IsMapEvent()) {
                env.SendEventMap(MakeMap(456, "def"), "A");
            }
            else if (rep.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] { 456, "def" }, "A");
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

            env.AssertPropsNew("s0", "myint,mystr,addprop".SplitCsv(), new object[] { 456, "def", 1 });

            env.UndeployAll();
        }

        private static void TryAssertionStreamInsertWWidenMap(
            RegressionEnvironment env,
            EventRepresentationChoice rep)
        {
            var path = new RegressionPath();
            var schemaSrc = rep.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedSrc)) +
                            "@name('schema') @public @buseventtype create schema Src as (myint int, mystr string)";
            env.CompileDeploy(schemaSrc, path);

            env.CompileDeploy(
                rep.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedD1)) +
                "@public create schema D1 as (myint int, mystr string, addprop long)",
                path);
            var eplOne = "insert into D1 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(env, path, rep, eplOne, "myint,mystr,addprop", new object[] { 123, "abc", 1L });

            env.CompileDeploy(
                rep.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedD2)) +
                "@public create schema D2 as (mystr string, myint int, addprop double)",
                path);
            var eplTwo = "insert into D2 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(env, path, rep, eplTwo, "myint,mystr,addprop", new object[] { 123, "abc", 1d });

            env.CompileDeploy(
                rep.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedD3)) +
                "@public create schema D3 as (mystr string, addprop int)",
                path);
            var eplThree = "insert into D3 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(env, path, rep, eplThree, "mystr,addprop", new object[] { "abc", 1 });

            env.CompileDeploy(
                rep.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedD4)) +
                "@public create schema D4 as (myint int, mystr string)",
                path);
            var eplFour = "insert into D4 select mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(env, path, rep, eplFour, "myint,mystr", new object[] { 123, "abc" });

            var eplFive = "insert into D4 select mysrc.*, 999 as myint, 'xxx' as mystr from Src as mysrc";
            RunStreamInsertAssertion(env, path, rep, eplFive, "myint,mystr", new object[] { 999, "xxx" });
            var eplSix = "insert into D4 select 999 as myint, 'xxx' as mystr, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(env, path, rep, eplSix, "myint,mystr", new object[] { 999, "xxx" });

            env.UndeployAll();
        }

        private static void TryAssertionInvalid(
            RegressionEnvironment env,
            EventRepresentationChoice rep)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                rep.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedSrc)) +
                "@public create schema Src as (myint int, mystr string)",
                path);

            // mismatch in type
            env.CompileDeploy(
                rep.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedE1)) +
                "@public create schema E1 as (myint long)",
                path);
            var message = !rep.IsAvroEvent()
                ? "Type by name 'E1' in property 'myint' expected System.Nullable<System.Int64> but receives System.Nullable<System.Int32>"
                : "Type by name 'E1' in property 'myint' expected schema '{\"type\":\"long\"}' but received schema '{\"type\":\"int\"}'";
            env.TryInvalidCompile(path, "insert into E1 select mysrc.* from Src as mysrc", message);

            // mismatch in column name
            env.CompileDeploy(
                rep.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedE2)) +
                "@public create schema E2 as (someprop long)",
                path);
            env.TryInvalidCompile(
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
            env.CompileDeploy("@name('s0') " + epl, path).AddListener("s0");

            if (rep.IsMapEvent()) {
                env.SendEventMap(MakeMap(123, "abc"), "Src");
            }
            else if (rep.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] { 123, "abc" }, "Src");
            }
            else if (rep.IsAvroEvent()) {
                var schema = env.RuntimeAvroSchemaByDeployment("schema", "Src");
                var @event = new GenericRecord(schema.AsRecordSchema());
                @event.Put("myint", 123);
                @event.Put("mystr", "abc");
                env.SendEventAvro(@event, "Src");
            }
            else if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
                var @object = new JObject();
                @object.Add("myint", 123);
                @object.Add("mystr", "abc");
                env.SendEventJson(@object.ToString(), "Src");
            }
            else {
                Assert.Fail();
            }

            env.AssertPropsNew("s0", fields.SplitCsv(), expected);
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
            var schema = env.RuntimeAvroSchemaByDeployment("schema", "A");
            var record = new GenericRecord(schema.AsRecordSchema());
            record.Put("myint", myint);
            record.Put("mystr", mystr);
            return record;
        }

        public class MyLocalJsonProvidedSrc
        {
            public int myint;
            public string mystr;
        }

        public class MyLocalJsonProvidedD1
        {
            public int myint;
            public string mystr;
            public long addprop;
        }

        public class MyLocalJsonProvidedD2
        {
            public int myint;
            public string mystr;
            public double addprop;
        }

        public class MyLocalJsonProvidedD3
        {
            public string mystr;
            public int addprop;
        }

        public class MyLocalJsonProvidedD4
        {
            public int myint;
            public string mystr;
        }

        public class MyLocalJsonProvidedE1
        {
            public long myint;
        }

        public class MyLocalJsonProvidedE2
        {
            public long someprop;
        }
    }
} // end of namespace