///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.@join.lookup;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableCreateIndex
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraMultiRangeAndKey(true));
            execs.Add(new InfraMultiRangeAndKey(false));
            execs.Add(new InfraHashBTreeWidening(true));
            execs.Add(new InfraHashBTreeWidening(false));
            execs.Add(new InfraWidening(true));
            execs.Add(new InfraWidening(false));
            execs.Add(new InfraCompositeIndex(true));
            execs.Add(new InfraCompositeIndex(false));
            execs.Add(new InfraLateCreate(true));
            execs.Add(new InfraLateCreate(false));
            execs.Add(new InfraLateCreateSceneTwo(true));
            execs.Add(new InfraLateCreateSceneTwo(false));
            execs.Add(new InfraMultipleColumnMultipleIndex(true));
            execs.Add(new InfraMultipleColumnMultipleIndex(false));
            execs.Add(new InfraDropCreate(true));
            execs.Add(new InfraDropCreate(false));
            execs.Add(new InfraOnSelectReUse(true));
            execs.Add(new InfraOnSelectReUse(false));
            execs.Add(new InfraInvalid(true));
            execs.Add(new InfraInvalid(false));
            execs.Add(new InfraMultikeyIndexFAF(true));
            execs.Add(new InfraMultikeyIndexFAF(false));
            return execs;
        }

        private static void RunQueryAssertion(
            RegressionEnvironment env,
            RegressionPath path,
            string epl,
            string[] fields,
            object[][] expected)
        {
            var result = env.CompileExecuteFAF(epl, path);
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, expected);
        }

        private static void SendEventLong(
            RegressionEnvironment env,
            string theString,
            long longPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.TheString = theString;
            theEvent.LongPrimitive = longPrimitive;
            env.SendEventBean(theEvent);
        }

        private static void SendEventShort(
            RegressionEnvironment env,
            string theString,
            short shortPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.TheString = theString;
            theEvent.ShortPrimitive = shortPrimitive;
            env.SendEventBean(theEvent);
        }

        private static void MakeSendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var b = new SupportBean(theString, intPrimitive);
            b.LongPrimitive = longPrimitive;
            env.SendEventBean(b);
        }

        private static void AssertCols(
            RegressionEnvironment env,
            string listOfP00,
            object[][] expected)
        {
            var p00s = listOfP00.SplitCsv();
            Assert.AreEqual(p00s.Length, expected.Length);
            for (var i = 0; i < p00s.Length; i++) {
                env.SendEventBean(new SupportBean_S0(0, p00s[i]));
                if (expected[i] == null) {
                    Assert.IsFalse(env.Listener("s0").IsInvoked);
                }
                else {
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        "col0,col1".SplitCsv(),
                        expected[i]);
                }
            }
        }

        private static int GetIndexCount(
            RegressionEnvironment env,
            bool namedWindow,
            string infraStmtName,
            string infraName)
        {
            return SupportInfraUtil.GetIndexCountNoContext(env, namedWindow, infraStmtName, infraName);
        }

        private static void AssertIndexesRef(
            RegressionEnvironment env,
            bool namedWindow,
            string name,
            string csvNames)
        {
            var entry = GetIndexEntry(env, namedWindow, name);
            if (string.IsNullOrEmpty(csvNames)) {
                Assert.IsNull(entry);
            }
            else {
                EPAssertionUtil.AssertEqualsAnyOrder(csvNames.SplitCsv(), entry.ReferringDeployments);
            }
        }

        private static void AssertIndexCountInstance(
            RegressionEnvironment env,
            bool namedWindow,
            string name,
            int count)
        {
            var repo = GetIndexInstanceRepo(env, namedWindow, name);
            Assert.AreEqual(count, repo.Tables.Count);
        }

        private static EventTableIndexRepository GetIndexInstanceRepo(
            RegressionEnvironment env,
            bool namedWindow,
            string name)
        {
            if (namedWindow) {
                var namedWindowInstance = SupportInfraUtil.GetInstanceNoContextNW(env, "create", name);
                return namedWindowInstance.RootViewInstance.IndexRepository;
            }

            var instance = SupportInfraUtil.GetInstanceNoContextTable(env, "create", name);
            return instance.IndexRepository;
        }

        private static EventTableIndexMetadataEntry GetIndexEntry(
            RegressionEnvironment env,
            bool namedWindow,
            string name)
        {
            var descOne = new IndexedPropDesc("col0", typeof(string));
            var index = new IndexMultiKey(
                false,
                Arrays.AsList(descOne),
                Collections.GetEmptyList<IndexedPropDesc>(),
                null);
            var meta = GetIndexMetaRepo(env, namedWindow, name);
            return meta.Indexes.Get(index);
        }

        private static EventTableIndexMetadata GetIndexMetaRepo(
            RegressionEnvironment env,
            bool namedWindow,
            string name)
        {
            if (namedWindow) {
                var processor = SupportInfraUtil.GetNamedWindow(env, "create", name);
                return processor.EventTableIndexMetadata;
            }

            var table = SupportInfraUtil.GetTable(env, "create", name);
            return table.EventTableIndexMetadata;
        }

        internal class InfraInvalid : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraInvalid(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? "create window MyInfraOne#keepall as (f1 string, f2 int)"
                    : "create table MyInfraOne as (f1 string primary key, f2 int primary key)";
                env.CompileDeploy(eplCreate, path);
                env.CompileDeploy("create index MyInfraIndex on MyInfraOne(f1)", path);

                env.CompileDeploy("create context ContextOne initiated by SupportBean terminated after 5 sec", path);
                env.CompileDeploy("create context ContextTwo initiated by SupportBean terminated after 5 sec", path);
                var eplCreateWContext = namedWindow
                    ? "context ContextOne create window MyInfraCtx#keepall as (f1 string, f2 int)"
                    : "context ContextOne create table MyInfraCtx as (f1 string primary key, f2 int primary key)";
                env.CompileDeploy(eplCreateWContext, path);

                // invalid context
                TryInvalidCompile(
                    env,
                    path,
                    "create unique index IndexTwo on MyInfraCtx(f1)",
                    (namedWindow ? "Named window" : "Table") +
                    " by name 'MyInfraCtx' has been declared for context 'ContextOne' and can only be used within the same context");
                TryInvalidCompile(
                    env,
                    path,
                    "context ContextTwo create unique index IndexTwo on MyInfraCtx(f1)",
                    (namedWindow ? "Named window" : "Table") +
                    " by name 'MyInfraCtx' has been declared for context 'ContextOne' and can only be used within the same context");

                TryInvalidCompile(
                    env,
                    path,
                    "create index MyInfraIndex on MyInfraOne(f1)",
                    "An index by name 'MyInfraIndex' already exists [");

                TryInvalidCompile(
                    env,
                    path,
                    "create index IndexTwo on MyInfraOne(fx)",
                    "Property named 'fx' not found");

                TryInvalidCompile(
                    env,
                    path,
                    "create index IndexTwo on MyInfraOne(f1, f1)",
                    "Property named 'f1' has been declared more then once [create index IndexTwo on MyInfraOne(f1, f1)]");

                TryInvalidCompile(
                    env,
                    path,
                    "create index IndexTwo on MyWindowX(f1, f1)",
                    "A named window or table by name 'MyWindowX' does not exist [create index IndexTwo on MyWindowX(f1, f1)]");

                TryInvalidCompile(
                    env,
                    path,
                    "create index IndexTwo on MyInfraOne(f1 bubu, f2)",
                    "Unrecognized advanced-type index 'bubu'");

                TryInvalidCompile(
                    env,
                    path,
                    "create gugu index IndexTwo on MyInfraOne(f2)",
                    "InvalId keyword 'gugu' in create-index encountered, expected 'unique' [create gugu index IndexTwo on MyInfraOne(f2)]");

                TryInvalidCompile(
                    env,
                    path,
                    "create unique index IndexTwo on MyInfraOne(f2 btree)",
                    "Combination of unique index with btree (range) is not supported [create unique index IndexTwo on MyInfraOne(f2 btree)]");

                // invalid insert-into unique index
                var eplCreateTwo = namedWindow
                    ? "@Name('create') create window MyInfraTwo#keepall as SupportBean"
                    : "@Name('create') create table MyInfraTwo(TheString string primary key, IntPrimitive int primary key)";
                env.CompileDeploy(eplCreateTwo, path);
                env.CompileDeploy(
                    "@Name('insert') insert into MyInfraTwo select TheString, IntPrimitive from SupportBean",
                    path);
                env.CompileDeploy("create unique index I1 on MyInfraTwo(TheString)", path);
                env.SendEventBean(new SupportBean("E1", 1));
                try {
                    env.SendEventBean(new SupportBean("E1", 2));
                    Assert.Fail();
                }
                catch (Exception ex) {
                    var text = namedWindow
                        ? "Unexpected exception in statement 'create': Unique index violation, index 'I1' is a unique index and key 'E1' already exists"
                        : "System.Exception: Unexpected exception in statement 'insert': Unique index violation, index 'I1' is a unique index and key 'E1' already exists";
                    Assert.AreEqual(text, ex.Message);
                }

                if (!namedWindow) {
                    env.CompileDeploy("create table MyTable (p0 string, sumint sum(int))", path);
                    TryInvalidCompile(
                        env,
                        path,
                        "create index MyIndex on MyTable(p0)",
                        "Tables without primary key column(s) do not allow creating an index [");
                }

                env.UndeployAll();
            }
        }

        internal class InfraOnSelectReUse : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraOnSelectReUse(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextCreateOne = namedWindow
                    ? "@Name('create') create window MyInfraONR#keepall as (f1 string, f2 int)"
                    : "@Name('create') create table MyInfraONR as (f1 string primary key, f2 int primary key)";
                env.CompileDeploy(stmtTextCreateOne, path);
                env.CompileDeploy(
                    "insert into MyInfraONR(f1, f2) select TheString, IntPrimitive from SupportBean",
                    path);
                env.CompileDeploy("@Name('indexOne') create index MyInfraONRIndex1 on MyInfraONR(f2)", path);
                var fields = "f1,f2".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 1));

                env.CompileDeploy(
                        "@Name('s0') on SupportBean_S0 s0 select nw.f1 as f1, nw.f2 as f2 from MyInfraONR nw where nw.f2 = s0.Id",
                        path)
                    .AddListener("s0");
                Assert.AreEqual(namedWindow ? 1 : 2, GetIndexCount(env, namedWindow, "create", "MyInfraONR"));

                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});

                // create second identical statement
                env.CompileDeploy(
                    "@Name('stmtTwo') on SupportBean_S0 s0 select nw.f1 as f1, nw.f2 as f2 from MyInfraONR nw where nw.f2 = s0.Id",
                    path);
                Assert.AreEqual(namedWindow ? 1 : 2, GetIndexCount(env, namedWindow, "create", "MyInfraONR"));

                env.UndeployModuleContaining("s0");
                Assert.AreEqual(namedWindow ? 1 : 2, GetIndexCount(env, namedWindow, "create", "MyInfraONR"));

                env.UndeployModuleContaining("stmtTwo");
                Assert.AreEqual(namedWindow ? 1 : 2, GetIndexCount(env, namedWindow, "create", "MyInfraONR"));

                env.UndeployModuleContaining("indexOne");

                // two-key index order test
                env.CompileDeploy("@Name('cw') create window MyInfraFour#keepall as SupportBean", path);
                env.CompileDeploy("create index Idx1 on MyInfraFour (TheString, IntPrimitive)", path);
                env.CompileDeploy(
                    "on SupportBean sb select * from MyInfraFour w where w.TheString = sb.TheString and w.IntPrimitive = sb.IntPrimitive",
                    path);
                env.CompileDeploy(
                    "on SupportBean sb select * from MyInfraFour w where w.IntPrimitive = sb.IntPrimitive and w.TheString = sb.TheString",
                    path);
                Assert.AreEqual(1, SupportInfraUtil.GetIndexCountNoContext(env, true, "cw", "MyInfraFour"));

                env.UndeployAll();
            }
        }

        internal class InfraDropCreate : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraDropCreate(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextCreateOne = namedWindow
                    ? "@Name('create') create window MyInfraDC#keepall as (f1 string, f2 int, f3 string, f4 string)"
                    : "@Name('create') create table MyInfraDC as (f1 string primary key, f2 int primary key, f3 string primary key, f4 string primary key)";
                env.CompileDeploy(stmtTextCreateOne, path);
                env.CompileDeploy(
                    "insert into MyInfraDC(f1, f2, f3, f4) select TheString, IntPrimitive, '>'||TheString||'<', '?'||TheString||'?' from SupportBean",
                    path);
                env.CompileDeploy("@Name('indexOne') create index MyInfraDCIndex1 on MyInfraDC(f1)", path);
                env.CompileDeploy("@Name('indexTwo') create index MyInfraDCIndex2 on MyInfraDC(f4)", path);
                var fields = "f1,f2".SplitCsv();

                env.SendEventBean(new SupportBean("E1", -2));

                env.UndeployModuleContaining("indexOne");

                var result = env.CompileExecuteFAF("select * from MyInfraDC where f1='E1'", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2}});

                result = env.CompileExecuteFAF("select * from MyInfraDC where f4='?E1?'", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2}});

                env.UndeployModuleContaining("indexTwo");

                result = env.CompileExecuteFAF("select * from MyInfraDC where f1='E1'", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2}});

                result = env.CompileExecuteFAF("select * from MyInfraDC where f4='?E1?'", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2}});

                path.Compileds.RemoveAt(path.Compileds.Count - 1);
                env.CompileDeploy("@Name('IndexThree') create index MyInfraDCIndex2 on MyInfraDC(f4)", path);

                result = env.CompileExecuteFAF("select * from MyInfraDC where f1='E1'", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2}});

                result = env.CompileExecuteFAF("select * from MyInfraDC where f4='?E1?'", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2}});

                env.UndeployModuleContaining("IndexThree");
                Assert.AreEqual(namedWindow ? 0 : 1, GetIndexCount(env, namedWindow, "create", "MyInfraDC"));

                env.UndeployAll();
            }
        }

        internal class InfraMultipleColumnMultipleIndex : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraMultipleColumnMultipleIndex(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextCreateOne = namedWindow
                    ? "create window MyInfraMCMI#keepall as (f1 string, f2 int, f3 string, f4 string)"
                    : "create table MyInfraMCMI as (f1 string primary key, f2 int, f3 string, f4 string)";
                env.CompileDeploy(stmtTextCreateOne, path);
                env.CompileDeploy(
                    "insert into MyInfraMCMI(f1, f2, f3, f4) select TheString, IntPrimitive, '>'||TheString||'<', '?'||TheString||'?' from SupportBean",
                    path);
                env.CompileDeploy("create index MyInfraMCMIIndex1 on MyInfraMCMI(f2, f3, f1)", path);
                env.CompileDeploy("create index MyInfraMCMIIndex2 on MyInfraMCMI(f2, f3)", path);
                env.CompileDeploy("create index MyInfraMCMIIndex3 on MyInfraMCMI(f2)", path);
                var fields = "f1,f2,f3,f4".SplitCsv();

                env.SendEventBean(new SupportBean("E1", -2));
                env.SendEventBean(new SupportBean("E2", -4));
                env.SendEventBean(new SupportBean("E3", -3));

                var result = env.CompileExecuteFAF("select * from MyInfraMCMI where f3='>E1<'", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

                result = env.CompileExecuteFAF("select * from MyInfraMCMI where f3='>E1<' and f2=-2", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

                result = env.CompileExecuteFAF("select * from MyInfraMCMI where f3='>E1<' and f2=-2 and f1='E1'", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

                result = env.CompileExecuteFAF("select * from MyInfraMCMI where f2=-2", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

                result = env.CompileExecuteFAF("select * from MyInfraMCMI where f1='E1'", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

                result = env.CompileExecuteFAF(
                    "select * from MyInfraMCMI where f3='>E1<' and f2=-2 and f1='E1' and f4='?E1?'",
                    path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

                env.UndeployAll();
            }
        }

        public class InfraLateCreate : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraLateCreate(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString", "IntPrimitive"};
                var path = new RegressionPath();

                // create infra
                var stmtTextCreate = namedWindow
                    ? "@Name('Create') create window MyInfra.win:keepall() as SupportBean"
                    : "@Name('Create') create table MyInfra(TheString string primary key, IntPrimitive int primary key)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("Create");

                // create insert into
                var stmtTextInsertOne =
                    "@Name('Insert') insert into MyInfra select TheString, IntPrimitive from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                env.SendEventBean(new SupportBean("A1", 1));
                env.SendEventBean(new SupportBean("B2", 2));
                env.SendEventBean(new SupportBean("B2", 1));

                // create index
                var stmtTextCreateIndex = "@Name('Index') create index MyInfra_IDX on MyInfra(TheString)";
                env.CompileDeploy(stmtTextCreateIndex, path);

                env.Milestone(0);

                // perform on-demand query
                var result = env.CompileExecuteFAF(
                    "select * from MyInfra where TheString = 'B2' order by IntPrimitive asc",
                    path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"B2", 1}, new object[] {"B2", 2}});

                // cleanup
                env.UndeployAll();

                env.Milestone(1);
            }
        }

        internal class InfraLateCreateSceneTwo : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraLateCreateSceneTwo(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextCreateOne = namedWindow
                    ? "create window MyInfraLC#keepall as (f1 string, f2 int, f3 string, f4 string)"
                    : "create table MyInfraLC as (f1 string primary key, f2 int primary key, f3 string primary key, f4 string primary key)";
                env.CompileDeploy(stmtTextCreateOne, path);
                env.CompileDeploy(
                    "insert into MyInfraLC(f1, f2, f3, f4) select TheString, IntPrimitive, '>'||TheString||'<', '?'||TheString||'?' from SupportBean",
                    path);

                env.SendEventBean(new SupportBean("E1", -4));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", -2));
                env.SendEventBean(new SupportBean("E1", -3));

                env.CompileDeploy("create index MyInfraLCIndex on MyInfraLC(f2, f3, f1)", path);
                var fields = "f1,f2,f3,f4".SplitCsv();

                env.Milestone(1);

                var result = env.CompileExecuteFAF("select * from MyInfraLC where f3='>E1<' order by f2 asc", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {
                        new object[] {"E1", -4, ">E1<", "?E1?"}, new object[] {"E1", -3, ">E1<", "?E1?"},
                        new object[] {"E1", -2, ">E1<", "?E1?"}
                    });

                env.UndeployAll();
            }
        }

        internal class InfraCompositeIndex : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraCompositeIndex(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextCreate = namedWindow
                    ? "create window MyInfraCI#keepall as (f1 string, f2 int, f3 string, f4 string)"
                    : "create table MyInfraCI as (f1 string primary key, f2 int, f3 string, f4 string)";
                env.CompileDeploy(stmtTextCreate, path);
                var compiledWindow = path.Compileds[0];
                env.CompileDeploy(
                    "insert into MyInfraCI(f1, f2, f3, f4) select TheString, IntPrimitive, '>'||TheString||'<', '?'||TheString||'?' from SupportBean",
                    path);
                env.CompileDeploy("@Name('indexOne') create index MyInfraCIIndex on MyInfraCI(f2, f3, f1)", path);
                var fields = "f1,f2,f3,f4".SplitCsv();

                env.SendEventBean(new SupportBean("E1", -2));

                var result = env.CompileExecuteFAF("select * from MyInfraCI where f3='>E1<'", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

                result = env.CompileExecuteFAF("select * from MyInfraCI where f3='>E1<' and f2=-2", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

                result = env.CompileExecuteFAF("select * from MyInfraCI where f3='>E1<' and f2=-2 and f1='E1'", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

                env.UndeployModuleContaining("indexOne");

                // test SODA
                path.Clear();
                path.Add(compiledWindow);
                env.EplToModelCompileDeploy("create index MyInfraCIIndexTwo on MyInfraCI(f2, f3, f1)", path)
                    .UndeployAll();
            }
        }

        internal class InfraWidening : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraWidening(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                // widen to long
                var stmtTextCreate = namedWindow
                    ? "create window MyInfraW#keepall as (f1 long, f2 string)"
                    : "create table MyInfraW as (f1 long primary key, f2 string primary key)";
                env.CompileDeploy(stmtTextCreate, path);
                env.CompileDeploy(
                    "insert into MyInfraW(f1, f2) select LongPrimitive, TheString from SupportBean",
                    path);
                env.CompileDeploy("create index MyInfraWIndex1 on MyInfraW(f1)", path);
                var fields = "f1,f2".SplitCsv();

                SendEventLong(env, "E1", 10L);

                var result = env.CompileExecuteFAF("select * from MyInfraW where f1=10", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {10L, "E1"}});

                // coerce to short
                stmtTextCreate = namedWindow
                    ? "create window MyInfraWTwo#keepall as (f1 short, f2 string)"
                    : "create table MyInfraWTwo as (f1 short primary key, f2 string primary key)";
                env.CompileDeploy(stmtTextCreate, path);
                env.CompileDeploy(
                    "insert into MyInfraWTwo(f1, f2) select ShortPrimitive, TheString from SupportBean",
                    path);
                env.CompileDeploy("create index MyInfraWTwoIndex1 on MyInfraWTwo(f1)", path);

                SendEventShort(env, "E1", 2);

                result = env.CompileExecuteFAF("select * from MyInfraWTwo where f1=2", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {(short) 2, "E1"}});

                env.UndeployAll();
            }
        }

        internal class InfraHashBTreeWidening : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraHashBTreeWidening(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                // widen to long
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? "create window MyInfraHBTW#keepall as (f1 long, f2 string)"
                    : "create table MyInfraHBTW as (f1 long primary key, f2 string primary key)";
                env.CompileDeploy(eplCreate, path);

                var eplInsert = "insert into MyInfraHBTW(f1, f2) select LongPrimitive, TheString from SupportBean";
                env.CompileDeploy(eplInsert, path);

                env.CompileDeploy("create index MyInfraHBTWIndex1 on MyInfraHBTW(f1 btree)", path);
                var fields = "f1,f2".SplitCsv();

                SendEventLong(env, "E1", 10L);
                var result = env.CompileExecuteFAF("select * from MyInfraHBTW where f1>9", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {10L, "E1"}});

                // SODA
                var epl = "create index IX1 on MyInfraHBTW(f1, f2 btree)";
                env.EplToModelCompileDeploy(epl, path);

                // SODA with unique
                var eplUnique = "create unique index IX2 on MyInfraHBTW(f1)";
                env.EplToModelCompileDeploy(eplUnique, path);

                // coerce to short
                var eplCreateTwo = namedWindow
                    ? "create window MyInfraHBTWTwo#keepall as (f1 short, f2 string)"
                    : "create table MyInfraHBTWTwo as (f1 short primary key, f2 string primary key)";
                env.CompileDeploy(eplCreateTwo, path);

                var eplInsertTwo =
                    "insert into MyInfraHBTWTwo(f1, f2) select ShortPrimitive, TheString from SupportBean";
                env.CompileDeploy(eplInsertTwo, path);
                env.CompileDeploy("create index MyInfraHBTWTwoIndex1 on MyInfraHBTWTwo(f1 btree)", path);

                SendEventShort(env, "E1", 2);

                result = env.CompileExecuteFAF("select * from MyInfraHBTWTwo where f1>=2", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {(short) 2, "E1"}});

                env.UndeployAll();
            }
        }

        internal class InfraMultiRangeAndKey : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraMultiRangeAndKey(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? "@Name('create') create window MyInfraMRAK#keepall as SupportBeanRange"
                    : "@Name('create') create table MyInfraMRAK(Id string primary key, key string, keyLong long, rangeStartLong long primary key, rangeEndLong long primary key)";
                env.CompileDeploy(eplCreate, path);

                var eplInsert = namedWindow
                    ? "insert into MyInfraMRAK select * from SupportBeanRange"
                    : "on SupportBeanRange t0 merge MyInfraMRAK t1 where t0.Id = t1.Id when not matched then insert select Id, key, keyLong, rangeStartLong, rangeEndLong";
                env.CompileDeploy(eplInsert, path);

                env.CompileDeploy(
                    "create index Idx1 on MyInfraMRAK(key hash, keyLong hash, rangeStartLong btree, rangeEndLong btree)",
                    path);
                var fields = "Id".SplitCsv();

                var query1 =
                    "select * from MyInfraMRAK where rangeStartLong > 1 and rangeEndLong > 2 and keyLong=1 and key='K1' order by Id asc";
                RunQueryAssertion(env, path, query1, fields, null);

                env.SendEventBean(SupportBeanRange.MakeLong("E1", "K1", 1L, 2L, 3L));
                RunQueryAssertion(
                    env,
                    path,
                    query1,
                    fields,
                    new[] {new object[] {"E1"}});

                env.SendEventBean(SupportBeanRange.MakeLong("E2", "K1", 1L, 2L, 4L));
                RunQueryAssertion(
                    env,
                    path,
                    query1,
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.Milestone(0);

                env.SendEventBean(SupportBeanRange.MakeLong("E3", "K1", 1L, 3L, 3L));
                RunQueryAssertion(
                    env,
                    path,
                    query1,
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                var query2 =
                    "select * from MyInfraMRAK where rangeStartLong > 1 and rangeEndLong > 2 and keyLong=1 order by Id asc";
                RunQueryAssertion(
                    env,
                    path,
                    query2,
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                Assert.AreEqual(namedWindow ? 1 : 2, GetIndexCount(env, namedWindow, "create", "MyInfraMRAK"));

                env.UndeployAll();
            }
        }

        public class InfraMultikeyIndexFAF : RegressionExecution
        {
            private readonly bool isNamedWindow;

            public InfraMultikeyIndexFAF(bool isNamedWindow)
            {
                this.isNamedWindow = isNamedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextCreate = isNamedWindow
                    ? "create window MyInfra.win:keepall() as (f1 string, f2 int, f3 string, f4 string)"
                    : "create table MyInfra as (f1 string primary key, f2 int, f3 string, f4 string)";
                env.CompileDeploy(stmtTextCreate, path);
                env.CompileDeploy(
                    "insert into MyInfra(f1, f2, f3, f4) select TheString, IntPrimitive, '>'||TheString||'<', '?'||TheString||'?' from SupportBean",
                    path);
                env.CompileDeploy("create index MyInfraIndex on MyInfra(f2, f3, f1)", path);
                var fields = "f1,f2,f3,f4".SplitCsv();

                env.SendEventBean(new SupportBean("E1", -2));

                env.Milestone(0);

                var result = env.CompileExecuteFAF("select * from MyInfra where f3='>E1<'", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

                env.Milestone(1);

                result = env.CompileExecuteFAF("select * from MyInfra where f3='>E1<' and f2=-2", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

                env.Milestone(2);

                result = env.CompileExecuteFAF("select * from MyInfra where f3='>E1<' and f2=-2 and f1='E1'", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace