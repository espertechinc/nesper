///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>
    /// Test for multithread-safety of named windows and fire-and-forget queries.
    /// </summary>
    [TestFixture]
    public class TestMTStmtNamedWindowFAF 
    {
        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            EPServiceProviderManager.PurgeAllProviders();

            Configuration configuration = SupportConfigFactory.GetConfiguration();

            _engine = EPServiceProviderManager.GetDefaultProvider(configuration);
            _engine.Initialize();
            _engine.EPAdministrator.CreateEPL(
                    "create window MyWindow.win:keepall() as select TheString, LongPrimitive from " + typeof(SupportBean).FullName);
            _engine.EPAdministrator.CreateEPL(
                    "insert into MyWindow(TheString, LongPrimitive) " +
                    " select Symbol, Volume \n" +
                    " from " + typeof(SupportMarketDataBean).FullName);
        }
    
        [Test]
        public void TestThreading()
        {
            TryIterate(2, 500);
        }
    
        private void TryIterate(int numThreads, int numRepeats)
        {
            var callList = CompatExtensions.XRange(0, numThreads)
                .Select(ii => new StmtNamedWindowQueryCallable(Convert.ToString(ii), _engine, numRepeats))
                .ToList();

            var threadPool = new DedicatedExecutorService("test", numThreads);
            var threadFutures = callList
                .Select(threadPool.Submit)
                .ToList();
            
            threadPool.Shutdown();
            threadPool.AwaitTermination(new TimeSpan(0,0,10));
    
            Thread.Sleep(100);

            threadFutures.ForEach(
                future => Assert.IsTrue(future.GetValueOrDefault()));
        }
    }
}
