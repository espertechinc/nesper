///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableMTGroupedWContextIntoTableWriteAsSharedTable 
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddEventType<SupportBean_S0>();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        /// <summary>
        /// Multiple writers share a key space that they aggregate into.
        /// Writer utilize a hash partition context.
        /// After all writers are done validate the space.
        /// </summary>
        [Test]
        public void TestMT() 
        {
            // with T, Count, G:  Each of T threads loops Count times and sends for each loop G events for each group.
            // for a total of T*Count*G events being processed, and G aggregations retained in a shared variable.
            // Group is the innermost loop.
            TryMT(8, 1000, 64);
        }
    
        private void TryMT(int numThreads, int numLoops, int numGroups) 
        {
            var eplDeclare =
                    "create table varTotal (key string primary key, total sum(int));\n" +
                    "create context ByStringHash\n" +
                    "  coalesce by consistent_hash_crc32(TheString) from SupportBean granularity 16 preallocate\n;" +
                    "context ByStringHash into table varTotal select TheString, sum(IntPrimitive) as total from SupportBean group by TheString;\n";
            var eplAssert = "select varTotal[p00].total as c0 from SupportBean_S0";
    
            RunAndAssert(_epService, eplDeclare, eplAssert, numThreads, numLoops, numGroups);
        }
    
        internal static void RunAndAssert(EPServiceProvider epService, string eplDeclare, string eplAssert, int numThreads, int numLoops, int numGroups)
        {
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplDeclare);
    
            // setup readers
            var writeThreads = new Thread[numThreads];
            var writeRunnables = new WriteRunnable[numThreads];
            for (var i = 0; i < writeThreads.Length; i++) {
                writeRunnables[i] = new WriteRunnable(epService, numLoops, numGroups);
                writeThreads[i] = new Thread(writeRunnables[i].Run);
            }
    
            // start
            foreach (var writeThread in writeThreads) {
                writeThread.Start();
            }
    
            // join
            Log.Info("Waiting for completion");
            foreach (var writeThread in writeThreads) {
                writeThread.Join();
            }
    
            // assert
            foreach (var writeRunnable in writeRunnables) {
                Assert.IsNull(writeRunnable.Exception);
            }
    
            // each group should total up to "numLoops*numThreads"
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(eplAssert).AddListener(listener);
            int? expected = numLoops * numThreads;
            for (var i = 0; i < numGroups; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G" + i));
                Assert.AreEqual(expected, listener.AssertOneGetNewAndReset().Get("c0"));
            }
        }
    
        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly int _numGroups;
            private readonly int _numLoops;
    
            private EPException _exception;
    
            public WriteRunnable(EPServiceProvider epService, int numLoops, int numGroups) {
                _epService = epService;
                _numGroups = numGroups;
                _numLoops = numLoops;
            }
    
            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    for (var i = 0; i < _numLoops; i++) {
                        for (var j = 0; j < _numGroups; j++) {
                            _epService.EPRuntime.SendEvent(new SupportBean("G" + j, 1));
                        }
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                Log.Info("Completed event send for write");
            }

            public int NumLoops
            {
                get { return _numLoops; }
            }

            public int NumGroups
            {
                get { return _numGroups; }
            }

            public EPException Exception
            {
                get { return _exception; }
            }
        }
    }
}
