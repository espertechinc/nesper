///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoTransposePattern
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithThisAsColumn(execs);
            WithTransposePONOEventPattern(execs);
            WithTransposeMapEventPattern(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTransposeMapEventPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTransposeMapEventPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithTransposePONOEventPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTransposePONOEventPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithThisAsColumn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoThisAsColumn());
            return execs;
        }

        private class EPLInsertIntoThisAsColumn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('window') @public create window OneWindow#time(1 day) as select TheString as alertId, This from SupportBeanWithThis",
                    path);
                env.CompileDeploy(
                    "insert into OneWindow select '1' as alertId, stream0.quote.This as This " +
                    " from pattern [every quote=SupportBeanWithThis(TheString='A')] as stream0",
                    path);
                env.CompileDeploy(
                    "insert into OneWindow select '2' as alertId, stream0.quote as This " +
                    " from pattern [every quote=SupportBeanWithThis(TheString='B')] as stream0",
                    path);

                env.SendEventBean(new SupportBeanWithThis("A", 10));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "window",
                    new[] { "alertId", "This.IntPrimitive" },
                    new[] { new object[] { "1", 10 } });

                env.SendEventBean(new SupportBeanWithThis("B", 20));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "window",
                    new[] { "alertId", "This.IntPrimitive" },
                    new[] { new object[] { "1", 10 }, new object[] { "2", 20 } });

                env.CompileDeploy(
                    "@name('window-2') @public create window TwoWindow#time(1 day) as select TheString as alertId, * from SupportBeanWithThis",
                    path);
                env.CompileDeploy(
                    "insert into TwoWindow select '3' as alertId, quote.* " +
                    " from pattern [every quote=SupportBeanWithThis(TheString='C')] as stream0",
                    path);

                env.SendEventBean(new SupportBeanWithThis("C", 30));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "window-2",
                    new[] { "alertId", "IntPrimitive" },
                    new[] { new object[] { "3", 30 } });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoTransposePONOEventPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne =
                    "@public insert into MyStreamABBean select a, b from pattern [a=SupportBean_A -> b=SupportBean_B]";
                env.CompileDeploy(stmtTextOne, path);

                var stmtTextTwo = "@name('s0') select a.Id, b.Id from MyStreamABBean";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_B("B1"));
                env.AssertPropsNew("s0", "a.Id,b.Id".SplitCsv(), new object[] { "A1", "B1" });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoTransposeMapEventPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne =
                    "@name('i1') @public insert into MyStreamABMap select a, b from pattern [a=AEventMap -> b=BEventMap]";
                env.CompileDeploy(stmtTextOne, path).AddListener("i1");
                env.AssertStatement(
                    "i1",
                    statement => {
                        ClassicAssert.AreEqual(typeof(IDictionary<string, object>), statement.EventType.GetPropertyType("a"));
                        ClassicAssert.AreEqual(typeof(IDictionary<string, object>), statement.EventType.GetPropertyType("b"));
                    });

                var stmtTextTwo = "@name('s0') select a.Id, b.Id from MyStreamABMap";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => {
                        ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("a.Id"));
                        ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("b.Id"));
                    });

                var eventOne = MakeMap(new[] { new object[] { "Id", "A1" } });
                var eventTwo = MakeMap(new[] { new object[] { "Id", "B1" } });

                env.SendEventMap(eventOne, "AEventMap");
                env.SendEventMap(eventTwo, "BEventMap");

                env.AssertPropsNew("s0", "a.Id,b.Id".SplitCsv(), new object[] { "A1", "B1" });
                env.AssertPropsNew("i1", "a,b".SplitCsv(), new object[] { eventOne, eventTwo });

                env.UndeployAll();
            }
        }

        private static IDictionary<string, object> MakeMap(object[][] entries)
        {
            var result = new Dictionary<string, object>();
            foreach (var entry in entries) {
                result.Put((string)entry[0], entry[1]);
            }

            return result;
        }
    }
} // end of namespace