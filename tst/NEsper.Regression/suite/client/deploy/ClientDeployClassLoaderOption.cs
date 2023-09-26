///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

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
            Withe(execs);
            return execs;
        }

        public static IList<RegressionExecution> Withe(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployClassLoaderOptionSimple());
            return execs;
        }

        private class ClientDeployClassLoaderOptionSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean";
                var compiled = env.Compile(epl);
                var options = new DeploymentOptions();
                var mySupportClassloader = new MySupportClassloader();
                options.DeploymentClassLoaderOption = _ => mySupportClassloader;

                env.Deployment.Deploy(compiled, options);

                Assert.IsFalse(mySupportClassloader.Names.IsEmpty());

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class MySupportClassloader : TypeResolver
        {
            private readonly IList<string> names = new List<string>();

            public Type ResolveType(
                string typeName,
                bool resolve)
            {
                names.Add(typeName);
                return TypeHelper.ResolveType(typeName, resolve);
            }

            public IList<string> Names => names;
        }
    }
} // end of namespace