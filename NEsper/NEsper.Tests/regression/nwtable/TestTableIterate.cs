///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableIterate
    {
        private const string METHOD_NAME = "method:SupportStaticMethodLib.FetchTwoRows3Cols()";

        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            foreach (var clazz in new Type[] {typeof(SupportBean)}) {
                _epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib));
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestIterate() {
            _epService.EPAdministrator.CreateEPL("@Resilient create table MyTable(pkey0 string primary key, pkey1 int primary key, c0 long)");
            _epService.EPAdministrator.CreateEPL("@Resilient insert into MyTable select TheString as pkey0, IntPrimitive as pkey1, LongPrimitive as c0 from SupportBean");
    
            SendSupportBean("E1", 10, 100);
            SendSupportBean("E2", 20, 200);
    
            RunAssertion(true);
            RunAssertion(false);
        }
    
        private void RunAssertion(bool useTable) {
            RunUnaggregatedUngroupedSelectStar(useTable);
            RunFullyAggregatedAndUngrouped(useTable);
            RunAggregatedAndUngrouped(useTable);
            RunFullyAggregatedAndGrouped(useTable);
            RunAggregatedAndGrouped(useTable);
            RunAggregatedAndGroupedRollup(useTable);
        }
    
        private void RunUnaggregatedUngroupedSelectStar(bool useTable) {
            var epl = "select * from " + (useTable ? "MyTable" : METHOD_NAME);
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "pkey0,pkey1,c0".Split(','), new object[][] { new object[] { "E1", 10, 100L }, new object[] { "E2", 20, 200L } });
        }
    
        private void RunFullyAggregatedAndUngrouped(bool useTable) {
            var epl = "select count(*) as thecnt from " + (useTable ? "MyTable" : METHOD_NAME);
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            for (var i = 0; i < 2; i++) {
                var @event = stmt.First();
                Assert.AreEqual(2L, @event.Get("thecnt"));
            }
        }
    
        private void RunAggregatedAndUngrouped(bool useTable) {
            var epl = "select pkey0, count(*) as thecnt from " + (useTable ? "MyTable" : METHOD_NAME);
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            for (var i = 0; i < 2; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "pkey0,thecnt".Split(','), new object[][] { new object[] { "E1", 2L }, new object[] { "E2", 2L } });
            }
        }
    
        private void RunFullyAggregatedAndGrouped(bool useTable) {
            var epl = "select pkey0, count(*) as thecnt from " + (useTable ? "MyTable" : METHOD_NAME) + " group by pkey0";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            for (var i = 0; i < 2; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "pkey0,thecnt".Split(','), new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 1L } });
            }
        }
    
        private void RunAggregatedAndGrouped(bool useTable) {
            var epl = "select pkey0, pkey1, count(*) as thecnt from " + (useTable ? "MyTable" : METHOD_NAME) + " group by pkey0";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            for (var i = 0; i < 2; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "pkey0,pkey1,thecnt".Split(','), new object[][] { new object[] { "E1", 10, 1L }, new object[] { "E2", 20, 1L } });
            }
        }
    
        private void RunAggregatedAndGroupedRollup(bool useTable) {
            var epl = "select pkey0, pkey1, count(*) as thecnt from " + (useTable ? "MyTable" : METHOD_NAME) + " group by rollup (pkey0, pkey1)";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            for (var i = 0; i < 2; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "pkey0,pkey1,thecnt".Split(','), new object[][]{
                        new object[]{"E1", 10, 1L},
                        new object[]{"E2", 20, 1L},
                        new object[]{"E1", null, 1L},
                        new object[]{"E2", null, 1L},
                        new object[]{null, null, 2L},
                });
            }
        }
    
        private SupportBean SendSupportBean(string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
}
