///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinPropertyAccess
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithRegularJoin(execs);
            WithOuterJoin(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOuterJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinOuterJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithRegularJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRegularJoin());
            return execs;
        }

        internal class EPLJoinRegularJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var combined = SupportBeanCombinedProps.MakeDefaultBean();
                var complex = SupportBeanComplexProps.MakeDefaultBean();
                Assert.AreEqual("0ma0", combined.GetIndexed(0).GetMapped("0ma").Value);

                var epl = "@name('s0') select Nested.Nested, S1.Indexed[0], Nested.Indexed[1] from " +
                          "SupportBeanComplexProps#length(3) Nested, " +
                          "SupportBeanCombinedProps#length(3) S1" +
                          " where Mapped('keyOne') = Indexed[2].Mapped('2ma').Value and" +
                          " Indexed[0].Mapped('0ma').Value = '0ma0'";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(combined);
                env.SendEventBean(complex);

                env.AssertListener(
                    "s0",
                    listener => {
                        var theEvent = listener.GetAndResetLastNewData()[0];
                        Assert.AreSame(complex.Nested, theEvent.Get("Nested.Nested"));
                        Assert.AreSame(combined.GetIndexed(0), theEvent.Get("S1.Indexed[0]"));
                        Assert.AreEqual(complex.GetIndexed(1), theEvent.Get("Nested.Indexed[1]"));
                    });

                env.UndeployAll();
            }
        }

        internal class EPLJoinOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from " +
                          "SupportBeanComplexProps#length(3) S0" +
                          " left outer join " +
                          "SupportBeanCombinedProps#length(3) S1" +
                          " on Mapped('keyOne') = Indexed[2].Mapped('2ma').Value";
                env.CompileDeploy(epl).AddListener("s0");

                var combined = SupportBeanCombinedProps.MakeDefaultBean();
                env.SendEventBean(combined);
                var complex = SupportBeanComplexProps.MakeDefaultBean();
                env.SendEventBean(complex);

                // double check that outer join criteria match
                Assert.AreEqual(complex.GetMapped("keyOne"), combined.GetIndexed(2).GetMapped("2ma").Value);

                env.AssertListener(
                    "s0",
                    listener => {
                        var theEvent = listener.GetAndResetLastNewData()[0];
                        Assert.AreEqual("Simple", theEvent.Get("S0.SimpleProperty"));
                        Assert.AreSame(complex, theEvent.Get("S0"));
                        Assert.AreSame(combined, theEvent.Get("S1"));
                    });

                env.UndeployAll();
            }
        }
    }
} // end of namespace