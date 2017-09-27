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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableMTUngroupedAccessReadIntoTableWriteFilterUse 
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
        /// Single writer updates a total sum, continuously adding 1 and subtracting 1.
        /// Two statements are set up, one listens to "0" and the other to "1"
        /// Single reader sends event and that event must be received by any one of the listeners.
        /// </summary>
        [Test]
        public void TestMT() 
        {
            TryMT(3);
        }
    
        private void TryMT(int numSeconds) 
        {
            var eplCreateVariable = "create table vartotal (total sum(int))";
            _epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            var eplInto = "into table vartotal select sum(IntPrimitive) as total from SupportBean";
            _epService.EPAdministrator.CreateEPL(eplInto);
    
            var listenerZero = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("select * from SupportBean_S0(1 = vartotal.total)").AddListener(listenerZero);
    
            var listenerOne = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("select * from SupportBean_S0(0 = vartotal.total)").AddListener(listenerOne);
    
            var writeRunnable = new WriteRunnable(_epService);
            var readRunnable = new ReadRunnable(_epService, listenerZero, listenerOne);
    
            // start
            var t1 = new Thread(writeRunnable.Run);
            var t2 = new Thread(readRunnable.Run);
            t1.Start();
            t2.Start();
    
            // wait
            Thread.Sleep(numSeconds * 1000);
    
            // shutdown
            writeRunnable.Shutdown = true;
            readRunnable.Shutdown = true;
    
            // join
            Log.Info("Waiting for completion");
            t1.Join();
            t2.Join();
    
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
            Assert.IsTrue(writeRunnable.NumEvents > 100);
            Assert.IsTrue(readRunnable.NumQueries > 100);
            Console.WriteLine("Send " + writeRunnable.NumEvents + " and performed " + readRunnable.NumQueries + " reads");
        }
    
        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;
    
            private EPException _exception;
            private bool _shutdown;
            private int _numEvents;
    
            public WriteRunnable(EPServiceProvider epService)
            {
                _epService = epService;
            }

            public int NumEvents
            {
                get { return _numEvents; }
            }

            public bool Shutdown
            {
                set { _shutdown = value; }
            }

            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    while(!_shutdown) {
                        _epService.EPRuntime.SendEvent(new SupportBean("E", 1));
                        _epService.EPRuntime.SendEvent(new SupportBean("E", -1));
                        _numEvents++;
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
            private readonly SupportUpdateListener _listenerZero;
            private readonly SupportUpdateListener _listenerOne;
    
            private EPException _exception;
            private bool _shutdown;
            private int _numQueries;
    
            public ReadRunnable(EPServiceProvider epService, SupportUpdateListener listenerZero, SupportUpdateListener listenerOne) {
                _epService = epService;
                _listenerZero = listenerZero;
                _listenerOne = listenerOne;
            }

            public int NumQueries
            {
                get { return _numQueries; }
            }

            public bool Shutdown
            {
                set { _shutdown = value; }
            }

            public void Run() {
                Log.Info("Started event send for read");
    
                try {
                    while(!_shutdown) {
                        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
                        _listenerZero.Reset();
                        _listenerOne.Reset();
                        _numQueries++;
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
