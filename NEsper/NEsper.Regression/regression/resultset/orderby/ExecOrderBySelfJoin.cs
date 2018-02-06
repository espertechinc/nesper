///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.orderby
{
    public class ExecOrderBySelfJoin : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            var fields = new []{"prio", "cnt"};
            string statementString =
                "select " +
                "c1.event_criteria_id as ecid, " +
                "c1.priority as priority, " +
                "c2.priority as prio, cast(count(*), int) as cnt from " +
                typeof(SupportHierarchyEvent).MaskTypeName() + "#lastevent as c1, " +
                typeof(SupportHierarchyEvent).MaskTypeName() + "#groupwin(event_criteria_id)#lastevent as c2, " +
                typeof(SupportHierarchyEvent).MaskTypeName() + "#groupwin(event_criteria_id)#lastevent as p " +
                "where c2.event_criteria_id in (c1.event_criteria_id,2,1) " +
                "and p.event_criteria_id in (c1.parent_event_criteria_id, c1.event_criteria_id) " +
                "order by c2.priority asc";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
    
            SendEvent(epService, 1, 1, null);
            SendEvent(epService, 3, 2, 2);
            SendEvent(epService, 3, 2, 2);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][] {
                new object[] {1, 2},
                new object[] {2, 2}
            });
        }
    
        private void SendEvent(EPServiceProvider epService, int? ecid, int? priority, int? parent) {
            var ev = new SupportHierarchyEvent(ecid, priority, parent);
            epService.EPRuntime.SendEvent(ev);
        }
    
        public class SupportHierarchyEvent {
            private readonly int? event_criteria_id;
            private readonly int? priority;
            private readonly int? parent_event_criteria_id;
    
            public SupportHierarchyEvent(int? event_criteria_id, int? priority, int? parent_event_criteria_id) {
                this.event_criteria_id = event_criteria_id;
                this.priority = priority;
                this.parent_event_criteria_id = parent_event_criteria_id;
            }

            [PropertyName("event_criteria_id")]
            public int? Event_criteria_id => event_criteria_id;

            [PropertyName("priority")]
            public int? Priority => priority;

            [PropertyName("parent_event_criteria_id")]
            public int? Parent_event_criteria_id => parent_event_criteria_id;

            public override String ToString()
            {
                return "ecid=" + event_criteria_id +
                       " prio=" + priority +
                       " parent=" + parent_event_criteria_id;
            }
        }
    
    }
} // end of namespace
