///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.support.SupportEventTypeAssertionEnum;
using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.define
{
	public class ExprDefineValueParameter
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ExprDefineValueParameterV());
			execs.Add(new ExprDefineValueParameterVV());
			execs.Add(new ExprDefineValueParameterVVV());
			execs.Add(new ExprDefineValueParameterEV());
			execs.Add(new ExprDefineValueParameterVEV());
			execs.Add(new ExprDefineValueParameterVEVE());
			execs.Add(new ExprDefineValueParameterEVE());
			execs.Add(new ExprDefineValueParameterEVEVE());
			execs.Add(new ExprDefineValueParameterInvalid());
			execs.Add(new ExprDefineValueParameterCache());
			execs.Add(new ExprDefineValueParameterVariable());
			execs.Add(new ExprDefineValueParameterSubquery());
			return execs;
		}

		private class ExprDefineValueParameterSubquery : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('s0') expression cc { (v1, v2) -> v1 || v2} " +
				             "select cc((select p00 from SupportBean_S0#lastevent), (select p01 from SupportBean_S0#lastevent)) as c0 from SupportBean_S1";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean_S1(0));
				Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

				env.UndeployAll();
			}
		}

		private class ExprDefineValueParameterV : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy("@name('s0') expression returnsSame {v -> v} select returnsSame(1) as c0 from SupportBean").AddListener("s0");
				string[] fields = "c0".SplitCsv();
				AssertTypeExpected(env, typeof(int?));

				env.SendEventBean(new SupportBean());
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {1});

				env.UndeployAll();
			}
		}

		private class ExprDefineValueParameterVV : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy("@name('s0') expression cc { (v1, v2) -> v1 || v2} select cc(p00, p01) as c0 from SupportBean_S0").AddListener("s0");
				AssertTypeExpected(env, typeof(string));

				SendAssert(env, "AB", "A", "B");
				SendAssert(env, null, "A", null);
				SendAssert(env, null, null, "B");
				SendAssert(env, "CD", "C", "D");

				env.UndeployAll();
			}
		}

		private class ExprDefineValueParameterVVV : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy("@name('s0') expression cc { (v1, v2, v3) -> v1 || v2 || v3} select cc(p00, p01, p02) as c0 from SupportBean_S0")
					.AddListener("s0");
				AssertTypeExpected(env, typeof(string));

				SendAssert(env, "ABC", "A", "B", "C");
				SendAssert(env, null, "A", null, "C");
				SendAssert(env, "DEF", "D", "E", "F");

				env.UndeployAll();
			}
		}

		private class ExprDefineValueParameterEV : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy("@name('s0') expression cc { (e,v) -> e.p00 || v} select cc(e, p01) as c0 from SupportBean_S0 as e").AddListener("s0");
				AssertTypeExpected(env, typeof(string));

				SendAssert(env, "AB", "A", "B");
				SendAssert(env, "BC", "B", "C");

				env.UndeployAll();
			}
		}

		private class ExprDefineValueParameterVEV : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy("@name('s0') expression cc { (v1,e,v2) -> v1 || e.p01 || v2} select cc(p00, e, p02) as c0 from SupportBean_S0 as e")
					.AddListener("s0");
				AssertTypeExpected(env, typeof(string));

				SendAssert(env, "ABC", "A", "B", "C");
				SendAssert(env, null, null, "B", null);
				SendAssert(env, "BCD", "B", "C", "D");

				env.UndeployAll();
			}
		}

		private class ExprDefineValueParameterVEVE : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl;

				epl = "@name('s0') expression cc { (v1,e1,v2,e2) -> v1 || e1.p01 || v2 || e2.p11} " +
				      "select cc(e1.p00, e1, e2.p10, e2) as c0 from SupportBean_S0#lastevent as e1, SupportBean_S1#lastevent as e2";
				AssertJoin(env, epl);

				epl = "@name('s0') expression cc { (v1,e1,v2,e2) -> v1 || e1.p01 || v2 || e2.p11} " +
				      "select cc(e1.p00, e1, e2.p10, e2) as c0 from SupportBean_S1#lastevent as e2, SupportBean_S0#lastevent as e1";
				AssertJoin(env, epl);
			}

			private void AssertJoin(
				RegressionEnvironment env,
				string epl)
			{
				env.CompileDeploy(epl).AddListener("s0");
				AssertTypeExpected(env, typeof(string));

				env.SendEventBean(new SupportBean_S0(1, "A", "B"));
				env.SendEventBean(new SupportBean_S1(2, "X", "Y"));
				Assert.AreEqual("ABXY", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

				env.SendEventBean(new SupportBean_S1(2, "Z", "P"));
				Assert.AreEqual("ABZP", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

				env.SendEventBean(new SupportBean_S0(1, "D", "E"));
				Assert.AreEqual("DEZP", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

				env.UndeployAll();
			}
		}

		private class ExprDefineValueParameterEVE : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('s0') expression cc { (e1,v,e2) -> e1.p00 || v || e2.p10} " +
				             "select cc(e2, 'x', e1) as c0 from SupportBean_S1#lastevent as e1, SupportBean_S0#lastevent as e2";
				env.CompileDeploy(epl).AddListener("s0");
				AssertTypeExpected(env, typeof(string));

				env.SendEventBean(new SupportBean_S0(1, "A"));
				env.SendEventBean(new SupportBean_S1(2, "1"));
				Assert.AreEqual("Ax1", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

				env.SendEventBean(new SupportBean_S1(2, "2"));
				Assert.AreEqual("Ax2", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

				env.SendEventBean(new SupportBean_S0(1, "B"));
				Assert.AreEqual("Bx2", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

				env.UndeployAll();
			}
		}

		private class ExprDefineValueParameterEVEVE : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();

				string expression = "@public create expression cc { (a,v1,b,v2,c) -> a.p00 || v1 || b.p00 || v2 || c.p00}";
				env.CompileDeploy(expression, path);

				string epl =
					"@name('s0') select cc(e2, 'x', e3, 'y', e1) as c0 from \n" +
					"SupportBean_S0(id=1)#lastevent as e1, SupportBean_S0(id=2)#lastevent as e2, SupportBean_S0(id=3)#lastevent as e3;\n" +
					"@name('s1') select cc(e2, 'x', e3, 'y', e1) as c0 from \n" +
					"SupportBean_S0(id=1)#lastevent as e3, SupportBean_S0(id=2)#lastevent as e2, SupportBean_S0(id=3)#lastevent as e1;\n" +
					"@name('s2') select cc(e1, 'x', e2, 'y', e3) as c0 from \n" +
					"SupportBean_S0(id=1)#lastevent as e3, SupportBean_S0(id=2)#lastevent as e2, SupportBean_S0(id=3)#lastevent as e1;\n";
				env.CompileDeploy(epl, path).AddListener("s0").AddListener("s1").AddListener("s2");
				AssertTypeExpected(env, typeof(string));

				env.SendEventBean(new SupportBean_S0(1, "A"));
				env.SendEventBean(new SupportBean_S0(3, "C"));
				env.SendEventBean(new SupportBean_S0(2, "B"));
				Assert.AreEqual("BxCyA", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
				Assert.AreEqual("BxAyC", env.Listener("s1").AssertOneGetNewAndReset().Get("c0"));
				Assert.AreEqual("CxByA", env.Listener("s2").AssertOneGetNewAndReset().Get("c0"));

				env.UndeployAll();
			}
		}

		private class ExprDefineValueParameterInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryInvalidCompile(
					env,
					"expression cc{(v1,v2) -> v1 || v2} select cc(1, 2) from SupportBean",
					"Failed to validate select-clause expression 'cc(1,2)': Failed to validate expression declaration 'cc': Failed to validate declared expression body expression 'v1||v2': Implicit conversion from datatype 'Integer' to string is not allowed");
			}
		}

		private class ExprDefineValueParameterCache : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create variable ExprDefineLocalService myService = new ExprDefineLocalService();\n" +
				             "create expression doit {v -> myService.calc(v)};\n" +
				             "@name('s0') select doit(theString) as c0 from SupportBean;\n";
				ExprDefineLocalService.Services.Clear();
				env.CompileDeploy(epl).AddListener("s0");
				ExprDefineLocalService service = ExprDefineLocalService.Services[0];

				env.SendEventBean(new SupportBean("E10", -1));
				Assert.AreEqual(10, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
				Assert.AreEqual(1, service.Calculations.Count);

				env.SendEventBean(new SupportBean("E10", -1));
				Assert.AreEqual(10, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
				Assert.AreEqual(2, service.Calculations.Count);

				ExprDefineLocalService.Services.Clear();
				env.UndeployAll();
			}
		}

		private class ExprDefineValueParameterVariable : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@public @buseventtype create schema A (value1 double, value2 double);\n" +
				             "create variable double C=1.2;\n" +
				             "create variable double D=1.5;\n" +
				             "\n" +
				             "create expression E {(V1,V2)=>max(V1,V2)};\n" +
				             "\n" +
				             "@name('s0') select E(value1,value2) as c0, E(value1,C) as c1, E(C,D) as c2 from A;\n";
				env.CompileDeploy(epl).AddListener("s0");
				string[] fields = "c0,c1,c2".SplitCsv();

				env.SendEventMap(CollectionUtil.BuildMap("value1", 1d, "value2", 1.5d), "A");
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					fields,
					new object[] {1.5d, 1.2d, 1.5d});

				env.Runtime.VariableService.SetVariableValue(env.DeploymentId("s0"), "D", 1.1d);

				env.SendEventMap(CollectionUtil.BuildMap("value1", 1.8d, "value2", 1.5d), "A");
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					fields,
					new object[] {1.8d, 1.8d, 1.2d});

				env.UndeployAll();
			}
		}

		private static void AssertTypeExpected(
			RegressionEnvironment env,
			Type clazz)
		{
			object[][] expectedColTypes = new object[][] {
				new object[] {"c0", clazz},
			};
			SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedColTypes, env.Statement("s0").EventType, NAME, TYPE);
		}

		private static void SendAssert(
			RegressionEnvironment env,
			string expected,
			string p00,
			string p01)
		{
			SendAssert(env, expected, p00, p01, null);
		}

		private static void SendAssert(
			RegressionEnvironment env,
			string expected,
			string p00,
			string p01,
			string p02)
		{
			string[] fields = "c0".SplitCsv();
			env.SendEventBean(new SupportBean_S0(0, p00, p01, p02));
			EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {expected});
		}

		public class ExprDefineLocalService
		{
			internal static readonly IList<ExprDefineLocalService> Services = new List<ExprDefineLocalService>();

			private IList<string> calculations = new List<string>();

			public ExprDefineLocalService()
			{
				Services.Add(this);
			}

			public int Calc(string value)
			{
				calculations.Add(value);
				return int.Parse(value.Substring(1));
			}

			public IList<string> Calculations => calculations;
		}
	}
} // end of namespace
