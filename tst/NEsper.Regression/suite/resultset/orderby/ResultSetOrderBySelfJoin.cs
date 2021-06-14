///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.resultset.orderby
{
    public class ResultSetOrderBySelfJoin
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetOrderBySelfJoinSimple());
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            int? ecid,
            int? priority,
            int? parent)
        {
            var ev = new SupportHierarchyEvent(ecid, priority, parent);
            env.SendEventBean(ev);
        }

        public class ResultSetOrderBySelfJoinSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"prio", "cnt"};
                var epl = "@Name('s0') select c1.Event_criteria_id as ecId, " +
                          "c1.Priority as priority, " +
                          "c2.Priority as prio, cast(count(*), int) as cnt from " +
                          "SupportHierarchyEvent#lastevent as c1, " +
                          "SupportHierarchyEvent#groupwin(Event_criteria_id)#lastevent as c2, " +
                          "SupportHierarchyEvent#groupwin(Event_criteria_id)#lastevent as p " +
                          "where c2.Event_criteria_id in (c1.Event_criteria_id,2,1) " +
                          "and p.Event_criteria_id in (c1.Parent_event_criteria_id, c1.Event_criteria_id) " +
                          "order by c2.Priority asc";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, 1, 1, null);

                env.Milestone(0);

                SendEvent(env, 3, 2, 2);
                SendEvent(env, 3, 2, 2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {1, 2}, new object[] {2, 2}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace