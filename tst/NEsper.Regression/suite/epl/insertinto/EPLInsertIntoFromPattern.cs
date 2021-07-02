///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoFromPattern
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithPropsWildcard(execs);
            WithProps(execs);
            WithNoProps(execs);
            WithFromPatternNamedWindow(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithFromPatternNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoFromPatternNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithNoProps(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoNoProps());
            return execs;
        }

        public static IList<RegressionExecution> WithProps(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoProps());
            return execs;
        }

        public static IList<RegressionExecution> WithPropsWildcard(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoPropsWildcard());
            return execs;
        }

        private static void SendEventsAndAssert(RegressionEnvironment env)
        {
            SendEventS1(env, 10, "");
            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.IsNull(theEvent.Get("es0Id"));
            Assert.AreEqual(10, theEvent.Get("es1Id"));

            env.Milestone(0);

            SendEventS0(env, 20, "");
            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(20, theEvent.Get("es0Id"));
            Assert.IsNull(theEvent.Get("es1Id"));
        }

        private static void SendEventS0(
            RegressionEnvironment env,
            int id,
            string p00)
        {
            var theEvent = new SupportBean_S0(id, p00);
            env.SendEventBean(theEvent);
        }

        private static void SendEventS1(
            RegressionEnvironment env,
            int id,
            string p10)
        {
            var theEvent = new SupportBean_S1(id, p10);
            env.SendEventBean(theEvent);
        }

        internal class EPLInsertIntoPropsWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtText = "insert into MyThirdStream(es0Id, es1Id) " +
                               "select es0.Id, es1.Id " +
                               "from " +
                               "pattern [every (es0=SupportBean_S0" +
                               " or es1=SupportBean_S1)]";
                env.CompileDeploy(stmtText, path);

                var stmtTwoText = "@Name('s0') select * from MyThirdStream";
                env.CompileDeploy(stmtTwoText, path).AddListener("s0");

                SendEventsAndAssert(env);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtText = "insert into MySecondStream(s0, s1) " +
                               "select es0, es1 " +
                               "from " +
                               "pattern [every (es0=SupportBean_S0" +
                               " or es1=SupportBean_S1)]";
                env.CompileDeploy(stmtText, path);

                var stmtTwoText = "@Name('s0') select s0.Id as es0Id, s1.Id as es1Id from MySecondStream";
                env.CompileDeploy(stmtTwoText, path).AddListener("s0");

                SendEventsAndAssert(env);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoNoProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtText = "insert into MyStream " +
                               "select es0, es1 " +
                               "from " +
                               "pattern [every (es0=SupportBean_S0" +
                               " or es1=SupportBean_S1)]";
                env.CompileDeploy(stmtText, path);

                var stmtTwoText = "@Name('s0') select es0.Id as es0Id, es1.Id as es1Id from MyStream#length(10)";
                env.CompileDeploy(stmtTwoText, path).AddListener("s0");

                SendEventsAndAssert(env);

                env.UndeployAll();
            }
        }

        public class EPLInsertIntoFromPatternNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create window PositionW.win:time(1 hour).std:unique(IntPrimitive) as select * from SupportBean",
                    path);
                env.CompileDeploy("insert into PositionW select * from SupportBean", path);
                env.CompileDeploy(
                    "@Name('s1') insert into Foo select * from pattern[every a = PositionW -> every b = PositionW]",
                    path);
                env.AddListener("s1").Milestone(0);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 1));
                Assert.IsTrue(env.Listener("s1").IsInvoked);

                env.UndeployAll();
            }
        }
    }
} // end of namespace