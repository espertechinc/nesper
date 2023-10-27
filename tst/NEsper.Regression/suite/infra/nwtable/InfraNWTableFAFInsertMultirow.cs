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
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableFAFInsertMultirow : IndexBackingTableInfo
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            Withw(execs);
            WithwRollback(execs);
            WithwInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithwInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertMultirowInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithwRollback(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertMultirowRollback(true));
            execs.Add(new InfraInsertMultirowRollback(false));
            return execs;
        }

        public static IList<RegressionExecution> Withw(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInsertMultirow(true));
            execs.Add(new InfraInsertMultirow(false));
            return execs;
        }

        private class InfraInsertMultirowRollback : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraInsertMultirowRollback(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fields = "k,v".Split(",");
                var epl = "@name('infra') @public ";
                if (namedWindow) {
                    epl += "create window MyInfra#keepall as (k string, v int);\n" +
                           "create unique index Idx on MyInfra (k);\n";
                }
                else {
                    epl += "create table MyInfra(k string primary key, v int);\n";
                }

                env.CompileDeploy(epl, path);

                var query = "insert into MyInfra values ('a', 0), ('b', 10), ('b', 11), ('c', 20)";
                try {
                    env.CompileExecuteFAF(query, path);
                    Assert.Fail();
                }
                catch (EPException ex) {
                    var indexName = namedWindow ? "Idx" : "MyInfra";
                    var expected =
                        "Unique index violation, index 'IDXNAME' is a unique index and key 'b' already exists".Replace(
                            "IDXNAME",
                            indexName);
                    Assert.AreEqual(expected, ex.Message);
                }

                env.AssertPropsPerRowIterator("infra", fields, Array.Empty<object[]>());

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
        }

        private class InfraInsertMultirow : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraInsertMultirow(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fields = "k,v".Split(",");
                var epl = "@name('infra') @public ";
                if (namedWindow) {
                    epl += "create window MyInfra#keepall as (k string, v int);\n";
                }
                else {
                    epl += "create table MyInfra(k string primary key, v int);\n";
                }

                epl += "@public create window LastSupportBean#lastevent as SupportBean;\n" +
                       "on SupportBean merge LastSupportBean insert select *\n";
                env.CompileDeploy(epl, path);

                var query = "insert into MyInfra values ('a', 1), ('b', 2)";
                env.CompileExecuteFAF(query, path);
                env.AssertPropsPerRowIterator(
                    "infra",
                    fields,
                    new object[][] { new object[] { "a", 1 }, new object[] { "b", 2 } });

                // test SODA
                query = "insert into MyInfra values (\"c\", 3), (\"d\", 4)";
                var model = env.EplToModel(query);
                Assert.AreEqual(query, model.ToEPL());
                env.CompileExecuteFAF(model, path);
                env.AssertPropsPerRowIterator(
                    "infra",
                    fields,
                    new object[][] {
                        new object[] { "a", 1 }, new object[] { "b", 2 }, new object[] { "c", 3 },
                        new object[] { "d", 4 }
                    });

                // test subquery
                env.CompileExecuteFAF("delete from MyInfra", path);
                env.SendEventBean(new SupportBean("x", 50));
                env.CompileExecuteFAF(
                    "insert into MyInfra values ('a', 1), " +
                    "((select TheString from LastSupportBean), (select IntPrimitive from LastSupportBean))",
                    path);
                env.AssertPropsPerRowIterator(
                    "infra",
                    fields,
                    new object[][] { new object[] { "a", 1 }, new object[] { "x", 50 } });

                // test 1000 rows
                env.CompileExecuteFAF("delete from MyInfra", path);
                var queryAndResult = BuildQuery(1000);
                env.CompileExecuteFAF(queryAndResult.First, path);
                env.AssertPropsPerRowIteratorAnyOrder("infra", fields, queryAndResult.Second);

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
        }

        private class InfraInsertMultirowInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@name('window') @public create window MyInfra#keepall as (k string, v int);\n";
                env.CompileDeploy(epl, path);

                env.TryInvalidCompileFAF(
                    path,
                    "insert into MyInfra (k, v) values ('a', 1), ('b')",
                    "Failed to validate multi-row insert at row 2 of 2: Number of supplied values in the select or values clause does not match insert-into clause");

                var queryMaxRows = BuildQuery(1001).First;
                env.TryInvalidCompileFAF(
                    path,
                    queryMaxRows,
                    "Insert-into number-of-rows exceeds the maximum of 1000 rows as the query provides 1001 rows");

                env.UndeployAll();
            }
        }

        private static Pair<string, object[][]> BuildQuery(int size)
        {
            var buf = new StringBuilder();
            buf.Append("insert into MyInfra values ");
            var delimiter = "";
            var expected = new List<object[]>();
            for (var i = 0; i < size; i++) {
                buf.Append(delimiter).Append("('$1', $2)".Replace("$1", "E" + i).Replace("$2", i.ToString()));
                delimiter = ",";
                expected.Add(new object[] { "E" + i, i });
            }

            return new Pair<string, object[][]>(buf.ToString(), expected.ToArray());
        }
    }
} // end of namespace