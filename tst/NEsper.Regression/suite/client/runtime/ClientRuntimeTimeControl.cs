///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.@internal.kernel.service;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
	public class ClientRuntimeTimeControl {

	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new ClientRuntimeSendTimeSpan());
	        execs.Add(new ClientRuntimeNextScheduledTime());
	        return execs;
	    }

	    private class ClientRuntimeSendTimeSpan : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.AdvanceTime(0);

	            env.CompileDeploy("@name('s0') select current_timestamp() as ct from pattern[every timer:interval(1.5 sec)]");
	            env.AddListener("s0");

	            env.AdvanceTimeSpan(3500);
	            env.AssertListener("s0", listener => {
	                Assert.AreEqual(2, listener.NewDataList.Count);
	                Assert.AreEqual(1500L, listener.NewDataList[0][0].Get("ct"));
	                Assert.AreEqual(3000L, listener.NewDataList[1][0].Get("ct"));
	                listener.Reset();
	            });

	            env.AdvanceTimeSpan(4500);
	            env.AssertListener("s0", listener => {
	                Assert.AreEqual(1, listener.NewDataList.Count);
	                Assert.AreEqual(4500L, listener.NewDataList[0][0].Get("ct"));
	                listener.Reset();
	            });

	            env.AdvanceTimeSpan(9000);
	            env.AssertListener("s0", listener => {
	                Assert.AreEqual(3, listener.NewDataList.Count);
	                Assert.AreEqual(6000L, listener.NewDataList[0][0].Get("ct"));
	                Assert.AreEqual(7500L, listener.NewDataList[1][0].Get("ct"));
	                Assert.AreEqual(9000L, listener.NewDataList[2][0].Get("ct"));
	                listener.Reset();
	            });

	            env.AdvanceTimeSpan(10499);
	            env.AssertListener("s0", listener => Assert.AreEqual(0, listener.NewDataList.Count));

	            env.AdvanceTimeSpan(10499);
	            env.AssertListener("s0", listener => Assert.AreEqual(0, listener.NewDataList.Count));

	            env.AdvanceTimeSpan(10500);
	            env.AssertListener("s0", listener => {
	                Assert.AreEqual(1, listener.NewDataList.Count);
	                Assert.AreEqual(10500L, listener.NewDataList[0][0].Get("ct"));
	                listener.Reset();
	            });

	            env.AdvanceTimeSpan(10500);
	            env.AssertListener("s0", listener => Assert.AreEqual(0, listener.NewDataList.Count));

	            env.AdvanceTimeSpan(14000, 200);
	            env.AssertListener("s0", listener => {
	                Assert.AreEqual(14000, env.EventService.CurrentTime);
	                Assert.AreEqual(2, listener.NewDataList.Count);
	                Assert.AreEqual(12100L, listener.NewDataList[0][0].Get("ct"));
	                Assert.AreEqual(13700L, listener.NewDataList[1][0].Get("ct"));
	            });

	            env.UndeployAll();
	        }
	    }

	    private class ClientRuntimeNextScheduledTime : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var runtimeSPI = (EPEventServiceSPI) env.EventService;

	            env.AdvanceTime(0);
	            Assert.IsNull(env.EventService.NextScheduledTime);
	            AssertSchedules(runtimeSPI.StatementNearestSchedules, Array.Empty<object[]>());

	            env.CompileDeploy("@name('s0') select * from pattern[timer:interval(2 sec)]");
	            Assert.AreEqual(2000L, (long) env.EventService.NextScheduledTime);
	            AssertSchedules(runtimeSPI.StatementNearestSchedules, new object[][]{new object[] {"s0", 2000L}});

	            env.CompileDeploy("@name('s2') select * from pattern[timer:interval(150 msec)]");
	            Assert.AreEqual(150L, (long) env.EventService.NextScheduledTime);
	            AssertSchedules(runtimeSPI.StatementNearestSchedules, new object[][]{new object[] {"s2", 150L}, new object[] {"s0", 2000L}});

	            env.UndeployModuleContaining("s2");
	            Assert.AreEqual(2000L, (long) env.EventService.NextScheduledTime);
	            AssertSchedules(runtimeSPI.StatementNearestSchedules, new object[][]{new object[] {"s0", 2000L}});

	            env.CompileDeploy("@name('s3') select * from pattern[timer:interval(3 sec) and timer:interval(4 sec)]");
	            Assert.AreEqual(2000L, (long) env.EventService.NextScheduledTime);
	            AssertSchedules(runtimeSPI.StatementNearestSchedules, new object[][]{new object[] {"s0", 2000L}, new object[] {"s3", 3000L}});

	            env.AdvanceTime(2500);
	            Assert.AreEqual(3000L, (long) env.EventService.NextScheduledTime);
	            AssertSchedules(runtimeSPI.StatementNearestSchedules, new object[][]{new object[] {"s3", 3000L}});

	            env.AdvanceTime(3500);
	            Assert.AreEqual(4000L, (long) env.EventService.NextScheduledTime);
	            AssertSchedules(runtimeSPI.StatementNearestSchedules, new object[][]{new object[] {"s3", 4000L}});

	            env.AdvanceTime(4500);
	            Assert.AreEqual(null, env.EventService.NextScheduledTime);
	            AssertSchedules(runtimeSPI.StatementNearestSchedules, Array.Empty<object[]>());

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private static void AssertSchedules(IDictionary<DeploymentIdNamePair, long> schedules, object[][] expected) {
	        Assert.AreEqual(expected.Length, schedules.Count);

	        ISet<int?> matchNumber = new HashSet<int?>();
	        foreach (var entry in schedules) {
	            var matchFound = false;
	            for (var i = 0; i < expected.Length; i++) {
	                if (matchNumber.Contains(i)) {
	                    continue;
	                }
	                if (expected[i][0].Equals(entry.Key.Name)) {
	                    matchFound = true;
	                    matchNumber.Add(i);
	                    if (expected[i][1] == null && entry.Value == null) {
	                        continue;
	                    }
	                    if (!expected[i][1].Equals(entry.Value)) {
	                        Assert.Fail("Failed to match value for key '" + entry.Key + "' expected '" + expected[i][i] + "' received '" + entry.Value + "'");
	                    }
	                }
	            }
	            if (!matchFound) {
	                Assert.Fail("Failed to find key '" + entry.Key + "'");
	            }
	        }
	    }
	}
} // end of namespace
