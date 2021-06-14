///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBeanSimple = com.espertech.esper.regressionlib.support.bean.SupportBeanSimple;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoWrapper
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithWrapperBean(execs);
            With3StreamWrapper(execs);
            WithOnSplitForkJoin(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOnSplitForkJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoOnSplitForkJoin());
            return execs;
        }

        public static IList<RegressionExecution> With3StreamWrapper(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertInto3StreamWrapper());
            return execs;
        }

        public static IList<RegressionExecution> WithWrapperBean(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoWrapperBean());
            return execs;
        }

        public class EPLInsertIntoOnSplitForkJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('A') \n" +
                             "on SupportBean_S0 event insert into AStream select transpose(" +
                             typeof(EPLInsertIntoWrapper).MaskTypeName() +
                             ".Transpose(event));\n" +
                             "\n" +
                             "@Name('B') on AStream insert into BStream select * where PropOne;\n" +
                             "\n" +
                             "@Name('C') select * from AStream;\n" +
                             "\n" +
                             "@Name('D') \n" +
                             "on BStream insert into DStreamOne \n" +
                             "select * where PropTwo\n" +
                             "insert into DStreamTwo select * where not PropTwo;\n" +
                             "\n" +
                             "@Name('E') on DStreamTwo\n" +
                             "insert into FinalStream select * insert into otherstream select * output all;\n" +
                             "\n" +
                             "@Name('F') on DStreamOne\n" +
                             "insert into FStreamOne select * where PropThree\n" +
                             "insert into FStreamTwo select * where not PropThree;\n" +
                             "\n" +
                             "@Name('G') on FStreamTwo\n" +
                             "insert into FinalStream select * insert into otherstream select * output all;\n" +
                             "\n" +
                             "@Name('final') select * from FinalStream;\n";
                env.CompileDeploy(epl).AddListener("final");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1, "true", "true", "false"));
                Assert.AreEqual(1, env.Listener("final").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(1, "true", "true", "true"));
                Assert.IsFalse(env.Listener("final").IsInvoked);

                env.UndeployAll();
            }
        }

        public class EPLInsertIntoWrapperBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RegressionPath path = new RegressionPath();
                env.CompileDeploy("@Name('i1') insert into WrappedBean select *, IntPrimitive as p0 from SupportBean", path);
                env.AddListener("i1");

                env.CompileDeploy("@Name('i2') insert into WrappedBean select sb from SupportEventContainsSupportBean sb", path);
                env.AddListener("i2");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(env.Listener("i1").AssertOneGetNewAndReset(), "TheString,IntPrimitive,p0".SplitCsv(), new object[] {"E1", 1, 1});

                env.SendEventBean(new SupportEventContainsSupportBean(new SupportBean("E2", 2)));
                EPAssertionUtil.AssertProps(env.Listener("i2").AssertOneGetNewAndReset(), "TheString,IntPrimitive,p0".SplitCsv(), new object[] {"E2", 2, null});

                env.UndeployAll();
            }
        }

        public class EPLInsertInto3StreamWrapper : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string statementOne = "@Name('s0') insert into StreamA select irstream * from SupportBeanSimple#length(2)";
                string statementTwo = "@Name('s1') insert into StreamB select irstream *, MyString||'A' as propA from StreamA#length(2)";
                string statementThree = "@Name('s2') insert into StreamC select irstream *, propA||'B' as propB from StreamB#length(2)";

                RegressionPath path = new RegressionPath();
                env.CompileDeploy(statementOne, path);
                env.CompileDeploy(statementTwo, path);
                env.CompileDeploy(statementThree, path).AddListener("s2");

                env.Milestone(0);

                env.SendEventBean(new SupportBeanSimple("e1", 1));
                EventBean @event = env.Listener("s2").AssertOneGetNewAndReset();
                Assert.AreEqual("e1", @event.Get("MyString"));
                Assert.AreEqual("e1AB", @event.Get("propB"));

                env.Milestone(1);

                env.SendEventBean(new SupportBeanSimple("e2", 1));
                @event = env.Listener("s2").AssertOneGetNewAndReset();
                Assert.AreEqual("e2", @event.Get("MyString"));
                Assert.AreEqual("e2AB", @event.Get("propB"));

                env.SendEventBean(new SupportBeanSimple("e3", 1));
                @event = env.Listener("s2").LastNewData[0];
                Assert.AreEqual("e3", @event.Get("MyString"));
                Assert.AreEqual("e3AB", @event.Get("propB"));
                @event = env.Listener("s2").LastOldData[0];
                Assert.AreEqual("e1", @event.Get("MyString"));
                Assert.AreEqual("e1AB", @event.Get("propB"));

                env.UndeployAll();
            }
        }

        public static MyEvent Transpose(SupportBean_S0 bean)
        {
            return new MyEvent(
                bean.Id,
                bean.P00.Equals("true"),
                bean.P01.Equals("true"),
                bean.P02.Equals("true"));
        }

        public class MyEvent
        {
            public MyEvent(
                int id,
                bool propOne,
                bool propTwo,
                bool propThree)
            {
                Id = id;
                PropOne = propOne;
                PropTwo = propTwo;
                PropThree = propThree;
            }

            public int Id { get; }

            public bool PropOne { get; }

            public bool PropTwo { get; }

            public bool PropThree { get; }
        }
    }
} // end of namespace