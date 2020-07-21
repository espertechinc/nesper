///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client.util;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.client.deploy
{
	public class ClientDeployVersion
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientDeployVersionMinorCheck());
			return execs;
		}

		private class ClientDeployVersionMinorCheck : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var filename = "regression/epcompiled_version_8.0.0.epl_jar_for_deployment";
				string file = FileUtil.FindClasspathFile(filename);
				if (file == null) {
					throw new RuntimeException("Failed to find file " + filename);
				}

				EPCompiled compiled = EPCompiledIOUtil.Read(new File(file));

				var versionMismatchMsg =
					"Major or minor version of compiler and runtime mismatch; The runtime version is 8.5.0 and the compiler version of the compiled unit is 8.0.0";
				AssertMessage(
					Assert.Throws<EPDeployDeploymentVersionException>(
						() => env.Runtime.DeploymentService.Deploy(compiled)),
					versionMismatchMsg);

				var ex1 = Assert.Throws<EPDeployDeploymentVersionException>(
					() => env.Runtime.DeploymentService.Rollout(Collections.SingletonList(new EPDeploymentRolloutCompiled(compiled))));
				Assert.AreEqual(0, ex1.RolloutItemNumber);
				AssertMessage(ex1, versionMismatchMsg);

				AssertMessage(
					Assert.Throws<EPException>(
						() => env.Runtime.FireAndForgetService.ExecuteQuery(compiled)),
					"Major or minor version of compiler and runtime mismatch; The runtime version is 8.5.0 and the compiler version of the compiled unit is 8.0.0");
			}
		}
	}
} // end of namespace
