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
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewParameterizedByContext
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ViewParameterizedByContextLengthWindow());
            execs.Add(new ViewParameterizedByContextDocSample());
            execs.Add(new ViewParameterizedByContextMoreWindows());
            return execs;
        }

        private static void RunAssertionWindow(
            RegressionEnvironment env,
            string window,
            AtomicLong milestone)
        {
            var epl =
                "create context CtxInitToTerm initiated by SupportContextInitEventWLength as miewl terminated after 1 year;\n" +
                "context CtxInitToTerm select * from SupportBean#" +
                window;
            env.CompileDeploy(epl);
            env.SendEventBean(new SupportContextInitEventWLength("P1", 2));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportContextInitEventWLength("P2", 20));

            env.UndeployAll();
        }

        internal class ViewParameterizedByContextMoreWindows : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                RunAssertionWindow(env, "length_batch(context.miewl.intSize)", milestone);
                RunAssertionWindow(env, "time(context.miewl.intSize)", milestone);
                RunAssertionWindow(env, "ext_timed(longPrimitive, context.miewl.intSize)", milestone);
                RunAssertionWindow(env, "time_batch(context.miewl.intSize)", milestone);
                RunAssertionWindow(env, "ext_timed_batch(longPrimitive, context.miewl.intSize)", milestone);
                RunAssertionWindow(env, "time_length_batch(context.miewl.intSize, context.miewl.intSize)", milestone);
                RunAssertionWindow(env, "time_accum(context.miewl.intSize)", milestone);
                RunAssertionWindow(env, "firstlength(context.miewl.intSize)", milestone);
                RunAssertionWindow(env, "firsttime(context.miewl.intSize)", milestone);
                RunAssertionWindow(env, "sort(context.miewl.intSize, IntPrimitive)", milestone);
                RunAssertionWindow(env, "rank(theString, context.miewl.intSize, theString)", milestone);
                RunAssertionWindow(env, "time_order(longPrimitive, context.miewl.intSize)", milestone);
            }
        }

        internal class ViewParameterizedByContextLengthWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context CtxInitToTerm initiated by SupportContextInitEventWLength as miewl terminated after 1 year;\n" +
                    "@Name('s0') context CtxInitToTerm select context.miewl.id as id, count(*) as cnt from SupportBean(theString=context.miewl.id)#length(context.miewl.intSize)";
                env.CompileDeploy(epl).AddListener("s0");
                var fields = "id,cnt".SplitCsv();

                SendInitEvent(env, "P1", 2);
                SendInitEvent(env, "P2", 4);
                SendInitEvent(env, "P3", 3);
                SendValueEvent(env, "P2");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"P1", 0L}, new object[] {"P2", 1L}, new object[] {"P3", 0L}});

                env.Milestone(0);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"P1", 0L}, new object[] {"P2", 1L}, new object[] {"P3", 0L}});

                for (var i = 0; i < 10; i++) {
                    SendValueEvent(env, "P1");
                    SendValueEvent(env, "P2");
                    SendValueEvent(env, "P3");
                }

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"P1", 2L}, new object[] {"P2", 4L}, new object[] {"P3", 3L}});

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"P1", 2L}, new object[] {"P2", 4L}, new object[] {"P3", 3L}});
                SendValueEvent(env, "P1");
                SendValueEvent(env, "P2");
                SendValueEvent(env, "P3");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"P1", 2L}, new object[] {"P2", 4L}, new object[] {"P3", 3L}});

                env.UndeployAll();
            }

            private void SendValueEvent(
                RegressionEnvironment env,
                string id)
            {
                env.SendEventBean(new SupportBean(id, -1));
            }

            private void SendInitEvent(
                RegressionEnvironment env,
                string id,
                int intSize)
            {
                env.SendEventBean(new SupportContextInitEventWLength(id, intSize));
            }
        }

        internal class ViewParameterizedByContextDocSample : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context CtxInitToTerm initiated by SupportContextInitEventWLength as miewl terminated after 1 year;\n" +
                    "@Name('s0') context CtxInitToTerm select context.miewl.id as id, count(*) as cnt from SupportBean(theString=context.miewl.id)#length(context.miewl.intSize);\n";
                env.CompileDeploy(epl).Milestone(0);

                env.SendEventBean(new SupportContextInitEventWLength("P1", 2));
                env.SendEventBean(new SupportContextInitEventWLength("P2", 4));
                env.SendEventBean(new SupportContextInitEventWLength("P3", 3));

                env.Milestone(1);

                for (var i = 0; i < 10; i++) {
                    env.SendEventBean(new SupportBean("P1", 0));
                    env.SendEventBean(new SupportBean("P2", 0));
                    env.SendEventBean(new SupportBean("P3", 0));
                }

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    "id,cnt".SplitCsv(),
                    new[] {new object[] {"P1", 2L}, new object[] {"P2", 4L}, new object[] {"P3", 3L}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace