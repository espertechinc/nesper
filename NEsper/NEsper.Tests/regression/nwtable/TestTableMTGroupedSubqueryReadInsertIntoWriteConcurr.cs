///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class TestTableMTGroupedSubqueryReadInsertIntoWriteConcurr 
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType(typeof(SupportBean));
            config.AddEventType(typeof(SupportBean_S0));
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
        }
    
        /// <summary>
        /// Primary key is single: {id}
        /// For a given number of seconds:
        /// Single writer insert-into such as {0} to {Count}.
        /// Single reader subquery-selects the count all rows.
        /// </summary>
        [Test]
        public void TestMT() 
        {
            TryMT(3);
        }
    
        private void TryMT(int numSeconds) 
        {
            var eplCreateVariable = "create table MyTable (pkey string primary key)";
            epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            var eplInsertInto = "insert into MyTable select TheString as pkey from SupportBean";
            epService.EPAdministrator.CreateEPL(eplInsertInto);
    
            // seed with count 1
            epService.EPRuntime.SendEvent(new SupportBean("E0", 0));
    
            // select/read
            var eplSubselect = "select (select count(*) from MyTable) as c0 from SupportBean_S0";
            var stmtSubselect = epService.EPAdministrator.CreateEPL(eplSubselect);
            var listener = new SupportUpdateListener();
            stmtSubselect.AddListener(listener);
    
            var writeRunnable = new WriteRunnable(epService);
            var readRunnable = new ReadRunnable(epService, listener);
    
            // start
            var writeThread = new Thread(writeRunnable.Run);
            var readThread = new Thread(readRunnable.Run);
            writeThread.Start();
            readThread.Start();
    
            // wait
            Thread.Sleep(numSeconds * 1000);
    
            // shutdown
            writeRunnable.Shutdown = true;
            readRunnable.Shutdown = true;
    
            // join
            log.Info("Waiting for completion");
            writeThread.Join();
            readThread.Join();
    
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
            Assert.IsTrue(writeRunnable.NumLoops > 100);
            Assert.IsTrue(readRunnable.NumQueries > 100);
            Console.WriteLine("Send " + writeRunnable.NumLoops + " and performed " + readRunnable.NumQueries + " reads");
        }
    
        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;
    
            private EPException _exception;
            private bool _shutdown;
            private int _numLoops;
    
            public WriteRunnable(EPServiceProvider epService) {
                this._epService = epService;
            }

            public int NumLoops
            {
                get { return _numLoops; }
            }

            public bool Shutdown
            {
                set { this._shutdown = value; }
            }

            public void Run()
            {
                log.Info("Started event send for write");
    
                try {
                    while(!_shutdown) {
                        _epService.EPRuntime.SendEvent(new SupportBean("E" + _numLoops + 1, 0));
                        _numLoops++;
                    }
                }
                catch (EPException ex) {
                    log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                log.Info("Completed event send for write");
            }

            public EPException Exception
            {
                get { return _exception; }
            }
        }

        public class ReadRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly SupportUpdateListener _listener;

            private int _numQueries;
            private EPException _exception;
            private bool _shutdown;

            public ReadRunnable(EPServiceProvider epService, SupportUpdateListener listener)
            {
                _epService = epService;
                _listener = listener;
            }

            public bool Shutdown
            {
                set { _shutdown = value; }
            }

            public void Run()
            {
                log.Info("Started event send for read");

                try
                {
                    while (!_shutdown)
                    {
                        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
                        var value = _listener.AssertOneGetNewAndReset().Get("c0");
                        Assert.IsTrue((long?) value >= 1);
                        _numQueries++;
                    }
                }
                catch (EPException ex)
                {
                    log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }

                log.Info("Completed event send for read");
            }

            public EPException Exception
            {
                get { return _exception; }
            }

            public int NumQueries
            {
                get { return _numQueries; }
            }
        }
    }
}
