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
                var epl = "@Name('s0') select c1.Event_criteria_Id as ecId, " +
                          "c1.priority as priority, " +
                          "c2.priority as prio, cast(count(*), int) as cnt from " +
                          "SupportHierarchyEvent#lastevent as c1, " +
                          "SupportHierarchyEvent#groupwin(Event_criteria_Id)#lastevent as c2, " +
                          "SupportHierarchyEvent#groupwin(Event_criteria_Id)#lastevent as p " +
                          "where c2.Event_criteria_Id in (c1.Event_criteria_Id,2,1) " +
                          "and p.Event_criteria_Id in (c1.parent_Event_criteria_Id, c1.Event_criteria_Id) " +
                          "order by c2.priority asc";
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