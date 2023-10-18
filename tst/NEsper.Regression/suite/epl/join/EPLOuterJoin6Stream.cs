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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;


namespace com.espertech.esper.regressionlib.suite.epl.join
{
	public class EPLOuterJoin6Stream {

	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new EPLJoinRootS0());
	        execs.Add(new EPLJoinRootS1());
	        execs.Add(new EPLJoinRootS2());
	        execs.Add(new EPLJoinRootS3());
	        execs.Add(new EPLJoinRootS4());
	        execs.Add(new EPLJoinRootS5());
	        return execs;
	    }

	    private class EPLJoinRootS0 : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            /// <summary>
	            /// Query:
	            /// </summary>
	            var epl = "@name('s0') select * from " +
	                      "SupportBean_S0#length(1000) as s0 " +
	                      " right outer join SupportBean_S1#length(1000) as s1 on s0.p00 = s1.p10 " +
	                      " right outer join SupportBean_S2#length(1000) as s2 on s0.p00 = s2.p20 " +
	                      " right outer join SupportBean_S3#length(1000) as s3 on s1.p10 = s3.p30 " +
	                      " right outer join SupportBean_S4#length(1000) as s4 on s2.p20 = s4.p40 " +
	                      " right outer join SupportBean_S5#length(1000) as s5 on s2.p20 = s5.p50 ";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            TryAssertion(env);
	        }
	    }

	    private class EPLJoinRootS1 : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            /// <summary>
	            /// Query:
	            /// </summary>
	            var epl = "@name('s0') select * from " +
	                      "SupportBean_S1#length(1000) as s1 " +
	                      " left outer join " + "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10 " +
	                      " right outer join SupportBean_S3#length(1000) as s3 on s1.p10 = s3.p30 " +
	                      " right outer join SupportBean_S2#length(1000) as s2 on s0.p00 = s2.p20 " +
	                      " right outer join SupportBean_S5#length(1000) as s5 on s2.p20 = s5.p50 " +
	                      " right outer join SupportBean_S4#length(1000) as s4 on s2.p20 = s4.p40 ";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            TryAssertion(env);
	        }
	    }

	    private class EPLJoinRootS2 : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            /// <summary>
	            /// Query:
	            /// </summary>
	            var epl = "@name('s0') select * from " +
	                      "SupportBean_S2#length(1000) as s2 " +
	                      " left outer join " + "SupportBean_S0#length(1000) as s0 on s0.p00 = s2.p20 " +
	                      " right outer join SupportBean_S1#length(1000) as s1 on s0.p00 = s1.p10 " +
	                      " right outer join SupportBean_S3#length(1000) as s3 on s1.p10 = s3.p30 " +
	                      " right outer join SupportBean_S4#length(1000) as s4 on s2.p20 = s4.p40 " +
	                      " right outer join SupportBean_S5#length(1000) as s5 on s2.p20 = s5.p50 ";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            TryAssertion(env);
	        }
	    }

	    private class EPLJoinRootS3 : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            /// <summary>
	            /// Query:
	            /// </summary>
	            var epl = "@name('s0') select * from " +
	                      "SupportBean_S3#length(1000) as s3 " +
	                      " left outer join SupportBean_S1#length(1000) as s1 on s1.p10 = s3.p30 " +
	                      " left outer join " + "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10 " +
	                      " right outer join SupportBean_S2#length(1000) as s2 on s0.p00 = s2.p20 " +
	                      " right outer join SupportBean_S5#length(1000) as s5 on s2.p20 = s5.p50 " +
	                      " right outer join SupportBean_S4#length(1000) as s4 on s2.p20 = s4.p40 ";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            TryAssertion(env);
	        }
	    }

	    private class EPLJoinRootS4 : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            /// <summary>
	            /// Query:
	            /// </summary>
	            var epl = "@name('s0') select * from " +
	                      "SupportBean_S4#length(1000) as s4 " +
	                      " left outer join SupportBean_S2#length(1000) as s2 on s2.p20 = s4.p40 " +
	                      " right outer join SupportBean_S5#length(1000) as s5 on s2.p20 = s5.p50 " +
	                      " left outer join " + "SupportBean_S0#length(1000) as s0 on s0.p00 = s2.p20 " +
	                      " right outer join SupportBean_S1#length(1000) as s1 on s0.p00 = s1.p10 " +
	                      " right outer join SupportBean_S3#length(1000) as s3 on s1.p10 = s3.p30 ";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            TryAssertion(env);
	        }
	    }

	    private class EPLJoinRootS5 : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            /// <summary>
	            /// Query:
	            /// </summary>
	            var epl = "@name('s0') select * from " +
	                      "SupportBean_S5#length(1000) as s5 " +
	                      " left outer join SupportBean_S2#length(1000) as s2 on s2.p20 = s5.p50 " +
	                      " right outer join SupportBean_S4#length(1000) as s4 on s2.p20 = s4.p40 " +
	                      " left outer join " + "SupportBean_S0#length(1000) as s0 on s0.p00 = s2.p20 " +
	                      " right outer join SupportBean_S1#length(1000) as s1 on s0.p00 = s1.p10 " +
	                      " right outer join SupportBean_S3#length(1000) as s3 on s1.p10 = s3.p30 ";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            TryAssertion(env);
	        }
	    }

	    private static void TryAssertion(RegressionEnvironment env) {
	        object[] s0Events;
	        object[] s1Events;
	        object[] s2Events;
	        object[] s3Events;
	        object[] s4Events;
	        object[] s5Events;

	        // Test s0 and s1=0, s2=0, s3=0, s4=0, s5=0
	        //
	        s0Events = SupportBean_S0.MakeS0("A", new string[]{"A-s0-1"});
	        SendEvent(env, s0Events);
	        env.AssertListenerNotInvoked("s0");

	        // Test s0 and s1=1, s2=0, s3=0, s4=0, s5=0
	        //
	        s1Events = SupportBean_S1.MakeS1("B", new string[]{"B-s1-1"});
	        SendEvent(env, s1Events);
	        env.AssertListenerNotInvoked("s0");

	        s0Events = SupportBean_S0.MakeS0("B", new string[]{"B-s0-1"});
	        SendEvent(env, s0Events);
	        env.AssertListenerNotInvoked("s0");

	        // Test s0 and s1=1, s2=1, s3=0, s4=0, s5=0
	        //
	        s1Events = SupportBean_S1.MakeS1("C", new string[]{"C-s1-1"});
	        SendEvent(env, s1Events);

	        s2Events = SupportBean_S2.MakeS2("C", new string[]{"C-s2-1"});
	        SendEvent(env, s2Events);
	        env.AssertListenerNotInvoked("s0");

	        s0Events = SupportBean_S0.MakeS0("C", new string[]{"C-s0-1"});
	        SendEvent(env, s0Events);
	        env.AssertListenerNotInvoked("s0");

	        // Test s0 and s1=1, s2=1, s3=1, s4=0, s5=0
	        //
	        s1Events = SupportBean_S1.MakeS1("D", new string[]{"D-s1-1"});
	        SendEvent(env, s1Events);

	        s2Events = SupportBean_S2.MakeS2("D", new string[]{"D-s2-1"});
	        SendEvent(env, s2Events);

	        s3Events = SupportBean_S3.MakeS3("D", new string[]{"D-s2-1"});
	        SendEvent(env, s3Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, s1Events[0], null, s3Events[0], null, null}});

	        s0Events = SupportBean_S0.MakeS0("D", new string[]{"D-s0-1"});
	        SendEvent(env, s0Events);
	        env.AssertListenerNotInvoked("s0");

	        // Test s0 and s1=1, s2=1, s3=1, s4=1, s5=0
	        //
	        s1Events = SupportBean_S1.MakeS1("E", new string[]{"E-s1-1"});
	        SendEvent(env, s1Events);

	        s2Events = SupportBean_S2.MakeS2("E", new string[]{"E-s2-1"});
	        SendEvent(env, s2Events);

	        s3Events = SupportBean_S3.MakeS3("E", new string[]{"E-s2-1"});
	        SendEvent(env, s3Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, s1Events[0], null, s3Events[0], null, null}});

	        s4Events = SupportBean_S4.MakeS4("E", new string[]{"E-s2-1"});
	        SendEvent(env, s4Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, null, null, null, s4Events[0], null}});

	        s0Events = SupportBean_S0.MakeS0("E", new string[]{"E-s0-1"});
	        SendEvent(env, s0Events);
	        env.AssertListenerNotInvoked("s0");

	        // Test s0 and s1=2, s2=1, s3=1, s4=1, s5=1
	        //
	        s1Events = SupportBean_S1.MakeS1("F", new string[]{"F-s1-1"});
	        SendEvent(env, s1Events);
	        env.AssertListenerNotInvoked("s0");

	        s2Events = SupportBean_S2.MakeS2("F", new string[]{"F-s2-1"});
	        SendEvent(env, s2Events);
	        env.AssertListenerNotInvoked("s0");

	        s3Events = SupportBean_S3.MakeS3("F", new string[]{"F-s3-1"});
	        SendEvent(env, s3Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, s1Events[0], null, s3Events[0], null, null}});

	        s4Events = SupportBean_S4.MakeS4("F", new string[]{"F-s2-1"});
	        SendEvent(env, s4Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, null, null, null, s4Events[0], null}});

	        s5Events = SupportBean_S5.MakeS5("F", new string[]{"F-s2-1"});
	        SendEvent(env, s5Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, null, s2Events[0], null, s4Events[0], s5Events[0]}});

	        s0Events = SupportBean_S0.MakeS0("F", new string[]{"F-s0-1"});
	        SendEvent(env, s0Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]}});

	        // Test s0 and s1=2, s2=2, s3=1, s4=1, s5=2
	        //
	        s1Events = SupportBean_S1.MakeS1("G", new string[]{"G-s1-1", "G-s1-2"});
	        SendEventsAndReset(env, s1Events);

	        s2Events = SupportBean_S2.MakeS2("G", new string[]{"G-s2-1", "G-s2-2"});
	        SendEventsAndReset(env, s2Events);

	        s3Events = SupportBean_S3.MakeS3("G", new string[]{"G-s3-1"});
	        SendEventsAndReset(env, s3Events);

	        s4Events = SupportBean_S4.MakeS4("G", new string[]{"G-s2-1"});
	        SendEventsAndReset(env, s4Events);

	        s5Events = SupportBean_S5.MakeS5("G", new string[]{"G-s5-1", "G-s5-2"});
	        SendEventsAndReset(env, s5Events);

	        s0Events = SupportBean_S0.MakeS0("G", new string[]{"G-s0-1"});
	        SendEvent(env, s0Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1]}});

	        // Test s0 and s1=2, s2=2, s3=2, s4=2, s5=2
	        //
	        s1Events = SupportBean_S1.MakeS1("H", new string[]{"H-s1-1", "H-s1-2"});
	        SendEventsAndReset(env, s1Events);

	        s2Events = SupportBean_S2.MakeS2("H", new string[]{"H-s2-1", "H-s2-2"});
	        SendEventsAndReset(env, s2Events);

	        s3Events = SupportBean_S3.MakeS3("H", new string[]{"H-s3-1", "H-s3-2"});
	        SendEventsAndReset(env, s3Events);

	        s4Events = SupportBean_S4.MakeS4("H", new string[]{"H-s4-1", "H-s4-2"});
	        SendEventsAndReset(env, s4Events);

	        s5Events = SupportBean_S5.MakeS5("H", new string[]{"H-s5-1", "H-s5-2"});
	        SendEventsAndReset(env, s5Events);

	        s0Events = SupportBean_S0.MakeS0("H", new string[]{"H-s0-1"});
	        SendEvent(env, s0Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[1], s5Events[1]}});

	        // Test s0 and s1=2, s2=1, s3=1, s4=3, s5=1
	        //
	        s1Events = SupportBean_S1.MakeS1("I", new string[]{"I-s1-1", "I-s1-2"});
	        SendEventsAndReset(env, s1Events);

	        s2Events = SupportBean_S2.MakeS2("I", new string[]{"I-s2-1"});
	        SendEventsAndReset(env, s2Events);

	        s3Events = SupportBean_S3.MakeS3("I", new string[]{"I-s3-1"});
	        SendEventsAndReset(env, s3Events);

	        s4Events = SupportBean_S4.MakeS4("I", new string[]{"I-s4-1", "I-s4-2", "I-s4-3"});
	        SendEventsAndReset(env, s4Events);

	        s5Events = SupportBean_S5.MakeS5("I", new string[]{"I-s5-1"});
	        SendEventsAndReset(env, s5Events);

	        s0Events = SupportBean_S0.MakeS0("I", new string[]{"I-s0-1"});
	        SendEvent(env, s0Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[2], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[2], s5Events[0]}});

	        // Test s1 and s3=0
	        //
	        s1Events = SupportBean_S1.MakeS1("J", new string[]{"J-s1-1"});
	        SendEvent(env, s1Events);
	        env.AssertListenerNotInvoked("s0");

	        // Test s1 and s0=1, s2=0, s3=1, s4=1, s5=0
	        //
	        s0Events = SupportBean_S0.MakeS0("K", new string[]{"K-s0-1"});
	        SendEvent(env, s0Events);

	        s3Events = SupportBean_S3.MakeS3("K", new string[]{"K-s3-1"});
	        SendEventsAndReset(env, s3Events);

	        s1Events = SupportBean_S1.MakeS1("K", new string[]{"K-s1-1"});
	        SendEvent(env, s1Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, s1Events[0], null, s3Events[0], null, null}});

	        // Test s1 and s0=1, s2=1, s3=1, s4=0, s5=1
	        //
	        s0Events = SupportBean_S0.MakeS0("L", new string[]{"L-s0-1"});
	        SendEvent(env, s0Events);
	        env.AssertListenerNotInvoked("s0");

	        s2Events = SupportBean_S2.MakeS2("L", new string[]{"L-s2-1"});
	        SendEvent(env, s2Events);
	        env.AssertListenerNotInvoked("s0");

	        s3Events = SupportBean_S3.MakeS3("L", new string[]{"L-s3-1"});
	        SendEventsAndReset(env, s3Events);

	        s5Events = SupportBean_S5.MakeS5("L", new string[]{"L-s5-1"});
	        SendEventsAndReset(env, s5Events);

	        s1Events = SupportBean_S1.MakeS1("L", new string[]{"L-s1-1"});
	        SendEvent(env, s1Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, s1Events[0], null, s3Events[0], null, null}});

	        // Test s1 and s0=1, s2=1, s3=1, s4=2, s5=1
	        //
	        s0Events = SupportBean_S0.MakeS0("M", new string[]{"M-s0-1"});
	        SendEvent(env, s0Events);

	        s2Events = SupportBean_S2.MakeS2("M", new string[]{"M-s2-1"});
	        SendEventsAndReset(env, s2Events);

	        s3Events = SupportBean_S3.MakeS3("M", new string[]{"M-s3-1"});
	        SendEventsAndReset(env, s3Events);

	        s4Events = SupportBean_S4.MakeS4("M", new string[]{"M-s4-1", "M-s4-2"});
	        SendEventsAndReset(env, s4Events);

	        s5Events = SupportBean_S5.MakeS5("M", new string[]{"M-s5-1"});
	        SendEventsAndReset(env, s5Events);

	        s1Events = SupportBean_S1.MakeS1("M", new string[]{"M-s1-1"});
	        SendEvent(env, s1Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]}});

	        // Test s2 and s0=1, s1=0, s3=0, s4=1, s5=2
	        //
	        s0Events = SupportBean_S0.MakeS0("N", new string[]{"N-s0-1"});
	        SendEvent(env, s0Events);

	        s4Events = SupportBean_S4.MakeS4("N", new string[]{"N-s4-1"});
	        SendEventsAndReset(env, s4Events);

	        s5Events = SupportBean_S5.MakeS5("N", new string[]{"N-s5-1", "N-s5-2"});
	        SendEventsAndReset(env, s5Events);

	        s2Events = SupportBean_S2.MakeS2("N", new string[]{"N-s2-1"});
	        SendEvent(env, s2Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, null, s2Events[0], null, s4Events[0], s5Events[0]},
	            new object[] {null, null, s2Events[0], null, s4Events[0], s5Events[1]}});

	        // Test s2 and s0=1, s1=1, s3=3, s4=1, s5=2
	        //
	        s0Events = SupportBean_S0.MakeS0("O", new string[]{"O-s0-1"});
	        SendEvent(env, s0Events);

	        s1Events = SupportBean_S1.MakeS1("O", new string[]{"O-s1-1"});
	        SendEvent(env, s1Events);

	        s3Events = SupportBean_S3.MakeS3("O", new string[]{"O-s3-1", "O-s3-2", "O-s3-3"});
	        SendEventsAndReset(env, s3Events);

	        s4Events = SupportBean_S4.MakeS4("O", new string[]{"O-s4-1"});
	        SendEventsAndReset(env, s4Events);

	        s5Events = SupportBean_S5.MakeS5("O", new string[]{"O-s5-1", "O-s5-2"});
	        SendEventsAndReset(env, s5Events);

	        s2Events = SupportBean_S2.MakeS2("O", new string[]{"O-s2-1"});
	        SendEvent(env, s2Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[2], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[2], s4Events[0], s5Events[1]}});

	        // Test s3 and s0=0, s1=0, s2=0, s4=0, s5=0
	        //
	        s3Events = SupportBean_S3.MakeS3("P", new string[]{"P-s1-1"});
	        SendEvent(env, s3Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, null, null, s3Events[0], null, null}});

	        // Test s3 and s0=0, s1=1, s2=0, s4=0, s5=0
	        //
	        s1Events = SupportBean_S1.MakeS1("Q", new string[]{"Q-s1-1"});
	        SendEvent(env, s1Events);

	        s3Events = SupportBean_S3.MakeS3("Q", new string[]{"Q-s1-1"});
	        SendEvent(env, s3Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, s1Events[0], null, s3Events[0], null, null}});

	        // Test s3 and s0=1, s1=2, s2=2, s4=0, s5=0
	        //
	        s0Events = SupportBean_S0.MakeS0("R", new string[]{"R-s0-1"});
	        SendEvent(env, s0Events);

	        s1Events = SupportBean_S1.MakeS1("R", new string[]{"R-s1-1", "R-s1-2"});
	        SendEvent(env, s1Events);

	        s2Events = SupportBean_S2.MakeS2("R", new string[]{"R-s2-1", "R-s2-1"});
	        SendEventsAndReset(env, s2Events);

	        s3Events = SupportBean_S3.MakeS3("R", new string[]{"R-s3-1"});
	        SendEvent(env, s3Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, s1Events[0], null, s3Events[0], null, null},
	            new object[] {null, s1Events[1], null, s3Events[0], null, null}});

	        // Test s3 and s0=2, s1=2, s2=1, s4=2, s5=2
	        //
	        s0Events = SupportBean_S0.MakeS0("S", new string[]{"S-s0-1", "S-s0-2"});
	        SendEvent(env, s0Events);

	        s1Events = SupportBean_S1.MakeS1("S", new string[]{"S-s1-1", "S-s1-2"});
	        SendEvent(env, s1Events);

	        s2Events = SupportBean_S2.MakeS2("S", new string[]{"S-s2-1", "S-s2-1"});
	        SendEventsAndReset(env, s2Events);

	        s4Events = SupportBean_S4.MakeS4("S", new string[]{"S-s4-1", "S-s4-2"});
	        SendEventsAndReset(env, s4Events);

	        s5Events = SupportBean_S5.MakeS5("S", new string[]{"S-s5-1", "S-s5-2"});
	        SendEventsAndReset(env, s5Events);

	        s3Events = SupportBean_S3.MakeS3("S", new string[]{"s-s3-1"});
	        SendEvent(env, s3Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[1]}});

	        // Test s4 and s0=1, s1=0, s2=1, s3=0, s5=0
	        //
	        s0Events = SupportBean_S0.MakeS0("U", new string[]{"U-s0-1"});
	        SendEventsAndReset(env, s0Events);

	        s2Events = SupportBean_S2.MakeS2("U", new string[]{"U-s1-1"});
	        SendEventsAndReset(env, s2Events);

	        s4Events = SupportBean_S4.MakeS4("U", new string[]{"U-s4-1"});
	        SendEvent(env, s4Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, null, null, null, s4Events[0], null}});

	        // Test s4 and s0=1, s1=0, s2=1, s3=0, s5=1
	        //
	        s0Events = SupportBean_S0.MakeS0("V", new string[]{"V-s0-1"});
	        SendEventsAndReset(env, s0Events);

	        s2Events = SupportBean_S2.MakeS2("V", new string[]{"V-s1-1"});
	        SendEventsAndReset(env, s2Events);

	        s5Events = SupportBean_S5.MakeS5("V", new string[]{"V-s5-1"});
	        SendEventsAndReset(env, s5Events);

	        s4Events = SupportBean_S4.MakeS4("V", new string[]{"V-s4-1"});
	        SendEvent(env, s4Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {null, null, s2Events[0], null, s4Events[0], s5Events[0]}});

	        // Test s4 and s0=1, s1=1, s2=1, s3=1, s5=2
	        //
	        s0Events = SupportBean_S0.MakeS0("W", new string[]{"W-s0-1"});
	        SendEvent(env, s0Events);

	        s1Events = SupportBean_S1.MakeS1("W", new string[]{"W-s1-1"});
	        SendEventsAndReset(env, s1Events);

	        s2Events = SupportBean_S2.MakeS2("W", new string[]{"W-s2-1"});
	        SendEventsAndReset(env, s2Events);

	        s3Events = SupportBean_S3.MakeS3("W", new string[]{"W-s3-1"});
	        SendEventsAndReset(env, s3Events);

	        s5Events = SupportBean_S5.MakeS5("W", new string[]{"W-s5-1", "W-s5-2"});
	        SendEventsAndReset(env, s5Events);

	        s4Events = SupportBean_S4.MakeS4("W", new string[]{"W-s4-1"});
	        SendEvent(env, s4Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]}});

	        // Test s5 and s0=1, s1=2, s2=2, s3=1, s4=1
	        //
	        s0Events = SupportBean_S0.MakeS0("X", new string[]{"X-s0-1"});
	        SendEvent(env, s0Events);

	        s1Events = SupportBean_S1.MakeS1("X", new string[]{"X-s1-1", "X-s1-2"});
	        SendEventsAndReset(env, s1Events);

	        s2Events = SupportBean_S2.MakeS2("X", new string[]{"X-s2-1", "X-s2-2"});
	        SendEvent(env, s2Events);

	        s3Events = SupportBean_S3.MakeS3("X", new string[]{"X-s3-1"});
	        SendEventsAndReset(env, s3Events);

	        s4Events = SupportBean_S4.MakeS4("X", new string[]{"X-s4-1"});
	        SendEventsAndReset(env, s4Events);

	        s5Events = SupportBean_S5.MakeS5("X", new string[]{"X-s5-1"});
	        SendEvent(env, s5Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]}});

	        // Test s5 and s0=2, s1=1, s2=1, s3=1, s4=1
	        //
	        s0Events = SupportBean_S0.MakeS0("Y", new string[]{"Y-s0-1", "Y-s0-2"});
	        SendEvent(env, s0Events);

	        s1Events = SupportBean_S1.MakeS1("Y", new string[]{"Y-s1-1"});
	        SendEventsAndReset(env, s1Events);

	        s2Events = SupportBean_S2.MakeS2("Y", new string[]{"Y-s2-1"});
	        SendEvent(env, s2Events);

	        s3Events = SupportBean_S3.MakeS3("Y", new string[]{"Y-s3-1"});
	        SendEventsAndReset(env, s3Events);

	        s4Events = SupportBean_S4.MakeS4("Y", new string[]{"Y-s4-1"});
	        SendEventsAndReset(env, s4Events);

	        s5Events = SupportBean_S5.MakeS5("Y", new string[]{"X-s5-1"});
	        SendEvent(env, s5Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]}});

	        // Test s5 and s0=1, s1=1, s2=1, s3=2, s4=2
	        //
	        s0Events = SupportBean_S0.MakeS0("Z", new string[]{"Z-s0-1"});
	        SendEvent(env, s0Events);

	        s1Events = SupportBean_S1.MakeS1("Z", new string[]{"Z-s1-1"});
	        SendEventsAndReset(env, s1Events);

	        s2Events = SupportBean_S2.MakeS2("Z", new string[]{"Z-s2-1"});
	        SendEventsAndReset(env, s2Events);

	        s3Events = SupportBean_S3.MakeS3("Z", new string[]{"Z-s3-1", "Z-s3-2"});
	        SendEventsAndReset(env, s3Events);

	        s4Events = SupportBean_S4.MakeS4("Z", new string[]{"Z-s4-1", "Z-s4-2"});
	        SendEventsAndReset(env, s4Events);

	        s5Events = SupportBean_S5.MakeS5("Z", new string[]{"Z-s5-1"});
	        SendEvent(env, s5Events);
	        AssertListenerUnd(env, new object[][]{
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1], s5Events[0]}});

	        env.UndeployAll();
	    }

	    private static void SendEventsAndReset(RegressionEnvironment env, object[] events) {
	        SendEvent(env, events);
	        env.ListenerReset("s0");
	    }

	    private static void SendEvent(RegressionEnvironment env, object[] events) {
	        for (var i = 0; i < events.Length; i++) {
	            env.SendEventBean(events[i]);
	        }
	    }

	    private static void AssertListenerUnd(RegressionEnvironment env, object[][] expected) {
	        env.AssertListener("s0", listener => {
	            var und = ArrayHandlingUtil.GetUnderlyingEvents(listener.GetAndResetLastNewData(), new string[]{"s0", "s1", "s2", "s3", "s4", "s5"});
	            EPAssertionUtil.AssertSameAnyOrder(expected, und);
	        });
	    }
	}
} // end of namespace
