///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
	public class EPLOuterJoinUnidirectional {

	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new EPLJoin2Stream());
	        execs.Add(new EPLJoin3StreamAllUnidirectional(false));
	        execs.Add(new EPLJoin3StreamAllUnidirectional(true));
	        execs.Add(new EPLJoin3StreamMixed());
	        execs.Add(new EPLJoin4StreamWhereClause());
	        execs.Add(new EPLJoinOuterInvalid());
	        return execs;
	    }

	    public class EPLJoinOuterInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            // all: unidirectional and full-outer-join

	            // no-view-declared
	            env.TryInvalidCompile(
	                "select * from SupportBean_A unidirectional full outer join SupportBean_B#keepall unidirectional",
	                "The unidirectional keyword requires that no views are declared onto the stream (applies to stream 1)");

	            // not-all-unidirectional
	            env.TryInvalidCompile(
	                "select * from SupportBean_A unidirectional full outer join SupportBean_B unidirectional full outer join SupportBean_C#keepall",
	                "The unidirectional keyword must either apply to a single stream or all streams in a full outer join");

	            // no iterate
	            SupportMessageAssertUtil.TryInvalidIterate(env,
	                "@name('s0') select * from SupportBean_A unidirectional full outer join SupportBean_B unidirectional",
	                "Iteration over a unidirectional join is not supported");
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.INVALIDITY);
	        }
	    }

	    private class EPLJoin2Stream : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select a.id as aid, b.id as bid from SupportBean_A as a unidirectional " +
	                      "full outer join SupportBean_B as b unidirectional";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            env.SendEventBean(new SupportBean_A("A1"));
	            AssertReceived2Stream(env, "A1", null);

	            env.SendEventBean(new SupportBean_B("B1"));
	            AssertReceived2Stream(env, null, "B1");

	            env.SendEventBean(new SupportBean_B("B2"));
	            AssertReceived2Stream(env, null, "B2");

	            env.SendEventBean(new SupportBean_A("A2"));
	            AssertReceived2Stream(env, "A2", null);

	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin3StreamAllUnidirectional : RegressionExecution {
	        private readonly bool soda;

	        public EPLJoin3StreamAllUnidirectional(bool soda) {
	            this.soda = soda;
	        }

	        public void Run(RegressionEnvironment env) {

	            var epl = "@name('s0') select * from SupportBean_A as a unidirectional " +
	                      "full outer join SupportBean_B as b unidirectional " +
	                      "full outer join SupportBean_C as c unidirectional";

	            env.CompileDeploy(soda, epl).AddListener("s0").Milestone(0);

	            env.SendEventBean(new SupportBean_A("A1"));
	            AssertReceived3Stream(env, "A1", null, null);

	            env.SendEventBean(new SupportBean_C("C1"));
	            AssertReceived3Stream(env, null, null, "C1");

	            env.SendEventBean(new SupportBean_C("C2"));
	            AssertReceived3Stream(env, null, null, "C2");

	            env.SendEventBean(new SupportBean_A("A2"));
	            AssertReceived3Stream(env, "A2", null, null);

	            env.SendEventBean(new SupportBean_B("B1"));
	            AssertReceived3Stream(env, null, "B1", null);

	            env.SendEventBean(new SupportBean_B("B2"));
	            AssertReceived3Stream(env, null, "B2", null);

	            env.UndeployAll();
	        }

	        public string Name() {
	            return $"{this.GetType().Name}{{soda={soda}}}";
	        }
	    }

	    private class EPLJoin3StreamMixed : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@public create window MyCWindow#keepall as SupportBean_C;\n" +
	                      "insert into MyCWindow select * from SupportBean_C;\n" +
	                      "@name('s0') select a.id as aid, b.id as bid, MyCWindow.id as cid, SupportBean_D.id as did " +
	                      "from pattern[every a=SupportBean_A -> b=SupportBean_B] t1 unidirectional " +
	                      "full outer join " +
	                      "MyCWindow unidirectional " +
	                      "full outer join " +
	                      "SupportBean_D unidirectional;\n";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            env.SendEventBean(new SupportBean_C("c1"));
	            AssertReceived3StreamMixed(env, null, null, "c1", null);

	            env.SendEventBean(new SupportBean_A("a1"));
	            env.SendEventBean(new SupportBean_B("b1"));
	            AssertReceived3StreamMixed(env, "a1", "b1", null, null);

	            env.SendEventBean(new SupportBean_A("a2"));
	            env.SendEventBean(new SupportBean_B("b2"));
	            AssertReceived3StreamMixed(env, "a2", "b2", null, null);

	            env.SendEventBean(new SupportBean_D("d1"));
	            AssertReceived3StreamMixed(env, null, null, null, "d1");

	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin4StreamWhereClause : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select * from SupportBean_A as a unidirectional " +
	                      "full outer join SupportBean_B as b unidirectional " +
	                      "full outer join SupportBean_C as c unidirectional " +
	                      "full outer join SupportBean_D as d unidirectional " +
	                      "where coalesce(a.id,b.id,c.id,d.id) in ('YES')";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            SendAssert(env, new SupportBean_A("A1"), false);
	            SendAssert(env, new SupportBean_A("YES"), true);
	            SendAssert(env, new SupportBean_C("YES"), true);
	            SendAssert(env, new SupportBean_C("C1"), false);
	            SendAssert(env, new SupportBean_D("YES"), true);
	            SendAssert(env, new SupportBean_B("YES"), true);
	            SendAssert(env, new SupportBean_B("B1"), false);

	            env.UndeployAll();
	        }
	    }

	    private static void SendAssert(RegressionEnvironment env, SupportBeanAtoFBase @event, bool b) {
	        env.SendEventBean(@event);
	        env.AssertListenerInvokedFlag("s0", b);
	    }

	    private static void AssertReceived2Stream(RegressionEnvironment env, string a, string b) {
	        var fields = "aid,bid".SplitCsv();
	        env.AssertPropsNew("s0", fields, new object[]{a, b});
	    }

	    private static void AssertReceived3Stream(RegressionEnvironment env, string a, string b, string c) {
	        var fields = "a.id,b.id,c.id".SplitCsv();
	        env.AssertPropsNew("s0", fields, new object[]{a, b, c});
	    }

	    private static void AssertReceived3StreamMixed(RegressionEnvironment env, string a, string b, string c, string d) {
	        var fields = "aid,bid,cid,did".SplitCsv();
	        env.AssertPropsNew("s0", fields, new object[]{a, b, c, d});
	    }
	}
} // end of namespace
