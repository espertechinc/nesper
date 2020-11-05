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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowConsumer
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowConsumerKeepAll());
            execs.Add(new InfraNamedWindowConsumerLengthWin());
            execs.Add(new InfraNamedWindowConsumerWBatch());
            return execs;
        }

        public class InfraNamedWindowConsumerKeepAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString" };
                var epl = "@Name('create') create window MyWindow.win:keepall() as SupportBean;\n" +
                          "@Name('insert') insert into MyWindow select * from SupportBean;\n" +
                          "@Name('select') select irstream * from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("select");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.UndeployAll();

                env.Milestone(2);
            }
        }

        public class InfraNamedWindowConsumerLengthWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1" };
                var epl = "create window MyWindow#length(2) as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "@Name('s0') select TheString as c0, sum(IntPrimitive) as c1 from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10});

                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 30});

                env.SendEventBean(new SupportBean("E3", 25));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 45});

                env.SendEventBean(new SupportBean("E4", 26));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 51});

                env.UndeployAll();
            }
        }

        public class InfraNamedWindowConsumerWBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create schema IncomingEvent(Id int);\n" +
                          "create schema RetainedEvent(Id int);\n" +
                          "insert into RetainedEvent select * from IncomingEvent#expr_batch(current_count >= 10000);\n" +
                          "create window RetainedEventWindow#keepall as RetainedEvent;\n" +
                          "insert into RetainedEventWindow select * from RetainedEvent;\n";
                env.CompileDeployWBusPublicType(epl, new RegressionPath());

                IDictionary<string, object> @event = new Dictionary<string, object>();
                @event.Put("Id", 1);
                for (var i = 0; i < 10000; i++) {
                    env.SendEventMap(@event, "IncomingEvent");
                }

                env.UndeployAll();
            }
        }
    }
} // end of namespace