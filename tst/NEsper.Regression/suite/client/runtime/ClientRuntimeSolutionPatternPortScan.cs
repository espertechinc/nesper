///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeSolutionPatternPortScan
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithPrimarySuccess(execs);
            WithKeepAlerting(execs);
            WithFallsUnderThreshold(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithFallsUnderThreshold(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimePortScanFallsUnderThreshold());
            return execs;
        }

        public static IList<RegressionExecution> WithKeepAlerting(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimePortScanKeepAlerting());
            return execs;
        }

        public static IList<RegressionExecution> WithPrimarySuccess(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimePortScanPrimarySuccess());
            return execs;
        }

        private class ClientRuntimePortScanPrimarySuccess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                SetCurrentTime(env, "8:00:00");
                DeployPortScan(env);
                SendEventMultiple(env, 20, "A", "B");
                env.AssertPropsNew("output", "type,cnt".SplitCsv(), new object[] { "DETECTED", 20L });
                env.UndeployAll();
            }
        }

        private class ClientRuntimePortScanKeepAlerting : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                SetCurrentTime(env, "8:00:00");
                DeployPortScan(env);
                SendEventMultiple(env, 20, "A", "B");
                env.AssertPropsNew("output", "type,cnt".SplitCsv(), new object[] { "DETECTED", 20L });

                SetCurrentTime(env, "8:00:29");
                SendEventMultiple(env, 20, "A", "B");

                SetCurrentTime(env, "8:00:59");
                SendEventMultiple(env, 20, "A", "B");
                env.AssertListenerNotInvoked("output");

                SetCurrentTime(env, "8:01:00");
                env.AssertPropsNew("output", "type,cnt".SplitCsv(), new object[] { "UPDATE", 20L });

                env.UndeployAll();
            }
        }

        private class ClientRuntimePortScanFallsUnderThreshold : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                SetCurrentTime(env, "8:00:00");
                DeployPortScan(env);
                SendEventMultiple(env, 20, "A", "B");
                env.AssertPropsNew("output", "type,cnt".SplitCsv(), new object[] { "DETECTED", 20L });

                SetCurrentTime(env, "8:01:00");
                env.AssertPropsPerRowLastNew(
                    "output",
                    "type,cnt".SplitCsv(),
                    new object[][] { new object[] { "DONE", 0L } });

                env.UndeployAll();
            }
        }

        private static void SendEventMultiple(
            RegressionEnvironment env,
            int count,
            string src,
            string dst)
        {
            for (var i = 0; i < count; i++) {
                SendEvent(env, src, dst, 16 + i, $"m{count}");
            }
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string src,
            string dst,
            int port,
            string marker)
        {
            env.SendEventObjectArray(new object[] { src, dst, port, marker }, "PortScanEvent");
        }

        private static void SetCurrentTime(
            RegressionEnvironment env,
            string time)
        {
            var timestamp = $"2002-05-30T{time}.000";
            var current = DateTimeParsingFunctions.ParseDefaultMSec(timestamp);
            Console.WriteLine($"Advancing time to {timestamp} msec {current}");
            env.AdvanceTimeSpan(current);
        }

        private static void DeployPortScan(RegressionEnvironment env)
        {
            var epl =
                "@public @buseventtype create objectarray schema PortScanEvent(src string, dst string, port int, marker string);\n" +
                "\n" +
                "create table ScanCountTable(src string primary key, dst string primary key, cnt count(*), win window(*) @type(PortScanEvent));\n" +
                "\n" +
                "into table ScanCountTable\n" +
                "insert into CountStream\n" +
                "select src, dst, count(*) as cnt, window(*) as win\n" +
                "from PortScanEvent#unique(src, dst, port)#time(30 sec) group by src,dst;\n" +
                "\n" +
                "create window SituationsWindow#keepall (src string, dst string, detectionTime long);\n" +
                "\n" +
                "on CountStream(cnt >= 20) as cs\n" +
                "merge SituationsWindow sw\n" +
                "where cs.src = sw.src and cs.dst = sw.dst\n" +
                "when not matched \n" +
                "  then insert select src, dst, current_timestamp as detectionTime\n" +
                "  then insert into OutputAlerts select 'DETECTED' as type, cs.cnt as cnt, cs.win as contributors;\n" +
                "\n" +
                "on pattern [every timer:at(*, *, *, *, *)] \n" +
                "insert into OutputAlerts \n" +
                "select 'UPDATE' as type, ScanCountTable[src, dst].cnt as cnt, ScanCountTable[src, dst].win as contributors\n" +
                "from SituationsWindow sc;\n" +
                "\n" +
                "on pattern [every timer:at(*, *, *, *, *)] \n" +
                "merge SituationsWindow sw\n" +
                "when matched and (select cnt from ScanCountTable where src = sw.src and dst = sw.dst) < 10\n" +
                "  then delete\n" +
                "  then insert into OutputAlerts select 'DONE' as type, ScanCountTable[src, dst].cnt as cnt, null as contributors \n" +
                "when matched and detectionTime.after(current_timestamp, 16 hours)\n" +
                "  then delete\n" +
                "  then insert into OutputAlerts select 'EXPIRED' as type, -1L as cnt, null as contributors;\n" +
                "\n" +
                // For more output: "@audit() select * from CountStream;\n" +
                "@name('output') select * from OutputAlerts;\n";
            var compiled = env.Compile(epl);
            env.Deploy(compiled).AddListener("output");
        }
    }
} // end of namespace