///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;


namespace com.espertech.esper.regressionlib.suite.@event.json
{
	public class EventJsonVisibility
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EventJsonVisibilityPublicSameModule());
			execs.Add(new EventJsonVisibilityPublicTwoModulesBinaryPath());
			execs.Add(new EventJsonVisibilityPublicTwoModulesRuntimePath());
			execs.Add(new EventJsonVisibilityProtected());
			return execs;
		}

		internal class EventJsonVisibilityProtected : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string moduleA = "module A; @protected create json schema JsonSchema(fruit string, size string);\n";
				string moduleB = "module B; @protected create json schema JsonSchema(carId string);\n";

				RegressionPath pathA = new RegressionPath();
				env.CompileDeploy(moduleA, pathA);
				RegressionPath pathB = new RegressionPath();
				env.CompileDeploy(moduleB, pathB);

				env.CompileDeploy(
						"insert into JsonSchema select theString as fruit, 'large' as size from SupportBean;\n" +
						"@name('a') select fruit, size from JsonSchema#keepall",
						pathA)
					.AddListener("a");
				env.CompileDeploy(
						"insert into JsonSchema select theString as carId from SupportBean;\n" +
						"@name('b') select carId from JsonSchema#keepall",
						pathB)
					.AddListener("b");

				env.SendEventBean(new SupportBean("E1", 0));
				AssertFruit(env.Listener("a").AssertOneGetNewAndReset());
				AssertCar(env.Listener("b").AssertOneGetNewAndReset());

				env.Milestone(0);

				AssertFruit(env.Statement("a").First());
				AssertCar(env.Statement("b").First());

				env.UndeployAll();
			}

			private void AssertCar(EventBean @event)
			{
				EPAssertionUtil.AssertProps(@event, "carId".SplitCsv(), new object[] {"E1"});
			}

			private void AssertFruit(EventBean @event)
			{
				EPAssertionUtil.AssertProps(@event, "fruit,size".SplitCsv(), new object[] {"E1", "large"});
			}
		}

		internal class EventJsonVisibilityPublicSameModule : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@public @buseventtype create json schema SimpleJson(fruit string, size string, color string);\n" +
				             "@name('s0') select fruit, size, color from SimpleJson#keepall;\n";
				env.CompileDeploy(epl).AddListener("s0");

				RunAssertionSimple(env);

				env.UndeployAll();
			}
		}

		internal class EventJsonVisibilityPublicTwoModulesBinaryPath : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				env.CompileDeploy("@public @buseventtype create json schema SimpleJson(fruit string, size string, color string)", path);
				env.CompileDeploy("@name('s0') select fruit, size, color from SimpleJson#keepall", path).AddListener("s0");

				RunAssertionSimple(env);

				env.UndeployAll();
			}
		}

		internal class EventJsonVisibilityPublicTwoModulesRuntimePath : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy("@public @buseventtype create json schema SimpleJson(fruit string, size string, color string)");
				string epl = "@name('s0') select fruit, size, color from SimpleJson#keepall";
				EPCompiled compiled = env.Compile(epl, new CompilerArguments(env.Runtime.RuntimePath));
				env.Deploy(compiled).AddListener("s0");

				RunAssertionSimple(env);

				env.UndeployAll();
			}
		}

		private static void RunAssertionSimple(RegressionEnvironment env)
		{
			string json = "{ \"fruit\": \"Apple\", \"size\": \"Large\", \"color\": \"Red\"}";
			env.SendEventJson(json, "SimpleJson");
			AssertFruitApple(env.Listener("s0").AssertOneGetNewAndReset());

			EventSender eventSender = env.Runtime.EventService.GetEventSender("SimpleJson");
			json = "{ \"fruit\": \"Peach\", \"size\": \"Small\", \"color\": \"Yellow\"}";
			eventSender.SendEvent(json);
			AssertFruitPeach(env.Listener("s0").AssertOneGetNewAndReset());

			env.Milestone(0);

			IEnumerator<EventBean> it = env.Statement("s0").GetEnumerator();
			AssertFruitApple(it.Advance());
			AssertFruitPeach(it.Advance());
		}

		private static void AssertFruitPeach(EventBean @event)
		{
			EPAssertionUtil.AssertProps(@event, "fruit,size,color".SplitCsv(), new object[] {"Peach", "Small", "Yellow"});
		}

		private static void AssertFruitApple(EventBean @event)
		{
			EPAssertionUtil.AssertProps(@event, "fruit,size,color".SplitCsv(), new object[] {"Apple", "Large", "Red"});
		}
	}
} // end of namespace
