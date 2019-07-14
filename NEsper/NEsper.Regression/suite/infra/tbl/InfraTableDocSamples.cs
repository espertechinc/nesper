///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableDocSamples
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraIncreasingUseCase());
            execs.Add(new InfraDoc());
            return execs;
        }

        internal class InfraIncreasingUseCase : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create schema ValueEvent(value long);\n" +
                    "create schema ResetEvent(startThreshold long);\n" +
                    "create table CurrentMaxTable(currentThreshold long);\n" +
                    "@Name('s0') insert into ThresholdTriggered select * from ValueEvent(value >= CurrentMaxTable.currentThreshold);\n" +
                    "on ResetEvent merge CurrentMaxTable when matched then update set currentThreshold = startThreshold when not matched then insert select startThreshold as currentThreshold;\n" +
                    "on ThresholdTriggered update CurrentMaxTable set currentThreshold = value + 100;\n";
                env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

                env.SendEventMap(Collections.SingletonDataMap("startThreshold", 100L), "ResetEvent");
                env.SendEventMap(Collections.SingletonDataMap("value", 30L), "ValueEvent");
                env.SendEventMap(Collections.SingletonDataMap("value", 99L), "ValueEvent");

                env.Milestone(0);

                env.SendEventMap(Collections.SingletonDataMap("value", 100L), "ValueEvent");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "value".SplitCsv(),
                    new object[] {100L});

                env.SendEventMap(Collections.SingletonDataMap("value", 101L), "ValueEvent");
                env.SendEventMap(Collections.SingletonDataMap("value", 103L), "ValueEvent");
                env.SendEventMap(Collections.SingletonDataMap("value", 130L), "ValueEvent");
                env.SendEventMap(Collections.SingletonDataMap("value", 199L), "ValueEvent");

                env.Milestone(1);

                env.SendEventMap(Collections.SingletonDataMap("value", 200L), "ValueEvent");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "value".SplitCsv(),
                    new object[] {200L});

                env.SendEventMap(Collections.SingletonDataMap("value", 201L), "ValueEvent");
                env.SendEventMap(Collections.SingletonDataMap("value", 260L), "ValueEvent");
                env.SendEventMap(Collections.SingletonDataMap("value", 301L), "ValueEvent");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "value".SplitCsv(),
                    new object[] {301L});

                env.UndeployAll();
            }
        }

        internal class InfraDoc : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create table agg_srcdst as (key0 string primary key, key1 string primary key, cnt count(*))",
                    path);
                env.CompileDeploy("create schema IPAddressFirewallAlert(ip_src string, ip_dst string)", path);
                env.CompileDeploy("select agg_srcdst[ip_src, ip_dst].cnt from IPAddressFirewallAlert", path);
                env.CompileDeploy("create schema PortScanEvent(ip_src string, ip_dst string)", path);
                env.CompileDeploy(
                    "into table agg_srcdst select count(*) as cnt from PortScanEvent group by ip_src, ip_dst",
                    path);

                env.CompileDeploy(
                    "create table MyStats (\n" +
                    "  myKey string primary key,\n" +
                    "  myAvedev avedev(int), // column holds a mean deviation of int-typed values\n" +
                    "  myAvg avg(double), // column holds a average of double-typed values\n" +
                    "  myCount count(*), // column holds a number of values\n" +
                    "  myMax max(int), // column holds a highest int-typed value\n" +
                    "  myMedian median(float), // column holds the median of float-typed values\n" +
                    "  myStddev stddev(java.math.BigDecimal), // column holds a standard deviation for BigDecimal values\n" +
                    "  mySum sum(long), // column holds a sum of long values\n" +
                    "  myFirstEver firstever(string), // column holds the first ever string value\n" +
                    "  myCountEver countever(*) // column holds the count-ever\n" +
                    ")",
                    path);

                env.CompileDeploy(
                    "create table MyStatsMore (\n" +
                    "  myKey string primary key,\n" +
                    "  myAvgFiltered avg(double, boolean), // column holds a average of double-typed values\n" +
                    "                      // and filtered by a boolean expression to be provided\n" +
                    "  myAvgDistinct avg(distinct double) // column holds a average of distinct double-typed values\n" +
                    ")",
                    path);

                env.CompileDeploy(
                    "create table MyEventAggregationTable (\n" +
                    "  myKey string primary key,\n" +
                    "  myWindow window(*) @type(SupportMySortValueEvent), // column holds a window of SupportMySortValueEvent events\n" +
                    "  mySorted sorted(mySortValue) @type(SupportMySortValueEvent), // column holds SupportMySortValueEvent events sorted by mySortValue\n" +
                    "  myMaxByEver maxbyever(mySortValue) @type(SupportMySortValueEvent) // column holds the single SupportMySortValueEvent event that \n" +
                    "        // provided the highest value of mySortValue ever\n" +
                    ")",
                    path);

                env.CompileDeploy("create context NineToFive start (0, 9, *, *, *) end (0, 17, *, *, *)", path);
                env.CompileDeploy(
                    "context NineToFive create table AverageSpeedTable (carId string primary key, avgSpeed avg(double))",
                    path);
                env.CompileDeploy(
                    "context NineToFive into table AverageSpeedTable select avg(speed) as avgSpeed from SupportTrafficEvent group by carId",
                    path);

                env.CompileDeploy(
                    "create table IntrusionCountTable (\n" +
                    "  fromAddress string primary key,\n" +
                    "  toAddress string primary key,\n" +
                    "  countIntrusion10Sec count(*),\n" +
                    "  countIntrusion60Sec count(*)," +
                    "  active boolean\n" +
                    ")",
                    path);
                env.CompileDeploy(
                    "into table IntrusionCountTable\n" +
                    "select count(*) as countIntrusion10Sec\n" +
                    "from SupportIntrusionEvent#time(10)\n" +
                    "group by fromAddress, toAddress",
                    path);
                env.CompileDeploy(
                    "into table IntrusionCountTable\n" +
                    "select count(*) as countIntrusion60Sec\n" +
                    "from SupportIntrusionEvent#time(60)\n" +
                    "group by fromAddress, toAddress",
                    path);

                env.CompileDeploy("create table TotalIntrusionCountTable (totalIntrusions count(*))", path);
                env.CompileDeploy(
                    "into table TotalIntrusionCountTable select count(*) as totalIntrusions from SupportIntrusionEvent",
                    path);
                env.CompileDeploy(
                    "expression alias totalIntrusions {count(*)}\n" +
                    "select totalIntrusions from SupportIntrusionEvent",
                    path);
                env.CompileDeploy(
                    "select TotalIntrusionCountTable.totalIntrusions from pattern[every timer:interval(60 sec)]",
                    path);

                env.CompileDeploy(
                    "create table MyTable (\n" +
                    "theWindow window(*) @type(SupportMySortValueEvent),\n" +
                    "theSorted sorted(mySortValue) @type(SupportMySortValueEvent)\n" +
                    ")",
                    path);
                env.CompileDeploy("select MyTable.theWindow.first(), MyTable.theSorted.maxBy() from SupportBean", path);

                env.CompileDeploy(
                    "select\n" +
                    "  (select * from IntrusionCountTable as intr\n" +
                    "   where intr.fromAddress = firewall.fromAddress and intr.toAddress = firewall.toAddress) \n" +
                    "from SupportIntrusionEvent as firewall",
                    path);
                env.CompileDeploy(
                    "select * from IntrusionCountTable as intr, SupportIntrusionEvent as firewall\n" +
                    "where intr.fromAddress = firewall.fromAddress and intr.toAddress = firewall.toAddress",
                    path);

                env.CompileDeploy(
                    "create table MyWindowTable (theWindow window(*) @type(SupportMySortValueEvent))",
                    path);
                env.CompileDeploy(
                    "select theWindow.first(), theWindow.last(), theWindow.window() from SupportMySortValueEvent, MyWindowTable",
                    path);

                env.UndeployAll();
            }
        }
    }
} // end of namespace