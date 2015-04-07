///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestThreadedConfigRoute 
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        [Test]
        public void TestOp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = true;
            config.EngineDefaults.ExpressionConfig.IsUdfCache = false;
            config.EngineDefaults.ThreadingConfig.IsThreadPoolRouteExec = true;
            config.EngineDefaults.ThreadingConfig.ThreadPoolRouteExecNumThreads = 5;
            config.AddEventType("SupportBean", typeof(SupportBean));
            config.AddImport(typeof(SupportStaticMethodLib).FullName);
    
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
    
            Log.Debug("Creating statements");
            int countStatements = 100;
            SupportListenerTimerHRes listener = new SupportListenerTimerHRes();
            for (int i = 0; i < countStatements; i++)
            {
                EPStatement innerStmt = epService.EPAdministrator.CreateEPL("select SupportStaticMethodLib.Sleep(10) from SupportBean");
                innerStmt.Events += listener.Update;
            }
    
            Log.Info("Sending trigger event");
            long delta = PerformanceObserver.TimeMillis(() => epService.EPRuntime.SendEvent(new SupportBean()));
            Assert.LessOrEqual(delta, 100, "Delta is " + delta);
            
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
    
            epService.Dispose();
        }
    }
}
