///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
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
    /// Test for multithread-safety and deterministic behavior when using insert-into.
    /// </summary>
    public class ExecMTDeterminismInsertInto : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void Run(EPServiceProvider epService) {
            TrySendCountFollowedBy(4, 100, ConfigurationEngineDefaults.ThreadingConfig.Locking.SUSPEND);
            TrySendCountFollowedBy(4, 100, ConfigurationEngineDefaults.ThreadingConfig.Locking.SPIN);
            TryChainedCountSum(epService, 3, 100);
            TryMultiInsertGroup(epService, 3, 10, 100);
        }
    
        private void TryMultiInsertGroup(EPServiceProvider engine, int numThreads, int numStatements, int numEvents) {
            // This should fail all test in this class
            // config.EngineDefaults.Threading.InsertIntoDispatchPreserveOrder = false;
    
            // setup statements
            var insertIntoStmts = new EPStatement[numStatements];
            for (int i = 0; i < numStatements; i++) {
                insertIntoStmts[i] = engine.EPAdministrator.CreateEPL("insert into MyStream select " + i + " as ident,count(*) as cnt from " + typeof(SupportBean).FullName);
            }
            EPStatement stmtInsertTwo = engine.EPAdministrator.CreateEPL("select ident, sum(cnt) as mysum from MyStream group by ident");
            var listener = new SupportUpdateListener();
            stmtInsertTwo.Events += listener.Update;
    
            // execute
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            var sharedStartLock = SupportContainer.Instance.RWLockManager().CreateDefaultLock();
            using (sharedStartLock.WriteLock.Acquire())
            {
                for (int i = 0; i < numThreads; i++)
                {
                    future[i] = threadPool.Submit(
                        new SendEventRWLockCallable(i, sharedStartLock.WriteLock, engine, new GeneratorIterator(numEvents)));
                }

                Thread.Sleep(100);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
    
            // assert result
            EventBean[] newEvents = listener.GetNewDataListFlattened();
            var resultsPerIdent = new List<long>[numStatements];
            foreach (EventBean theEvent in newEvents)
            {
                int ident = theEvent.Get("ident").AsInt();
                if (resultsPerIdent[ident] == null) {
                    resultsPerIdent[ident] = new List<long>();
                }
                long mysum = (long) theEvent.Get("mysum");
                resultsPerIdent[ident].Add(mysum);
            }
    
            for (int statement = 0; statement < numStatements; statement++) {
                for (int i = 0; i < numEvents - 1; i++) {
                    long expected = Total(i + 1);
                    Assert.AreEqual(expected, resultsPerIdent[statement][i]);
                }
            }
    
            // destroy
            for (int i = 0; i < numStatements; i++) {
                insertIntoStmts[i].Dispose();
            }
            stmtInsertTwo.Dispose();
        }
    
        private void TryChainedCountSum(EPServiceProvider epService, int numThreads, int numEvents) {
            // setup statements
            EPStatement stmtInsertOne = epService.EPAdministrator.CreateEPL("insert into MyStreamOne select count(*) as cnt from " + typeof(SupportBean).FullName);
            EPStatement stmtInsertTwo = epService.EPAdministrator.CreateEPL("insert into MyStreamTwo select sum(cnt) as mysum from MyStreamOne");
            var listener = new SupportUpdateListener();
            stmtInsertTwo.Events += listener.Update;
    
            // execute
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            var sharedStartLock = SupportContainer.Instance.RWLockManager().CreateDefaultLock();
            using (sharedStartLock.WriteLock.Acquire())
            {
                for (int i = 0; i < numThreads; i++)
                {
                    future[i] = threadPool.Submit(
                        new SendEventRWLockCallable(i, sharedStartLock.WriteLock, epService, new GeneratorIterator(numEvents)));
                }

                Thread.Sleep(100);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
    
            // assert result
            EventBean[] newEvents = listener.GetNewDataListFlattened();
            for (int i = 0; i < numEvents - 1; i++) {
                long expected = Total(i + 1);
                Assert.AreEqual(expected, newEvents[i].Get("mysum"));
            }
    
            stmtInsertOne.Dispose();
            stmtInsertTwo.Dispose();
        }
    
        private long Total(int num) {
            long total = 0;
            for (int i = 1; i < num + 1; i++) {
                total += i;
            }
            return total;
        }
    
        private void TrySendCountFollowedBy(int numThreads, int numEvents, ConfigurationEngineDefaults.ThreadingConfig.Locking locking) {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Threading.InsertIntoDispatchLocking = locking;
            config.EngineDefaults.Threading.InsertIntoDispatchTimeout = 5000; // 5 second timeout
            // This should fail all test in this class
            // config.EngineDefaults.Threading.InsertIntoDispatchPreserveOrder = false;
    
            EPServiceProvider engine = EPServiceProviderManager.GetProvider(
                SupportContainer.Instance, this.GetType().Name, config);
            engine.Initialize();
    
            // setup statements
            EPStatement stmtInsert = engine.EPAdministrator.CreateEPL("insert into MyStream select count(*) as cnt from " + typeof(SupportBean).FullName);
            stmtInsert.Events += (sender, args) => Log.Debug(".update cnt=" + args.NewEvents[0].Get("cnt"));
    
            var listeners = new SupportUpdateListener[numEvents];
            for (int i = 0; i < numEvents; i++) {
                string text = "select * from pattern [MyStream(cnt=" + (i + 1) + ") -> MyStream(cnt=" + (i + 2) + ")]";
                EPStatement stmt = engine.EPAdministrator.CreateEPL(text);
                listeners[i] = new SupportUpdateListener();
                stmt.Events += listeners[i].Update;
            }
    
            // execute
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            var sharedStartLock = SupportContainer.Instance.RWLockManager().CreateDefaultLock();
            using (sharedStartLock.WriteLock.Acquire())
            {
                for (int i = 0; i < numThreads; i++)
                {
                    future[i] = threadPool.Submit(
                        new SendEventRWLockCallable(i, sharedStartLock.WriteLock, engine, new GeneratorIterator(numEvents)));
                }

                Thread.Sleep(100);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
    
            // assert result
            for (int i = 0; i < numEvents - 1; i++) {
                Assert.AreEqual(1, listeners[i].NewDataList.Count, "Listener not invoked: #" + i);
            }
    
            engine.Dispose();
        }
    }
} // end of namespace
