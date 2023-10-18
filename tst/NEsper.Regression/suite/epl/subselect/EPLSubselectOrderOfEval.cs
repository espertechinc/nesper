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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectOrderOfEval
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithCorrelatedSubqueryOrder(execs);
            WithOrderOfEvaluationSubselectFirst(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOrderOfEvaluationSubselectFirst(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectOrderOfEvaluationSubselectFirst());
            return execs;
        }

        public static IList<RegressionExecution> WithCorrelatedSubqueryOrder(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectCorrelatedSubqueryOrder());
            return execs;
        }

        internal class EPLSubselectCorrelatedSubqueryOrder : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "select * from SupportTradeEventTwo#lastevent;\n" +
                          "@name('s0') select window(tl.*) as longItems, " +
                          "       (SELECT window(ts.*) AS shortItems FROM SupportTradeEventTwo#time(20 minutes) as ts WHERE ts.SecurityID=tl.SecurityID) " +
                          "from SupportTradeEventTwo#time(20 minutes) as tl " +
                          "where tl.SecurityID = 1000" +
                          "group by tl.SecurityID";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportTradeEventTwo(PerformanceObserver.MilliTime, 1000, 50, 1));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        Assert.AreEqual(1, ((object[])@event.Get("longItems")).Length);
                        Assert.AreEqual(1, ((object[])@event.Get("shortItems")).Length);
                    });

                env.SendEventBean(new SupportTradeEventTwo(PerformanceObserver.MilliTime + 10, 1000, 50, 1));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        Assert.AreEqual(2, ((object[])@event.Get("longItems")).Length);
                        Assert.AreEqual(2, ((object[])@event.Get("shortItems")).Length);
                    });

                env.UndeployAll();
            }
        }

        internal class EPLSubselectOrderOfEvaluationSubselectFirst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') @Name('s0')select * from SupportBean(IntPrimitive<10) where IntPrimitive not in (select IntPrimitive from SupportBean#unique(IntPrimitive))";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 5));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();

                var eplTwo =
                    "@name('s0') select * from SupportBean where IntPrimitive not in (select IntPrimitive from SupportBean(IntPrimitive<10)#unique(IntPrimitive))";
                env.CompileDeployAddListenerMile(eplTwo, "s0", 1);

                env.SendEventBean(new SupportBean("E1", 5));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }
    }
} // end of namespace