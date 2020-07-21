///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.autoname.two;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
	public class ClientCompileEventTypeAutoName
	{

		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientCompileAutoNameResolve());
			execs.Add(new ClientCompileAutoNameAmbiguous());
			return execs;
		}

		public class ClientCompileAutoNameResolve : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create schema MANE as MyAutoNameEvent;\n" +
				             "@name('s0') select p0 from MANE;\n";
				EPCompiled compiled = env.CompileWBusPublicType(epl);
				env.Deploy(compiled).AddListener("s0");

				env.SendEventBean(new MyAutoNameEvent("test"), "MANE");
				Assert.AreEqual("test", env.Listener("s0").AssertOneGetNewAndReset().Get("p0"));

				env.UndeployAll();
			}
		}

		public class ClientCompileAutoNameAmbiguous : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryInvalidCompile(
					env,
					"create schema SupportAmbigousEventType as SupportAmbigousEventType",
					"Failed to resolve name 'SupportAmbigousEventType', the class was ambigously found both in package 'com.espertech.esper.regressionlib.support.autoname.one' and in package 'com.espertech.esper.regressionlib.support.autoname.two'");
			}
		}
	}
} // end of namespace
