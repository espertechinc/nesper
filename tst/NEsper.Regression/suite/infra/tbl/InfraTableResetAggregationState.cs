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
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    public class InfraTableResetAggregationState
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithRowSum(execs);
            WithRowSumWTableAlias(execs);
            WithSelective(execs);
            WithVariousAggs(execs);
            WithInvalid(execs);
            WithDocSample(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDocSample(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableResetDocSample());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableResetInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithVariousAggs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableResetVariousAggs());
            return execs;
        }

        public static IList<RegressionExecution> WithSelective(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableResetSelective());
            return execs;
        }

        public static IList<RegressionExecution> WithRowSumWTableAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableResetRowSumWTableAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithRowSum(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableResetRowSum());
            return execs;
        }

        private class InfraTableResetDocSample : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create table IntrusionCountTable (\n" +
                          "  fromAddress string primary key,\n" +
                          "  toAddress string primary key,\n" +
                          "  countIntrusion10Sec count(*),\n" +
                          "  countIntrusion60Sec count(*)\n" +
                          ");\n" +
                          "create schema IntrusionReset(fromAddress string, toAddress string);\n" +
                          "on IntrusionReset as resetEvent merge IntrusionCountTable as tableRow\n" +
                          "where resetEvent.fromAddress = tableRow.fromAddress and resetEvent.toAddress = tableRow.toAddress\n" +
                          "when matched then update set countIntrusion10Sec.reset(), countIntrusion60Sec.reset();\n" +
                          "" +
                          "on IntrusionReset as resetEvent merge IntrusionCountTable as tableRow\n" +
                          "where resetEvent.fromAddress = tableRow.fromAddress and resetEvent.toAddress = tableRow.toAddress\n" +
                          "when matched then update set tableRow.reset();\n";
                env.Compile(epl);
            }
        }

        private class InfraTableResetInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var prefix = "@name('table') create table MyTable(asum sum(int));\n";

                var invalidSelectAggReset = prefix +
                                            "on SupportBean_S0 merge MyTable when matched then insert into MyStream select asum.reset()";
                env.TryInvalidCompile(
                    invalidSelectAggReset,
                    "Failed to validate select-clause expression 'asum.reset()': The table aggregation'reset' method is only available for the on-merge update action");

                var invalidSelectRowReset = prefix +
                                            "on SupportBean_S0 merge MyTable as mt when matched then insert into MyStream select mt.reset()";
                env.TryInvalidCompile(
                    invalidSelectRowReset,
                    "Failed to validate select-clause expression 'mt.reset()'");

                var invalidAggResetWParams =
                    prefix + "on SupportBean_S0 merge MyTable as mt when matched then update set asum.reset(1)";
                env.TryInvalidCompile(
                    invalidAggResetWParams,
                    "Failed to validate update assignment expression 'asum.reset(1)': The table aggregation 'reset' method does not allow parameters");

                var invalidRowResetWParams =
                    prefix + "on SupportBean_S0 merge MyTable as mt when matched then update set mt.reset(1)";
                env.TryInvalidCompile(
                    invalidRowResetWParams,
                    "Failed to validate update assignment expression 'mt.reset(1)': The table aggregation 'reset' method does not allow parameters");
            }
        }

        private class InfraTableResetVariousAggs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('table') create table MyTable(" +
                          "  myAvedev avedev(int),\n" +
                          "  myCount count(*),\n" +
                          "  myCountDistinct count(distinct int),\n" +
                          "  myMax max(int),\n" +
                          "  myMedian median(int),\n" +
                          "  myStddev stddev(int),\n" +
                          "  myFirstEver firstever(string),\n" +
                          "  myCountEver countever(*)," +
                          "  myMaxByEver maxbyever(intPrimitive) @type(SupportBean)," +
                          "  myPluginAggSingle myaggsingle(*)," +
                          "  myPluginAggAccess referenceCountedMap(string)," +
                          "  myWordcms countMinSketch()" +
                          ");\n" +
                          "into table MyTable select" +
                          "  avedev(intPrimitive) as myAvedev," +
                          "  count(*) as myCount," +
                          "  count(distinct intPrimitive) as myCountDistinct," +
                          "  max(intPrimitive) as myMax," +
                          "  median(intPrimitive) as myMedian," +
                          "  stddev(intPrimitive) as myStddev," +
                          "  firstever(theString) as myFirstEver," +
                          "  countever(*) as myCountEver," +
                          "  maxbyever(*) as myMaxByEver," +
                          "  myaggsingle(*) as myPluginAggSingle," +
                          "  referenceCountedMap(theString) as myPluginAggAccess," +
                          "  countMinSketchAdd(theString) as myWordcms" +
                          "   " +
                          "from SupportBean#keepall;\n" +
                          "on SupportBean_S0 merge MyTable mt when matched then update set mt.reset();\n" +
                          "@name('s0') select MyTable.myWordcms.countMinSketchFrequency(p10) as c0 from SupportBean_S1;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var fieldSetOne =
                    "myAvedev,myCount,myCountDistinct,myMax,myMedian,myStddev,myFirstEver,myCountEver,myMaxByEver"
                        .SplitCsv();

                SendEventSetAssert(env, fieldSetOne);

                env.Milestone(0);

                SendResetAssert(env, fieldSetOne);

                env.Milestone(1);

                SendEventSetAssert(env, fieldSetOne);

                env.UndeployAll();
            }

            private void SendEventSetAssert(
                RegressionEnvironment env,
                string[] fieldSetOne)
            {
                SendBean(env, "E1", 10);
                SendBean(env, "E2", 10);
                var e3 = SendBean(env, "E3", 30);

                env.AssertIterator(
                    "table",
                    iterator => {
                        var row = iterator.Advance();
                        EPAssertionUtil.AssertProps(
                            row,
                            fieldSetOne,
                            new object[] { 8.88888888888889d, 3L, 2L, 30, 10.0, 11.547005383792515d, "E1", 3L, e3 });
                        Assert.AreEqual(-3, row.Get("myPluginAggSingle"));
                        Assert.AreEqual(3, ((IDictionary<string, object>)row.Get("myPluginAggAccess")).Count);
                    });

                AssertCountMinSketch(env, "E1", 1);
            }

            private void AssertCountMinSketch(
                RegressionEnvironment env,
                string theString,
                long expected)
            {
                env.SendEventBean(new SupportBean_S1(0, theString));
                env.AssertEqualsNew("s0", "c0", expected);
            }

            private void SendResetAssert(
                RegressionEnvironment env,
                string[] fieldSetOne)
            {
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertIterator(
                    "table",
                    iterator => {
                        var row = iterator.Advance();
                        EPAssertionUtil.AssertProps(
                            row,
                            fieldSetOne,
                            new object[] { null, 0L, 0L, null, null, null, null, 0L, null });
                        Assert.AreEqual(0, row.Get("myPluginAggSingle"));
                        Assert.AreEqual(0, ((IDictionary<string, object>)row.Get("myPluginAggAccess")).Count);
                    });

                AssertCountMinSketch(env, "E1", 0);
            }
        }

        private class InfraTableResetSelective : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('table') create table MyTable(k string primary key, " +
                          "  avgone avg(int), avgtwo avg(int)," +
                          "  winone window(*) @type(SupportBean), wintwo window(*) @type(SupportBean)" +
                          ");\n" +
                          "into table MyTable select theString, " +
                          "  avg(intPrimitive) as avgone, avg(intPrimitive) as avgtwo," +
                          "  window(*) as winone, window(*) as wintwo " +
                          "from SupportBean#keepall group by theString;\n" +
                          "on SupportBean_S0 merge MyTable where p00 = k  when matched then update set avgone.reset(), winone.reset();\n" +
                          "on SupportBean_S1 merge MyTable where p10 = k  when matched then update set avgtwo.reset(), wintwo.reset();\n";
                env.CompileDeploy(epl);
                var propertyNames = "k,avgone,avgtwo,winone,wintwo".SplitCsv();

                var s0 = SendBean(env, "G1", 1);
                var s1 = SendBean(env, "G2", 10);
                var s2 = SendBean(env, "G2", 2);
                var s3 = SendBean(env, "G1", 20);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "table",
                    propertyNames,
                    new object[][] {
                        new object[] { "G1", 10.5d, 10.5d, new SupportBean[] { s0, s3 }, new SupportBean[] { s0, s3 } },
                        new object[] { "G2", 6d, 6d, new SupportBean[] { s1, s2 }, new SupportBean[] { s1, s2 } }
                    });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0, "G2"));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "table",
                    propertyNames,
                    new object[][] {
                        new object[] { "G1", 10.5d, 10.5d, new SupportBean[] { s0, s3 }, new SupportBean[] { s0, s3 } },
                        new object[] { "G2", null, 6d, null, new SupportBean[] { s1, s2 } }
                    });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S1(0, "G1"));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "table",
                    propertyNames,
                    new object[][] {
                        new object[] { "G1", 10.5d, null, new SupportBean[] { s0, s3 }, null },
                        new object[] { "G2", null, 6d, null, new SupportBean[] { s1, s2 } }
                    });

                env.UndeployAll();
            }
        }

        private class InfraTableResetRowSumWTableAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionReset(
                    env,
                    "on SupportBean_S0 merge MyTable as mt when matched then update set mt.reset();\n");
            }
        }

        private class InfraTableResetRowSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionReset(env, "on SupportBean_S0 merge MyTable when matched then update set asum.reset();\n");
            }
        }

        private static void RunAssertionReset(
            RegressionEnvironment env,
            string onMerge)
        {
            var epl = "@name('table') create table MyTable(asum sum(int));\n" +
                      "into table MyTable select sum(intPrimitive) as asum from SupportBean;\n" +
                      onMerge;
            env.CompileDeploy(epl);

            SendBeanAssertSum(env, 10, 10);
            SendBeanAssertSum(env, 11, 21);
            SendResetAssertSum(env);

            env.Milestone(0);

            AssertTableSum(env, null);
            SendBeanAssertSum(env, 20, 20);
            SendBeanAssertSum(env, 21, 41);
            SendResetAssertSum(env);

            env.Milestone(1);

            SendBeanAssertSum(env, 30, 30);

            env.UndeployAll();
        }

        private static SupportBean SendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            env.SendEventBean(sb);
            return sb;
        }

        private static void SendBeanAssertSum(
            RegressionEnvironment env,
            int intPrimitive,
            int expected)
        {
            env.SendEventBean(new SupportBean("E1", intPrimitive));
            AssertTableSum(env, expected);
        }

        private static void SendResetAssertSum(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBean_S0(0));
            AssertTableSum(env, null);
        }

        private static void AssertTableSum(
            RegressionEnvironment env,
            int? expected)
        {
            env.AssertIterator("table", iterator => Assert.AreEqual(expected, iterator.Advance().Get("asum")));
        }
    }
} // end of namespace