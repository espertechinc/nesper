///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientThreadedConfigRoute : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Threading.IsInternalTimerEnabled = true;
            configuration.EngineDefaults.Expression.IsUdfCache = false;
            configuration.EngineDefaults.Threading.IsThreadPoolRouteExec = true;
            configuration.EngineDefaults.Threading.ThreadPoolRouteExecNumThreads = 5;
            configuration.AddEventType<SupportBean>();
            configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
        }
    
        public override void Run(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientThreadedConfigRoute))) {
                return;
            }
    
            Log.Debug("Creating statements");
            int countStatements = 100;
            var listener = new SupportListenerTimerHRes();
            for (int i = 0; i < countStatements; i++) {
                EPStatement statement = epService.EPAdministrator.CreateEPL("select SupportStaticMethodLib.Sleep(10) from SupportBean");
                statement.Events += listener.Update;
            }
    
            Log.Info("Sending trigger event");
            long start = PerformanceObserver.NanoTime;
            epService.EPRuntime.SendEvent(new SupportBean());
            long end = PerformanceObserver.NanoTime;
            long delta = (end - start) / 1000000;
            Assert.IsTrue(delta < 100, "Delta is " + delta);
    
            Thread.Sleep(2000);
            Assert.AreEqual(100, listener.NewEvents.Count);
            listener.NewEvents.Clear();
    
            // destroy all statements
            epService.EPAdministrator.DestroyAllStatements();
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select SupportStaticMethodLib.Sleep(10) from SupportBean, SupportBean");
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean());
            Thread.Sleep(100);
            Assert.AreEqual(1, listener.NewEvents.Count);
        }
    }
} // end of namespace
