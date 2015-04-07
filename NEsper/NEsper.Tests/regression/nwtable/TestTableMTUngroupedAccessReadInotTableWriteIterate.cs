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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableMTUngroupedAccessReadInotTableWriteIterate 
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType(typeof(SupportBean));
            config.AddEventType(typeof(SupportBean_S0));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        /// <summary>
        /// Proof that multiple threads iterating the same statement
        /// can safely access a row that is currently changing.
        /// </summary>
        [Test]
        public void TestMT() 
        {
            TryMT(3, 3);
        }
    
        private void TryMT(int numReadThreads, int numSeconds) 
        {
            var eplCreateVariable = "create table vartotal (s0 sum(int), s1 sum(double), s2 sum(long))";
            _epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            var eplInto = "into table vartotal select sum(IntPrimitive) as s0, " +
                    "sum(DoublePrimitive) as s1, sum(LongPrimitive) as s2 from SupportBean";
            _epService.EPAdministrator.CreateEPL(eplInto);
            _epService.EPRuntime.SendEvent(MakeSupportBean("E", 1, 1, 1));
    
            var iterateStatement = _epService.EPAdministrator.CreateEPL("select vartotal.s0 as c0, vartotal.s1 as c1, vartotal.s2 as c2 from SupportBean_S0.std:lastevent()");
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
    
            // setup writer
            var writeRunnable = new WriteRunnable(_epService);
            var writeThread = new Thread(writeRunnable.Run);
    
            // setup readers
            var readThreads = new Thread[numReadThreads];
            var readRunnables = new ReadRunnable[numReadThreads];
            for (var i = 0; i < readThreads.Length; i++) {
                readRunnables[i] = new ReadRunnable(iterateStatement);
                readThreads[i] = new Thread(readRunnables[i].Run);
            }
    
            // start
            foreach (var readThread in readThreads) {
                readThread.Start();
            }
            writeThread.Start();
    
            // wait
            Thread.Sleep(numSeconds * 1000);
    
            // shutdown
            writeRunnable.Shutdown = true;
            foreach (var readRunnable in readRunnables) {
                readRunnable.Shutdown = true;
            }
    
            // join
            Log.Info("Waiting for completion");
            writeThread.Join();
            foreach (var readThread in readThreads) {
                readThread.Join();
            }
    
            // assert
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsTrue(writeRunnable.NumEvents > 100);
            foreach (var readRunnable in readRunnables) {
                Assert.IsNull(readRunnable.Exception);
                Assert.IsTrue(readRunnable.NumQueries > 100);
            }
        }
    
        private static SupportBean MakeSupportBean(string theString, int intPrimitive, double doublePrimitive, long longPrimitive) {
            var b = new SupportBean(theString, intPrimitive);
            b.DoublePrimitive = doublePrimitive;
            b.LongPrimitive = longPrimitive;
            return b;
        }
    
        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;

            internal EPException Exception;
            internal bool Shutdown;
            internal int NumEvents;
    
            public WriteRunnable(EPServiceProvider epService) {
                _epService = epService;
            }

            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    while(!Shutdown) {
                        _epService.EPRuntime.SendEvent(MakeSupportBean("E", 1, 1, 1));
                        NumEvents++;
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }
    
                Log.Info("Completed event send for write");
            }
    
            public EPException GetException() {
                return Exception;
            }
        }
    
        public class ReadRunnable {
    
            private readonly EPStatement _iterateStatement;

            internal EPException Exception;
            internal bool Shutdown;
            internal int NumQueries;
    
            public ReadRunnable(EPStatement iterateStatement) {
                _iterateStatement = iterateStatement;
            }

            public void Run() {
                Log.Info("Started event send for read");
    
                try {
                    while(!Shutdown) {
                        using(var iterator = _iterateStatement.GetSafeEnumerator())
                        {
                            Assert.IsTrue(iterator.MoveNext());
                            var @event = iterator.Current;
                            var c0 = @event.Get("c0").AsInt();
                            Assert.AreEqual((double) c0, @event.Get("c1"));
                            Assert.AreEqual((long) c0, @event.Get("c2"));
                        }
                        NumQueries++;
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }
    
                Log.Info("Completed event send for read");
            }
    
            public EPException GetException() {
                return Exception;
            }
        }
    }
}
