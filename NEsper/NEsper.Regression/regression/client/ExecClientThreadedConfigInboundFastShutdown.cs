///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.client
{
    public class ExecClientThreadedConfigInboundFastShutdown : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Threading.IsThreadPoolInbound = true;
            configuration.EngineDefaults.Threading.ThreadPoolInboundNumThreads = 2;
            configuration.AddEventType(typeof(MyEvent));
            configuration.AddPlugInSingleRowFunction("sleepaLittle", GetType(), "SleepaLittle");
        }
    
        public override void Run(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select sleepaLittle(100) from MyEvent");
            stmt.Subscriber = new MySubscriber();
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new MyEvent());
            }
        }
    
        public static void SleepaLittle(long time) {
            try {
                Thread.Sleep((int) time);
            } catch (ThreadInterruptedException) {
            }
        }
    
        public class MySubscriber {
            public void Update(object[] args) {
            }
        }
    
        public class MyEvent {
        }
    }
} // end of namespace
