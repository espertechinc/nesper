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
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableComparative
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            var eplNamedWindow =
                "create window TotalsWindow#unique(TheString) as (TheString string, total int);" +
                "insert into TotalsWindow select TheString, sum(IntPrimitive) as total from SupportBean group by TheString;" +
                "@Name('s0') select P00 as c0, " +
                "    (select total from TotalsWindow tw where tw.TheString = s0.P00) as c1 from SupportBean_S0 as s0;";
            execs.Add(new InfraNWTableComparativeGroupByTopLevelSingleAgg("named window", 1000, eplNamedWindow, 1));

            var eplTable =
                "create table varTotal (key string primary key, total sum(int));\n" +
                "into table varTotal select TheString, sum(IntPrimitive) as total from SupportBean group by TheString;\n" +
                "@Name('s0') select P00 as c0, varTotal[P00].total as c1 from SupportBean_S0;\n";
            execs.Add(new InfraNWTableComparativeGroupByTopLevelSingleAgg("table", 1000, eplTable, 1));
            return execs;
        }

        internal class InfraNWTableComparativeGroupByTopLevelSingleAgg : RegressionExecution
        {
            private readonly string caseName;
            private readonly string epl;
            private readonly int numEvents;
            private readonly int numSets;

            public InfraNWTableComparativeGroupByTopLevelSingleAgg(
                string caseName,
                int numEvents,
                string epl,
                int numSets)
            {
                this.caseName = caseName;
                this.numEvents = numEvents;
                this.epl = epl;
                this.numSets = numSets;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                env.CompileDeploy(epl).AddListener("s0");

                var startLoad = PerformanceObserver.NanoTime;
                for (var i = 0; i < numEvents; i++) {
                    env.SendEventBean(new SupportBean("E" + i, i));
                }

                var deltaLoad = PerformanceObserver.NanoTime - startLoad;

                var startQuery = PerformanceObserver.NanoTime;
                for (var j = 0; j < numSets; j++) {
                    for (var i = 0; i < numEvents; i++) {
                        var key = "E" + i;
                        env.SendEventBean(new SupportBean_S0(0, key));
                        EPAssertionUtil.AssertProps(
                            env.Listener("s0").AssertOneGetNewAndReset(),
                            fields,
                            new object[] {key, i});
                    }
                }

                var deltaQuery = PerformanceObserver.NanoTime - startQuery;

                /// <summary>
                /// System.out.println(caseName + ": Load " + deltaLoad/1000000d +
                /// " Query " + deltaQuery / 1000000d +
                /// " Total " + (deltaQuery+deltaLoad) / 1000000d );
                /// </summary>
                env.UndeployAll();
            }
        }
    }
} // end of namespace