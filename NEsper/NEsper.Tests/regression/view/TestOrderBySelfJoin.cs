///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestOrderBySelfJoin 
    {
    	private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestOrderedSelfJoin()
    	{
            String[] fields = new [] {"prio", "cnt"};
            String statementString = "select c1.EventCriteriaId as ecid, " +
                        "c1.Priority as priority, " +
                        "c2.Priority as prio, cast(count(*), int) as cnt from " +
        	            typeof(SupportHierarchyEvent).FullName + ".std:lastevent() as c1, " +
        	            typeof(SupportHierarchyEvent).FullName + ".std:groupwin(EventCriteriaId).std:lastevent() as c2, " +
        	            typeof(SupportHierarchyEvent).FullName + ".std:groupwin(EventCriteriaId).std:lastevent() as p " +
                        "where c2.EventCriteriaId in (c1.EventCriteriaId,2,1) " +
                        "and p.EventCriteriaId in (c1.ParentEventCriteriaId, c1.EventCriteriaId) " +
                        "order by c2.Priority asc";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
    
            SendEvent(1, 1, null);
            SendEvent(3, 2, 2);
            SendEvent(3, 2, 2);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new [] { new Object[] {1,2}, new Object[] {2,2}});
        }

        private void SendEvent(int? ecid, int? priority, int? parent)
    	{
    	    SupportHierarchyEvent ev = new SupportHierarchyEvent(ecid,priority,parent);
    	    _epService.EPRuntime.SendEvent(ev);
    	}    
    }

    public class SupportHierarchyEvent
    {
        public SupportHierarchyEvent(int? eventCriteriaId, int? priority, int? parentEventCriteriaId)
        {
            EventCriteriaId = eventCriteriaId;
            Priority = priority;
            ParentEventCriteriaId = parentEventCriteriaId;
        }

        public int? EventCriteriaId { get; private set; }

        public int? Priority { get; private set; }

        public int? ParentEventCriteriaId { get; private set; }

        public override String ToString()
        {
            return "ecid=" + EventCriteriaId +
                   " prio=" + Priority +
                   " parent=" + ParentEventCriteriaId;
        }
    }
}
