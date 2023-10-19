///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertEquals

// assertTrue

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTPerfIntervalOps : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@name('create') @public create window AWindow#keepall as SupportTimeStartEndA", path);
            env.CompileDeploy("insert into AWindow select * from SupportTimeStartEndA", path);

            env.AssertStatement(
                "create",
                statement => {
                    var eventTypeNW = statement.EventType;
                    Assert.AreEqual("longdateStart", eventTypeNW.StartTimestampPropertyName);
                    Assert.AreEqual("longdateEnd", eventTypeNW.EndTimestampPropertyName);
                });

            // preload
            for (var i = 0; i < 10000; i++) {
                env.SendEventBean(SupportTimeStartEndA.Make("A" + i, "2002-05-30T09:00:00.000", 100));
            }

            env.SendEventBean(SupportTimeStartEndA.Make("AEarlier", "2002-05-30T08:00:00.000", 100));
            env.SendEventBean(SupportTimeStartEndA.Make("ALater", "2002-05-30T10:00:00.000", 100));

            // assert BEFORE
            var eplBefore =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.before(b)";
            RunAssertion(env, path, eplBefore, "2002-05-30T09:00:00.000", 0, "AEarlier");

            var eplBeforeMSec =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.longdateEnd.before(b.longdateStart)";
            RunAssertion(env, path, eplBeforeMSec, "2002-05-30T09:00:00.000", 0, "AEarlier");

            var eplBeforeMSecMix1 =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.longdateEnd.before(b)";
            RunAssertion(env, path, eplBeforeMSecMix1, "2002-05-30T09:00:00.000", 0, "AEarlier");

            var eplBeforeMSecMix2 =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.before(b.longdateStart)";
            RunAssertion(env, path, eplBeforeMSecMix2, "2002-05-30T09:00:00.000", 0, "AEarlier");

            // assert AFTER
            var eplAfter =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.after(b)";
            RunAssertion(env, path, eplAfter, "2002-05-30T09:00:00.000", 0, "ALater");

            // assert COINCIDES
            var eplCoincides =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.coincides(b)";
            RunAssertion(env, path, eplCoincides, "2002-05-30T08:00:00.000", 100, "AEarlier");

            // assert DURING
            var eplDuring =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.during(b)";
            RunAssertion(env, path, eplDuring, "2002-05-30T07:59:59.000", 2000, "AEarlier");

            // assert FINISHES
            var eplFinishes =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.finishes(b)";
            RunAssertion(env, path, eplFinishes, "2002-05-30T07:59:59.950", 150, "AEarlier");

            // assert FINISHED-BY
            var eplFinishedBy =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.finishedBy(b)";
            RunAssertion(env, path, eplFinishedBy, "2002-05-30T08:00:00.050", 50, "AEarlier");

            // assert INCLUDES
            var eplIncludes =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.includes(b)";
            RunAssertion(env, path, eplIncludes, "2002-05-30T08:00:00.050", 20, "AEarlier");

            // assert MEETS
            var eplMeets =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.meets(b)";
            RunAssertion(env, path, eplMeets, "2002-05-30T08:00:00.100", 0, "AEarlier");

            // assert METBY
            var eplMetBy =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.metBy(b)";
            RunAssertion(env, path, eplMetBy, "2002-05-30T07:59:59.950", 50, "AEarlier");

            // assert OVERLAPS
            var eplOverlaps =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.overlaps(b)";
            RunAssertion(env, path, eplOverlaps, "2002-05-30T08:00:00.050", 100, "AEarlier");

            // assert OVERLAPPEDY
            var eplOverlappedBy =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.overlappedBy(b)";
            RunAssertion(env, path, eplOverlappedBy, "2002-05-30T09:59:59.950", 100, "ALater");
            RunAssertion(env, path, eplOverlappedBy, "2002-05-30T07:59:59.950", 100, "AEarlier");

            // assert STARTS
            var eplStarts =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.starts(b)";
            RunAssertion(env, path, eplStarts, "2002-05-30T08:00:00.000", 150, "AEarlier");

            // assert STARTEDBY
            var eplEnds =
                "select a.key as c0 from AWindow as a, SupportTimeStartEndB b unidirectional where a.startedBy(b)";
            RunAssertion(env, path, eplEnds, "2002-05-30T08:00:00.000", 50, "AEarlier");

            env.UndeployAll();
        }

        private void RunAssertion(
            RegressionEnvironment env,
            RegressionPath path,
            string epl,
            string timestampB,
            long durationB,
            string expectedAKey)
        {
            env.CompileDeploy("@name('s0') " + epl, path).AddListener("s0");

            // query
            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 1000; i++) {
                env.SendEventBean(SupportTimeStartEndB.Make("B", timestampB, durationB));
                env.AssertEqualsNew("s0", "c0", expectedAKey);
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
            Assert.That(delta, Is.LessThan(500), "Delta=" + delta / 1000d);

            env.UndeployModuleContaining("s0");
        }
    }
} // end of namespace