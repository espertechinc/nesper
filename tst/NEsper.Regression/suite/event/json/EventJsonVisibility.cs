///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
	public class EventJsonVisibility {

	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new EventJsonVisibilityPublicSameModule());
	        execs.Add(new EventJsonVisibilityPublicTwoModulesBinaryPath());
	        execs.Add(new EventJsonVisibilityPublicTwoModulesRuntimePath());
	        execs.Add(new EventJsonVisibilityProtected());
	        return execs;
	    }

	    private class EventJsonVisibilityProtected : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var moduleA = "module A.X; @protected create json schema JsonSchema(fruit string, size string);\n";
	            var moduleB = "module B; @protected create json schema JsonSchema(carId string);\n";

	            var pathA = new RegressionPath();
	            env.CompileDeploy(moduleA, pathA);
	            var pathB = new RegressionPath();
	            env.CompileDeploy(moduleB, pathB);

	            env.CompileDeploy("module A.X; insert into JsonSchema select theString as fruit, 'large' as size from SupportBean;\n" +
	                "@name('a') select fruit, size from JsonSchema#keepall", pathA).AddListener("a");
	            env.CompileDeploy("module B; insert into JsonSchema select theString as carId from SupportBean;\n" +
	                "@name('b') select carId from JsonSchema#keepall", pathB).AddListener("b");

	            env.SendEventBean(new SupportBean("E1", 0));
	            env.AssertEventNew("a", this.AssertFruit);
	            env.AssertEventNew("b", this.AssertCar);

	            env.Milestone(0);

	            env.AssertIterator("a", it => AssertFruit(it.Advance()));
	            env.AssertIterator("b", it => AssertCar(it.Advance()));

	            env.UndeployAll();
	        }

	        private void AssertCar(EventBean @event) {
	            EPAssertionUtil.AssertProps(@event, "carId".SplitCsv(), new object[]{"E1"});
	        }

	        private void AssertFruit(EventBean @event) {
	            EPAssertionUtil.AssertProps(@event, "fruit,size".SplitCsv(), new object[]{"E1", "large"});
	        }
	    }

	    private class EventJsonVisibilityPublicSameModule : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@public @buseventtype create json schema SimpleJson(fruit string, size string, color string);\n" +
	                      "@name('s0') select fruit, size, color from SimpleJson#keepall;\n";
	            env.CompileDeploy(epl).AddListener("s0");

	            RunAssertionSimple(env);

	            env.UndeployAll();
	        }
	    }

	    private class EventJsonVisibilityPublicTwoModulesBinaryPath : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@public @buseventtype create json schema SimpleJson(fruit string, size string, color string)", path);
	            env.CompileDeploy("@name('s0') select fruit, size, color from SimpleJson#keepall", path).AddListener("s0");

	            RunAssertionSimple(env);

	            env.UndeployAll();
	        }
	    }

	    private class EventJsonVisibilityPublicTwoModulesRuntimePath : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.CompileDeploy("@public @buseventtype create json schema SimpleJson(fruit string, size string, color string)");
	            var epl = "@name('s0') select fruit, size, color from SimpleJson#keepall";
	            var compiled = env.CompileWRuntimePath(epl);
	            env.Deploy(compiled).AddListener("s0");

	            RunAssertionSimple(env);

	            env.UndeployAll();
	        }
	    }

	    private static void RunAssertionSimple(RegressionEnvironment env) {
	        var json = "{ \"fruit\": \"Apple\", \"size\": \"Large\", \"color\": \"Red\"}";
	        env.SendEventJson(json, "SimpleJson");
	        env.AssertEventNew("s0", EventJsonVisibility.AssertFruitApple);

	        json = "{ \"fruit\": \"Peach\", \"size\": \"Small\", \"color\": \"Yellow\"}";
	        env.SendEventJson(json, "SimpleJson");
	        env.AssertEventNew("s0", EventJsonVisibility.AssertFruitPeach);

	        env.Milestone(0);

	        env.AssertIterator("s0", it => {
	            AssertFruitApple(it.Advance());
	            AssertFruitPeach(it.Advance());
	        });
	    }

	    private static void AssertFruitPeach(EventBean @event) {
	        EPAssertionUtil.AssertProps(@event, "fruit,size,color".SplitCsv(), new object[]{"Peach", "Small", "Yellow"});
	    }

	    private static void AssertFruitApple(EventBean @event) {
	        EPAssertionUtil.AssertProps(@event, "fruit,size,color".SplitCsv(), new object[]{"Apple", "Large", "Red"});
	    }
	}
} // end of namespace
