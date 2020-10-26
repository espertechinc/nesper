///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextInitTermPrioritized
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new NonOverlappingSubqueryAndInvalid());
            execs.Add(new AtNowWithSelectedEventEnding());
            return execs;
        }

        private static void SendTimeEvent(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        internal class NonOverlappingSubqueryAndInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T10:00:00.000");

                var path = new RegressionPath();
                var epl =
                    "\n @Name('ctx') create context RuleActivityTime as start (0, 9, *, *, *) end (0, 17, *, *, *);" +
                    "\n @Name('window') context RuleActivityTime create window EventsWindow#firstunique(ProductID) as SupportProductIdEvent;" +
                    "\n @Name('variable') create variable boolean IsOutputTriggered_2 = false;" +
                    "\n @Name('A') context RuleActivityTime insert into EventsWindow select * from SupportProductIdEvent(not exists (select * from EventsWindow));" +
                    "\n @Name('B') context RuleActivityTime insert into EventsWindow select * from SupportProductIdEvent(not exists (select * from EventsWindow));" +
                    "\n @Name('C') context RuleActivityTime insert into EventsWindow select * from SupportProductIdEvent(not exists (select * from EventsWindow));" +
                    "\n @Name('D') context RuleActivityTime insert into EventsWindow select * from SupportProductIdEvent(not exists (select * from EventsWindow));" +
                    "\n @Name('out') context RuleActivityTime select * from EventsWindow";
                env.CompileDeploy(epl, path).AddListener("out");

                env.SendEventBean(new SupportProductIdEvent("A1"));

                // invalid - subquery not the same context
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "insert into EventsWindow select * from SupportProductIdEvent(not exists (select * from EventsWindow))",
                    "Failed to validate subquery number 1 querying EventsWindow: Named window by name 'EventsWindow' has been declared for context 'RuleActivityTime' and can only be used within the same context");

                env.UndeployAll();
            }
        }

        internal class AtNowWithSelectedEventEnding : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString" };
                var epl = "@Priority(1) create context C1 start @now end SupportBean;\n" +
                          "@Name('s0') @Priority(0) context C1 select * from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.UndeployAll();
            }
        }
    }
} // end of namespace