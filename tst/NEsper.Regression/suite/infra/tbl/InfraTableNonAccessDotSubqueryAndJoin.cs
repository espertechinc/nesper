///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    /// NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableNonAccessDotSubqueryAndJoin : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionUse(env, false);
            RunAssertionUse(env, true);
        }

        private static void RunAssertionUse(
            RegressionEnvironment env,
            bool soda)
        {
            var path = new RegressionPath();
            var eplCreate = "@public create table MyTable (" +
                            "col0 string, " +
                            "col1 sum(int), " +
                            "col2 sorted(IntPrimitive) @type('SupportBean'), " +
                            "col3 int[], " +
                            "col4 window(*) @type('SupportBean')" +
                            ")";
            env.CompileDeploy(soda, eplCreate, path);

            var eplIntoTable = "@name('into') into table MyTable select sum(IntPrimitive) as col1, sorted() as col2, " +
                               "window(*) as col4 from SupportBean#length(3)";
            env.CompileDeploy(soda, eplIntoTable, path);
            var sentSB = new SupportBean[2];
            sentSB[0] = MakeSendSupportBean(env, "E1", 20);
            sentSB[1] = MakeSendSupportBean(env, "E2", 21);
            env.UndeployModuleContaining("into");

            var eplMerge =
                "@name('merge') on SupportBean merge MyTable when matched then update set col3={1,2,4,2}, col0=\"x\"";
            env.CompileDeploy(soda, eplMerge, path);
            MakeSendSupportBean(env, null, -1);
            env.UndeployModuleContaining("merge");

            var eplSelect = "@name('s0') select " +
                            "col0 as c0_1, mt.col0 as c0_2, " +
                            "col1 as c1_1, mt.col1 as c1_2, " +
                            "col2 as c2_1, mt.col2 as c2_2, " +
                            "col2.minBy() as c2_3, mt.col2.maxBy() as c2_4, " +
                            "col2.sorted().firstOf() as c2_5, mt.col2.sorted().firstOf() as c2_6, " +
                            "col3.mostFrequent() as c3_1, mt.col3.mostFrequent() as c3_2, " +
                            "col4 as c4_1 " +
                            "from SupportBean unidirectional, MyTable as mt";
            env.CompileDeploy(soda, eplSelect, path).AddListener("s0");

            var expectedType = new object[][] {
                new object[] { "c0_1", typeof(string) }, new object[] { "c0_2", typeof(string) },
                new object[] { "c1_1", typeof(int?) }, new object[] { "c1_2", typeof(int?) },
                new object[] { "c2_1", typeof(SupportBean[]) }, new object[] { "c2_2", typeof(SupportBean[]) },
                new object[] { "c2_3", typeof(SupportBean) }, new object[] { "c2_4", typeof(SupportBean) },
                new object[] { "c2_5", typeof(SupportBean) }, new object[] { "c2_6", typeof(SupportBean) },
                new object[] { "c3_1", typeof(int?) }, new object[] { "c3_2", typeof(int?) },
                new object[] { "c4_1", typeof(SupportBean[]) }
            };
            env.AssertStatement(
                "s0",
                statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                    expectedType,
                    statement.EventType,
                    SupportEventTypeAssertionEnum.NAME,
                    SupportEventTypeAssertionEnum.TYPE));

            MakeSendSupportBean(env, null, -1);
            env.AssertEventNew(
                "s0",
                @event => {
                    EPAssertionUtil.AssertProps(
                        @event,
                        "c0_1,c0_2,c1_1,c1_2".SplitCsv(),
                        new object[] { "x", "x", 41, 41 });
                    EPAssertionUtil.AssertProps(@event, "c2_1,c2_2".SplitCsv(), new object[] { sentSB, sentSB });
                    EPAssertionUtil.AssertProps(@event, "c2_3,c2_4".SplitCsv(), new object[] { sentSB[0], sentSB[1] });
                    EPAssertionUtil.AssertProps(@event, "c2_5,c2_6".SplitCsv(), new object[] { sentSB[0], sentSB[0] });
                    EPAssertionUtil.AssertProps(@event, "c3_1,c3_2".SplitCsv(), new object[] { 2, 2 });
                    EPAssertionUtil.AssertProps(@event, "c4_1".SplitCsv(), new object[] { sentSB });
                });

            // unnamed column
            var eplSelectUnnamed = "@name('s1') select col2.sorted().firstOf(), mt.col2.sorted().firstOf()" +
                                   " from SupportBean unidirectional, MyTable mt";
            env.CompileDeploy(eplSelectUnnamed, path);
            var expectedTypeUnnamed = new object[][] {
                new object[] { "col2.sorted().firstOf()", typeof(SupportBean) },
                new object[] { "mt.col2.sorted().firstOf()", typeof(SupportBean) }
            };
            env.AssertStatement(
                "s1",
                statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                    expectedTypeUnnamed,
                    statement.EventType,
                    SupportEventTypeAssertionEnum.NAME,
                    SupportEventTypeAssertionEnum.TYPE));

            // invalid: ambiguous resolution
            env.TryInvalidCompile(
                path,
                "" +
                "select col0 from SupportBean#lastevent, MyTable, MyTable",
                "Failed to validate select-clause expression 'col0': Ambiguous table column 'col0' should be prefixed by a stream name [");

            env.UndeployAll();
        }

        private static SupportBean MakeSendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var b = new SupportBean(theString, intPrimitive);
            env.SendEventBean(b);
            return b;
        }
    }
} // end of namespace