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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableMTUngroupedAccessWithinRowFAFConsistency 
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
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
        /// Single writer updates the group (round-robin) count, sum and avg.
        /// A FAF reader thread pulls the value and checks they are consistent.
        /// </summary>
        [Test]
        public void TestMT() 
        {
            TryMT(2);
        }
    
        private void TryMT(int numSeconds) 
        {
            var eplCreateVariable = "create table vartotal (cnt count(*), sumint sum(int), avgint avg(int))";
            _epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            var eplInto = "into table vartotal select count(*) as cnt, sum(IntPrimitive) as sumint, avg(IntPrimitive) as avgint from SupportBean";
            _epService.EPAdministrator.CreateEPL(eplInto);
    
            _epService.EPAdministrator.CreateEPL("create window MyWindow.std:lastevent() as SupportBean_S0");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean_S0");
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
    
            var writeRunnable = new WriteRunnable(_epService);
            var readRunnable = new ReadRunnable(_epService);
    
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
    
        public class WriteRunnable {
    
            private readonly EPServiceProvider epService;

            internal EPException Exception;
            internal bool Shutdown;
            internal int NumEvents;
    
            public WriteRunnable(EPServiceProvider epService) {
                this.epService = epService;
            }
    
            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    while(!Shutdown) {
                        epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
                        NumEvents++;
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }
    
                Log.Info("Completed event send for write");
            }
        }
    
        public class ReadRunnable
        {
            private readonly EPServiceProvider _epService;

            internal EPException Exception;
            internal bool Shutdown;
            internal int NumQueries;
    
            public ReadRunnable(EPServiceProvider epService) {
                this._epService = epService;
            }
    
            public void Run() {
                Log.Info("Started event send for read");
    
                // warmup
                Thread.Sleep(100);
    
                try
                {
                    const string eplSelect = "select vartotal.cnt as c0, vartotal.sumint as c1, vartotal.avgint as c2 from MyWindow";

                    while(!Shutdown) {
                        var result = _epService.EPRuntime.ExecuteQuery(eplSelect);
                        var count = result.Array[0].Get("c0").AsLong();
                        var sumint = result.Array[0].Get("c1").AsInt();
                        var avgint = result.Array[0].Get("c2").AsDouble();
                        Assert.AreEqual(2d, avgint);
                        Assert.AreEqual(sumint, count*2);
                        NumQueries++;
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }
    
                Log.Info("Completed event send for read");
            }
        }
    }
}
