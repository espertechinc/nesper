///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    /// NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableRollup
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithRollupOneDim(execs);
            WithRollupTwoDim(execs);
            WithGroupingSetThreeDim(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithGroupingSetThreeDim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraGroupingSetThreeDim());
            return execs;
        }

        public static IList<RegressionExecution> WithRollupTwoDim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraRollupTwoDim());
            return execs;
        }

        public static IList<RegressionExecution> WithRollupOneDim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraRollupOneDim());
            return execs;
        }

        private class InfraRollupOneDim : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldsOut = "TheString,Total".SplitCsv();
                var path = new RegressionPath();

                env.CompileDeploy("@public create table MyTableR1D(pk string primary key, Total sum(int))", path);
                env.CompileDeploy(
                        "@name('into') into table MyTableR1D insert into MyStreamOne select TheString, sum(IntPrimitive) as Total from SupportBean#length(4) group by rollup(TheString)",
                        path)
                    .AddListener("into");
                env.CompileDeploy("@name('s0') select MyTableR1D[P00].Total as c0 from SupportBean_S0", path)
                    .AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 10));
                AssertValuesListener(
                    env,
                    new object[][]
                        { new object[] { null, 10 }, new object[] { "E1", 10 }, new object[] { "E2", null } });
                env.AssertPropsPerRowLastNew(
                    "into",
                    fieldsOut,
                    new object[][] { new object[] { "E1", 10 }, new object[] { null, 10 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 200));
                AssertValuesListener(
                    env,
                    new object[][]
                        { new object[] { null, 210 }, new object[] { "E1", 10 }, new object[] { "E2", 200 } });
                env.AssertPropsPerRowLastNew(
                    "into",
                    fieldsOut,
                    new object[][] { new object[] { "E2", 200 }, new object[] { null, 210 } });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E1", 11));
                AssertValuesListener(
                    env,
                    new object[][]
                        { new object[] { null, 221 }, new object[] { "E1", 21 }, new object[] { "E2", 200 } });
                env.AssertPropsPerRowLastNew(
                    "into",
                    fieldsOut,
                    new object[][] { new object[] { "E1", 21 }, new object[] { null, 221 } });

                env.SendEventBean(new SupportBean("E2", 201));
                AssertValuesListener(
                    env,
                    new object[][]
                        { new object[] { null, 422 }, new object[] { "E1", 21 }, new object[] { "E2", 401 } });
                env.AssertPropsPerRowLastNew(
                    "into",
                    fieldsOut,
                    new object[][] { new object[] { "E2", 401 }, new object[] { null, 422 } });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E1", 12)); // {"E1", 10} leaving window
                AssertValuesListener(
                    env,
                    new object[][]
                        { new object[] { null, 424 }, new object[] { "E1", 23 }, new object[] { "E2", 401 } });
                env.AssertPropsPerRowLastNew(
                    "into",
                    fieldsOut,
                    new object[][] { new object[] { "E1", 23 }, new object[] { null, 424 } });

                env.UndeployAll();
            }
        }

        private class InfraRollupTwoDim : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "k0,k1,Total".SplitCsv();

                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public @buseventtype create objectarray schema MyEventTwo(k0 int, k1 int, col int)",
                    path);
                env.CompileDeploy(
                    "@public create table MyTableR2D(k0 int primary key, k1 int primary key, Total sum(int))",
                    path);
                env.CompileDeploy(
                    "into table MyTableR2D insert into MyStreamTwo select sum(col) as Total from MyEventTwo#length(3) group by rollup(k0,k1)",
                    path);

                env.SendEventObjectArray(new object[] { 1, 10, 100 }, "MyEventTwo");
                env.SendEventObjectArray(new object[] { 2, 10, 200 }, "MyEventTwo");
                env.SendEventObjectArray(new object[] { 1, 20, 300 }, "MyEventTwo");

                AssertValuesIterate(
                    env,
                    path,
                    "MyTableR2D",
                    fields,
                    new object[][] {
                        new object[] { null, null, 600 }, new object[] { 1, null, 400 }, new object[] { 2, null, 200 },
                        new object[] { 1, 10, 100 }, new object[] { 2, 10, 200 }, new object[] { 1, 20, 300 }
                    });

                env.Milestone(0);

                env.SendEventObjectArray(new object[] { 1, 10, 400 }, "MyEventTwo"); // expires {1, 10, 100}

                AssertValuesIterate(
                    env,
                    path,
                    "MyTableR2D",
                    fields,
                    new object[][] {
                        new object[] { null, null, 900 }, new object[] { 1, null, 700 }, new object[] { 2, null, 200 },
                        new object[] { 1, 10, 400 }, new object[] { 2, 10, 200 }, new object[] { 1, 20, 300 }
                    });

                env.UndeployAll();
            }
        }

        private class InfraGroupingSetThreeDim : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public @buseventtype create objectarray schema MyEventThree(k0 int, k1 int, k2 int, col int)",
                    path);

                env.CompileDeploy(
                    "@public create table MyTableGS3D(k0 int primary key, k1 int primary key, k2 int primary key, Total sum(int))",
                    path);
                env.CompileDeploy(
                    "into table MyTableGS3D insert into MyStreamThree select sum(col) as Total from MyEventThree#length(3) group by grouping sets(k0,k1,k2)",
                    path);

                var fields = "k0,k1,k2,Total".SplitCsv();
                env.SendEventObjectArray(new object[] { 1, 10, 100, 1000 }, "MyEventThree");
                env.SendEventObjectArray(new object[] { 2, 10, 200, 2000 }, "MyEventThree");

                env.Milestone(0);

                env.SendEventObjectArray(new object[] { 1, 20, 300, 3000 }, "MyEventThree");

                AssertValuesIterate(
                    env,
                    path,
                    "MyTableGS3D",
                    fields,
                    new object[][] {
                        new object[] { 1, null, null, 4000 }, new object[] { 2, null, null, 2000 },
                        new object[] { null, 10, null, 3000 }, new object[] { null, 20, null, 3000 },
                        new object[] { null, null, 100, 1000 }, new object[] { null, null, 200, 2000 },
                        new object[] { null, null, 300, 3000 }
                    });

                env.Milestone(1);

                env.SendEventObjectArray(
                    new object[] { 1, 10, 400, 4000 },
                    "MyEventThree"); // expires {1, 10, 100, 1000}

                env.Milestone(2);

                AssertValuesIterate(
                    env,
                    path,
                    "MyTableGS3D",
                    fields,
                    new object[][] {
                        new object[] { 1, null, null, 7000 }, new object[] { 2, null, null, 2000 },
                        new object[] { null, 10, null, 6000 }, new object[] { null, 20, null, 3000 },
                        new object[] { null, null, 100, null }, new object[] { null, null, 400, 4000 },
                        new object[] { null, null, 200, 2000 }, new object[] { null, null, 300, 3000 }
                    });

                env.UndeployAll();
            }
        }

        private static void AssertValuesIterate(
            RegressionEnvironment env,
            RegressionPath path,
            string name,
            string[] fields,
            object[][] objects)
        {
            env.AssertThat(
                () => {
                    var result = env.CompileExecuteFAF("select * from " + name, path);
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields, objects);
                });
        }

        private static void AssertValuesListener(
            RegressionEnvironment env,
            object[][] objects)
        {
            for (var i = 0; i < objects.Length; i++) {
                var p00 = (string)objects[i][0];
                var expected = (int?)objects[i][1];
                env.SendEventBean(new SupportBean_S0(0, p00));
                var index = i;
                env.AssertListener(
                    "s0",
                    listener => ClassicAssert.AreEqual(expected, listener.AssertOneGetNewAndReset().Get("c0")));
            }
        }
    }
} // end of namespace