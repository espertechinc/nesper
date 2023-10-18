///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertTrue

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
	public class EPLSubselectFilteredPerformance {
	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new EPLSubselectPerformanceOneCriteria());
	        execs.Add(new EPLSubselectPerformanceTwoCriteria());
	        execs.Add(new EPLSubselectPerformanceJoin3CriteriaSceneOne());
	        execs.Add(new EPLSubselectPerformanceJoin3CriteriaSceneTwo());
	        return execs;
	    }

	    private class EPLSubselectPerformanceOneCriteria : RegressionExecution {
	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
	        }

	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select (select p10 from SupportBean_S1#length(100000) where id = s0.id) as value from SupportBean_S0 as s0";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            // preload with 10k events
	            for (var i = 0; i < 10000; i++) {
	                env.SendEventBean(new SupportBean_S1(i, Convert.ToString(i)));
	            }

	            var startTime = PerformanceObserver.MilliTime;
	            for (var i = 0; i < 10000; i++) {
	                var index = 5000 + i % 1000;
	                env.SendEventBean(new SupportBean_S0(index, Convert.ToString(index)));
	                env.AssertEqualsNew("s0", "value", Convert.ToString(index));
	            }
	            var endTime = PerformanceObserver.MilliTime;
	            var delta = endTime - startTime;

	            Assert.That(delta, Is.LessThan(1000), "Failed perf test, delta=" + delta);
	            env.UndeployAll();
	        }
	    }

	    private class EPLSubselectPerformanceTwoCriteria : RegressionExecution {
	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
	        }

	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select (select p10 from SupportBean_S1#length(100000) where s0.id = id and p10 = s0.p00) as value from SupportBean_S0 as s0";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            // preload with 10k events
	            for (var i = 0; i < 10000; i++) {
	                env.SendEventBean(new SupportBean_S1(i, Convert.ToString(i)));
	            }

	            var startTime = PerformanceObserver.MilliTime;
	            for (var i = 0; i < 10000; i++) {
	                var index = 5000 + i % 1000;
	                env.SendEventBean(new SupportBean_S0(index, Convert.ToString(index)));
	                env.AssertEqualsNew("s0", "value", Convert.ToString(index));
	            }
	            var endTime = PerformanceObserver.MilliTime;
	            var delta = endTime - startTime;

	            Assert.That(delta, Is.LessThan(1000), "Failed perf test, delta=" + delta);
	            env.UndeployAll();
	        }
	    }

	    private class EPLSubselectPerformanceJoin3CriteriaSceneOne : RegressionExecution {
	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
	        }

	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select (select p00 from SupportBean_S0#length(100000) where p00 = s1.p10 and p01 = s2.p20 and p02 = s3.p30) as value " +
	                           "from SupportBean_S1#length(100000) as s1, SupportBean_S2#length(100000) as s2, SupportBean_S3#length(100000) as s3 where s1.id = s2.id and s2.id = s3.id";
	            TryPerfJoin3Criteria(env, stmtText);
	        }
	    }

	    private class EPLSubselectPerformanceJoin3CriteriaSceneTwo : RegressionExecution {
	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
	        }

	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select (select p00 from SupportBean_S0#length(100000) where p01 = s2.p20 and p00 = s1.p10 and p02 = s3.p30 and id >= 0) as value " +
	                           "from SupportBean_S3#length(100000) as s3, SupportBean_S1#length(100000) as s1, SupportBean_S2#length(100000) as s2 where s2.id = s3.id and s1.id = s2.id";
	            TryPerfJoin3Criteria(env, stmtText);
	        }
	    }

	    private static void TryPerfJoin3Criteria(RegressionEnvironment env, string stmtText) {
	        env.CompileDeployAddListenerMileZero(stmtText, "s0");

	        // preload with 10k events
	        for (var i = 0; i < 10000; i++) {
	            env.SendEventBean(new SupportBean_S0(i, Convert.ToString(i), Convert.ToString(i + 1), Convert.ToString(i + 2)));
	        }

	        var startTime = PerformanceObserver.MilliTime;
	        for (var index = 0; index < 5000; index++) {
	            env.SendEventBean(new SupportBean_S1(index, Convert.ToString(index)));
	            env.SendEventBean(new SupportBean_S2(index, Convert.ToString(index + 1)));
	            env.SendEventBean(new SupportBean_S3(index, Convert.ToString(index + 2)));
	            env.AssertEqualsNew("s0", "value", Convert.ToString(index));
	        }
	        var endTime = PerformanceObserver.MilliTime;
	        var delta = endTime - startTime;

	        Assert.That(delta, Is.LessThan(1500), "Failed perf test, delta=" + delta);
	        env.UndeployAll();
	    }
	}
} // end of namespace
