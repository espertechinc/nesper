///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety of insert-into and aggregation per group.
    /// </summary>
    [TestFixture]
    public class TestMTStmtNamedWindowIterate 
    {
        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _engine = EPServiceProviderManager.GetDefaultProvider(configuration);
            _engine.Initialize();
    
            _engine.EPAdministrator.CreateEPL(
                    "create window MyWindow#groupwin(TheString)#keepall as select TheString, LongPrimitive from " + typeof(SupportBean).FullName);
    
            _engine.EPAdministrator.CreateEPL(
                    "insert into MyWindow(TheString, LongPrimitive) " +
                    " select Symbol, Volume \n" +
                    " from " + typeof(SupportMarketDataBean).FullName);
        }
    
        [Test]
        public void Test4Threads()
        {
            TryIterate(4, 250);
        }
    
        [Test]
        public void Test2Threads()
        {
            TryIterate(2, 500);
        }
    
        private void TryIterate(int numThreads, int numRepeats)
        {
            var callList = CompatExtensions.XRange(0, numThreads)
                .Select(ii => new StmtNamedWindowIterateCallable(Convert.ToString(ii), _engine, numRepeats))
                .ToList();

            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var threadFutures = callList
                .Select(threadPool.Submit)
                .ToList();
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(new TimeSpan(0,0,10));

            threadFutures.ForEach(
                future => Assert.IsTrue(future.GetValueOrDefault()));
        }
    }
}
