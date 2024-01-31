///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    /// NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableFAFExecuteQuery : IndexBackingTableInfo
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInsert(execs);
            WithDelete(execs);
            WithUpdate(execs);
            WithSelect(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithUpdate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFUpdate());
            return execs;
        }

        public static IList<RegressionExecution> WithDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithInsert(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFAFInsert());
            return execs;
        }

        private class InfraFAFInsert : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var propertyNames = "p0,p1".SplitCsv();
                env.CompileDeploy("@name('create') @public create table MyTableINS as (p0 string, p1 int)", path);

                var eplInsertInto = "insert into MyTableINS (p0, p1) select 'a', 1";
                var resultOne = env.CompileExecuteFAF(eplInsertInto, path);
                AssertFAFInsertResult(resultOne, propertyNames, env.Statement("create"));
                env.AssertPropsPerRowIterator("create", propertyNames, new object[][] { new object[] { "a", 1 } });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('create') @public create table MyTableDEL as (p0 string primary key, thesum sum(int))",
                    path);
                env.CompileDeploy(
                    "into table MyTableDEL select TheString, sum(IntPrimitive) as thesum from SupportBean group by TheString",
                    path);
                for (var i = 0; i < 10; i++) {
                    env.SendEventBean(new SupportBean("G" + i, i));
                }

                ClassicAssert.AreEqual(10L, GetTableCount(env.Statement("create")));
                env.CompileExecuteFAF("delete from MyTableDEL", path);
                ClassicAssert.AreEqual(0L, GetTableCount(env.Statement("create")));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFUpdate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fields = "p0,p1".SplitCsv();
                env.CompileDeploy(
                    "@name('TheTable') @public create table MyTableUPD as (p0 string primary key, p1 string, thesum sum(int))",
                    path);
                env.CompileDeploy(
                    "into table MyTableUPD select TheString, sum(IntPrimitive) as thesum from SupportBean group by TheString",
                    path);
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.CompileExecuteFAF("update MyTableUPD set p1 = 'ABC'", path);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("TheTable").GetEnumerator(),
                    fields,
                    new object[][] { new object[] { "E1", "ABC" }, new object[] { "E2", "ABC" } });
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraFAFSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fields = "p0".SplitCsv();
                env.CompileDeploy(
                    "@name('TheTable') @public create table MyTableSEL as (p0 string primary key, thesum sum(int))",
                    path);
                env.CompileDeploy(
                    "into table MyTableSEL select TheString, sum(IntPrimitive) as thesum from SupportBean group by TheString",
                    path);
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                var result = env.CompileExecuteFAF("select * from MyTableSEL", path);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    result.Array,
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private static long GetTableCount(EPStatement stmt)
        {
            return EPAssertionUtil.EnumeratorCount(stmt.GetEnumerator());
        }

        private static void AssertFAFInsertResult(
            EPFireAndForgetQueryResult resultOne,
            string[] propertyNames,
            EPStatement stmt)
        {
            ClassicAssert.AreEqual(0, resultOne.Array.Length);
            ClassicAssert.AreSame(resultOne.EventType, stmt.EventType);
        }
    }
} // end of namespace