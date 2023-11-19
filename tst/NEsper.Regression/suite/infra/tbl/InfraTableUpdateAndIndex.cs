///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    /// NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableUpdateAndIndex
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEarlyUniqueIndexViolation(execs);
            WithLateUniqueIndexViolation(execs);
            WithFAFUpdate(execs);
            WithTableKeyUpdateSingleKey(execs);
            WithTableKeyUpdateMultiKey(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTableKeyUpdateMultiKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableKeyUpdateMultiKey());
            return execs;
        }

        public static IList<RegressionExecution> WithTableKeyUpdateSingleKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableKeyUpdateSingleKey());
            return execs;
        }

        public static IList<RegressionExecution> WithFAFUpdate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFUpdate());
            return execs;
        }

        public static IList<RegressionExecution> WithLateUniqueIndexViolation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraLateUniqueIndexViolation());
            return execs;
        }

        public static IList<RegressionExecution> WithEarlyUniqueIndexViolation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraEarlyUniqueIndexViolation());
            return execs;
        }

        private class InfraEarlyUniqueIndexViolation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('create') @public create table MyTableEUIV as (pkey0 string primary key, pkey1 int primary key, thecnt count(*))",
                    path);
                env.CompileDeploy(
                    "into table MyTableEUIV select count(*) as thecnt from SupportBean group by TheString, IntPrimitive",
                    path);
                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E1", 20));

                // invalid index being created
                try {
                    var compiled = env.Compile("create unique index SecIndex on MyTableEUIV(pkey0)", path);
                    env.Runtime.DeploymentService.Deploy(compiled);
                    Assert.Fail();
                }
                catch (EPDeployException ex) {
                    SupportMessageAssertUtil.AssertMessage(
                        ex.Message,
                        "Failed to deploy: Unique index violation, index 'SecIndex' is a unique index and key 'E1' already exists");
                }

                // try fire-and-forget update of primary key to non-unique value
                try {
                    env.CompileExecuteFAF("update MyTableEUIV set pkey1 = 0", path);
                    Assert.Fail();
                }
                catch (EPException ex) {
                    SupportMessageAssertUtil.AssertMessage(
                        ex,
                        "Unique index violation, index 'MyTableEUIV' is a unique index and key 'MultiKey[E1,0]' already exists");
                    // assert events are unchanged - no update actually performed
                    env.AssertPropsPerRowIteratorAnyOrder(
                        "create",
                        "pkey0,pkey1".SplitCsv(),
                        new object[][] { new object[] { "E1", 10 }, new object[] { "E1", 20 } });
                }

                // try on-update unique index violation
                env.CompileDeploy("@name('on-update') on SupportBean_S1 update MyTableEUIV set pkey1 = 0", path);
                try {
                    env.SendEventBean(new SupportBean_S1(0));
                    Assert.Fail();
                }
                catch (EPException ex) {
                    SupportMessageAssertUtil.AssertMessage(
                        ex.InnerException,
                        "Unexpected exception in statement 'on-update': Unique index violation, index 'MyTableEUIV' is a unique index and key 'MultiKey[E1,0]' already exists");
                    // assert events are unchanged - no update actually performed
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        env.Statement("create").GetEnumerator(),
                        "pkey0,pkey1".SplitCsv(),
                        new object[][] { new object[] { "E1", 10 }, new object[] { "E1", 20 } });
                }

                // disallow on-merge unique key updates
                try {
                    env.CompileWCheckedEx(
                        "@name('on-merge') on SupportBean_S1 merge MyTableEUIV when matched then update set pkey1 = 0",
                        path);
                    Assert.Fail();
                }
                catch (EPCompileException ex) {
                    SupportMessageAssertUtil.AssertMessage(
                        ex.InnerException,
                        "Validation failed in when-matched (clause 1): On-merge statements may not update unique keys of tables");
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class InfraLateUniqueIndexViolation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('create') @public create table MyTableLUIV as (" +
                    "pkey0 string primary key, " +
                    "pkey1 int primary key, " +
                    "col0 int, " +
                    "thecnt count(*))",
                    path);

                env.CompileDeploy(
                    "into table MyTableLUIV select count(*) as thecnt from SupportBean group by TheString, IntPrimitive",
                    path);
                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));

                // On-merge exists before creating a unique index
                env.CompileDeploy(
                    "@name('on-merge') on SupportBean_S1 merge MyTableLUIV " +
                    "when matched then update set col0 = 0",
                    path);
                try {
                    var compiled = env.Compile("create unique index MyUniqueSecondary on MyTableLUIV (col0)", path);
                    path.Compileds.Remove(compiled);
                    env.Runtime.DeploymentService.Deploy(compiled);
                    Assert.Fail();
                }
                catch (EPDeployException ex) {
                    SupportMessageAssertUtil.AssertMessage(
                        ex,
                        "Failed to deploy: Create-index adds a unique key on columns that are updated by one or more on-merge statements");
                }

                env.UndeployModuleContaining("on-merge");

                // on-update exists before creating a unique index
                env.CompileDeploy("@name('on-update') on SupportBean_S1 update MyTableLUIV set pkey1 = 0", path);
                env.CompileDeploy("create unique index MyUniqueSecondary on MyTableLUIV (pkey1)", path);
                try {
                    env.SendEventBean(new SupportBean_S1(0));
                    Assert.Fail();
                }
                catch (EPException ex) {
                    SupportMessageAssertUtil.AssertMessage(
                        ex.InnerException,
                        "Unexpected exception in statement 'on-update': Unique index violation, index 'MyUniqueSecondary' is a unique index and key '0' already exists");
                    // assert events are unchanged - no update actually performed
                    env.AssertPropsPerRowIteratorAnyOrder(
                        "create",
                        "pkey0,pkey1".SplitCsv(),
                        new object[][] { new object[] { "E1", 10 }, new object[] { "E2", 20 } });
                }

                // unregister
                env.UndeployModuleContaining("on-update");
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class InfraFAFUpdate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create table MyTableFAFU as (pkey0 string primary key, col0 int, col1 int, thecnt count(*))",
                    path);
                env.CompileDeploy("create index MyIndex on MyTableFAFU(col0)", path);

                env.CompileDeploy(
                    "into table MyTableFAFU select count(*) as thecnt from SupportBean group by TheString",
                    path);
                env.SendEventBean(new SupportBean("E1", 0));
                env.SendEventBean(new SupportBean("E2", 0));

                env.CompileExecuteFAF("update MyTableFAFU set col0 = 1 where pkey0='E1'", path);
                env.CompileExecuteFAF("update MyTableFAFU set col0 = 2 where pkey0='E2'", path);
                AssertFAFOneRowResult(
                    env,
                    path,
                    "select pkey0 from MyTableFAFU where col0=1",
                    "pkey0",
                    new object[] { "E1" });

                env.CompileExecuteFAF("update MyTableFAFU set col1 = 100 where pkey0='E1'", path);
                AssertFAFOneRowResult(
                    env,
                    path,
                    "select pkey0 from MyTableFAFU where col1=100",
                    "pkey0",
                    new object[] { "E1" });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraTableKeyUpdateMultiKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "pkey0,pkey1,c0".SplitCsv();
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('s1') @public create table MyTableMultiKey(pkey0 string primary key, pkey1 int primary key, c0 long)",
                    path);
                env.CompileDeploy(
                    "insert into MyTableMultiKey select TheString as pkey0, IntPrimitive as pkey1, LongPrimitive as c0 from SupportBean",
                    path);
                env.CompileDeploy("on SupportBean_S0 update MyTableMultiKey set pkey0 = P01 where pkey0 = P00", path);

                SendSupportBean(env, "E1", 10, 100);
                SendSupportBean(env, "E2", 20, 200);
                SendSupportBean(env, "E3", 30, 300);

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0, "E2", "E20"));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s1",
                    fields,
                    new object[][] {
                        new object[] { "E1", 10, 100L }, new object[] { "E20", 20, 200L },
                        new object[] { "E3", 30, 300L }
                    });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(0, "E1", "E10"));

                env.Milestone(2);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s1",
                    fields,
                    new object[][] {
                        new object[] { "E10", 10, 100L }, new object[] { "E20", 20, 200L },
                        new object[] { "E3", 30, 300L }
                    });

                env.SendEventBean(new SupportBean_S0(0, "E3", "E30"));

                env.Milestone(3);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s1",
                    fields,
                    new object[][] {
                        new object[] { "E10", 10, 100L }, new object[] { "E20", 20, 200L },
                        new object[] { "E30", 30, 300L }
                    });

                env.UndeployAll();
            }
        }

        private class InfraTableKeyUpdateSingleKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "pkey0,c0".SplitCsv();
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('s0') @public create table MyTableSingleKey(pkey0 string primary key, c0 int)",
                    path);
                env.CompileDeploy(
                    "insert into MyTableSingleKey select TheString as pkey0, IntPrimitive as c0 from SupportBean",
                    path);
                env.CompileDeploy("on SupportBean_S0 update MyTableSingleKey set pkey0 = P01 where pkey0 = P00", path);

                SendSupportBean(env, "E1", 10);
                SendSupportBean(env, "E2", 20);
                SendSupportBean(env, "E3", 30);

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0, "E2", "E20"));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E1", 10 }, new object[] { "E20", 20 }, new object[] { "E3", 30 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(0, "E1", "E10"));

                env.Milestone(2);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E10", 10 }, new object[] { "E20", 20 }, new object[] { "E3", 30 } });

                env.SendEventBean(new SupportBean_S0(0, "E3", "E30"));

                env.Milestone(3);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E10", 10 }, new object[] { "E20", 20 }, new object[] { "E30", 30 } });

                env.UndeployAll();
            }
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

        private static void SendSupportBean(
            RegressionEnvironment env,
            string @string,
            int intPrimitive)
        {
            var bean = new SupportBean(@string, intPrimitive);
            env.SendEventBean(bean);
        }

        private static void AssertFAFOneRowResult(
            RegressionEnvironment env,
            RegressionPath path,
            string epl,
            string fields,
            object[] objects)
        {
            var result = env.CompileExecuteFAF(epl, path);
            Assert.AreEqual(1, result.Array.Length);
            EPAssertionUtil.AssertProps(result.Array[0], fields.SplitCsv(), objects);
        }
    }
} // end of namespace