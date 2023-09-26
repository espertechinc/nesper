///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeThreadedConfigInboundFastShutdown : RegressionExecutionWithConfigure
    {
        public bool EnableHATest => true;
        public bool HAWithCOnly => false;

        public void Configure(Configuration configuration)
        {
            configuration.Runtime.Threading.IsThreadPoolInbound = true;
            configuration.Runtime.Threading.ThreadPoolInboundNumThreads = 2;
            configuration.Common.AddEventType(typeof(MyEvent));
            configuration.Compiler.AddPlugInSingleRowFunction("SleepaLittle", GetType().FullName, "SleepaLittle");
            configuration.Compiler.ByteCode.IsAllowSubscriber = true;
        }

        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy("@name('s0') select sleepaLittle(100) from MyEvent");
            env.Statement("s0").Subscriber = new MySubscriber();
            for (var i = 0; i < 10000; i++) {
                env.SendEventBean(new MyEvent());
            }

            env.UndeployAll();
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.OBSERVEROPS);
        }

        public static void SleepaLittle(int time)
        {
            try {
                Thread.Sleep(time);
            }
            catch (ThreadInterruptedException) {
            }
        }

        public class MySubscriber
        {
            public void Update(object[] args)
            {
            }
        }

        public class MyEvent
        {
        }
    }
} // end of namespace