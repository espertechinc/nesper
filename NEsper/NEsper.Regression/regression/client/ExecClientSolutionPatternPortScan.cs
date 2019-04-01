///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientSolutionPatternPortScan : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Run(EPServiceProvider epService) {
            RunAssertionPortScan_PrimarySuccess(epService);
            RunAssertionPortScan_KeepAlerting(epService);
            RunAssertionPortScan_FallsUnderThreshold(epService);
        }
    
        private void RunAssertionPortScan_PrimarySuccess(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            SetCurrentTime(epService, "8:00:00");
            SupportUpdateListener listener = DeployPortScan(epService);
            SendEventMultiple(epService, 20, "A", "B");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "type,cnt".Split(','), new object[]{"DETECTED", 20L});
            UndeployRemoveAll(epService);
        }
    
        private void RunAssertionPortScan_KeepAlerting(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            SetCurrentTime(epService, "8:00:00");
            SupportUpdateListener listener = DeployPortScan(epService);
            SendEventMultiple(epService, 20, "A", "B");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "type,cnt".Split(','), new object[]{"DETECTED", 20L});
    
            SetCurrentTime(epService, "8:00:29");
            SendEventMultiple(epService, 20, "A", "B");
    
            SetCurrentTime(epService, "8:00:59");
            SendEventMultiple(epService, 20, "A", "B");
            Assert.IsFalse(listener.IsInvoked);
    
            SetCurrentTime(epService, "8:01:00");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "type,cnt".Split(','), new object[]{"UPDATE", 20L});
    
            UndeployRemoveAll(epService);
        }
    
        private void RunAssertionPortScan_FallsUnderThreshold(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            SetCurrentTime(epService, "8:00:00");
            SupportUpdateListener listener = DeployPortScan(epService);
            SendEventMultiple(epService, 20, "A", "B");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "type,cnt".Split(','), new object[]{"DETECTED", 20L});
    
            SetCurrentTime(epService, "8:01:00");
            EPAssertionUtil.AssertProps(listener.GetAndResetLastNewData()[0], "type,cnt".Split(','), new object[]{"DONE", 0L});
    
            UndeployRemoveAll(epService);
        }
    
        private void SendEventMultiple(EPServiceProvider epService, int count, string src, string dst) {
            for (int i = 0; i < count; i++) {
                SendEvent(epService, src, dst, 16 + i, "m" + count);
            }
        }
    
        private void SendEvent(EPServiceProvider epService, string src, string dst, int port, string marker) {
            epService.EPRuntime.SendEvent(new object[]{src, dst, port, marker}, "PortScanEvent");
        }
    
        private void SetCurrentTime(EPServiceProvider epService, string time) {
            string timestamp = "2002-05-30T" + time + ".000";
            long current = DateTimeParser.ParseDefaultMSec(timestamp);
            Log.Info("Advancing time to " + timestamp + " msec " + current);
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(current));
        }
    
        private SupportUpdateListener DeployPortScan(EPServiceProvider epService) {
            string epl =
                    "create objectarray schema PortScanEvent(src string, dst string, port int, marker string);\n" +
                            "\n" +
                            "create table ScanCountTable(src string primary key, dst string primary key, cnt count(*), win window(*) @Type(PortScanEvent));\n" +
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
                            // For more output: "@Audit() select * from CountStream;\n" +
                            "@Name('output') select * from OutputAlerts;\n";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("output").Events += listener.Update;
            return listener;
        }
    
        private void UndeployRemoveAll(EPServiceProvider epService) {
            DeploymentInformation[] deployments = epService.EPAdministrator.DeploymentAdmin.DeploymentInformation;
            foreach (DeploymentInformation deployment in deployments) {
                epService.EPAdministrator.DeploymentAdmin.UndeployRemove(deployment.DeploymentId);
            }
        }
    }
} // end of namespace
