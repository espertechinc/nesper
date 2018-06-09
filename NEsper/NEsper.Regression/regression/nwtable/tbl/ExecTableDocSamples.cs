///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableDocSamples : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1),
                    typeof(TrafficEvent), typeof(IntrusionEvent), typeof(MyEvent)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionIncreasingUseCase(epService);
            RunAssertionDoc(epService);
        }
    
        private void RunAssertionIncreasingUseCase(EPServiceProvider epService) {
            string epl =
                    "create schema ValueEvent(value long);\n" +
                            "create schema ResetEvent(startThreshold long);\n" +
                            "create table CurrentMaxTable(currentThreshold long);\n" +
                            "@Name('trigger') insert into ThresholdTriggered select * from ValueEvent(value >= CurrentMaxTable.currentThreshold);\n" +
                            "on ResetEvent merge CurrentMaxTable when matched then update set currentThreshold = startThreshold when not matched then insert select startThreshold as currentThreshold;\n" +
                            "on ThresholdTriggered update CurrentMaxTable set currentThreshold = value + 100;\n";
            DeploymentResult d = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("trigger").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("startThreshold", 100L), "ResetEvent");
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("value", 30L), "ValueEvent");
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("value", 99L), "ValueEvent");
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("value", 100L), "ValueEvent");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "value".Split(','), new object[]{100L});
    
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("value", 101L), "ValueEvent");
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("value", 103L), "ValueEvent");
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("value", 130L), "ValueEvent");
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("value", 199L), "ValueEvent");
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("value", 200L), "ValueEvent");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "value".Split(','), new object[]{200L});
    
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("value", 201L), "ValueEvent");
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("value", 260L), "ValueEvent");
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("value", 301L), "ValueEvent");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "value".Split(','), new object[]{301L});
    
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(d.DeploymentId);
        }
    
        private void RunAssertionDoc(EPServiceProvider epService) {
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
                    "  myStddev stddev(decimal), // column holds a standard deviation for BigDecimal values\n" +
                    "  mySum sum(long), // column holds a sum of long values\n" +
                    "  myFirstEver firstever(string), // column holds the first ever string value\n" +
                    "  myCountEver countever(*) // column holds the count-ever\n" +
                    ")");
            epService.EPAdministrator.CreateEPL("create table MyStatsMore (\n" +
                    "  myKey string primary key,\n" +
                    "  myAvgFiltered avg(double, bool), // column holds a average of double-typed values\n" +
                    "                      // and filtered by a bool expression to be provided\n" +
                    "  myAvgDistinct avg(distinct double) // column holds a average of distinct double-typed values\n" +
                    ")");
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
            epService.EPAdministrator.CreateEPL("create table MyEventAggregationTable (\n" +
                    "  myKey string primary key,\n" +
                    "  myWindow window(*) @Type(MyEvent), // column holds a window of MyEvent events\n" +
                    "  mySorted sorted(mySortValue) @Type(MyEvent), // column holds MyEvent events sorted by mySortValue\n" +
                    "  myMaxByEver maxbyever(mySortValue) @Type(MyEvent) // column holds the single MyEvent event that \n" +
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
                    "  active bool\n" +
                    ")");
            epService.EPAdministrator.CreateEPL("into table IntrusionCountTable\n" +
                    "select count(*) as countIntrusion10Sec\n" +
                    "from IntrusionEvent#time(10)\n" +
                    "group by fromAddress, toAddress");
            epService.EPAdministrator.CreateEPL("into table IntrusionCountTable\n" +
                    "select count(*) as countIntrusion60Sec\n" +
                    "from IntrusionEvent#time(60)\n" +
                    "group by fromAddress, toAddress");
    
            epService.EPAdministrator.CreateEPL("create table TotalIntrusionCountTable (totalIntrusions count(*))");
            epService.EPAdministrator.CreateEPL("into table TotalIntrusionCountTable select count(*) as totalIntrusions from IntrusionEvent");
            epService.EPAdministrator.CreateEPL("expression alias totalIntrusions {count(*)}\n" +
                    "select totalIntrusions from IntrusionEvent");
            epService.EPAdministrator.CreateEPL("select TotalIntrusionCountTable.totalIntrusions from pattern[every timer:interval(60 sec)]");
    
            epService.EPAdministrator.CreateEPL("create table MyTable (\n" +
                    "theWindow window(*) @Type(MyEvent),\n" +
                    "theSorted sorted(mySortValue) @Type(MyEvent)\n" +
                    ")");
            epService.EPAdministrator.CreateEPL("select MyTable.theWindow.first(), MyTable.theSorted.maxBy() from SupportBean");
    
            epService.EPAdministrator.CreateEPL("select\n" +
                    "  (select * from IntrusionCountTable as intr\n" +
                    "   where intr.fromAddress = firewall.fromAddress and intr.toAddress = firewall.toAddress) \n" +
                    "from IntrusionEvent as firewall");
            epService.EPAdministrator.CreateEPL("select * from IntrusionCountTable as intr, IntrusionEvent as firewall\n" +
                    "where intr.fromAddress = firewall.fromAddress and intr.toAddress = firewall.toAddress");
    
            epService.EPAdministrator.CreateEPL("create table MyWindowTable (theWindow window(*) @Type(MyEvent))");
            epService.EPAdministrator.CreateEPL("select theWindow.first(), theWindow.last(), theWindow.window() from MyEvent, MyWindowTable");
        }
    
        public class MyEvent {
#pragma warning disable CS0649
            private int mySortValue;
#pragma warning restore CS0649

            public int GetMySortValue() {
                return mySortValue;
            }
        }
    
        public class TrafficEvent {
#pragma warning disable CS0649
            private string carId;
            private double speed;
#pragma warning restore CS0649

            public string GetCarId() {
                return carId;
            }
    
            public double GetSpeed() {
                return speed;
            }
        }
    
        public class IntrusionEvent {
#pragma warning disable CS0649
            private string fromAddress;
            private string toAddress;
#pragma warning restore CS0649

            public string GetFromAddress() {
                return fromAddress;
            }
    
            public string GetToAddress() {
                return toAddress;
            }
        }
    }
} // end of namespace
