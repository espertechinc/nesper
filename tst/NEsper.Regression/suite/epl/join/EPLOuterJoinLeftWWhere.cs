///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework; // assertEquals

// assertSame

namespace com.espertech.esper.regressionlib.suite.epl.join
{
	public class EPLOuterJoinLeftWWhere {

	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new EPLJoinWhereNotNullIs());
	        execs.Add(new EPLJoinWhereNotNullNE());
	        execs.Add(new EPLJoinWhereNullIs());
	        execs.Add(new EPLJoinWhereNullEq());
	        execs.Add(new EPLJoinWhereJoinOrNull());
	        execs.Add(new EPLJoinWhereJoin());
	        execs.Add(new EPLJoinEventType());
	        return execs;
	    }

	    private class EPLJoinWhereNotNullIs : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            SetupStatement(env, "where s1.p11 is not null");
	            TryWhereNotNull(env);
	            env.UndeployAll();
	        }
	    }

	    private class EPLJoinWhereNotNullNE : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            SetupStatement(env, "where s1.p11 is not null");
	            TryWhereNotNull(env);
	            env.UndeployAll();
	        }
	    }

	    private class EPLJoinWhereNullIs : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            SetupStatement(env, "where s1.p11 is null");
	            TryWhereNull(env);
	            env.UndeployAll();
	        }
	    }

	    private class EPLJoinWhereNullEq : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            SetupStatement(env, "where s1.p11 is null");
	            TryWhereNull(env);
	            env.UndeployAll();
	        }
	    }

	    private class EPLJoinWhereJoinOrNull : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            SetupStatement(env, "where s0.p01 = s1.p11 or s1.p11 is null");

	            var eventS0 = new SupportBean_S0(0, "0", "[a]");
	            SendEvent(eventS0, env);
	            env.AssertEventNew("s0", @event => CompareEvent(@event, eventS0, null));

	            // Send events to test the join for multiple rows incl. null value
	            var s1Bean1 = new SupportBean_S1(1000, "5", "X");
	            var s1Bean2 = new SupportBean_S1(1001, "5", "Y");
	            var s1Bean3 = new SupportBean_S1(1002, "5", "X");
	            var s1Bean4 = new SupportBean_S1(1003, "5", null);
	            var s0 = new SupportBean_S0(1, "5", "X");
	            SendEvent(env, new object[]{s1Bean1, s1Bean2, s1Bean3, s1Bean4, s0});

	            env.AssertListener("s0", listener => {
	                Assert.AreEqual(3, listener.LastNewData.Length);
	                var received = new object[3];
	                for (var i = 0; i < 3; i++) {
	                    Assert.AreSame(s0, listener.LastNewData[i].Get("s0"));
	                    received[i] = listener.LastNewData[i].Get("s1");
	                }
	                EPAssertionUtil.AssertEqualsAnyOrder(new object[]{s1Bean1, s1Bean3, s1Bean4}, received);
	            });

	            env.UndeployAll();
	        }
	    }

	    private class EPLJoinWhereJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            SetupStatement(env, "where s0.p01 = s1.p11");

	            var eventsS0 = new SupportBean_S0[15];
	            var eventsS1 = new SupportBean_S1[15];
	            var count = 100;
	            for (var i = 0; i < eventsS0.Length; i++) {
	                eventsS0[i] = new SupportBean_S0(count++, Convert.ToString(i));
	            }
	            count = 200;
	            for (var i = 0; i < eventsS1.Length; i++) {
	                eventsS1[i] = new SupportBean_S1(count++, Convert.ToString(i));
	            }

	            // Send S0[0] p01=a
	            eventsS0[0].P01 = "[a]";
	            SendEvent(eventsS0[0], env);
	            env.AssertListenerNotInvoked("s0");

	            // Send S1[1] p11=b
	            eventsS1[1].P11 = "[b]";
	            SendEvent(eventsS1[1], env);
	            env.AssertListenerNotInvoked("s0");

	            // Send S0[1] p01=c, no match expected
	            eventsS0[1].P01 = "[c]";
	            SendEvent(eventsS0[1], env);
	            env.AssertListenerNotInvoked("s0");

	            // Send S1[2] p11=d
	            eventsS1[2].P11 = "[d]";
	            SendEvent(eventsS1[2], env);
	            // Send S0[2] p01=d
	            eventsS0[2].P01 = "[d]";
	            SendEvent(eventsS0[2], env);
	            env.AssertEventNew("s0", @event => CompareEvent(@event, eventsS0[2], eventsS1[2]));

	            // Send S1[3] and S0[3] with differing props, no match expected
	            eventsS1[3].P11 = "[e]";
	            SendEvent(eventsS1[3], env);
	            eventsS0[3].P01 = "[e1]";
	            SendEvent(eventsS0[3], env);
	            env.AssertListenerNotInvoked("s0");

	            env.UndeployAll();
	        }
	    }

	    private class EPLJoinEventType : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            SetupStatement(env, "");
	            env.AssertStatement("s0", statement => {
	                var type = statement.EventType;
	                Assert.AreEqual(typeof(SupportBean_S0), type.GetPropertyType("s0"));
	                Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("s1"));
	            });
	            env.UndeployAll();
	        }
	    }

	    private static void TryWhereNotNull(RegressionEnvironment env) {
	        var s1Bean1 = new SupportBean_S1(1000, "5", "X");
	        var s1Bean2 = new SupportBean_S1(1001, "5", null);
	        var s1Bean3 = new SupportBean_S1(1002, "6", null);
	        SendEvent(env, new object[]{s1Bean1, s1Bean2, s1Bean3});
	        env.AssertListenerNotInvoked("s0");

	        var s0 = new SupportBean_S0(1, "5", "X");
	        SendEvent(s0, env);
	        env.AssertEventNew("s0", @event => CompareEvent(@event, s0, s1Bean1));
	    }

	    private static void TryWhereNull(RegressionEnvironment env) {
	        var s1Bean1 = new SupportBean_S1(1000, "5", "X");
	        var s1Bean2 = new SupportBean_S1(1001, "5", null);
	        var s1Bean3 = new SupportBean_S1(1002, "6", null);
	        SendEvent(env, new object[]{s1Bean1, s1Bean2, s1Bean3});
	        env.AssertListenerNotInvoked("s0");

	        var s0 = new SupportBean_S0(1, "5", "X");
	        SendEvent(s0, env);
	        env.AssertEventNew("s0", @event => CompareEvent(@event, s0, s1Bean2));
	    }

	    private static void CompareEvent(EventBean receivedEvent, SupportBean_S0 expectedS0, SupportBean_S1 expectedS1) {
	        Assert.AreSame(expectedS0, receivedEvent.Get("s0"));
	        Assert.AreSame(expectedS1, receivedEvent.Get("s1"));
	    }

	    private static void SendEvent(RegressionEnvironment env, object[] events) {
	        for (var i = 0; i < events.Length; i++) {
	            SendEvent(events[i], env);
	        }
	    }

	    private static void SetupStatement(RegressionEnvironment env, string whereClause) {
	        var joinStatement =
		        $"@name('s0') select * from SupportBean_S0#length(5) as s0 left outer join SupportBean_S1#length(5) as s1 on s0.p00 = s1.p10 {whereClause}";
	        env.CompileDeployAddListenerMileZero(joinStatement, "s0");
	    }

	    private static void SendEvent(object theEvent, RegressionEnvironment env) {
	        env.SendEventBean(theEvent);
	    }
	}
} // end of namespace
