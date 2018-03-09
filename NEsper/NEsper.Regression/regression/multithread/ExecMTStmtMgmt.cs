///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;


using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety for creating and stopping various statements.
    /// </summary>
    public class ExecMTStmtMgmt : RegressionExecution {
        private static readonly string EVENT_NAME = typeof(SupportMarketDataBean).FullName;
        private static readonly object[][] STMT = new object[][]{
                // true for EPL, false for Pattern; Statement text
                new object[] {true, "select * from " + EVENT_NAME + " where symbol = 'IBM'"},
                new object[] {true, "select * from " + EVENT_NAME + " (symbol = 'IBM')"},
                new object[] {true, "select * from " + EVENT_NAME + " (price>1)"},
                new object[] {true, "select * from " + EVENT_NAME + " (feed='RT')"},
                new object[] {true, "select * from " + EVENT_NAME + " (symbol='IBM', price>1, feed='RT')"},
                new object[] {true, "select * from " + EVENT_NAME + " (price>1, feed='RT')"},
                new object[] {true, "select * from " + EVENT_NAME + " (symbol='IBM', feed='RT')"},
                new object[] {true, "select * from " + EVENT_NAME + " (symbol='IBM', feed='RT') where price between 0 and 1000"},
                new object[] {true, "select * from " + EVENT_NAME + " (symbol='IBM') where price between 0 and 1000 and feed='RT'"},
                new object[] {true, "select * from " + EVENT_NAME + " (symbol='IBM') where 'a'='a'"},
                new object[] {false, "every a=" + EVENT_NAME + "(symbol='IBM')"},
                new object[] {false, "every a=" + EVENT_NAME + "(symbol='IBM', price < 1000)"},
                new object[] {false, "every a=" + EVENT_NAME + "(feed='RT', price < 1000)"},
                new object[] {false, "every a=" + EVENT_NAME + "(symbol='IBM', feed='RT')"},
        };
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionPatterns(epService);
            RunAssertionEachStatementAlone(epService);
            RunAssertionStatementsMixed(epService);
            RunAssertionStatementsAll(epService);
        }
    
        private void RunAssertionPatterns(EPServiceProvider epService) {
            int numThreads = 3;
            object[][] statements;
    
            statements = new object[][]{STMT[10]};
            TryStatementCreateSendAndStop(epService, numThreads, statements, 10);
            epService.EPAdministrator.DestroyAllStatements();
    
            statements = new object[][]{STMT[10], STMT[11]};
            TryStatementCreateSendAndStop(epService, numThreads, statements, 10);
            epService.EPAdministrator.DestroyAllStatements();
    
            statements = new object[][]{STMT[10], STMT[11], STMT[12]};
            TryStatementCreateSendAndStop(epService, numThreads, statements, 10);
            epService.EPAdministrator.DestroyAllStatements();
    
            statements = new object[][]{STMT[10], STMT[11], STMT[12], STMT[13]};
            TryStatementCreateSendAndStop(epService, numThreads, statements, 10);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionEachStatementAlone(EPServiceProvider epService) {
            int numThreads = 4;
            for (int i = 0; i < STMT.Length; i++) {
                var statements = new object[][]{STMT[i]};
                TryStatementCreateSendAndStop(epService, numThreads, statements, 10);
            }
        }
    
        private void RunAssertionStatementsMixed(EPServiceProvider epService) {
            int numThreads = 2;
            var statements = new object[][]{STMT[1], STMT[4], STMT[6], STMT[7], STMT[8]};
            TryStatementCreateSendAndStop(epService, numThreads, statements, 10);
    
            statements = new object[][]{STMT[1], STMT[7], STMT[8], STMT[11], STMT[12]};
            TryStatementCreateSendAndStop(epService, numThreads, statements, 10);
        }
    
        private void RunAssertionStatementsAll(EPServiceProvider epService) {
            int numThreads = 3;
            TryStatementCreateSendAndStop(epService, numThreads, STMT, 10);
        }
    
        private void TryStatementCreateSendAndStop(EPServiceProvider epService, int numThreads, object[][] statements, int numRepeats) {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new StmtMgmtCallable(epService, statements, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            var statementDigest = new StringBuilder();
            for (int i = 0; i < statements.Length; i++) {
                statementDigest.Append(CompatExtensions.Render(statements[i]));
            }
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault(), "Failed in " + statementDigest);
            }
        }
    }
} // end of namespace
