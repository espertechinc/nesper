///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestOrderBySelfJoin 
	{
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	    }

        [Test]
	    public void TestOrderedSelfJoin()
		{
	        var fields = new string[] {"prio", "cnt"};
	        var statementString = "select c1.event_criteria_id as ecid, " +
	                    "c1.priority as priority, " +
	                    "c2.priority as prio, cast(count(*), int) as cnt from " +
	    	            TypeHelper.MaskTypeName<SupportHierarchyEvent>() + "#lastevent as c1, " +
                        TypeHelper.MaskTypeName<SupportHierarchyEvent>() + "#groupwin(event_criteria_id)#lastevent as c2, " +
                        TypeHelper.MaskTypeName<SupportHierarchyEvent>() + "#groupwin(event_criteria_id)#lastevent as p " +
	                    "where c2.event_criteria_id in (c1.event_criteria_id,2,1) " +
	                    "and p.event_criteria_id in (c1.parent_event_criteria_id, c1.event_criteria_id) " +
	                    "order by c2.priority asc";
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);

	        SendEvent(1, 1, null);
	        SendEvent(3, 2, 2);
	        SendEvent(3, 2, 2);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][]{ new object[] {1, 2}, new object[] {2, 2}});
	    }

	    private void SendEvent(int? ecid, int? priority, int? parent)
		{
		    var ev = new SupportHierarchyEvent(ecid,priority,parent);
		    _epService.EPRuntime.SendEvent(ev);
		}

	    public class SupportHierarchyEvent
        {
	        public SupportHierarchyEvent(int? event_criteria_id, int? priority, int? parent_event_criteria_id)
	        {
	            Event_criteria_id = event_criteria_id;
	            Priority = priority;
	            Parent_event_criteria_id = parent_event_criteria_id;
	        }

	        public int? Event_criteria_id { get; private set; }

	        public int? Priority { get; private set; }

	        public int? Parent_event_criteria_id { get; private set; }

	        public override string ToString()
	        {
	            return "ecid=" + Event_criteria_id +
	                   " prio=" + Priority +
	                   " parent=" + Parent_event_criteria_id;
	        }
	    }

	}
} // end of namespace
