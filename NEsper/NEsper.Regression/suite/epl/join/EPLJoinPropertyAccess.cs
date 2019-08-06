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
            execs.Add(new EPLJoinRegularJoin());
            execs.Add(new EPLJoinOuterJoin());
            return execs;
        }

        internal class EPLJoinRegularJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var combined = SupportBeanCombinedProps.MakeDefaultBean();
                var complex = SupportBeanComplexProps.MakeDefaultBean();
                Assert.AreEqual("0ma0", combined.GetIndexed(0).GetMapped("0ma").Value);

                var epl = "@Name('s0') select nested.nested, s1.indexed[0], nested.indexed[1] from " +
                          "SupportBeanComplexProps#length(3) nested, " +
                          "SupportBeanCombinedProps#length(3) s1" +
                          " where mapped('keyOne') = indexed[2].mapped('2ma').Value and" +
                          " indexed[0].mapped('0ma').Value = '0ma0'";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(combined);
                env.SendEventBean(complex);

                var theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreSame(complex.Nested, theEvent.Get("nested.nested"));
                Assert.AreSame(combined.GetIndexed(0), theEvent.Get("s1.indexed[0]"));
                Assert.AreEqual(complex.GetIndexed(1), theEvent.Get("nested.indexed[1]"));

                env.UndeployAll();
            }
        }

        internal class EPLJoinOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from " +
                          "SupportBeanComplexProps#length(3) s0" +
                          " left outer join " +
                          "SupportBeanCombinedProps#length(3) s1" +
                          " on mapped('keyOne') = indexed[2].mapped('2ma').Value";
                env.CompileDeploy(epl).AddListener("s0");

                var combined = SupportBeanCombinedProps.MakeDefaultBean();
                env.SendEventBean(combined);
                var complex = SupportBeanComplexProps.MakeDefaultBean();
                env.SendEventBean(complex);

                // double check that outer join criteria match
                Assert.AreEqual(complex.GetMapped("keyOne"), combined.GetIndexed(2).GetMapped("2ma").Value);

                var theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("simple", theEvent.Get("s0.simpleProperty"));
                Assert.AreSame(complex, theEvent.Get("s0"));
                Assert.AreSame(combined, theEvent.Get("s1"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace