///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;
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
                var epl = "@name('A') \n" +
                          "on SupportBean_S0 event insert into AStream select transpose(" + typeof(EPLInsertIntoWrapper).MaskTypeName() + ".Transpose(event));\n" +
                          "\n" +
                          "@name('B') on AStream insert into BStream select * where PropOne;\n" +
                          "\n" +
                          "@name('C') select * from AStream;\n" +
                          "\n" +
                          "@name('D') \n" +
                          "on BStream insert into DStreamOne \n" +
                          "select * where PropTwo\n" +
                          "insert into DStreamTwo select * where not PropTwo;\n" +
                          "\n" +
                          "@name('E') on DStreamTwo\n" +
                          "insert into FinalStream select * insert into otherstream select * output all;\n" +
                          "\n" +
                          "@name('F') on DStreamOne\n" +
                          "insert into FStreamOne select * where PropThree\n" +
                          "insert into FStreamTwo select * where not PropThree;\n" +
                          "\n" +
                          "@name('G') on FStreamTwo\n" +
                          "insert into FinalStream select * insert into otherstream select * output all;\n" +
                          "\n" +
                          "@name('final') select * from FinalStream;\n";
                env.CompileDeploy(epl).AddListener("final");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1, "true", "true", "false"));
                env.AssertEqualsNew("final", "Id", 1);

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(1, "true", "true", "true"));
                env.AssertListenerNotInvoked("final");

                env.UndeployAll();
            }
        }

        public class EPLInsertIntoWrapperBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('i1') @public insert into WrappedBean select *, IntPrimitive as p0 from SupportBean",
                    path);
                env.AddListener("i1");

                env.CompileDeploy(
                    "@name('i2') @public insert into WrappedBean select sb from SupportEventContainsSupportBean sb",
                    path);
                env.AddListener("i2");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("i1", "TheString,IntPrimitive,p0".SplitCsv(), new object[] { "E1", 1, 1 });

                env.SendEventBean(new SupportEventContainsSupportBean(new SupportBean("E2", 2)));
                env.AssertPropsNew("i2", "TheString,IntPrimitive,p0".SplitCsv(), new object[] { "E2", 2, null });

                env.UndeployAll();
            }
        }

        public class EPLInsertInto3StreamWrapper : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementOne =
                    "@name('s0') @public insert into StreamA select irstream * from SupportBeanSimple#length(2)";
                var statementTwo =
                    "@name('s1') @public insert into StreamB select irstream *, MyString||'A' as propA from StreamA#length(2)";
                var statementThree =
                    "@name('s2') @public insert into StreamC select irstream *, propA||'B' as propB from StreamB#length(2)";

                var path = new RegressionPath();
                env.CompileDeploy(statementOne, path);
                env.CompileDeploy(statementTwo, path);
                env.CompileDeploy(statementThree, path).AddListener("s2");

                env.Milestone(0);

                env.SendEventBean(new SupportBeanSimple("e1", 1));
                env.AssertEventNew(
                    "s2",
                    @event => {
                        ClassicAssert.AreEqual("e1", @event.Get("MyString"));
                        ClassicAssert.AreEqual("e1AB", @event.Get("propB"));
                    });

                env.Milestone(1);

                env.SendEventBean(new SupportBeanSimple("e2", 1));
                env.AssertEventNew(
                    "s2",
                    @event => {
                        ClassicAssert.AreEqual("e2", @event.Get("MyString"));
                        ClassicAssert.AreEqual("e2AB", @event.Get("propB"));
                    });

                env.SendEventBean(new SupportBeanSimple("e3", 1));
                env.AssertListener(
                    "s2",
                    listener => {
                        var @event = listener.LastNewData[0];
                        ClassicAssert.AreEqual("e3", @event.Get("MyString"));
                        ClassicAssert.AreEqual("e3AB", @event.Get("propB"));
                        @event = listener.LastOldData[0];
                        ClassicAssert.AreEqual("e1", @event.Get("MyString"));
                        ClassicAssert.AreEqual("e1AB", @event.Get("propB"));
                    });

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