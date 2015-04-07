///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>Test for multithread-safety (or lack thereof) for iterators: iterators fail with concurrent mods as expected behavior </summary>
    [TestFixture]
    public class TestMTStmtIterate 
    {
        private EPServiceProvider _engine;
    
        [TearDown]
        public void TearDown()
        {
            _engine.Dispose();
        }
    
        [Test]
        public void TestIteratorSingleStmt()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _engine = EPServiceProviderManager.GetProvider("TestMTStmtIterate", config);
    
            EPStatement[] stmt = new EPStatement[] {_engine.EPAdministrator.CreateEPL(
                    " select TheString from " + typeof(SupportBean).FullName + ".win:time(5 min)")};
    
            TrySend(2, 10, stmt);
        }
    
        [Test]
        public void TestIteratorMultiStmtNoViewShare()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResourcesConfig.IsShareViews = false;
            _engine = EPServiceProviderManager.GetProvider("TestMTStmtIterate", config);
    
            EPStatement[] stmt = new EPStatement[3];
            for (int i = 0; i < stmt.Length; i++)
            {
                String name = "Stmt_" + i;
                String stmtText = "@Name('" + name + "') select TheString from " + typeof(SupportBean).FullName + ".win:time(5 min)";
                stmt[i] = _engine.EPAdministrator.CreateEPL(stmtText);
            }
    
            TrySend(4, 10, stmt);
        }
    
        [Test]
        public void TestIteratorMultiStmtViewShare()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResourcesConfig.IsShareViews = true;
            _engine = EPServiceProviderManager.GetProvider("TestMTStmtIterate", config);
    
            EPStatement[] stmt = new EPStatement[3];
            for (int i = 0; i < stmt.Length; i++)
            {
                String name = "Stmt_" + i;
                String stmtText = "@Name('" + name + "') select TheString from " + typeof(SupportBean).FullName + ".win:time(5 min)";
                stmt[i] = _engine.EPAdministrator.CreateEPL(stmtText);
            }
    
            TrySend(4, 10, stmt);
        }
    
        private void TrySend(int numThreads, int numRepeats, EPStatement[] stmt)
        {
            BasicExecutorService threadPool = Executors.NewFixedThreadPool(numThreads);
            Future<bool>[] future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                ICallable<bool> callable = new StmtIterateCallable(i, _engine, stmt, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(5));
    
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
        }
    }
}
