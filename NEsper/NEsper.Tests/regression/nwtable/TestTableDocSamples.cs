///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableDocSamples  {
    
        private EPServiceProvider epService;
    
        [SetUp]
        public void SetUp() {
            epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            foreach (Type clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1),
                    typeof(TrafficEvent), typeof(IntrusionEvent), typeof(MyEvent)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestDoc() {
            epService.EPAdministrator.CreateEPL("create table agg_srcdst as (key0 string primary key, key1 string primary key, cnt count(*))");
            epService.EPAdministrator.CreateEPL("create schema IPAddressFirewallAlert(ip_src string, ip_dst string)");
            epService.EPAdministrator.CreateEPL("select agg_srcdst[ip_src, ip_dst].cnt from IPAddressFirewallAlert");
            epService.EPAdministrator.CreateEPL("create schema PortScanEvent(ip_src string, ip_dst string)");
            epService.EPAdministrator.CreateEPL("into table agg_srcdst select count(*) as cnt from PortScanEvent group by ip_src, ip_dst");
    
            epService.EPAdministrator.CreateEPL("create table MyStats (\n" +
                    "  myKey string primary key,\n" +
                    "  myAvedev avedev(int), // column holds a mean deviation of int-typed values\n" +
                    "  myAvg avg(double), // column holds a average of double-typed values\n" +
                    "  myCount count(*), // column holds a number of values\n" +
                    "  myMax max(int), // column holds a highest int-typed value\n" +
                    "  myMedian median(float), // column holds the median of float-typed values\n" +
                    "  myStddev stddev(decimal), // column holds a standard deviation for decimal values\n" +
                    "  mySum sum(long), // column holds a sum of long values\n" +
                    "  myFirstEver firstever(string), // column holds the first ever string value\n" +
                    "  myCountEver countever(*) // column holds the count-ever\n" +
                    ")");
            epService.EPAdministrator.CreateEPL("create table MyStatsMore (\n" +
                    "  myKey string primary key,\n" +
                    "  myAvgFiltered avg(double, boolean), // column holds a average of double-typed values\n" +
                    "                      // and filtered by a boolean expression to be provided\n" +
                    "  myAvgDistinct avg(distinct double) // column holds a average of distinct double-typed values\n" +
                    ")");
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
            epService.EPAdministrator.CreateEPL("create table MyEventAggregationTable (\n" +
                    "  myKey string primary key,\n" +
                    "  myWindow window(*) @type(MyEvent), // column holds a window of MyEvent events\n" +
                    "  mySorted sorted(mySortValue) @type(MyEvent), // column holds MyEvent events sorted by mySortValue\n" +
                    "  myMaxByEver maxbyever(mySortValue) @type(MyEvent) // column holds the single MyEvent event that \n" +
                    "        // provided the highest value of mySortValue ever\n" +
                    ")");
    
            epService.EPAdministrator.CreateEPL("create context NineToFive start (0, 9, *, *, *) end (0, 17, *, *, *)");
            epService.EPAdministrator.CreateEPL("context NineToFive create table AverageSpeedTable (carId string primary key, avgSpeed avg(double))");
            epService.EPAdministrator.CreateEPL("context NineToFive into table AverageSpeedTable select avg(speed) as avgSpeed from TrafficEvent group by carId");
    
            epService.EPAdministrator.CreateEPL("create table IntrusionCountTable (\n" +
                    "  fromAddress string primary key,\n" +
                    "  toAddress string primary key,\n" +
                    "  countIntrusion10Sec count(*),\n" +
                    "  countIntrusion60Sec count(*)," +
                    "  active boolean\n" +
                    ")");
            epService.EPAdministrator.CreateEPL("into table IntrusionCountTable\n" +
                    "select count(*) as countIntrusion10Sec\n" +
                    "from IntrusionEvent.win:time(10)\n" +
                    "group by fromAddress, toAddress");
            epService.EPAdministrator.CreateEPL("into table IntrusionCountTable\n" +
                    "select count(*) as countIntrusion60Sec\n" +
                    "from IntrusionEvent.win:time(60)\n" +
                    "group by fromAddress, toAddress");
    
            epService.EPAdministrator.CreateEPL("create table TotalIntrusionCountTable (totalIntrusions count(*))");
            epService.EPAdministrator.CreateEPL("into table TotalIntrusionCountTable select count(*) as totalIntrusions from IntrusionEvent");
            epService.EPAdministrator.CreateEPL("expression alias totalIntrusions {count(*)}\n" +
                    "select totalIntrusions from IntrusionEvent");
            epService.EPAdministrator.CreateEPL("select TotalIntrusionCountTable.totalIntrusions from pattern[every timer:interval(60 sec)]");
    
            epService.EPAdministrator.CreateEPL("create table MyTable (\n" +
                    "theWindow window(*) @type(MyEvent),\n" +
                    "theSorted sorted(mySortValue) @type(MyEvent)\n" +
                    ")");
            epService.EPAdministrator.CreateEPL("select MyTable.theWindow.first(), MyTable.theSorted.maxBy() from SupportBean");
    
            epService.EPAdministrator.CreateEPL("select\n" +
                    "  (select * from IntrusionCountTable as intr\n" +
                    "   where intr.fromAddress = firewall.fromAddress and intr.toAddress = firewall.toAddress) \n" +
                    "from IntrusionEvent as firewall");
            epService.EPAdministrator.CreateEPL("select * from IntrusionCountTable as intr, IntrusionEvent as firewall\n" +
                    "where intr.fromAddress = firewall.fromAddress and intr.toAddress = firewall.toAddress");
    
            epService.EPAdministrator.CreateEPL("create table MyWindowTable (theWindow window(*) @type(MyEvent))");
            epService.EPAdministrator.CreateEPL("select theWindow.first(), theWindow.last(), theWindow.window() from MyEvent, MyWindowTable");
        }
    
        internal class MyEvent
        {
            public int MySortValue { get; private set; }
        }
    
        internal class TrafficEvent
        {
            public string CarId { get; private set; }
            public double Speed { get; private set; }
        }
    
        internal class IntrusionEvent
        {
            public string FromAddress { get; private set; }
            public string ToAddress { get; private set; }
        }
    }
}
