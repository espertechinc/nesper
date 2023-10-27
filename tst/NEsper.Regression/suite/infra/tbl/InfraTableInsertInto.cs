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

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    /// NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableInsertInto
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithInsertIntoAndDelete(execs);
            WithInsertIntoSameModuleUnkeyed(execs);
            WithInsertIntoTwoModulesUnkeyed(execs);
            WithInsertIntoSelfAccess(execs);
            WithNamedWindowMergeInsertIntoTable(execs);
            WithInsertIntoWildcard(execs);
            WithInsertIntoFromNamedWindow(execs);
            WithInsertIntoSameModuleKeyed(execs);
            WithSplitStream(execs);
            WithTableInsertIntoLenientPropCount(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTableInsertIntoLenientPropCount(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableInsertIntoLenientPropCount());
            return execs;
        }

        public static IList<RegressionExecution> WithSplitStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSplitStream());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertIntoSameModuleKeyed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertIntoSameModuleKeyed());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertIntoFromNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertIntoFromNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertIntoWildcard(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertIntoWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowMergeInsertIntoTable(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowMergeInsertIntoTable());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertIntoSelfAccess(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertIntoSelfAccess());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertIntoTwoModulesUnkeyed(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertIntoTwoModulesUnkeyed());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertIntoSameModuleUnkeyed(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertIntoSameModuleUnkeyed());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertIntoAndDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertIntoAndDelete());
            return execs;
        }

        private class InfraTableInsertIntoLenientPropCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public @name('table') create table MyTable(c0 string primary key, c1 int primary key);\n" +
                    "insert into MyTable select TheString as c0 from SupportBean;\n" +
                    "insert into MyTable select Id as c1 from SupportBean_S0;\n";
                env.CompileDeploy(epl, path);
                var fields = "c0,c1".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsPerRowIterator("table", fields, new object[][] { new object[] { "E1", null } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(10));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "table",
                    fields,
                    new object[][] { new object[] { "E1", null }, new object[] { null, 10 } });

                env.UndeployAll();
            }
        }

        public class InfraInsertIntoAndDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "pkey0,pkey1,c0".SplitCsv();
                var path = new RegressionPath();

                var eplCreateTable =
                    "@name('table') @public create table MyTable(c0 long, pkey1 int primary key, pkey0 string primary key)";
                env.CompileDeploy(eplCreateTable, path);

                var eplIntoTable =
                    "@name('Insert-Into-Table') insert into MyTable select IntPrimitive as pkey1, LongPrimitive as c0, TheString as pkey0 from SupportBean";
                env.CompileDeploy(eplIntoTable, path);

                var eplDeleteTable =
                    "@name('Delete-Table') on SupportBean_S0 delete from MyTable where pkey1 = Id and pkey0 = P00";
                env.CompileDeploy(eplDeleteTable, path);

                env.Milestone(1);

                env.AssertPropsPerRowIteratorAnyOrder("table", fields, Array.Empty<object[]>());

                SendSupportBean(env, "E1", 10, 100); // insert E1

                env.Milestone(2);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "table",
                    fields,
                    new object[][] { new object[] { "E1", 10, 100L } });
                env.SendEventBean(new SupportBean_S0(10, "E1")); // delete E1

                env.Milestone(3);

                env.AssertPropsPerRowIteratorAnyOrder("table", fields, Array.Empty<object[]>());
                SendSupportBean(env, "E1", 11, 101); // insert E1 again

                env.Milestone(4);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "table",
                    fields,
                    new object[][] { new object[] { "E1", 11, 101L } });

                SendSupportBean(env, "E2", 20, 200); // insert E2

                env.Milestone(5);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "table",
                    fields,
                    new object[][] { new object[] { "E1", 11, 101L }, new object[] { "E2", 20, 200L } });
                env.SendEventBean(new SupportBean_S0(20, "E2")); // delete E2

                env.Milestone(6);

                env.SendEventBean(new SupportBean_S0(11, "E1")); // delete E1

                env.Milestone(7);
                env.AssertPropsPerRowIteratorAnyOrder("table", fields, Array.Empty<object[]>());

                SendSupportBean(env, "E1", 12, 102); // insert E1
                SendSupportBean(env, "E2", 21, 201); // insert E2
                env.AssertPropsPerRowIteratorAnyOrder(
                    "table",
                    fields,
                    new object[][] { new object[] { "E1", 12, 102L }, new object[] { "E2", 21, 201L } });

                env.UndeployAll();
            }
        }

        private class InfraInsertIntoSameModuleUnkeyed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString".SplitCsv();
                var epl = "@name('create') @public create table MyTableSM(TheString string);\n" +
                          "@name('tbl-insert') insert into MyTableSM select TheString from SupportBean;\n";
                env.CompileDeploy(epl);

                env.AssertPropsPerRowIterator("create", fields, Array.Empty<object[]>());

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1" } });

                env.Milestone(0);

                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1" } });

                env.UndeployAll();
            }
        }

        private class InfraInsertIntoSelfAccess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('create') @public create table MyTableIISA(pkey string primary key)", path);
                env.CompileDeploy(
                    "insert into MyTableIISA select TheString as pkey from SupportBean where MyTableIISA[TheString] is null",
                    path);

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    "pkey".SplitCsv(),
                    new object[][] { new object[] { "E1" } });

                env.Milestone(0);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    "pkey".SplitCsv(),
                    new object[][] { new object[] { "E1" } });

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    "pkey".SplitCsv(),
                    new object[][] { new object[] { "E1" } });

                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    "pkey".SplitCsv(),
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.Milestone(1);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    "pkey".SplitCsv(),
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });
                env.SendEventBean(new SupportBean("E2", 0));

                env.UndeployAll();
            }
        }

        private class InfraNamedWindowMergeInsertIntoTable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('create') @public create table MyTableNWM(pkey string)", path);
                env.CompileDeploy("@public create window MyWindow#keepall as SupportBean", path);
                env.CompileDeploy(
                    "on SupportBean as sb merge MyWindow when not matched " +
                    "then insert into MyTableNWM select sb.TheString as pkey",
                    path);

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    "pkey".SplitCsv(),
                    new object[][] { new object[] { "E1" } });

                env.UndeployAll();
            }
        }

        private class InfraSplitStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('createOne') @public create table MyTableOne(pkey string primary key, col int)",
                    path);
                env.CompileDeploy(
                    "@name('createTwo') @public create table MyTableTwo(pkey string primary key, col int)",
                    path);

                var eplSplit = "@name('split') @public on SupportBean \n" +
                               "  insert into MyTableOne select TheString as pkey, IntPrimitive as col where IntPrimitive > 0\n" +
                               "  insert into MyTableTwo select TheString as pkey, IntPrimitive as col where IntPrimitive < 0\n" +
                               "  insert into OtherStream select TheString as pkey, IntPrimitive as col where IntPrimitive = 0\n";
                env.CompileDeploy(eplSplit, path);

                env.CompileDeploy("@name('s1') select * from OtherStream", path).AddListener("s1");

                env.SendEventBean(new SupportBean("E1", 1));
                AssertSplitStream(env, new object[][] { new object[] { "E1", 1 } }, Array.Empty<object[]>());

                env.SendEventBean(new SupportBean("E2", -2));
                AssertSplitStream(
                    env,
                    new object[][] { new object[] { "E1", 1 } },
                    new object[][] { new object[] { "E2", -2 } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", -3));
                AssertSplitStream(
                    env,
                    new object[][] { new object[] { "E1", 1 } },
                    new object[][] { new object[] { "E2", -2 }, new object[] { "E3", -3 } });
                env.AssertListenerNotInvoked("s1");

                env.SendEventBean(new SupportBean("E4", 0));
                AssertSplitStream(
                    env,
                    new object[][] { new object[] { "E1", 1 } },
                    new object[][] { new object[] { "E2", -2 }, new object[] { "E3", -3 } });
                env.AssertPropsNew("s1", "pkey,col".SplitCsv(), new object[] { "E4", 0 });

                env.UndeployAll();
            }

            private static void AssertSplitStream(
                RegressionEnvironment env,
                object[][] tableOneRows,
                object[][] tableTwoRows)
            {
                var fields = "pkey,col".SplitCsv();
                env.AssertPropsPerRowIteratorAnyOrder("createOne", fields, tableOneRows);
                env.AssertPropsPerRowIteratorAnyOrder("createTwo", fields, tableTwoRows);
            }
        }

        private class InfraInsertIntoFromNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyWindow#unique(TheString) as SupportBean", path);
                env.CompileDeploy("insert into MyWindow select * from SupportBean", path);
                env.CompileDeploy(
                    "@name('create') @public create table MyTableIIF(pkey0 string primary key, pkey1 int primary key)",
                    path);
                env.CompileDeploy(
                    "on SupportBean_S1 insert into MyTableIIF select TheString as pkey0, IntPrimitive as pkey1 from MyWindow",
                    path);
                var fields = "pkey0,pkey1".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 10));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(1));
                env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "E1", 10 } });

                env.CompileExecuteFAFNoResult("delete from MyTableIIF", path);

                env.SendEventBean(new SupportBean("E1", 10));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean_S1(2));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 10 }, new object[] { "E2", 20 } });

                env.UndeployAll();
            }
        }

        private class InfraInsertIntoTwoModulesUnkeyed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString".SplitCsv();
                var path = new RegressionPath();
                env.CompileDeploy("@name('create') @public create table MyTableIIU(TheString string)", path);
                env.CompileDeploy("@name('tbl-insert') insert into MyTableIIU select TheString from SupportBean", path);

                env.AssertPropsPerRowIterator("create", fields, Array.Empty<object[]>());

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1" } });

                env.Milestone(0);

                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1" } });

                env.AssertThat(
                    () => {
                        try {
                            env.SendEventBean(new SupportBean("E2", 0));
                            Assert.Fail();
                        }
                        catch (EPException ex) {
                            SupportMessageAssertUtil.AssertMessage(
                                ex,
                                "java.lang.RuntimeException: Unexpected exception in statement 'tbl-insert': Unique index violation, table 'MyTableIIU' is a declared to hold a single un-keyed row");
                        }
                    });

                env.UndeployAll();
            }
        }

        private class InfraInsertIntoSameModuleKeyed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "pkey,thesum".SplitCsv();
                var epl = "@name('create') create table MyTableIIK(" +
                          "pkey string primary key," +
                          "thesum sum(int));\n";
                epl += "insert into MyTableIIK select TheString as pkey from SupportBean;\n";
                epl += "into table MyTableIIK select sum(Id) as thesum from SupportBean_S0 group by P00;\n";
                epl += "on SupportBean_S1 insert into MyTableIIK select P10 as pkey;\n";
                epl +=
                    "on SupportBean_S2 merge MyTableIIK where P20 = pkey when not matched then insert into MyTableIIK select P20 as pkey;\n";
                env.CompileDeploy(epl);

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", null } });

                env.SendEventBean(new SupportBean_S0(10, "E1"));
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 10 } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 10 }, new object[] { "E2", null } });

                env.SendEventBean(new SupportBean_S0(20, "E2"));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(11, "E1"));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 21 }, new object[] { "E2", 20 } });

                // assert on-insert and on-merge
                env.SendEventBean(new SupportBean_S1(0, "E3"));
                env.SendEventBean(new SupportBean_S2(0, "E4"));

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(3, "E3"));
                env.SendEventBean(new SupportBean_S0(4, "E4"));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] {
                        new object[] { "E1", 21 }, new object[] { "E2", 20 }, new object[] { "E3", 3 },
                        new object[] { "E4", 4 }
                    });

                env.UndeployAll();
            }
        }

        private class InfraInsertIntoWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionWildcard(env, false, rep);
                }
            }
        }

        private static void TryAssertionWildcard(
            RegressionEnvironment env,
            bool bean,
            EventRepresentationChoice rep)
        {
            var path = new RegressionPath();

            EPCompiled schemaCompiled;
            if (bean) {
                schemaCompiled = env.Compile(
                    "@public @buseventtype create schema MySchema as " + typeof(MyP0P1Event).FullName);
            }
            else {
                schemaCompiled = env.Compile(
                    rep.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMySchema)) +
                    "@public @buseventtype create schema MySchema (p0 string, p1 string)");
            }

            path.Add(schemaCompiled);
            env.Deploy(schemaCompiled);

            env.CompileDeploy("@name('create') @public create table TheTable (p0 string, p1 string)", path);
            env.CompileDeploy("insert into TheTable select * from MySchema", path);

            if (bean) {
                env.SendEventBean(new MyP0P1Event("a", "b"), "MySchema");
            }
            else if (rep.IsMapEvent()) {
                IDictionary<string, object> map = new Dictionary<string, object>();
                map.Put("p0", "a");
                map.Put("p1", "b");
                env.SendEventMap(map, "MySchema");
            }
            else if (rep.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] { "a", "b" }, "MySchema");
            }
            else if (rep.IsAvroEvent()) {
                var theEvent = new GenericRecord(env.RuntimeAvroSchemaPreconfigured("MySchema").AsRecordSchema());
                theEvent.Put("p0", "a");
                theEvent.Put("p1", "b");
                env.SendEventAvro(theEvent, "MySchema");
            }
            else if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
                env.SendEventJson(
                    new JObject(new JProperty("p0", "a"), new JProperty("p1", "b")).ToString(),
                    "MySchema");
            }
            else {
                Assert.Fail();
            }

            env.AssertIterator(
                "create",
                iterator => EPAssertionUtil.AssertProps(
                    iterator.Advance(),
                    "p0,p1".SplitCsv(),
                    new object[] { "a", "b" }));
            env.UndeployAll();
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string @string,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(@string, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
        }

        public class MyP0P1Event
        {
            private readonly string p0;
            private readonly string p1;

            internal MyP0P1Event(
                string p0,
                string p1)
            {
                this.p0 = p0;
                this.p1 = p1;
            }

            public string P0 => p0;

            public string P1 => p1;
        }

        [Serializable]
        public class MyLocalJsonProvidedMySchema
        {
            public string p0;
            public string p1;
        }
    }
} // end of namespace