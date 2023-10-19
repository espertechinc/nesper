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

using NUnit.Framework; // assertEquals

// assertTrue

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectMultirow
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSingleColumn(execs);
            WithUnderlyingCorrelated(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithUnderlyingCorrelated(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectMultirowUnderlyingCorrelated());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleColumn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectMultirowSingleColumn());
            return execs;
        }

        private class EPLSubselectMultirowSingleColumn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                // test named window as well as stream
                var epl = "@public create window SupportWindow#length(3) as SupportBean;\n" +
                          "insert into SupportWindow select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                epl =
                    "@name('s0') select p00, (select window(intPrimitive) from SupportBean#keepall sb) as val from SupportBean_S0 as s0;\n";
                env.CompileDeploy(epl, path).AddListener("s0").Milestone(0);

                var fields = "p00,val".SplitCsv();

                env.AssertStatement(
                    "s0",
                    statement => {
                        var rows = new object[][] {
                            new object[] { "p00", typeof(string) },
                            new object[] { "val", typeof(int?[]) }
                        };
                        for (var i = 0; i < rows.Length; i++) {
                            var message = "Failed assertion for " + rows[i][0];
                            var prop = statement.EventType.PropertyDescriptors[i];
                            Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                            Assert.AreEqual(rows[i][1], prop.PropertyType, message);
                        }
                    });

                env.SendEventBean(new SupportBean("T1", 5));
                env.SendEventBean(new SupportBean("T2", 10));
                env.SendEventBean(new SupportBean("T3", 15));
                env.SendEventBean(new SupportBean("T1", 6));
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        Assert.IsTrue(@event.Get("val") is int?[]);
                        EPAssertionUtil.AssertProps(@event, fields, new object[] { null, new int?[] { 5, 10, 15, 6 } });
                    });

                // test named window and late start
                env.UndeployModuleContaining("s0");

                epl =
                    "@name('s0') select p00, (select window(intPrimitive) from SupportWindow) as val from SupportBean_S0 as s0";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(0));
                env.AssertPropsNew("s0", fields, new object[] { null, new int?[] { 10, 15, 6 } }); // length window 3

                env.SendEventBean(new SupportBean("T1", 5));
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertPropsNew("s0", fields, new object[] { null, new int?[] { 15, 6, 5 } }); // length window 3

                env.UndeployAll();
            }
        }

        private class EPLSubselectMultirowUnderlyingCorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select p00, " +
                               "(select window(sb.*) from SupportBean#keepall sb where theString = s0.p00) as val " +
                               "from SupportBean_S0 as s0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var rows = new object[][] {
                            new object[] { "p00", typeof(string) },
                            new object[] { "val", typeof(SupportBean[]) }
                        };
                        for (var i = 0; i < rows.Length; i++) {
                            var message = "Failed assertion for " + rows[i][0];
                            var prop = statement.EventType.PropertyDescriptors[i];
                            Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                            Assert.AreEqual(rows[i][1], prop.PropertyType, message);
                        }
                    });

                env.SendEventBean(new SupportBean_S0(1, "T1"));
                env.AssertEqualsNew("s0", "val", null);

                var sb1 = new SupportBean("T1", 10);
                env.SendEventBean(sb1);
                env.SendEventBean(new SupportBean_S0(2, "T1"));

                env.AssertEventNew(
                    "s0",
                    received => {
                        Assert.AreEqual(typeof(SupportBean[]), received.Get("val").GetType());
                        EPAssertionUtil.AssertEqualsAnyOrder((object[])received.Get("val"), new object[] { sb1 });
                    });

                var sb2 = new SupportBean("T2", 20);
                env.SendEventBean(sb2);
                var sb3 = new SupportBean("T2", 30);
                env.SendEventBean(sb3);
                env.SendEventBean(new SupportBean_S0(3, "T2"));

                env.AssertEventNew(
                    "s0",
                    received => EPAssertionUtil.AssertEqualsAnyOrder(
                        (object[])received.Get("val"),
                        new object[] { sb2, sb3 }));

                env.UndeployAll();
            }
        }
    }
} // end of namespace