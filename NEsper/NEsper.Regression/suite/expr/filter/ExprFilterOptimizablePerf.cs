///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
	public class ExprFilterOptimizablePerf
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
WithOr(execs);
WithEqualsWithFunc(execs);
WithTrueWithFunc(execs);
WithEqualsDeclaredExpr(execs);
WithTrueDeclaredExpr(execs);
			return execs;
		}
public static IList<RegressionExecution> WithTrueDeclaredExpr(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new ExprFilterOptimizablePerfTrueDeclaredExpr());
    return execs;
}public static IList<RegressionExecution> WithEqualsDeclaredExpr(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new ExprFilterOptimizablePerfEqualsDeclaredExpr());
    return execs;
}public static IList<RegressionExecution> WithTrueWithFunc(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new ExprFilterOptimizablePerfTrueWithFunc());
    return execs;
}public static IList<RegressionExecution> WithEqualsWithFunc(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new ExprFilterOptimizablePerfEqualsWithFunc());
    return execs;
}public static IList<RegressionExecution> WithOr(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new ExprFilterOptimizablePerfOr());
    return execs;
}
		private class ExprFilterOptimizablePerfEqualsWithFunc : RegressionExecution
		{
			public bool ExcludeWhenInstrumented()
			{
				return true;
			}

			public void Run(RegressionEnvironment env)
			{
				// func(...) = value
				TryOptimizableEquals(env, new RegressionPath(), "select * from SupportBean(libSplit(TheString) = !NUM!)", 10);
			}
		}

		private class ExprFilterOptimizablePerfTrueWithFunc : RegressionExecution
		{
			public bool ExcludeWhenInstrumented()
			{
				return true;
			}

			public void Run(RegressionEnvironment env)
			{
				// func(...) implied true
				TryOptimizableBoolean(env, new RegressionPath(), "select * from SupportBean(libE1True(TheString))");
			}
		}

		private class ExprFilterOptimizablePerfEqualsDeclaredExpr : RegressionExecution
		{
			public bool ExcludeWhenInstrumented()
			{
				return true;
			}

			public void Run(RegressionEnvironment env)
			{
				// declared expression (...) = value
				RegressionPath path = new RegressionPath();
				env.CompileDeploy("@Name('create-expr') create expression thesplit {TheString => libSplit(TheString)}", path).AddListener("create-expr");
				TryOptimizableEquals(env, path, "select * from SupportBean(thesplit(*) = !NUM!)", 10);
			}
		}

		private class ExprFilterOptimizablePerfTrueDeclaredExpr : RegressionExecution
		{
			public bool ExcludeWhenInstrumented()
			{
				return true;
			}

			public void Run(RegressionEnvironment env)
			{
				// declared expression (...) implied true
				RegressionPath path = new RegressionPath();
				env.CompileDeploy("@Name('create-expr') create expression theE1Test {TheString => libE1True(TheString)}", path).AddListener("create-expr");
				TryOptimizableBoolean(env, path, "select * from SupportBean(theE1Test(*))");
			}
		}

		private class ExprFilterOptimizablePerfOr : RegressionExecution
		{
			public bool ExcludeWhenInstrumented()
			{
				return true;
			}

			public void Run(RegressionEnvironment env)
			{
				SupportUpdateListener listener = new SupportUpdateListener();
				for (int i = 0; i < 100; i++) {
					string epl = "@Name('s" + i + "') select * from SupportBean(TheString = '" + i + "' or IntPrimitive=" + i + ")";
					EPCompiled compiled = env.Compile(epl);
					env.Deploy(compiled).Statement("s" + i).AddListener(listener);
				}

				var delta = PerformanceObserver.TimeMillis(
					() => {
						// System.out.println("Starting " + DateTime.print(new Date()));
						for (int i = 0; i < 10000; i++) {
							env.SendEventBean(new SupportBean("100", 1));
							Assert.IsTrue(listener.IsInvoked);
							listener.Reset();
						}
					});
#if DEBUG
				Assert.That(delta, Is.LessThan(1500));
#else
				Assert.That(delta, Is.LessThan(500));
#endif

				env.UndeployAll();
			}
		}

		private static void TryOptimizableEquals(
			RegressionEnvironment env,
			RegressionPath path,
			string epl,
			int numStatements)
		{
			// test function returns lookup value and "Equals"
			for (int i = 0; i < numStatements; i++) {
				string text = "@Name('s" + i + "') " + epl.Replace("!NUM!", i.ToString());
				env.CompileDeploy(text, path).AddListener("s" + i);
			}

			env.Milestone(0);

			var loops = 1000;
			var delta = PerformanceObserver.TimeMillis(
				() => {
					SupportStaticMethodLib.ResetCountInvoked();
					for (int i = 0; i < loops; i++) {
						env.SendEventBean(new SupportBean("E_" + i % numStatements, 0));
						SupportListener listener = env.Listener("s" + i % numStatements);
						Assert.IsTrue(listener.GetAndClearIsInvoked());
					}
				});

			Assert.AreEqual(loops, SupportStaticMethodLib.CountInvoked);

			Assert.IsTrue(delta < 1000, "Delta is " + delta);
			env.UndeployAll();
		}

		private static void TryOptimizableBoolean(
			RegressionEnvironment env,
			RegressionPath path,
			string epl)
		{

			// test function returns lookup value and "Equals"
			int count = 10;
			for (int i = 0; i < count; i++) {
				EPCompiled compiled = env.Compile("@Name('s" + i + "')" + epl, path);
				EPDeploymentService admin = env.Runtime.DeploymentService;
				try {
					admin.Deploy(compiled);
				}
				catch (EPDeployException) {
					Assert.Fail();
				}
			}

			env.Milestone(0);

			SupportUpdateListener listener = new SupportUpdateListener();
			for (int i = 0; i < 10; i++) {
				env.Statement("s" + i).AddListener(listener);
			}

			var loops = 10000;
			var delta = PerformanceObserver.TimeMillis(
				() => {
					SupportStaticMethodLib.ResetCountInvoked();
					for (int i = 0; i < loops; i++) {
						string key = "E_" + i % 100;
						env.SendEventBean(new SupportBean(key, 0));
						if (key.Equals("E_1")) {
							Assert.AreEqual(count, listener.NewDataList.Count);
							listener.Reset();
						}
						else {
							Assert.IsFalse(listener.IsInvoked);
						}
					}
				});
			Assert.AreEqual(loops, SupportStaticMethodLib.CountInvoked);

			Assert.IsTrue(delta < 1000, "Delta is " + delta);
			env.UndeployAll();
		}
	}
} // end of namespace
