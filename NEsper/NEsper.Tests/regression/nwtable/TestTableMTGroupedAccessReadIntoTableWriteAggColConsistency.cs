///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableMTGroupedAccessReadIntoTableWriteAggColConsistency 
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType(typeof(Local10ColEvent));
            config.AddEventType<SupportBean>();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        /// <summary>
        /// Table:
        /// create table vartotal (key string primary key, tc0 sum(int), tc1 sum(int) ... tc9 sum(int))
        /// Seed the table with a number of groups, no new ones are added or deleted during the test.
        /// For a given number of seconds and a given number of groups:
        /// - Single writer updates a group (round-robin), each group associates with 10 columns .
        /// - Count readers pull a group's columns, round-robin, check that all 10 values are consistent.
        /// - The 10 values are sum-int totals that are expected to all have the same value.
        /// </summary>
        [Test]
        public void TestMT() 
        {
            TryMT(10, 3);
        }
    
        private void TryMT(int numGroups, int numSeconds) 
        {
            var eplCreateVariable = "create table vartotal (key string primary key, " + CollectionUtil.ToString(GetDeclareCols()) + ")";
            _epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            var eplInto = "into table vartotal select " + CollectionUtil.ToString(GetIntoCols()) + " from Local10ColEvent group by groupKey";
            _epService.EPAdministrator.CreateEPL(eplInto);
    
            // initialize groups
            var groups = new string[numGroups];
            for (var i = 0; i < numGroups; i++) {
                groups[i] = "G" + i;
                _epService.EPRuntime.SendEvent(new Local10ColEvent(groups[i], 0));
            }
    
            var writeRunnable = new WriteRunnable(_epService, groups);
            var readRunnable = new ReadRunnable(_epService, groups);
    
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
    
        private ICollection<string> GetDeclareCols()
        {
            IList<string> cols = new List<string>();
            for (var i = 0; i < 10; i++) {  // 10 columns, not configurable
                cols.Add("tc" + i + " sum(int)");
            }
            return cols;
        }
    
        private ICollection<string> GetIntoCols()
        {
            IList<string> cols = new List<string>();
            for (var i = 0; i < 10; i++) {  // 10 columns, not configurable
                cols.Add("sum(c" + i + ") as tc" + i);
            }
            return cols;
        }
    
        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly string[] _groups;

            internal int NumEvents;
    
            public WriteRunnable(EPServiceProvider epService, string[] groups)
            {
                _epService = epService;
                _groups = groups;
            }

            public bool Shutdown { get; set; }

            public void Run()
            {
                Log.Info("Started event send for write");
    
                try {
                    while(!Shutdown) {
                        var groupNum = NumEvents % _groups.Length;
                        _epService.EPRuntime.SendEvent(new Local10ColEvent(_groups[groupNum], NumEvents));
                        NumEvents++;
                    }
                }
                catch (EPRuntimeException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }
    
                Log.Info("Completed event send for write");
            }

            public EPException Exception { get; private set; }
        }
    
        public class ReadRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly string[] _groups;

            internal int NumQueries;
    
            public ReadRunnable(EPServiceProvider epService, string[] groups)
            {
                _epService = epService;
                _groups = groups;
            }

            public bool Shutdown { get; set; }

            public void Run()
            {
                Log.Info("Started event send for read");
    
                try {
                    var eplSelect = "select vartotal[TheString] as out from SupportBean";
                    var listener = new SupportUpdateListener();
                    _epService.EPAdministrator.CreateEPL(eplSelect).Events += listener.Update;
    
                    while(!Shutdown) {
                        var groupNum = NumQueries % _groups.Length;
                        _epService.EPRuntime.SendEvent(new SupportBean(_groups[groupNum], 0));
                        var @event = listener.AssertOneGetNewAndReset();
                        AssertEvent((IDictionary<string, object>)@event.Get("out"));
                        NumQueries++;
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }
    
                Log.Info("Completed event send for read");
            }
    
            private void AssertEvent(IDictionary<string, object> info)
            {
                var tc0 = info.Get("tc0");
                for (var i = 1; i < 10; i++) {
                    Assert.AreEqual(tc0, info.Get("tc" + i));
                }
            }

            public EPException Exception { get; private set; }
        }
    
        public class Local10ColEvent
        {
            public Local10ColEvent(string groupKey, int value)
            {
                GroupKey = groupKey;
                C0 = value;
                C1 = value;
                C2 = value;
                C3 = value;
                C4 = value;
                C5 = value;
                C6 = value;
                C7 = value;
                C8 = value;
                C9 = value;
            }

            public string GroupKey { get; private set; }

            public int C0 { get; private set; }

            public int C1 { get; private set; }

            public int C2 { get; private set; }

            public int C3 { get; private set; }

            public int C4 { get; private set; }

            public int C5 { get; private set; }

            public int C6 { get; private set; }

            public int C7 { get; private set; }

            public int C8 { get; private set; }

            public int C9 { get; private set; }
        }
    }
}
