///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableMTUngroupedIntoTableWriteMultiWriterAgg 
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddEventType(typeof(SupportBean_S0));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        /// <summary>
        /// For a given number of seconds:
        /// Configurable number of into-writers update a shared aggregation.
        /// At the end of the test we read and assert.
        /// </summary>
        [Test]
        public void TestMT() 
        {
            TryMT(3, 10000);
        }
    
        private void TryMT(int numThreads, int numEvents) 
        {
            var eplCreateVariable = "create table varagg (theEvents window(*) @type(SupportBean))";
            _epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            var threads = new Thread[numThreads];
            var runnables = new WriteRunnable[numThreads];
            for (var i = 0; i < threads.Length; i++) {
                runnables[i] = new WriteRunnable(_epService, numEvents, i);
                threads[i] = new Thread(runnables[i].Run);
                threads[i].Start();
            }
    
            // join
            Log.Info("Waiting for completion");
            for (var i = 0; i < threads.Length; i++) {
                threads[i].Join();
                Assert.IsNull(runnables[i].Exception);
            }
    
            // verify
            var listener = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("select varagg.theEvents as c0 from SupportBean_S0").AddListener(listener);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            var @event = listener.AssertOneGetNewAndReset();
            var window = (SupportBean[]) @event.Get("c0");
            Assert.AreEqual(numThreads*3, window.Length);
        }
    
        public class WriteRunnable {
            internal readonly EPServiceProvider EPService;
            internal readonly int NumEvents;
            internal readonly int ThreadNum;

            internal EPException Exception;
    
            public WriteRunnable(EPServiceProvider epService, int numEvents, int threadNum) {
                EPService = epService;
                NumEvents = numEvents;
                ThreadNum = threadNum;
            }
    
            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    var eplInto = "into table varagg select window(*) as theEvents from SupportBean(TheString='E" + ThreadNum + "').win:length(3)";
                    EPService.EPAdministrator.CreateEPL(eplInto);
    
                    for (var i = 0; i < NumEvents; i++) {
                        EPService.EPRuntime.SendEvent(new SupportBean("E" + ThreadNum, i));
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }
    
                Log.Info("Completed event send for write");
            }
        }
    }
}
