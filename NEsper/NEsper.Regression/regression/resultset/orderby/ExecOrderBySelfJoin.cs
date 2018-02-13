///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.orderby
{
    public class ExecOrderBySelfJoin : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            var fields = new string[]{"prio", "cnt"};
            string statementString = "select c1.event_criteria_id as ecid, " +
                    "c1.priority as priority, " +
                    "c2.priority as prio, Cast(count(*), int) as cnt from " +
                    typeof(SupportHierarchyEvent).Name + "#lastevent as c1, " +
                    typeof(SupportHierarchyEvent).Name + "#Groupwin(event_criteria_id)#lastevent as c2, " +
                    typeof(SupportHierarchyEvent).Name + "#Groupwin(event_criteria_id)#lastevent as p " +
                    "where c2.event_criteria_id in (c1.event_criteria_id,2,1) " +
                    "and p.event_criteria_id in (c1.parent_event_criteria_id, c1.event_criteria_id) " +
                    "order by c2.priority asc";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
    
            SendEvent(epService, 1, 1, null);
            SendEvent(epService, 3, 2, 2);
            SendEvent(epService, 3, 2, 2);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new Object[][]{new object[] {1, 2}, new object[] {2, 2}});
        }
    
        private void SendEvent(EPServiceProvider epService, int? ecid, int? priority, int? parent) {
            var ev = new SupportHierarchyEvent(ecid, priority, parent);
            epService.EPRuntime.SendEvent(ev);
        }
    
        public class SupportHierarchyEvent {
            private int? event_criteria_id;
            private int? priority;
            private int? parent_event_criteria_id;
    
            public SupportHierarchyEvent(int? event_criteria_id, int? priority, int? parent_event_criteria_id) {
                this.event_criteria_id = event_criteria_id;
                this.priority = priority;
                this.parent_event_criteria_id = parent_event_criteria_id;
            }
    
            public int? GetEvent_criteria_id() {
                return event_criteria_id;
            }
    
            public int? GetPriority() {
                return priority;
            }
    
            public int? GetParent_event_criteria_id() {
                return parent_event_criteria_id;
            }
    
            public override String ToString() {
                return "ecid=" + event_criteria_id +
                        " prio=" + priority +
                        " parent=" + parent_event_criteria_id;
            }
        }
    
    }
} // end of namespace
