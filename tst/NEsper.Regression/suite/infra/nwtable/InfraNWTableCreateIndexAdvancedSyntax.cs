///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.soda;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
	public class InfraNWTableCreateIndexAdvancedSyntax : RegressionExecution {
	    public void Run(RegressionEnvironment env) {
	        AssertCompileSODA(env, "create index MyIndex on MyWindow((x,y) dummy_name(\"a\",10101))");
	        AssertCompileSODA(env, "create index MyIndex on MyWindow(x dummy_name)");
	        AssertCompileSODA(env, "create index MyIndex on MyWindow((x,y,z) dummy_name)");
	        AssertCompileSODA(env, "create index MyIndex on MyWindow(x dummy_name, (y,z) dummy_name_2(\"a\"), p dummyname3)");

	        var path = new RegressionPath();
	        env.CompileDeploy("@public create window MyWindow#keepall as SupportSpatialPoint", path);

	        env.TryInvalidCompile(path, "create index MyIndex on MyWindow(())",
	            "Invalid empty list of index expressions");

	        env.TryInvalidCompile(path, "create index MyIndex on MyWindow(intPrimitive+1)",
	            "Invalid index expression 'intPrimitive+1'");

	        env.TryInvalidCompile(path, "create index MyIndex on MyWindow((x, y))",
	            "Invalid multiple index expressions");

	        env.TryInvalidCompile(path, "create index MyIndex on MyWindow(x.y)",
	            "Invalid index expression 'x.y'");

	        env.TryInvalidCompile(path, "create index MyIndex on MyWindow(id xxxx)",
	            "Unrecognized advanced-type index 'xxxx'");

	        env.UndeployAll();
	    }

	    private static void AssertCompileSODA(RegressionEnvironment env, string epl) {
	        var model = env.EplToModel(epl);
	        Assert.AreEqual(epl, model.ToEPL());
	    }
	}
} // end of namespace
