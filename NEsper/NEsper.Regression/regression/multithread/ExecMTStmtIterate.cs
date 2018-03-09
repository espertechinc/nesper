///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety (or lack thereof) for iterators: iterators fail with concurrent mods as expected behavior
    /// </summary>
    public class ExecMTStmtIterate : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionIteratorSingleStmt(epService);
            RunAssertionIteratorMultiStmtNoViewShare();
            RunAssertionIteratorMultiStmtViewShare();
        }
    
        private void RunAssertionIteratorSingleStmt(EPServiceProvider epService) {
            var stmt = new EPStatement[]{epService.EPAdministrator.CreateEPL(
                    " select TheString from " + typeof(SupportBean).FullName + "#time(5 min)")};
    
            TrySend(epService, 2, 10, stmt);
        }
    
        private void RunAssertionIteratorMultiStmtNoViewShare() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResources.IsShareViews = false;
            EPServiceProvider engine = EPServiceProviderManager.GetProvider(
                SupportContainer.Instance, typeof(ExecMTStmtIterate).Name, config);
    
            var stmt = new EPStatement[3];
            for (int i = 0; i < stmt.Length; i++) {
                string name = "Stmt_" + i;
                string stmtText = "@Name('" + name + "') select TheString from " + typeof(SupportBean).FullName + "#time(5 min)";
                stmt[i] = engine.EPAdministrator.CreateEPL(stmtText);
            }
    
            TrySend(engine, 4, 10, stmt);
    
            engine.Dispose();
        }
    
        private void RunAssertionIteratorMultiStmtViewShare() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResources.IsShareViews = true;
            EPServiceProvider engine = EPServiceProviderManager.GetProvider(
                SupportContainer.Instance, typeof(ExecMTStmtIterate).Name, config);
    
            var stmt = new EPStatement[3];
            for (int i = 0; i < stmt.Length; i++) {
                string name = "Stmt_" + i;
                string stmtText = "@Name('" + name + "') select TheString from " + typeof(SupportBean).FullName + "#time(5 min)";
                stmt[i] = engine.EPAdministrator.CreateEPL(stmtText);
            }
    
            TrySend(engine, 4, 10, stmt);
    
            engine.Dispose();
        }
    
        private void TrySend(EPServiceProvider epService, int numThreads, int numRepeats, EPStatement[] stmt) {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new StmtIterateCallable(i, epService, stmt, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(5, TimeUnit.SECONDS);
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
        }
    }
} // end of namespace
