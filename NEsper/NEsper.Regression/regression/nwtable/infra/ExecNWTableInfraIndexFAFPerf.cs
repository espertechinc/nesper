///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraIndexFAFPerf : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("justCount", typeof(InvocationCounter).Name, "justCount");
    
            RunAssertionFAFKeyBTreePerformance(epService, true);
            RunAssertionFAFKeyBTreePerformance(epService, false);
    
            RunAssertionFAFKeyAndRangePerformance(epService, true);
            RunAssertionFAFKeyAndRangePerformance(epService, false);
    
            RunAssertionFAFRangePerformance(epService, true);
            RunAssertionFAFRangePerformance(epService, false);
    
            RunAssertionFAFKeyPerformance(epService, true);
            RunAssertionFAFKeyPerformance(epService, false);
    
            RunAssertionFAFInKeywordSingleIndex(epService, true);
            RunAssertionFAFInKeywordSingleIndex(epService, false);
        }
    
        private void RunAssertionFAFKeyBTreePerformance(EPServiceProvider epService, bool namedWindow) {
            // create window one
            string eplCreate = namedWindow ?
                    "create window MyInfraFAFKB#keepall as SupportBean" :
                    "create table MyInfraFAFKB (theString string primary key, intPrimitive int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfraFAFKB select theString, intPrimitive from SupportBean");
            EPStatement idx = epService.EPAdministrator.CreateEPL("create index idx1 on MyInfraFAFKB(intPrimitive btree)");
    
            // insert X rows
            int maxRows = 10000;   //for performance testing change to int maxRows = 100000;
            for (int i = 0; i < maxRows; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("A", i));
            }
            epService.EPRuntime.SendEvent(new SupportBean("B", 100));
    
            // fire single-key queries
            string eplIdx1One = "select intPrimitive as sumi from MyInfraFAFKB where intPrimitive = 5501";
            RunFAFAssertion(epService, eplIdx1One, 5501);
    
            string eplIdx1Two = "select sum(intPrimitive) as sumi from MyInfraFAFKB where intPrimitive > 9997";
            RunFAFAssertion(epService, eplIdx1Two, 9998 + 9999);
    
            // drop index, create multikey btree
            idx.Dispose();
            epService.EPAdministrator.CreateEPL("create index idx2 on MyInfraFAFKB(intPrimitive btree, theString btree)");
    
            string eplIdx2One = "select intPrimitive as sumi from MyInfraFAFKB where intPrimitive = 5501 and theString = 'A'";
            RunFAFAssertion(epService, eplIdx2One, 5501);
    
            string eplIdx2Two = "select sum(intPrimitive) as sumi from MyInfraFAFKB where intPrimitive in [5000:5004) and theString = 'A'";
            RunFAFAssertion(epService, eplIdx2Two, 5000 + 5001 + 5003 + 5002);
    
            string eplIdx2Three = "select sum(intPrimitive) as sumi from MyInfraFAFKB where intPrimitive=5001 and theString between 'A' and 'B'";
            RunFAFAssertion(epService, eplIdx2Three, 5001);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraFAFKB", false);
        }
    
        private void RunFAFAssertion(EPServiceProvider epService, string epl, int? expected) {
            long start = PerformanceObserver.MilliTime;
            int loops = 500;
    
            EPOnDemandPreparedQuery query = epService.EPRuntime.PrepareQuery(epl);
            for (int i = 0; i < loops; i++) {
                RunFAFQuery(query, expected);
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            Assert.IsTrue(delta < 1500, "delta=" + delta);
        }
    
        private void RunAssertionFAFKeyAndRangePerformance(EPServiceProvider epService, bool namedWindow) {
            // create window one
            string eplCreate = namedWindow ?
                    "create window MyInfraFAFKR#keepall as SupportBean" :
                    "create table MyInfraFAFKR (theString string primary key, intPrimitive int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfraFAFKR select theString, intPrimitive from SupportBean");
            epService.EPAdministrator.CreateEPL("create index idx1 on MyInfraFAFKR(theString hash, intPrimitive btree)");
    
            // insert X rows
            int maxRows = 10000;   //for performance testing change to int maxRows = 100000;
            for (int i = 0; i < maxRows; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("A", i));
            }
    
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive not in [3:9997]", 1 + 2 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive not in [3:9997)", 1 + 2 + 9997 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive not in (3:9997]", 1 + 2 + 3 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive not in (3:9997)", 1 + 2 + 3 + 9997 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'B' and intPrimitive not in (3:9997)", null);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive between 200 and 202", 603);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive between 202 and 199", 199 + 200 + 201 + 202);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive >= 200 and intPrimitive <= 202", 603);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive >= 202 and intPrimitive <= 200", null);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive > 9997", 9998 + 9999);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive >= 9997", 9997 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive < 5", 4 + 3 + 2 + 1);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive <= 5", 5 + 4 + 3 + 2 + 1);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive in [200:202]", 603);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive in [200:202)", 401);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive in (200:202]", 403);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraFAFKR where theString = 'A' and intPrimitive in (200:202)", 201);
    
            // test no value returned
            EPOnDemandPreparedQuery query = epService.EPRuntime.PrepareQuery("select * from MyInfraFAFKR where theString = 'A' and intPrimitive < 0");
            EPOnDemandQueryResult result = query.Execute();
            Assert.AreEqual(0, result.Array.Length);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraFAFKR", false);
        }
    
        private void RunAssertionFAFRangePerformance(EPServiceProvider epService, bool namedWindow) {
            // create window one
            string eplCreate = namedWindow ?
                    "create window MyInfraRP#keepall as SupportBean" :
                    "create table MyInfraRP (theString string primary key, intPrimitive int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfraRP select theString, intPrimitive from SupportBean");
            epService.EPAdministrator.CreateEPL("create index idx1 on MyInfraRP(intPrimitive btree)");
    
            // insert X rows
            int maxRows = 10000;   //for performance testing change to int maxRows = 100000;
            for (int i = 0; i < maxRows; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("K", i));
            }
    
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive between 200 and 202", 603);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive between 202 and 199", 199 + 200 + 201 + 202);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive >= 200 and intPrimitive <= 202", 603);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive >= 202 and intPrimitive <= 200", null);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive > 9997", 9998 + 9999);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive >= 9997", 9997 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive < 5", 4 + 3 + 2 + 1);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive <= 5", 5 + 4 + 3 + 2 + 1);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive in [200:202]", 603);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive in [200:202)", 401);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive in (200:202]", 403);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive in (200:202)", 201);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive not in [3:9997]", 1 + 2 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive not in [3:9997)", 1 + 2 + 9997 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive not in (3:9997]", 1 + 2 + 3 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(intPrimitive) as sumi from MyInfraRP where intPrimitive not in (3:9997)", 1 + 2 + 3 + 9997 + 9998 + 9999);
    
            // test no value returned
            EPOnDemandPreparedQuery query = epService.EPRuntime.PrepareQuery("select * from MyInfraRP where intPrimitive < 0");
            EPOnDemandQueryResult result = query.Execute();
            Assert.AreEqual(0, result.Array.Length);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraRP", false);
        }
    
        private void RunAssertionFAFKeyPerformance(EPServiceProvider epService, bool namedWindow) {
            // create window one
            string stmtTextCreateOne = namedWindow ?
                    "create window MyInfraOne#keepall as (f1 string, f2 int)" :
                    "create table MyInfraOne (f1 string primary key, f2 int primary key)";
            epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            epService.EPAdministrator.CreateEPL("insert into MyInfraOne(f1, f2) select theString, intPrimitive from SupportBean");
            epService.EPAdministrator.CreateEPL("create index MyInfraOneIndex on MyInfraOne(f1)");
    
            // insert X rows
            int maxRows = 100;   //for performance testing change to int maxRows = 100000;
            for (int i = 0; i < maxRows; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("K" + i, i));
            }
    
            // fire N queries each returning 1 row
            long start = PerformanceObserver.MilliTime;
            string queryText = "select * from MyInfraOne where f1='K10'";
            EPOnDemandPreparedQuery query = epService.EPRuntime.PrepareQuery(queryText);
            int loops = 10000;
    
            for (int i = 0; i < loops; i++) {
                EPOnDemandQueryResult resultX = query.Execute();
                Assert.AreEqual(1, resultX.Array.Length);
                Assert.AreEqual("K10", resultX.Array[0].Get("f1"));
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            Assert.IsTrue(delta < 500, "delta=" + delta);
    
            // test no value returned
            queryText = "select * from MyInfraOne where f1='KX'";
            query = epService.EPRuntime.PrepareQuery(queryText);
            EPOnDemandQueryResult result = query.Execute();
            Assert.AreEqual(0, result.Array.Length);
    
            // test query null
            queryText = "select * from MyInfraOne where f1=null";
            query = epService.EPRuntime.PrepareQuery(queryText);
            result = query.Execute();
            Assert.AreEqual(0, result.Array.Length);
    
            // insert null and test null
            epService.EPRuntime.SendEvent(new SupportBean(null, -2));
            result = query.Execute();
            Assert.AreEqual(0, result.Array.Length);
    
            // test two values
            epService.EPRuntime.SendEvent(new SupportBean(null, -1));
            query = epService.EPRuntime.PrepareQuery("select * from MyInfraOne where f1 is null order by f2 asc");
            result = query.Execute();
            Assert.AreEqual(2, result.Array.Length);
            Assert.AreEqual(-2, result.Array[0].Get("f2"));
            Assert.AreEqual(-1, result.Array[1].Get("f2"));
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraOne", false);
        }
    
        private void RunFAFQuery(EPOnDemandPreparedQuery query, int? expectedValue) {
            EPOnDemandQueryResult result = query.Execute();
            Assert.AreEqual(1, result.Array.Length);
            Assert.AreEqual(expectedValue, result.Array[0].Get("sumi"));
        }
    
        private void RunAssertionFAFInKeywordSingleIndex(EPServiceProvider epService, bool namedWindow) {
            string eplCreate = namedWindow ?
                    "create window MyInfraIKW#keepall as MyEvent" :
                    "create table MyInfraIKW (id string primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("create index idx on MyInfraIKW(id)");
            epService.EPAdministrator.CreateEPL("insert into MyInfraIKW select id from MyEvent");
    
            int eventCount = 10;
            for (int i = 0; i < eventCount; i++) {
                epService.EPRuntime.SendEvent(new MyEvent("E" + i));
            }
    
            InvocationCounter.SetCount(0);
            string fafEPL = "select * from MyInfraIKW as mw where JustCount(mw) and id in ('notfound')";
            epService.EPRuntime.ExecuteQuery(fafEPL);
            Assert.AreEqual(0, InvocationCounter.GetCount());
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraIKW", false);
        }
    
        public class MyEvent {
            private string id;
    
            public MyEvent(string id) {
                this.id = id;
            }
    
            public string GetId() {
                return id;
            }
        }
    
        public class InvocationCounter {
            private static int count;
    
            public static void SetCount(int count) {
                InvocationCounter.count = count;
            }
    
            public static int GetCount() {
                return count;
            }
    
            public static bool JustCount(Object o) {
                count++;
                return true;
            }
        }
    }
} // end of namespace
