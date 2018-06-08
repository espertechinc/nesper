///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.logger;

using NEsper.Examples.QoS_SLA.monitor;

namespace NEsper.Examples.QoS_SLA
{
    public class AppMain
    {
        public static void Main()
        {
            LoggerNLog.BasicConfig();
            LoggerNLog.Register();

            using (TestAverageLatencyAlertMonitor testAverageLatencyAlertMonitor = new TestAverageLatencyAlertMonitor()) {
                testAverageLatencyAlertMonitor.SetUp();
                testAverageLatencyAlertMonitor.TestLatencyAlert();
            }

            using (TestDynamicLatencyAlertMonitor testDynamicLatencyAlertMonitor = new TestDynamicLatencyAlertMonitor()) {
                testDynamicLatencyAlertMonitor.SetUp();
                testDynamicLatencyAlertMonitor.TestLatencyAlert();
            }

            using (TestErrorRateMonitor testErrorRateMonitor = new TestErrorRateMonitor()) {
                testErrorRateMonitor.SetUp();
                testErrorRateMonitor.TestAlert();
            }

            using (TestLatencySpikeMonitor testLatencySpikeMonitor = new TestLatencySpikeMonitor()) {
                testLatencySpikeMonitor.SetUp();
                testLatencySpikeMonitor.TestLatencyAlert();
            }

            using (TestServiceHealthMonitor testServiceHealthMonitor = new TestServiceHealthMonitor()) {
                testServiceHealthMonitor.SetUp();
                testServiceHealthMonitor.TestLatencyAlert();
            }

            using (TestSpikeAndErrorRateMonitor testSpikeAndErrorRateMonitor = new TestSpikeAndErrorRateMonitor()) {
                testSpikeAndErrorRateMonitor.SetUp();
                testSpikeAndErrorRateMonitor.TestAlert();
            }
        }
    }
}
