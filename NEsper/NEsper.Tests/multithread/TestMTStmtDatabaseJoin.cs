///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>Test for multithread-safety for database joins.  </summary>
    [TestFixture]
    public class TestMTStmtDatabaseJoin 
    {
        private EPServiceProvider _engine;
    
        private readonly static String EVENT_NAME = typeof(SupportBean).FullName;
    
        [SetUp]
        public void SetUp()
        {
            var configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            //configDB.ConnectionReadOnly = true;
            configDB.ConnectionTransactionIsolation = System.Data.IsolationLevel.Serializable;
            configDB.ConnectionAutoCommit = true;
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddDatabaseReference("MyDB", configDB);

            _engine = EPServiceProviderManager.GetProvider("TestMTStmtDatabaseJoin", configuration);
        }
    
        [TearDown]
        public void TearDown()
        {
            _engine.Dispose();
        }
    
        [Test]
        public void TestJoin()
        {
            EPStatement stmt = _engine.EPAdministrator.CreateEPL("select * \n" +
                    "  from " + EVENT_NAME + ".win:length(1000) as s0,\n" +
                    "      sql:MyDB ['select myvarchar from mytesttable where ${IntPrimitive} = mytesttable.mybigint'] as s1"
                    );
            TrySendAndReceive(4, stmt, 1000);
            TrySendAndReceive(2, stmt, 2000);
        }
    
        private void TrySendAndReceive(int numThreads, EPStatement statement, int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new StmtDatabaseJoinCallable(_engine, statement, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault(), "Failed in " + statement.Text);
            }
        }
    }
}
