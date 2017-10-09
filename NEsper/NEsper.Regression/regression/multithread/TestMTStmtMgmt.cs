///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Text;
using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety for creating and stopping various statements.
    /// </summary>
    [TestFixture]
    public class TestMTStmtMgmt 
    {
        private EPServiceProvider _engine;

        private readonly static String EVENT_NAME = typeof(SupportMarketDataBean).FullName;
        private readonly static Object[][] STMT = new Object[][] {
                // true for EPL, false for Pattern; Statement text
                new Object[] {true, "select * from " + EVENT_NAME + " where Symbol = 'IBM'"},
                new Object[] {true, "select * from " + EVENT_NAME + " (Symbol = 'IBM')"},
                new Object[] {true, "select * from " + EVENT_NAME + " (Price>1)"},
                new Object[] {true, "select * from " + EVENT_NAME + " (Feed='RT')"},
                new Object[] {true, "select * from " + EVENT_NAME + " (Symbol='IBM', Price>1, Feed='RT')"},
                new Object[] {true, "select * from " + EVENT_NAME + " (Price>1, Feed='RT')"},
                new Object[] {true, "select * from " + EVENT_NAME + " (Symbol='IBM', Feed='RT')"},
                new Object[] {true, "select * from " + EVENT_NAME + " (Symbol='IBM', Feed='RT') where Price between 0 and 1000"},
                new Object[] {true, "select * from " + EVENT_NAME + " (Symbol='IBM') where Price between 0 and 1000 and Feed='RT'"},
                new Object[] {true, "select * from " + EVENT_NAME + " (Symbol='IBM') where 'a'='a'"},
                new Object[] {false, "every a=" + EVENT_NAME + "(Symbol='IBM')"},
                new Object[] {false, "every a=" + EVENT_NAME + "(Symbol='IBM', Price < 1000)"},
                new Object[] {false, "every a=" + EVENT_NAME + "(Feed='RT', Price < 1000)"},
                new Object[] {false, "every a=" + EVENT_NAME + "(Symbol='IBM', Feed='RT')"},
        };
    
        [SetUp]
        public void SetUp()
        {
            EPServiceProviderManager.PurgeAllProviders();

            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _engine = EPServiceProviderManager.GetDefaultProvider(configuration);
            _engine.Initialize();
        }
    
        [Test]
        public void TestPatterns()
        {
            int numThreads = 3;
            Object[][] statements;
    
            statements = new Object[][] {STMT[10]};
            TryStatementCreateSendAndStop(numThreads, statements, 10);
    
            statements = new Object[][] {STMT[10], STMT[11]};
            TryStatementCreateSendAndStop(numThreads, statements, 10);
    
            statements = new Object[][] {STMT[10], STMT[11], STMT[12]};
            TryStatementCreateSendAndStop(numThreads, statements, 10);
    
            statements = new Object[][] {STMT[10], STMT[11], STMT[12], STMT[13]};
            TryStatementCreateSendAndStop(numThreads, statements, 10);
        }
    
        [Test]
        public void TestEachStatementAlone()
        {
            int numThreads = 4;
            for (int i = 0; i < STMT.Length; i++)
            {
                Object[][] statements = new Object[][] {STMT[i]};
                TryStatementCreateSendAndStop(numThreads, statements, 10);
            }
        }
    
        [Test]
        public void TestStatementsMixed()
        {
            int numThreads = 2;
            Object[][] statements = new Object[][] {STMT[1], STMT[4], STMT[6], STMT[7], STMT[8]};
            TryStatementCreateSendAndStop(numThreads, statements, 10);
    
            statements = new Object[][] {STMT[1], STMT[7], STMT[8], STMT[11], STMT[12]};
            TryStatementCreateSendAndStop(numThreads, statements, 10);
        }
    
        [Test]
        public void TestStatementsAll()
        {
            int numThreads = 3;
            TryStatementCreateSendAndStop(numThreads, STMT, 10);
        }
    
        private void TryStatementCreateSendAndStop(int numThreads, Object[][] statements, int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new StmtMgmtCallable(_engine, statements, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(new TimeSpan(0,0,10));
    
            StringBuilder statementDigest = new StringBuilder();
            for (int i = 0; i < statements.Length; i++)
            {
                statementDigest.Append(statements[i].Render());
            }
    
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault(), "Failed in " + statementDigest);
            }
        }
    }
}
