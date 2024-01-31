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
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoPopulateEventTypeColumnBean
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithFromSubquerySingle(execs);
            WithFromSubqueryMulti(execs);
            WithSingleToMulti(execs);
            WithMultiToSingle(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoColBeanInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiToSingle(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoColBeanMultiToSingle());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleToMulti(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoColBeanSingleToMulti());
            return execs;
        }

        public static IList<RegressionExecution> WithFromSubqueryMulti(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoColBeanFromSubqueryMulti("objectarray", false));
            execs.Add(new EPLInsertIntoColBeanFromSubqueryMulti("objectarray", true));
            execs.Add(new EPLInsertIntoColBeanFromSubqueryMulti("map", false));
            execs.Add(new EPLInsertIntoColBeanFromSubqueryMulti("map", true));
            return execs;
        }

        public static IList<RegressionExecution> WithFromSubquerySingle(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoColBeanFromSubquerySingle("objectarray", false));
            execs.Add(new EPLInsertIntoColBeanFromSubquerySingle("objectarray", true));
            execs.Add(new EPLInsertIntoColBeanFromSubquerySingle("map", false));
            execs.Add(new EPLInsertIntoColBeanFromSubquerySingle("map", true));
            return execs;
        }

        private class EPLInsertIntoColBeanMultiToSingle : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create schema EventOne(sb SupportBean)", path);
                env.CompileDeploy(
                    "insert into EventOne select (select * from SupportBean#keepall) as sb from SupportBean_S0",
                    path);
                env.CompileDeploy("@name('s0') select * from EventOne#keepall", path).AddListener("s0");

                var bean = new SupportBean("E1", 1);
                env.SendEventBean(bean);
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEventNew("s0", @event => { ClassicAssert.AreSame(bean, @event.Get("sb")); });

                env.Milestone(0);

                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "sb.TheString" },
                    new object[][] { new object[] { "E1" } });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoColBeanSingleToMulti : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create schema EventOne(sbarr SupportBean[])", path);
                env.CompileDeploy(
                    "insert into EventOne select maxby(IntPrimitive) as sbarr from SupportBean as sb",
                    path);
                env.CompileDeploy("@name('s0') select * from EventOne#keepall", path).AddListener("s0");

                var bean = new SupportBean("E1", 1);
                env.SendEventBean(bean);
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var events = (SupportBean[])@event.Get("sbarr");
                        ClassicAssert.AreEqual(1, events.Length);
                        ClassicAssert.AreSame(bean, events[0]);
                    });

                env.Milestone(0);

                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "sbarr[0].TheString" },
                    new object[][] { new object[] { "E1" } });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoColBeanFromSubqueryMulti : RegressionExecution
        {
            private readonly string typeType;
            private readonly bool filter;

            public EPLInsertIntoColBeanFromSubqueryMulti(
                string typeType,
                bool filter)
            {
                this.typeType = typeType;
                this.filter = filter;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create " + typeType + " schema EventOne(sbarr SupportBean_S0[])", path);

                env.CompileDeploy(
                        "@name('s0') @public insert into EventOne select " +
                        "(select * from SupportBean_S0#keepall " +
                        (filter ? "where 1=1" : "") +
                        ") as sbarr " +
                        "from SupportBean",
                        path)
                    .AddListener("s0");
                env.CompileDeploy("@name('s1') select * from EventOne#keepall", path);

                var s0One = new SupportBean_S0(1, "x1");
                env.SendEventBean(s0One);
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEventNew("s0", @event => AssertS0(@event, s0One));

                env.Milestone(0);

                var s0Two = new SupportBean_S0(2, "x2", "y2");
                env.SendEventBean(s0Two);
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertEventNew("s0", @event => AssertS0(@event, s0One, s0Two));
                env.AssertPropsPerRowIterator(
                    "s1",
                    "sbarr[0].Id,sbarr[1].Id".Split(","),
                    new object[][] { new object[] { 1, null }, new object[] { 1, 2 } });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "typeType='" +
                       typeType +
                       '\'' +
                       ", filter=" +
                       filter +
                       '}';
            }
        }

        private class EPLInsertIntoColBeanFromSubquerySingle : RegressionExecution
        {
            private readonly string typeType;
            private readonly bool filter;

            public EPLInsertIntoColBeanFromSubquerySingle(
                string typeType,
                bool filter)
            {
                this.typeType = typeType;
                this.filter = filter;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create " + typeType + " schema EventOne(sb SupportBean_S0)", path);

                var fields = "sb.P00".Split(",");
                var epl = "@name('s0') insert into EventOne select " +
                          "(select * from SupportBean_S0#length(2) " +
                          (filter ? "where Id >= 100" : "") +
                          ") as sb " +
                          "from SupportBean;\n " +
                          "@name('s1') select * from EventOne#keepall";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "x1"));
                env.SendEventBean(new SupportBean("E1", 1));
                var expected = filter ? new object[] { null } : new object[] { "x1" };
                env.AssertPropsNew("s0", fields, expected);

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(100, "x2"));
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertEqualsNew("s0", fields[0], filter ? "x2" : null);
                if (!filter) {
                    env.AssertPropsPerRowIterator(
                        "s1",
                        "sb.Id".Split(","),
                        new object[][] { new object[] { 1 }, new object[] { null } });
                }

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "typeType='" +
                       typeType +
                       '\'' +
                       ", filter=" +
                       filter +
                       '}';
            }
        }

        private class EPLInsertIntoColBeanInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // enumeration type is incompatible
                env.CompileDeploy("@public create schema TypeOne(sbs SupportBean[])", path);
                env.TryInvalidCompile(
                    path,
                    "insert into TypeOne select (select * from SupportBean_S0#keepall) as sbs from SupportBean_S1",
                    $"Incompatible type detected attempting to insert into column 'sbs' type '{typeof(SupportBean).CleanName()}' compared to selected type 'SupportBean_S0' [insert into TypeOne select (select * from SupportBean_S0#keepall) as sbs from SupportBean_S1]");

                env.CompileDeploy("@public create schema TypeTwo(sbs SupportBean)", path);
                env.TryInvalidCompile(
                    path,
                    "insert into TypeTwo select (select * from SupportBean_S0#keepall) as sbs from SupportBean_S1",
                    $"Incompatible type detected attempting to insert into column 'sbs' type '{typeof(SupportBean).CleanName()}' compared to selected type 'SupportBean_S0'");

                env.UndeployAll();
            }
        }

        private static void AssertS0(
            EventBean @event,
            params SupportBean_S0[] expected)
        {
            var inner = (SupportBean_S0[])@event.Get("sbarr");
            EPAssertionUtil.AssertEqualsExactOrder(expected, inner);
        }
    }
} // end of namespace