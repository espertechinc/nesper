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
            WithClassLoaderOptionSimple(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithClassLoaderOptionSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployTypeResolverOptionSimple());
            return execs;
        }

        private class ClientDeployTypeResolverOptionSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean";
                var compiled = env.Compile(epl);
                var options = new DeploymentOptions();
                var mySupportTypeResolver = new MySupportTypeResolver();
                options.DeploymentTypeResolverOption = _ => mySupportTypeResolver;

                env.Deployment.Deploy(compiled, options);

                Assert.IsFalse(mySupportTypeResolver.Names.IsEmpty());

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class MySupportTypeResolver : TypeResolver
        {
            private readonly IList<string> _names = new List<string>();

            public Type ResolveType(
                string typeName,
                bool resolve)
            {
                _names.Add(typeName);
                return TypeHelper.ResolveType(typeName, resolve);
            }

            public IList<string> Names => _names;
        }
    }
} // end of namespace