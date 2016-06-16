///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
    public class TestTableMTGroupedAccessReadIntoTableWriteNewRowCreation 
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
        /// Table:
        /// create table varTotal (key string primary key, total sum(int));
        /// For a given number of events
        /// - Single writer expands the group-key space by sending additional keys.
        /// - Single reader against a last-inserted group gets the non-zero-value.
        /// </summary>
        [Test]
        public void TestMT() 
        {
            TryMT(10000);
        }
    
        private void TryMT(int numEvents) 
        {
            var epl =
                    "create table varTotal (key string primary key, total sum(int));\n" +
                    "into table varTotal select TheString, sum(IntPrimitive) as total from SupportBean group by TheString;\n" +
                    "@Name('listen') select varTotal[p00].total as c0 from SupportBean_S0;\n";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            _epService.EPRuntime.SendEvent(new SupportBean("A", 10));

            IBlockingQueue<string> queueCreated = new LinkedBlockingQueue<string>();
            var writeRunnable = new WriteRunnable(_epService, numEvents, queueCreated);
            var readRunnable = new ReadRunnable(_epService, numEvents, queueCreated);
    
            // start
            var t1 = new Thread(writeRunnable.Run);
            var t2 = new Thread(readRunnable.Run);
            t1.Start();
            t2.Start();
    
            // join
            Log.Info("Waiting for completion");
            t1.Join();
            t2.Join();
    
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
        }
    
        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly int _numEvents;
            private readonly IBlockingQueue<string> _queueCreated;
            private EPException _exception;

            public WriteRunnable(EPServiceProvider epService, int numEvents, IBlockingQueue<string> queueCreated)
            {
                this._epService = epService;
                _numEvents = numEvents;
                _queueCreated = queueCreated;
            }
    
            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    for (var i = 0; i < _numEvents; i++) {
                        var key = "E" + i;
                        _epService.EPRuntime.SendEvent(new SupportBean(key, 10));
                        _queueCreated.Push(key);
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                Log.Info("Completed event send for write");
            }

            public EPException Exception
            {
                get { return _exception; }
            }
        }
    
        public class ReadRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly int _numEvents;
            private readonly IBlockingQueue<string> _queueCreated;
            private EPException _exception;
    
            public ReadRunnable(EPServiceProvider epService, int numEvents, IBlockingQueue<string> queueCreated)
            {
                _epService = epService;
                _numEvents = numEvents;
                _queueCreated = queueCreated;
            }
    
            public void Run()
            {
                Log.Info("Started event send for read");
                var listener = new SupportUpdateListener();
                _epService.EPAdministrator.GetStatement("listen").AddListener(listener);
    
                try {
                    var currentEventId = "A";
                    for (var i = 0; i < _numEvents; i++) {
                        if (_queueCreated.Count != 0) {
                            currentEventId = _queueCreated.Pop();
                        }
                        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, currentEventId));
                        var value = listener.AssertOneGetNewAndReset().Get("c0").AsInt();
                        Assert.AreEqual(10, value);
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                Log.Info("Completed event send for read");
            }

            public EPException Exception
            {
                get { return _exception; }
            }
        }
    }
}
