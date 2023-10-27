///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    /// NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowConsumer
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithKeepAll(execs);
            WithLengthWin(execs);
            WithWBatch(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowConsumerWBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthWin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowConsumerLengthWin());
            return execs;
        }

        public static IList<RegressionExecution> WithKeepAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowConsumerKeepAll());
            return execs;
        }

        public class InfraNamedWindowConsumerKeepAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString".SplitCsv();
                var epl = "@name('create') create window MyWindow.win:keepall() as SupportBean;\n" +
                          "@name('insert') insert into MyWindow select * from SupportBean;\n" +
                          "@name('select') select irstream * from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("select");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("select", fields, new object[] { "E1" });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 10));
                env.AssertPropsNew("select", fields, new object[] { "E2" });

                env.UndeployAll();

                env.Milestone(2);
            }
        }

        public class InfraNamedWindowConsumerLengthWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var epl = "create window MyWindow#length(2) as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "@name('s0') select TheString as c0, sum(IntPrimitive) as c1 from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10 });

                env.SendEventBean(new SupportBean("E2", 20));
                env.AssertPropsNew("s0", fields, new object[] { "E2", 30 });

                env.SendEventBean(new SupportBean("E3", 25));
                env.AssertPropsNew("s0", fields, new object[] { "E3", 45 });

                env.SendEventBean(new SupportBean("E4", 26));
                env.AssertPropsNew("s0", fields, new object[] { "E4", 51 });

                env.UndeployAll();
            }
        }

        public class InfraNamedWindowConsumerWBatch : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@buseventtype @public create schema IncomingEvent(Id int);\n" +
                          "create schema RetainedEvent(Id int);\n" +
                          "insert into RetainedEvent select * from IncomingEvent#expr_batch(current_count >= 10000);\n" +
                          "create window RetainedEventWindow#keepall as RetainedEvent;\n" +
                          "insert into RetainedEventWindow select * from RetainedEvent;\n";
                env.CompileDeploy(epl, new RegressionPath());

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