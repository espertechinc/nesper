///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraIndexFAFPerf : IndexBackingTableInfo
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            // Optionally turn this on: (don't leave it on, too much output)
            // config.getEngineDefaults().getLogging().setEnableQueryPlan(true);
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        [Test]
        public void TestFAFKeyBTreePerformance()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionFAFKeyBTreePerformance(true);
            RunAssertionFAFKeyBTreePerformance(false);
        }
    
        [Test]
        public void TestFAFKeyAndRangePerformance()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionFAFKeyAndRangePerformance(true);
            RunAssertionFAFKeyAndRangePerformance(false);
        }
    
        [Test]
        public void TestFAFRangePerformance()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionFAFRangePerformance(true);
            RunAssertionFAFRangePerformance(false);
        }
    
        [Test]
        public void TestFAFKeyPerformance()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
    
            RunAssertionFAFKeyPerformance(true);
            RunAssertionFAFKeyPerformance(false);
        }
    
        [Test]
        public void TestFAFInKeywordSingleIndex()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("justCount", typeof(InvocationCounter).FullName, "JustCount");
    
            RunAssertionFAFInKeywordSingleIndex(true);
            RunAssertionFAFInKeywordSingleIndex(false);
        }
    
        private void RunAssertionFAFKeyBTreePerformance(bool namedWindow)
        {
            // create window one
            var eplCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as SupportBean" :
                    "create table MyInfra (TheString string primary key, IntPrimitive int primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra select TheString, IntPrimitive from SupportBean");
            var idx = _epService.EPAdministrator.CreateEPL("create index idx1 on MyInfra(IntPrimitive btree)");
    
            // insert X rows
            var maxRows = 10000;   //for performance testing change to int maxRows = 100000;
            for (var i = 0; i < maxRows; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("A", i));
            }
            _epService.EPRuntime.SendEvent(new SupportBean("B", 100));
    
            // fire single-key queries
            var eplIdx1One = "select IntPrimitive as sumi from MyInfra where IntPrimitive = 5501";
            RunFAFAssertion(eplIdx1One, 5501);
    
            var eplIdx1Two = "select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive > 9997";
            RunFAFAssertion(eplIdx1Two, 9998 + 9999);
    
            // drop index, create multikey btree
            idx.Dispose();
            _epService.EPAdministrator.CreateEPL("create index idx2 on MyInfra(IntPrimitive btree, TheString btree)");
    
            var eplIdx2One = "select IntPrimitive as sumi from MyInfra where IntPrimitive = 5501 and TheString = 'A'";
            RunFAFAssertion(eplIdx2One, 5501);
    
            var eplIdx2Two = "select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive in [5000:5004) and TheString = 'A'";
            RunFAFAssertion(eplIdx2Two, 5000+5001+5003+5002);
    
            var eplIdx2Three = "select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive=5001 and TheString between 'A' and 'B'";
            RunFAFAssertion(eplIdx2Three, 5001);
            
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunFAFAssertion(string epl, int? expected)
        {
            const int loops = 500;

            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    var query = _epService.EPRuntime.PrepareQuery(epl);
                    for (var i = 0; i < loops; i++)
                    {
                        RunFAFQuery(query, expected);
                    }
                });

            Assert.That(delta, Is.LessThan(100000));
            //Assert.That(delta, Is.LessThan(1000));
        }
    
        private void RunAssertionFAFKeyAndRangePerformance(bool namedWindow)
        {
            // create window one
            var eplCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as SupportBean" :
                    "create table MyInfra (TheString string primary key, IntPrimitive int primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra select TheString, IntPrimitive from SupportBean");
            _epService.EPAdministrator.CreateEPL("create index idx1 on MyInfra(TheString hash, IntPrimitive btree)");
    
            // insert X rows
            var maxRows = 10000;   //for performance testing change to int maxRows = 100000;
            for (var i=0; i < maxRows; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("A", i));
            }
    
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive not in [3:9997]", 1+2+9998+9999);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive not in [3:9997)", 1+2+9997+9998+9999);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive not in (3:9997]", 1+2+3+9998+9999);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive not in (3:9997)", 1+2+3+9997+9998+9999);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'B' and IntPrimitive not in (3:9997)", null);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive between 200 and 202", 603);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive between 202 and 199", 199+200+201+202);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive >= 200 and IntPrimitive <= 202", 603);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive >= 202 and IntPrimitive <= 200", null);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive > 9997", 9998 + 9999);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive >= 9997", 9997 + 9998 + 9999);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive < 5", 4+3+2+1);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive <= 5", 5+4+3+2+1);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive in [200:202]", 603);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive in [200:202)", 401);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive in (200:202]", 403);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where TheString = 'A' and IntPrimitive in (200:202)", 201);
    
            // test no value returned
            var query = _epService.EPRuntime.PrepareQuery("select * from MyInfra where TheString = 'A' and IntPrimitive < 0");
            var result = query.Execute();
            Assert.AreEqual(0, result.Array.Length);
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionFAFRangePerformance(bool namedWindow)
        {
            // create window one
            var eplCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as SupportBean" :
                    "create table MyInfra (TheString string primary key, IntPrimitive int primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra select TheString, IntPrimitive from SupportBean");
            _epService.EPAdministrator.CreateEPL("create index idx1 on MyInfra(IntPrimitive btree)");
    
            // insert X rows
            var maxRows = 10000;   //for performance testing change to int maxRows = 100000;
            for (var i=0; i < maxRows; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("K", i));
            }
    
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive between 200 and 202", 603);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive between 202 and 199", 199+200+201+202);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive >= 200 and IntPrimitive <= 202", 603);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive >= 202 and IntPrimitive <= 200", null);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive > 9997", 9998 + 9999);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive >= 9997", 9997 + 9998 + 9999);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive < 5", 4+3+2+1);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive <= 5", 5+4+3+2+1);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive in [200:202]", 603);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive in [200:202)", 401);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive in (200:202]", 403);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive in (200:202)", 201);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive not in [3:9997]", 1+2+9998+9999);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive not in [3:9997)", 1+2+9997+9998+9999);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive not in (3:9997]", 1+2+3+9998+9999);
            RunFAFAssertion("select sum(IntPrimitive) as sumi from MyInfra where IntPrimitive not in (3:9997)", 1+2+3+9997+9998+9999);
    
            // test no value returned
            var query = _epService.EPRuntime.PrepareQuery("select * from MyInfra where IntPrimitive < 0");
            var result = query.Execute();
            Assert.AreEqual(0, result.Array.Length);
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        public void RunAssertionFAFKeyPerformance(bool namedWindow)
        {
            // create window one
            var stmtTextCreateOne = namedWindow ?
                    "create window MyInfraOne.win:keepall() as (f1 string, f2 int)" :
                    "create table MyInfraOne (f1 string primary key, f2 int primary key)";
            _epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            _epService.EPAdministrator.CreateEPL("insert into MyInfraOne(f1, f2) select TheString, IntPrimitive from SupportBean");
            _epService.EPAdministrator.CreateEPL("create index MyInfraOneIndex on MyInfraOne(f1)");
    
            // insert X rows
            var maxRows = 100;   //for performance testing change to int maxRows = 100000;
            for (var i=0; i < maxRows; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("K" + i, i));
            }
    
            // fire Count queries each returning 1 row
            var queryText = "select * from MyInfraOne where f1='K10'";
            var query = _epService.EPRuntime.PrepareQuery(queryText);
            const int loops = 10000;

            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (var i = 0; i < loops; i++)
                    {
                        var resultX = query.Execute();
                        Assert.AreEqual(1, resultX.Array.Length);
                        Assert.AreEqual("K10", resultX.Array[0].Get("f1"));
                    }
                });

            Assert.IsTrue(delta < 500, "delta=" + delta);
            
            // test no value returned
            queryText = "select * from MyInfraOne where f1='KX'";
            query = _epService.EPRuntime.PrepareQuery(queryText);
            var result = query.Execute();
            Assert.AreEqual(0, result.Array.Length);
    
            // test query null
            queryText = "select * from MyInfraOne where f1=null";
            query = _epService.EPRuntime.PrepareQuery(queryText);
            result = query.Execute();
            Assert.AreEqual(0, result.Array.Length);
            
            // insert null and test null
            _epService.EPRuntime.SendEvent(new SupportBean(null, -2));
            result = query.Execute();
            Assert.AreEqual(0, result.Array.Length);
    
            // test two values
            _epService.EPRuntime.SendEvent(new SupportBean(null, -1));
            query = _epService.EPRuntime.PrepareQuery("select * from MyInfraOne where f1 is null order by f2 asc");
            result = query.Execute();
            Assert.AreEqual(2, result.Array.Length);
            Assert.AreEqual(-2, result.Array[0].Get("f2"));
            Assert.AreEqual(-1, result.Array[1].Get("f2"));
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfraOne", false);
        }
    
        private void RunFAFQuery(EPOnDemandPreparedQuery query, int? expectedValue)
        {
            var result = query.Execute();
            Assert.AreEqual(1, result.Array.Length);
            Assert.AreEqual(expectedValue, result.Array[0].Get("sumi"));
        }
    
        private void RunAssertionFAFInKeywordSingleIndex(bool namedWindow)
        {
            var eplCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as MyEvent" :
                    "create table MyInfra (id string primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            _epService.EPAdministrator.CreateEPL("create index idx on MyInfra(id)");
            _epService.EPAdministrator.CreateEPL("insert into MyInfra select id from MyEvent");
    
            var eventCount = 10;
            for (var i = 0; i < eventCount; i++) {
                _epService.EPRuntime.SendEvent(new MyEvent("E" + i));
            }
    
            InvocationCounter.Count = 0;
            var fafEPL = "select * from MyInfra as mw where justCount(mw) and id in ('notfound')";
            _epService.EPRuntime.ExecuteQuery(fafEPL);
            Assert.AreEqual(0, InvocationCounter.Count);
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        public class MyEvent
        {
            public MyEvent(string id)
            {
                Id = id;
            }

            public string Id { get; private set; }
        }
    
        public class InvocationCounter
        {
            private static int _count;

            public static int Count
            {
                get { return _count; }
                set { _count = value; }
            }

            public static bool JustCount(object o)
            {
                _count++;
                return true;
            }
        }
    }
}
