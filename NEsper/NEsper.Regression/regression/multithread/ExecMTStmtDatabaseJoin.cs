///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>Test for multithread-safety for database joins.</summary>
    public class ExecMTStmtDatabaseJoin : RegressionExecution
    {
        private static readonly string EVENT_NAME = typeof(SupportBean).FullName;
    
        public override void Configure(Configuration configuration)
        {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            configDB.ConnectionTransactionIsolation = System.Data.IsolationLevel.Serializable;
            configDB.ConnectionAutoCommit = true;
            configuration.AddDatabaseReference("MyDB", configDB);
        }
    
        public override void Run(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                "select * \n" +
                "  from " + EVENT_NAME + "#length(1000) as s0,\n" +
                "      sql:MyDB ['select myvarchar from mytesttable where ${IntPrimitive} = mytesttable.mybigint'] as s1"
            );
            TrySendAndReceive(epService, 4, stmt, 1000);
            TrySendAndReceive(epService, 2, stmt, 2000);
        }
    
        private void TrySendAndReceive(EPServiceProvider epService, int numThreads, EPStatement statement, int numRepeats) {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new StmtDatabaseJoinCallable(epService, statement, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault(), "Failed in " + statement.Text);
            }
        }
    }
} // end of namespace
