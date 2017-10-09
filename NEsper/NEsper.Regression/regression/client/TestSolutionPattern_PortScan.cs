///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
	public class TestSolutionPattern_PortScan
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            SetCurrentTime("8:00:00");
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().Name);}

	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestPortScan_PrimarySuccess()
        {
	        DeployPortScan();
	        SendEventMultiple(20, "A", "B");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "type,cnt".Split(','), new object[] {"DETECTED", 20L});
	    }

        [Test]
	    public void TestPortScan_KeepAlerting()
        {
	        DeployPortScan();
	        SendEventMultiple(20, "A", "B");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "type,cnt".Split(','), new object[] {"DETECTED", 20L});

            SetCurrentTime("8:00:29");
	        SendEventMultiple(20, "A", "B");

	        SetCurrentTime("8:00:59");
	        SendEventMultiple(20, "A", "B");
	        Assert.IsFalse(_listener.IsInvoked);

            SetCurrentTime("8:01:00");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "type,cnt".Split(','), new object[] {"UPDATE", 20L});
	    }

        [Test]
	    public void TestPortScan_FallsUnderThreshold()
        {
	        DeployPortScan();
	        SendEventMultiple(20, "A", "B");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "type,cnt".Split(','), new object[] {"DETECTED", 20L});

	        SetCurrentTime("8:01:00");
	        EPAssertionUtil.AssertProps(_listener.GetAndResetLastNewData()[0], "type,cnt".Split(','), new object[] {"DONE", 0L});
	    }

	    private void SendEventMultiple(int count, string src, string dst)
        {
	        for (var i = 0; i < count; i++) {
	            SendEvent(src, dst, 16+i, "m" + count);
	        }
	    }

	    private void SendEvent(string src, string dst, int port, string marker)
        {
	       _epService.EPRuntime.SendEvent(new object[] {src, dst, port, marker}, "PortScanEvent");
	    }

	    private void SetCurrentTime(string time)
        {
	        var timestamp = "2002-05-30T" + time + ".000";
	        var current = DateTimeParser.ParseDefaultMSec(timestamp);
	        Debug.WriteLine("Advancing time to " + timestamp + " msec " + current);
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(current));
	    }

	    private void DeployPortScan()
        {
	        var epl =
	                "create objectarray schema PortScanEvent(src string, dst string, port int, marker string);\n" +
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
	                "@Name('output') select * from OutputAlerts;\n";
	        _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
	        _epService.EPAdministrator.GetStatement("output").AddListener(_listener);
	        Debug.WriteLine(epl);
	    }
	}
} // end of namespace
