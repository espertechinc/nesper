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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableOnMergePerf
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraPerformance(true, EventRepresentationChoice.OBJECTARRAY));
            execs.Add(new InfraPerformance(true, EventRepresentationChoice.MAP));
            execs.Add(new InfraPerformance(true, EventRepresentationChoice.DEFAULT));
            execs.Add(new InfraPerformance(false, EventRepresentationChoice.OBJECTARRAY));
            return execs;
        }

        internal class InfraPerformance : RegressionExecution
        {
            private readonly bool namedWindow;
            private readonly EventRepresentationChoice outputType;

            public InfraPerformance(
                bool namedWindow,
                EventRepresentationChoice outputType)
            {
                this.namedWindow = namedWindow;
                this.outputType = outputType;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? outputType.GetAnnotationText() +
                      "@name('create') create window MyWindow#keepall as (c1 string, c2 int)"
                    : "@name('create') create table MyWindow(c1 string primary key, c2 int)";
                env.CompileDeploy(eplCreate, path);
                Assert.IsTrue(outputType.MatchesClass(env.Statement("create").EventType.UnderlyingType));

                // preload events
                env.CompileDeploy(
                    "@name('insert') insert into MyWindow select TheString as c1, IntPrimitive as c2 from SupportBean",
                    path);
                var totalUpdated = 5000;
                for (var i = 0; i < totalUpdated; i++) {
                    env.SendEventBean(new SupportBean("E" + i, 0));
                }

                env.UndeployModuleContaining("insert");

                var epl = "@name('s0') on SupportBean sb merge MyWindow nw where nw.c1 = sb.TheString " +
                          "when matched then update set nw.c2=sb.IntPrimitive";
                env.CompileDeploy(epl, path);

                // prime
                for (var i = 0; i < 100; i++) {
                    env.SendEventBean(new SupportBean("E" + i, 1));
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < totalUpdated; i++) {
                    env.SendEventBean(new SupportBean("E" + i, 1));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                // verify
                var events = env.Statement("create").GetEnumerator();
                var count = 0;
                while (events.MoveNext()) {
                    var next = events.Current;
                    Assert.AreEqual(1, next.Get("c2"));
                    count++;
                }

                Assert.AreEqual(totalUpdated, count);
                Assert.That(delta, Is.LessThan(500), "Delta=" + delta);

                env.UndeployAll();
            }
        }
    }
} // end of namespace