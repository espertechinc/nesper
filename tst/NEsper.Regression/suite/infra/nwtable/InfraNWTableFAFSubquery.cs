///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil; // tryInvalidFAFCompile
using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableFAFSubquery : IndexBackingTableInfo
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSimple(execs);
            WithSimpleJoin(execs);
            WithInsert(execs);
            WithUpdateUncorrelated(execs);
            WithDeleteUncorrelated(execs);
            WithSelectCorrelated(execs);
            WithUpdateCorrelatedSet(execs);
            WithUpdateCorrelatedWhere(execs);
            WithDeleteCorrelatedWhere(execs);
            WithContextBothWindows(execs);
            WithContextSelect(execs);
            WithSelectWhere(execs);
            WithSelectGroupBy(execs);
            WithSelectIndexPerfWSubstitution(execs);
            WithSelectIndexPerfCorrelated(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubqueryInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectIndexPerfCorrelated(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubquerySelectIndexPerfCorrelated(true));
            execs.Add(new InfraFAFSubquerySelectIndexPerfCorrelated(false));
            return execs;
        }

        public static IList<RegressionExecution> WithSelectIndexPerfWSubstitution(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubquerySelectIndexPerfWSubstitution(true));
            execs.Add(new InfraFAFSubquerySelectIndexPerfWSubstitution(false));
            return execs;
        }

        public static IList<RegressionExecution> WithSelectGroupBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubquerySelectGroupBy());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubquerySelectWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithContextSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubqueryContextSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithContextBothWindows(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubqueryContextBothWindows());
            return execs;
        }

        public static IList<RegressionExecution> WithDeleteCorrelatedWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubqueryDeleteCorrelatedWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateCorrelatedWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubqueryUpdateCorrelatedWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateCorrelatedSet(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubqueryUpdateCorrelatedSet());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectCorrelated(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubquerySelectCorrelated());
            return execs;
        }

        public static IList<RegressionExecution> WithDeleteUncorrelated(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubqueryDeleteUncorrelated());
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateUncorrelated(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubqueryUpdateUncorrelated());
            return execs;
        }

        public static IList<RegressionExecution> WithInsert(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubqueryInsert(true));
            execs.Add(new InfraFAFSubqueryInsert(false));
            return execs;
        }

        public static IList<RegressionExecution> WithSimpleJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubquerySimpleJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSubquerySimple(true));
            execs.Add(new InfraFAFSubquerySimple(false));
            return execs;
        }

        public class InfraFAFSubqueryInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@public create window WinSB#keepall as SupportBean;\n" +
                          "@public create context MyContext partition by Id from SupportBean_S0;\n" +
                          "@public context MyContext create window PartitionedWinS0#keepall as SupportBean_S0;\n";
                env.Compile(epl, path);

                TryInvalidFAFCompile(
                    env,
                    path,
                    "select (select * from SupportBean#lastevent) from WinSB",
                    "Fire-and-forget queries only allow subqueries against named windows and tables");

                TryInvalidFAFCompile(
                    env,
                    path,
                    "select (select * from WinSB(TheString='x')) from WinSB",
                    "Failed to plan subquery number 1 querying WinSB: Subqueries in fire-and-forget queries do not allow filter expressions");

                TryInvalidFAFCompile(
                    env,
                    path,
                    "select (select * from PartitionedWinS0) from WinSB",
                    "Failed to plan subquery number 1 querying PartitionedWinS0: Mismatch in context specification, the context for the named window 'PartitionedWinS0' is 'MyContext' and the query specifies no context");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET, RegressionFlag.INVALIDITY);
            }
        }

        private class InfraFAFSubquerySelectIndexPerfCorrelated : RegressionExecution
        {
            private bool namedWindow;

            public InfraFAFSubquerySelectIndexPerfCorrelated(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create window WinSB#keepall as SupportBean;\n" +
                    "insert into WinSB select * from SupportBean;\n";
                if (namedWindow) {
                    epl += "@public create window Infra#unique(Id) as (Id int, Value string);\n";
                }
                else {
                    epl += "@public create table Infra(Id int primary key, Value string);\n";
                }

                epl += "@public create index InfraIndex on Infra(Value);\n" +
                       "insert into Infra select Id, P00 as Value from SupportBean_S0;\n";
                env.CompileDeploy(epl, path);

                var numRows = 10000; // less than 1M
                for (var i = 0; i < numRows; i++) {
                    SendSB(env, "v" + i, 0);
                    SendS0(env, -1 * i, "v" + i);
                }

                var start = PerformanceObserver.MilliTime;
                var query = "select (select Id from Infra as i where i.Value = wsb.TheString) as c0 from WinSB as wsb";
                var result = CompileExecute(env, path, query);
                var delta = PerformanceObserver.MilliTime - start;
                Assert.That(delta, Is.LessThan(1000), "delta is " + delta);
                Assert.AreEqual(numRows, result.Array.Length);
                for (var i = 0; i < numRows; i++) {
                    Assert.AreEqual(-1 * i, result.Array[i].Get("c0"));
                }

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubquerySelectIndexPerfWSubstitution : RegressionExecution
        {
            private bool namedWindow;

            public InfraFAFSubquerySelectIndexPerfWSubstitution(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create window WinSB#lastevent as SupportBean;\n" +
                    "insert into WinSB select * from SupportBean;\n";
                if (namedWindow) {
                    epl += "@public create window Infra#unique(Id) as (Id int, Value string);\n";
                }
                else {
                    epl += "@public create table Infra(Id int primary key, Value string);\n";
                }

                epl += "@public create index InfraIndex on Infra(Value);\n" +
                       "insert into Infra select Id, P00 as Value from SupportBean_S0;\n";
                env.CompileDeploy(epl, path);

                SendSB(env, "E1", -1);
                for (var i = 0; i < 10000; i++) {
                    SendS0(env, i, "v" + i);
                }

                var query = "select (select Id from Infra as i where i.Value = ?:p0:string) as c0 from WinSB";
                var compiled = env.CompileFAF(query, path);
                var prepared = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);

                var start = PerformanceObserver.MilliTime;
                for (var i = 5000; i < 6000; i++) {
                    prepared.SetObject("p0", "v" + i);
                    var result = env.Runtime.FireAndForgetService.ExecuteQuery(prepared);
                    Assert.AreEqual(1, result.Array.Length);
                    Assert.AreEqual(i, result.Array[0].Get("c0"));
                }

                var delta = PerformanceObserver.MilliTime - start;
                Assert.That(delta, Is.LessThan(1000), "delta is " + delta);

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubquerySelectWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create window WinS0#keepall as SupportBean_S0;\n" +
                    "@public create window WinSB#keepall as SupportBean;\n" +
                    "insert into WinS0 select * from SupportBean_S0;\n" +
                    "insert into WinSB select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                var query = "select (select IntPrimitive from WinSB where TheString = 'x') as c0 from WinS0";
                SendS0(env, 0, null);
                AssertQuerySingle(env, path, query, null);

                SendSB(env, "E1", 1);
                AssertQuerySingle(env, path, query, null);

                SendSB(env, "x", 2);
                AssertQuerySingle(env, path, query, 2);

                SendSB(env, "x", 3);
                AssertQuerySingle(env, path, query, null);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubquerySelectGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create window WinS0#keepall as SupportBean_S0;\n" +
                    "@public create window WinSB#keepall as SupportBean;\n" +
                    "insert into WinS0 select * from SupportBean_S0;\n" +
                    "insert into WinSB select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                var query =
                    "select (select TheString, sum(IntPrimitive) as thesum from WinSB group by TheString) as c0 from WinS0";
                SendS0(env, 0, null);

                SendSB(env, "E1", 10);
                SendSB(env, "E1", 11);
                var result = (IDictionary<string, object>)RunQuerySingle(env, path, query);
                Assert.AreEqual("E1", result.Get("TheString"));
                Assert.AreEqual(21, result.Get("thesum"));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubqueryContextSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create context MyContext partition by Id from SupportBean_S0;\n" +
                    "@public context MyContext create window WinS0#keepall as SupportBean_S0;\n" +
                    "context MyContext on SupportBean_S0 as s0 merge WinS0 insert select *;\n" +
                    "@public create window WinSB#lastevent as SupportBean;\n" +
                    "insert into WinSB select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                SendS0(env, 1, "a");
                SendS0(env, 2, "b");
                SendSB(env, "E1", 1);

                var query = "context MyContext select P00, (select TheString from WinSB) as TheString from WinS0";
                AssertQueryMultirowAnyOrder(
                    env,
                    path,
                    query,
                    "P00,TheString",
                    new object[][] { new object[] { "a", "E1" }, new object[] { "b", "E1" } });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubqueryContextBothWindows : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create context MyContext partition by Id from SupportBean_S0, Id from SupportBean_S1;\n" +
                    "@public context MyContext create window WinS0#keepall as SupportBean_S0;\n" +
                    "@public context MyContext create window WinS1#keepall as SupportBean_S1;\n" +
                    "context MyContext on SupportBean_S0 as s0 merge WinS0 insert select *;\n" +
                    "context MyContext on SupportBean_S1 as s1 merge WinS1 insert select *;\n";
                env.CompileDeploy(epl, path);

                SendS0(env, 1, "a");
                SendS0(env, 2, "b");
                SendS0(env, 3, "c");
                SendS1(env, 1, "X");
                SendS1(env, 2, "Y");
                SendS1(env, 3, "Z");

                var query = "context MyContext select P00, (select P10 from WinS1) as P10 from WinS0";
                AssertQueryMultirowAnyOrder(
                    env,
                    path,
                    query,
                    "P00,P10",
                    new object[][] { new object[] { "a", "X" }, new object[] { "b", "Y" }, new object[] { "c", "Z" } });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubqueryDeleteCorrelatedWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create window WinS0#keepall as SupportBean_S0;\n" +
                    "@public create window WinSB#unique(IntPrimitive) as SupportBean;\n" +
                    "insert into WinS0 select * from SupportBean_S0;\n" +
                    "insert into WinSB select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                SendS0(env, 1, "a");
                SendS0(env, 2, "b");
                SendS0(env, 3, "c");

                SendSB(env, "a", 0);
                SendSB(env, "b", 2);

                var update = "delete from WinS0 as wins0 where Id = (select IntPrimitive from WinSB winsb where winsb.TheString = wins0.P00)";
                CompileExecute(env, path, update);

                var query = "select * from WinS0";
                AssertQueryMultirowAnyOrder(
                    env,
                    path,
                    query,
                    "Id,P00",
                    new object[][] { new object[] { 1, "a" }, new object[] { 3, "c" } });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubqueryUpdateCorrelatedWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create window WinS0#keepall as SupportBean_S0;\n" +
                    "@public create window WinSB#unique(IntPrimitive) as SupportBean;\n" +
                    "insert into WinS0 select * from SupportBean_S0;\n" +
                    "insert into WinSB select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                SendS0(env, 1, "a");
                SendS0(env, 2, "b");
                SendS0(env, 3, "c");

                SendSB(env, "a", 0);
                SendSB(env, "b", 2);

                var update =
                    "update WinS0 as wins0 set P00 = 'x' where Id = (select IntPrimitive from WinSB winsb where winsb.TheString = wins0.P00)";
                CompileExecute(env, path, update);

                var query = "select * from WinS0";
                AssertQueryMultirowAnyOrder(
                    env,
                    path,
                    query,
                    "Id,P00",
                    new object[][] { new object[] { 1, "a" }, new object[] { 2, "x" }, new object[] { 3, "c" } });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubqueryUpdateCorrelatedSet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create window WinS0#keepall as SupportBean_S0;\n" +
                    "@public create window WinSB#unique(IntPrimitive) as SupportBean;\n" +
                    "insert into WinS0 select * from SupportBean_S0;\n" +
                    "insert into WinSB select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                SendS0(env, 1, "a");
                SendS0(env, 2, "b");
                SendS0(env, 3, "c");

                SendSB(env, "X", 2);
                SendSB(env, "Y", 1);
                SendSB(env, "Z", 3);

                var update =
                    "update WinS0 as wins0 set P00 = (select TheString from WinSB winsb where winsb.IntPrimitive = wins0.Id)";
                CompileExecute(env, path, update);

                var query = "select * from WinS0";
                AssertQueryMultirowAnyOrder(
                    env,
                    path,
                    query,
                    "Id,P00",
                    new object[][] { new object[] { 1, "Y" }, new object[] { 2, "X" }, new object[] { 3, "Z" } });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubquerySelectCorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create window WinS0#keepall as SupportBean_S0;\n" +
                    "@public create window WinSB#unique(IntPrimitive) as SupportBean;\n" +
                    "insert into WinS0 select * from SupportBean_S0;\n" +
                    "insert into WinSB select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                SendS0(env, 1, "a");
                SendS0(env, 2, "b");
                SendS0(env, 3, "c");

                SendSB(env, "X", 2);
                SendSB(env, "Y", 1);
                SendSB(env, "Z", 3);

                var query =
                    "select Id, (select TheString from WinSB winsb where winsb.IntPrimitive = wins0.Id) as TheString from WinS0 as wins0";
                AssertQueryMultirowAnyOrder(
                    env,
                    path,
                    query,
                    "Id,TheString",
                    new object[][] { new object[] { 1, "Y" }, new object[] { 2, "X" }, new object[] { 3, "Z" } });

                SendSB(env, "Q", 1);
                SendSB(env, "R", 3);
                SendSB(env, "S", 2);
                AssertQueryMultirowAnyOrder(
                    env,
                    path,
                    query,
                    "Id,TheString",
                    new object[][] { new object[] { 1, "Q" }, new object[] { 2, "S" }, new object[] { 3, "R" } });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubqueryDeleteUncorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create window Win#keepall as (Key string, Value int);\n" +
                    "@public create window WinSB#lastevent as SupportBean;\n" +
                    "insert into WinSB select * from SupportBean;\n";
                env.CompileDeploy(epl, path);
                CompileExecute(env, path, "insert into Win select 'k1' as Key, 1 as Value");
                CompileExecute(env, path, "insert into Win select 'k2' as Key, 2 as Value");
                CompileExecute(env, path, "insert into Win select 'k3' as Key, 3 as Value");

                var delete = "delete from Win where Value = (select IntPrimitive from WinSB)";
                var query = "select * from Win";

                AssertQueryMultirowAnyOrder(
                    env,
                    path,
                    query,
                    "Key,Value",
                    new object[][] { new object[] { "k1", 1 }, new object[] { "k2", 2 }, new object[] { "k3", 3 } });

                CompileExecute(env, path, delete);
                AssertQueryMultirowAnyOrder(
                    env,
                    path,
                    query,
                    "Key,Value",
                    new object[][] { new object[] { "k1", 1 }, new object[] { "k2", 2 }, new object[] { "k3", 3 } });

                SendSB(env, "E1", 2);
                CompileExecute(env, path, delete);
                AssertQueryMultirowAnyOrder(
                    env,
                    path,
                    query,
                    "Key,Value",
                    new object[][] { new object[] { "k1", 1 }, new object[] { "k3", 3 } });

                SendSB(env, "E1", 1);
                CompileExecute(env, path, delete);
                AssertQueryMultirowAnyOrder(env, path, query, "Key,Value", new object[][] { new object[] { "k3", 3 } });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubqueryUpdateUncorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create window Win#lastevent as (Value int);\n" +
                    "@public create window WinSB#lastevent as SupportBean;\n" +
                    "insert into WinSB select * from SupportBean;\n";
                env.CompileDeploy(epl, path);
                CompileExecute(env, path, "insert into Win select 1 as Value");

                var update = "update Win set Value = (select IntPrimitive from WinSB)";
                var query = "select Value as c0 from Win";

                AssertQuerySingle(env, path, query, 1);

                CompileExecute(env, path, update);
                AssertQuerySingle(env, path, query, null);

                SendSB(env, "E1", 10);
                CompileExecute(env, path, update);
                AssertQuerySingle(env, path, query, 10);

                SendSB(env, "E2", 20);
                CompileExecute(env, path, update);
                AssertQuerySingle(env, path, query, 20);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubqueryInsert : RegressionExecution
        {
            private bool namedWindow;

            public InfraFAFSubqueryInsert(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@public create window Win#keepall as (Value string);\n";
                if (namedWindow) {
                    epl +=
                        "@public create window InfraSB#lastevent as SupportBean;\n" +
                        "insert into InfraSB select * from SupportBean;\n";
                }
                else {
                    epl +=
                        "@public create table InfraSB(TheString string);\n" +
                        "on SupportBean as sb merge InfraSB as issb" +
                        "  when not matched then insert select TheString when matched then update set issb.TheString=sb.TheString;\n";
                }

                env.CompileDeploy(epl, path);

                var insert = "insert into Win(Value) select (select TheString from InfraSB)";
                var query = "select * from Win";

                CompileExecute(env, path, insert);
                AssertQueryMultirowAnyOrder(env, path, query, "Value", new object[][] { new object[] { null } });

                SendSB(env, "E1", 0);
                CompileExecute(env, path, insert);
                AssertQueryMultirowAnyOrder(
                    env,
                    path,
                    query,
                    "Value",
                    new object[][] { new object[] { null }, new object[] { "E1" } });

                SendSB(env, "E2", 0);
                CompileExecute(env, path, insert);
                AssertQueryMultirowAnyOrder(
                    env,
                    path,
                    query,
                    "Value",
                    new object[][] { new object[] { null }, new object[] { "E1" }, new object[] { "E2" } });

                env.UndeployAll();
            }

            public string Name()
            {
                return $"{this.GetType().Name}{{namedWindow={namedWindow}}}";
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubquerySimpleJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create window WinSB#lastevent as SupportBean;\n" +
                    "@public create window WinS0#keepall as SupportBean_S0;\n" +
                    "@public create window WinS1#keepall as SupportBean_S1;\n" +
                    "insert into WinSB select * from SupportBean;\n" +
                    "insert into WinS0 select * from SupportBean_S0;\n" +
                    "insert into WinS1 select * from SupportBean_S1;\n";
                var query = "select (select TheString from WinSB) as c0, P00, P10 from WinS0, WinS1";
                env.CompileDeploy(epl, path);

                AssertQueryNoRows(env, path, query, typeof(string));

                SendS0(env, 1, "S0_0");
                SendS1(env, 2, "S1_0");
                AssertQuerySingle(env, path, query, null);

                SendSB(env, "SB_0", 0);
                AssertQuerySingle(env, path, query, "SB_0");

                SendS0(env, 3, "S0_1");
                AssertQueryMultirowAnyOrder(
                    env,
                    path,
                    query,
                    "c0,P00,P10",
                    new object[][] {
                        new object[] { "SB_0", "S0_0", "S1_0" },
                        new object[] { "SB_0", "S0_1", "S1_0" }
                    });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSubquerySimple : RegressionExecution
        {
            bool namedWindow;

            public InfraFAFSubquerySimple(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create window WinSB#lastevent as SupportBean;\n" +
                    "insert into WinSB select * from SupportBean;\n";
                if (namedWindow) {
                    epl +=
                        "@public create window InfraS0#lastevent as SupportBean_S0;\n" +
                        "insert into InfraS0 select * from SupportBean_S0;\n";
                }
                else {
                    epl +=
                        "@public create table InfraS0(Id int primary key, P00 string);\n" +
                        "on SupportBean_S0 as s0 merge InfraS0 as is0 where s0.Id = is0.Id" +
                        "  when not matched then insert select Id, P00 when matched then update set is0.P00=s0.P00;\n";
                }

                var query = "select (select P00 from InfraS0) as c0 from WinSB";
                env.CompileDeploy(epl, path);

                AssertQueryNoRows(env, path, query, typeof(string));

                SendSB(env, "E1", 1);
                AssertQuerySingle(env, path, query, null);

                SendS0(env, 1, "a");
                AssertQuerySingle(env, path, query, "a");

                SendS0(env, 1, "b");
                AssertQuerySingle(env, path, query, "b");

                env.UndeployAll();
            }

            public string Name()
            {
                return $"{this.GetType().Name}{{namedWindow={namedWindow}}}";
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private static void AssertQueryNoRows(
            RegressionEnvironment env,
            RegressionPath path,
            string query,
            Type resultType)
        {
            var compiled = env.CompileFAF(query, path);
            var result = env.Runtime.FireAndForgetService.ExecuteQuery(compiled);
            Assert.AreEqual(0, result.Array?.Length ?? 0);
            Assert.AreEqual(result.EventType.GetPropertyType("c0"), resultType);
        }

        private static void AssertQuerySingle(
            RegressionEnvironment env,
            RegressionPath path,
            string query,
            object c0Expected)
        {
            var result = RunQuerySingle(env, path, query);
            Assert.AreEqual(c0Expected, result);
        }

        private static object RunQuerySingle(
            RegressionEnvironment env,
            RegressionPath path,
            string query)
        {
            var result = CompileExecute(env, path, query);
            Assert.AreEqual(1, result.Array.Length);
            return result.Array[0].Get("c0");
        }

        private static void AssertQueryMultirowAnyOrder(
            RegressionEnvironment env,
            RegressionPath path,
            string query,
            string fieldCSV,
            object[][] expected)
        {
            var result = CompileExecute(env, path, query);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fieldCSV.SplitCsv(), expected);
        }

        private static EPFireAndForgetQueryResult CompileExecute(
            RegressionEnvironment env,
            RegressionPath path,
            string query)
        {
            var compiled = env.CompileFAF(query, path);
            return env.Runtime.FireAndForgetService.ExecuteQuery(compiled);
        }

        private static void SendS0(
            RegressionEnvironment env,
            int id,
            string p00)
        {
            env.SendEventBean(new SupportBean_S0(id, p00));
        }

        private static void SendS1(
            RegressionEnvironment env,
            int id,
            string p10)
        {
            env.SendEventBean(new SupportBean_S1(id, p10));
        }

        private static void SendSB(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }
    }
} // end of namespace