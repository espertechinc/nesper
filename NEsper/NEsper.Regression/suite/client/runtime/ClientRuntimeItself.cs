///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeItself
    {
        public const string TEST_SERVICE_NAME = "TEST_SERVICE_NAME";
        public const int TEST_SECRET_VALUE = 12345;

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientRuntimeItselfTransientConfiguration());
            return execs;
        }

        internal class ClientRuntimeItselfTransientConfiguration : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from SupportBean");
                var listener = new MyListener();
                env.Statement("s0").AddListener(listener);

                env.SendEventBean(new SupportBean());
                Assert.AreEqual(TEST_SECRET_VALUE, listener.SecretValue);

                env.UndeployAll();
            }
        }

        public class MyLocalService
        {
            public MyLocalService(int secretValue)
            {
                SecretValue = secretValue;
            }

            internal int SecretValue { get; }
        }

        public class MyListener : UpdateListener
        {
            internal int SecretValue { get; private set; }

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                var runtime = eventArgs.Runtime;
                var svc = (MyLocalService) runtime.ConfigurationTransient.Get(TEST_SERVICE_NAME);
                SecretValue = svc.SecretValue;
            }
        }
    }
} // end of namespace