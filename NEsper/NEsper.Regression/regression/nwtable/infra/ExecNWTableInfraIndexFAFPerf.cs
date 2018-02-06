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
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("justCount", typeof(InvocationCounter), "JustCount");
    
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
                    "create table MyInfraFAFKB (TheString string primary key, IntPrimitive int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfraFAFKB select TheString, IntPrimitive from SupportBean");
            EPStatement idx = epService.EPAdministrator.CreateEPL("create index idx1 on MyInfraFAFKB(IntPrimitive btree)");
    
            // insert X rows
            int maxRows = 10000;   //for performance testing change to int maxRows = 100000;
            for (int i = 0; i < maxRows; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("A", i));
            }
            epService.EPRuntime.SendEvent(new SupportBean("B", 100));
    
            // fire single-key queries
            string eplIdx1One = "select IntPrimitive as sumi from MyInfraFAFKB where IntPrimitive = 5501";
            RunFAFAssertion(epService, eplIdx1One, 5501);
    
            string eplIdx1Two = "select sum(IntPrimitive) as sumi from MyInfraFAFKB where IntPrimitive > 9997";
            RunFAFAssertion(epService, eplIdx1Two, 9998 + 9999);
    
            // drop index, create multikey btree
            idx.Dispose();
            epService.EPAdministrator.CreateEPL("create index idx2 on MyInfraFAFKB(IntPrimitive btree, TheString btree)");
    
            string eplIdx2One = "select IntPrimitive as sumi from MyInfraFAFKB where IntPrimitive = 5501 and TheString = 'A'";
            RunFAFAssertion(epService, eplIdx2One, 5501);
    
            string eplIdx2Two = "select sum(IntPrimitive) as sumi from MyInfraFAFKB where IntPrimitive in [5000:5004) and TheString = 'A'";
            RunFAFAssertion(epService, eplIdx2Two, 5000 + 5001 + 5003 + 5002);
    
            string eplIdx2Three = "select sum(IntPrimitive) as sumi from MyInfraFAFKB where IntPrimitive=5001 and TheString between 'A' and 'B'";
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
                    "create table MyInfraFAFKR (TheString string primary key, IntPrimitive int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfraFAFKR select TheString, IntPrimitive from SupportBean");
            epService.EPAdministrator.CreateEPL("create index idx1 on MyInfraFAFKR(TheString hash, IntPrimitive btree)");
    
            // insert X rows
            int maxRows = 10000;   //for performance testing change to int maxRows = 100000;
            for (int i = 0; i < maxRows; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("A", i));
            }
    
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive not in [3:9997]", 1 + 2 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive not in [3:9997)", 1 + 2 + 9997 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive not in (3:9997]", 1 + 2 + 3 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive not in (3:9997)", 1 + 2 + 3 + 9997 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'B' and IntPrimitive not in (3:9997)", null);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive between 200 and 202", 603);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive between 202 and 199", 199 + 200 + 201 + 202);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive >= 200 and IntPrimitive <= 202", 603);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive >= 202 and IntPrimitive <= 200", null);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive > 9997", 9998 + 9999);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive >= 9997", 9997 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive < 5", 4 + 3 + 2 + 1);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive <= 5", 5 + 4 + 3 + 2 + 1);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive in [200:202]", 603);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive in [200:202)", 401);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive in (200:202]", 403);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraFAFKR where TheString = 'A' and IntPrimitive in (200:202)", 201);
    
            // test no value returned
            EPOnDemandPreparedQuery query = epService.EPRuntime.PrepareQuery("select * from MyInfraFAFKR where TheString = 'A' and IntPrimitive < 0");
            EPOnDemandQueryResult result = query.Execute();
            Assert.AreEqual(0, result.Array.Length);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraFAFKR", false);
        }
    
        private void RunAssertionFAFRangePerformance(EPServiceProvider epService, bool namedWindow) {
            // create window one
            string eplCreate = namedWindow ?
                    "create window MyInfraRP#keepall as SupportBean" :
                    "create table MyInfraRP (TheString string primary key, IntPrimitive int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfraRP select TheString, IntPrimitive from SupportBean");
            epService.EPAdministrator.CreateEPL("create index idx1 on MyInfraRP(IntPrimitive btree)");
    
            // insert X rows
            int maxRows = 10000;   //for performance testing change to int maxRows = 100000;
            for (int i = 0; i < maxRows; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("K", i));
            }
    
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive between 200 and 202", 603);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive between 202 and 199", 199 + 200 + 201 + 202);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive >= 200 and IntPrimitive <= 202", 603);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive >= 202 and IntPrimitive <= 200", null);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive > 9997", 9998 + 9999);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive >= 9997", 9997 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive < 5", 4 + 3 + 2 + 1);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive <= 5", 5 + 4 + 3 + 2 + 1);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive in [200:202]", 603);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive in [200:202)", 401);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive in (200:202]", 403);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive in (200:202)", 201);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive not in [3:9997]", 1 + 2 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive not in [3:9997)", 1 + 2 + 9997 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive not in (3:9997]", 1 + 2 + 3 + 9998 + 9999);
            RunFAFAssertion(epService, "select sum(IntPrimitive) as sumi from MyInfraRP where IntPrimitive not in (3:9997)", 1 + 2 + 3 + 9997 + 9998 + 9999);
    
            // test no value returned
            EPOnDemandPreparedQuery query = epService.EPRuntime.PrepareQuery("select * from MyInfraRP where IntPrimitive < 0");
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
            epService.EPAdministrator.CreateEPL("insert into MyInfraOne(f1, f2) select TheString, IntPrimitive from SupportBean");
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
    
            InvocationCounter.Count = 0;
            string fafEPL = "select * from MyInfraIKW as mw where justCount(mw) and id in ('notfound')";
            epService.EPRuntime.ExecuteQuery(fafEPL);
            Assert.AreEqual(0, InvocationCounter.Count);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraIKW", false);
        }
    
        public class MyEvent {
            public MyEvent(string id) {
                Id = id;
            }

            public string Id { get; }
        }
    
        public class InvocationCounter {
            private static int _count;

            public static int Count {
                get => _count;
                set => _count = value;
            }

            public static bool JustCount(Object o) {
                _count++;
                return true;
            }
        }
    }
} // end of namespace
