///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeLockLogging : RegressionExecutionWithConfigure
    {
        public void Configure(Configuration configuration)
        {
            configuration.Runtime.Threading.IsInternalTimerEnabled = false;
            configuration.Common.AddEventType("SupportBean", typeof(SupportBean));
            configuration.Runtime.Logging.IsEnableLockActivity = true;
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.RUNTIMEOPS);
        }

        public void Run(RegressionEnvironment env)
        {
            RunAssertionLockLogging(env);
        }

        private void RunAssertionLockLogging(RegressionEnvironment env)
        {
            var epl = "@name('s0') select count(*) as c0 from SupportBean";
            env.CompileDeploy(epl);

            env.SendEventBean(new SupportBean());
            env.AssertSafeEnumerator(
                "s0",
                en => {
                    Assert.AreEqual(1L, en.Advance().Get("c0"));
                    en.Dispose();
                });

            env.UndeployAll();
        }
    }
} // end of namespace