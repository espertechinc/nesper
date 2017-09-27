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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety (or lack thereof) for iterators: iterators fail with concurrent mods as expected behavior
    /// </summary>
    [TestFixture]
    public class TestMTUpdate 
    {
        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _engine = EPServiceProviderManager.GetProvider("TestMTUpdate", config);
        }
    
        [TearDown]
        public void TearDown()
        {
            _engine.Initialize();
        }
    
        [Test]
        public void TestUpdateCreateDelete()
        {
            EPStatement stmt = _engine.EPAdministrator.CreateEPL("select TheString from " + typeof(SupportBean).FullName);

            var strings = new List<string>().AsSyncList();
            stmt.Events += (sender, eventArgs) => strings.Add((String)eventArgs.NewEvents[0].Get("TheString"));
    
            TrySend(2, 50000);
    
            bool found = false;
            foreach (String value in strings)
            {
                if (value == "a")
                {
                    found = true;
                }
            }
            Assert.IsTrue(found);
    
            _engine.Dispose();
        }
    
        private void TrySend(int numThreads, int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new StmtUpdateSendCallable(i, _engine, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            for (int i = 0; i < 50; i++)
            {
                EPStatement stmtUpd = _engine.EPAdministrator.CreateEPL("Update istream " + typeof(SupportBean).FullName + " set TheString='a'");
                Thread.Sleep(10);
                stmtUpd.Dispose();
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(new TimeSpan(0, 0, 5));
    
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(((bool?) future[i].GetValueOrDefault()).GetValueOrDefault(false));
            }
        }
    }
}
