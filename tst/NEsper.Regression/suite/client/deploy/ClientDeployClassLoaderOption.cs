///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.deploy
{
	public class ClientDeployClassLoaderOption
	{

		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientDeployClassLoaderOptionSimple());
			return execs;
		}

		private class ClientDeployClassLoaderOptionSimple : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@Name('s0') select * from SupportBean";
				var compiled = env.Compile(epl);
				var options = new DeploymentOptions();
				var mySupportClassloader = new MySupportClassloader();
				options.DeploymentClassLoaderOption = _ => mySupportClassloader;

				env.Deployment.Deploy(compiled, options);

				Assert.IsFalse(mySupportClassloader.Names.IsEmpty());

				env.UndeployAll();
			}
		}

		public class MySupportClassloader : ClassLoader
		{
			private readonly IList<string> names = new List<string>();

			public Type GetClass(string typeName)
			{
				names.Add(typeName);
				return TypeHelper.ResolveType(typeName);
			}

			public IList<string> Names => names;
		}
	}
} // end of namespace
