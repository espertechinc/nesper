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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.common.@internal.support.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoTransposePattern
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoThisAsColumn());
            execs.Add(new EPLInsertIntoTransposePONOEventPattern());
            execs.Add(new EPLInsertIntoTransposeMapEventPattern());
            return execs;
        }

        private static IDictionary<string, object> MakeMap(object[][] entries)
        {
            var result = new Dictionary<string, object>();
            foreach (var entry in entries) {
                result.Put((string) entry[0], entry[1]);
            }

            return result;
        }

        internal class EPLInsertIntoThisAsColumn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('window') create window OneWindow#time(1 day) as select TheString as alertId, This from SupportBean",
                    path);
                env.CompileDeploy(
                    "insert into OneWindow select '1' as alertId, stream0.quote.This as This " +
                    " from pattern [every quote=SupportBean(TheString='A')] as stream0",
                    path);
                env.CompileDeploy(
                    "insert into OneWindow select '2' as alertId, stream0.quote as This " +
                    " from pattern [every quote=SupportBean(TheString='B')] as stream0",
                    path);

                env.SendEventBean(new SupportBean("A", 10));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("window"),
                    new[] {"alertId", "this.IntPrimitive"},
                    new[] {new object[] {"1", 10}});

                env.SendEventBean(new SupportBean("B", 20));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("window"),
                    new[] {"alertId", "this.IntPrimitive"},
                    new[] {new object[] {"1", 10}, new object[] {"2", 20}});

                env.CompileDeploy(
                    "@Name('window-2') create window TwoWindow#time(1 day) as select TheString as alertId, * from SupportBean",
                    path);
                env.CompileDeploy(
                    "insert into TwoWindow select '3' as alertId, quote.* " +
                    " from pattern [every quote=SupportBean(TheString='C')] as stream0",
                    path);

                env.SendEventBean(new SupportBean("C", 30));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("window-2"),
                    new[] {"alertId", "IntPrimitive"},
                    new[] {new object[] {"3", 30}});

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoTransposePONOEventPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne =
                    "insert into MyStreamABBean select a, b from pattern [a=SupportBean_A -> b=SupportBean_B]";
                env.CompileDeploy(stmtTextOne, path);

                var stmtTextTwo = "@Name('s0') select a.Id, b.Id from MyStreamABBean";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_B("B1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.Id","b.Id" },
                    new object[] {"A1", "B1"});

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoTransposeMapEventPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne =
                    "@Name('i1') insert into MyStreamABMap select a, b from pattern [a=AEventMap -> b=BEventMap]";
                env.CompileDeploy(stmtTextOne, path).AddListener("i1");
                Assert.AreEqual(
                    typeof(IDictionary<string, object>),
                    env.Statement("i1").EventType.GetPropertyType("a"));
                Assert.AreEqual(
                    typeof(IDictionary<string, object>),
                    env.Statement("i1").EventType.GetPropertyType("b"));

                var stmtTextTwo = "@Name('s0') select a.Id, b.Id from MyStreamABMap";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");
                Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("a.Id"));
                Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("b.Id"));

                var eventOne = MakeMap(new[] {new object[] {"Id", "A1"}});
                var eventTwo = MakeMap(new[] {new object[] {"Id", "B1"}});

                env.SendEventMap(eventOne, "AEventMap");
                env.SendEventMap(eventTwo, "BEventMap");

                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    theEvent,
                    new [] { "a.Id","b.Id" },
                    new object[] {"A1", "B1"});

                theEvent = env.Listener("i1").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    theEvent,
                    new [] { "a","b" },
                    new object[] {eventOne, eventTwo});

                env.UndeployAll();
            }
        }
    }
} // end of namespace