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

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableComparative
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithGroupByTopLevelSingleAgg(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithGroupByTopLevelSingleAgg(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();

            var eplNamedWindow =
"create window TotalsWindow#unique(TheString) as (TheString string, Total int);"+
"insert into TotalsWindow select TheString, sum(IntPrimitive) as Total from SupportBean group by TheString;"+
                "@name('s0') select P00 as c0, " +
"    (select Total from TotalsWindow tw where tw.TheString = S0.P00) as c1 from SupportBean_S0 as S0;";

            execs.Add(new InfraNWTableComparativeGroupByTopLevelSingleAgg("named window", 1000, eplNamedWindow, 1));

            var eplTable =
"create table varTotal (key string primary key, Total sum(int));\n"+
"into table varTotal select TheString, sum(IntPrimitive) as Total from SupportBean group by TheString;\n"+
"@name('s0') select P00 as c0, varTotal[P00].Total as c1 from SupportBean_S0;\n";
            execs.Add(new InfraNWTableComparativeGroupByTopLevelSingleAgg("table", 1000, eplTable, 1));
            return execs;
        }

        internal class InfraNWTableComparativeGroupByTopLevelSingleAgg : RegressionExecution
        {
            private readonly string caseName;
            private readonly string epl;
            private readonly int numEvents;
            private readonly int numSets;

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

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
                var fields = new[] { "c0", "c1" };
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
                        env.AssertPropsNew(
                            "s0",
                            fields,
                            new object[] { key, i });
                    }
                }

                var deltaQuery = PerformanceObserver.NanoTime - startQuery;

                // Console.WriteLine(caseName + ": Load " + deltaLoad/1000000d +
                // " Query " + deltaQuery / 1000000d +
                // " Total " + (deltaQuery+deltaLoad) / 1000000d );

                env.UndeployAll();
            }

            public string Name()
            {
                return $"{this.GetType().Name}{{caseName='{caseName}'}}'";
            }
        }
    }
} // end of namespace