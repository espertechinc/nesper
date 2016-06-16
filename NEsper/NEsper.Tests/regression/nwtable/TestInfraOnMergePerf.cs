///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraOnMergePerf
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _mergeListener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _mergeListener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            _mergeListener = null;
        }
    
        [Test]
        public void TestPerformance() {
            RunAssertionPerformance(true, EventRepresentationEnum.OBJECTARRAY);
            RunAssertionPerformance(true, EventRepresentationEnum.MAP);
            RunAssertionPerformance(true, EventRepresentationEnum.DEFAULT);
            RunAssertionPerformance(false, EventRepresentationEnum.OBJECTARRAY);
        }
    
        private void RunAssertionPerformance(bool namedWindow, EventRepresentationEnum outputType) {
    
            string eplCreate = namedWindow ?
                outputType.GetAnnotationText() + " create window MyWindow.win:keepall() as (c1 string, c2 int)" :
                "create table MyWindow(c1 string primary key, c2 int)";
            EPStatement stmtNamedWindow = _epService.EPAdministrator.CreateEPL(eplCreate);
            Assert.AreEqual(outputType.GetOutputClass(), stmtNamedWindow.EventType.UnderlyingType);
    
            // preload events
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("insert into MyWindow select TheString as c1, IntPrimitive as c2 from SupportBean");
            int totalUpdated = 5000;
            for (int i = 0; i < totalUpdated; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, 0));
            }
            stmt.Dispose();
    
            string epl =  "on SupportBean sb merge MyWindow nw where nw.c1 = sb.TheString " +
                          "when matched then update set nw.c2=sb.IntPrimitive";
            stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(_mergeListener);
    
            // prime
            for (int i = 0; i < 100; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, 1));
            }
            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < totalUpdated; i++)
                    {
                        _epService.EPRuntime.SendEvent(new SupportBean("E" + i, 1));
                    }
                });
    
            // verify
            IEnumerator<EventBean> events = stmtNamedWindow.GetEnumerator();
            int count = 0;
            while(events.MoveNext())
            {
                EventBean next = events.Current;
                Assert.AreEqual(1, next.Get("c2"));
                count++;
            }
            Assert.AreEqual(totalUpdated, count);
            Assert.IsTrue(delta < 500, "Delta=" + delta);
            
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", true);
        }
    }
}
