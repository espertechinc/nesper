///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableMTUngroupedSubqueryReadMergeWriteColumnUpd 
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType(typeof(SupportBean));
            config.AddEventType(typeof(SupportBean_S0));
            config.AddEventType(typeof(SupportBean_S1));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        /// <summary>
        /// Tests column-consistency for subquery:
        /// create table MyTable(p0 string, p1 string, ..., p4 string)   (5 props)
        /// Set row single: MyTable={p0="1", p1="1", p2="1", p3="1", p4="1"}
        /// A writer-thread uses an on-merge statement to update the p0 to p4 columns from "1" to "2", then "2" to "1"
        /// A reader-thread uses a subquery checking ("p1="1" and p2="1" and p3="1" and p4="1")
        /// </summary>
        [Test]
        public void TestMT() 
        {
            TryMT(2);
        }
    
        private void TryMT(int numSeconds) 
        {
            var epl =
                    "create table MyTable (p0 string, p1 string, p2 string, p3 string, p4 string);\n" +
                    "on SupportBean merge MyTable " +
                    "  when not matched then insert select '1' as p0, '1' as p1, '1' as p2, '1' as p3, '1' as p4;\n" +
                    "on SupportBean_S0 merge MyTable " +
                    "  when matched then update set p0=p00, p1=p00, p2=p00, p3=p00, p4=p00;\n" +
                    "@Name('out') select " +
                            "(select p0 from MyTable where " +
                            "  (p0='1' and p1='1' and p2='1' and p3='1' and p4='1') " +
                            "  or (p0='2' and p1='2' and p2='2' and p3='2' and p4='2')) as c0 " +
                            "from SupportBean_S1;\n";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            // preload
            _epService.EPRuntime.SendEvent(new SupportBean());
    
            var writeRunnable = new TestTableMTUngroupedJoinColumnConsistency.Update_1_2_WriteRunnable(_epService);
            var readRunnable = new ReadRunnable(_epService);
    
            // start
            var threadWrite = new Thread(writeRunnable.Run);
            var threadRead = new Thread(readRunnable.Run);
            threadWrite.Start();
            threadRead.Start();
    
            // wait
            Thread.Sleep(numSeconds * 1000);
    
            // shutdown
            writeRunnable.Shutdown = true;
            readRunnable.Shutdown = true;
    
            // join
            Log.Info("Waiting for completion");
            threadWrite.Join();
            threadRead.Join();
    
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
            Console.WriteLine("Write loops " + writeRunnable.NumLoops + " and performed " + readRunnable.NumQueries + " reads");
            Assert.IsTrue(writeRunnable.NumLoops > 1);
            Assert.IsTrue(readRunnable.NumQueries > 100);
        }
    
        public class ReadRunnable {
    
            private readonly EPServiceProvider _epService;
            private readonly SupportUpdateListener _listener;
    
            private EPException _exception;
            private bool _shutdown;
            private int _numQueries;
    
            public ReadRunnable(EPServiceProvider epService) {
                this._epService = epService;
                _listener = new SupportUpdateListener();
                epService.EPAdministrator.GetStatement("out").AddListener(_listener);
            }

            public bool Shutdown
            {
                set { this._shutdown = value; }
            }

            public void Run() {
                Log.Info("Started event send for read");
    
                try {
                    while(!_shutdown) {
                        _epService.EPRuntime.SendEvent(new SupportBean_S1(0, null));
                        var value = _listener.AssertOneGetNewAndReset().Get("c0");
                        if (value == null) {
                            throw new IllegalStateException("Value is null");
                        }
                        _numQueries++;
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                Log.Info("Completed event send for read");
            }

            public int NumQueries
            {
                get { return _numQueries; }
            }

            public EPException Exception
            {
                get { return _exception; }
            }
        }
    }
}
