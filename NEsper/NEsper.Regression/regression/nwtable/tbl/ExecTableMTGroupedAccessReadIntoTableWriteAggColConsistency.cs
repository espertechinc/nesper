///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableMTGroupedAccessReadIntoTableWriteAggColConsistency : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Table:
        ///     create table vartotal (key string primary key, tc0 sum(int), tc1 sum(int) ... tc9 sum(int))
        ///     <para>
        ///         Seed the table with a number of groups, no new ones are added or deleted during the test.
        ///         For a given number of seconds and a given number of groups:
        ///         - Single writer updates a group (round-robin), each group associates with 10 columns .
        ///         - N readers pull a group's columns, round-robin, check that all 10 values are consistent.
        ///         - The 10 values are sum-int totals that are expected to all have the same value.
        ///     </para>
        /// </summary>
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType(typeof(Local10ColEvent));
            configuration.AddEventType<SupportBean>();
        }

        public override void Run(EPServiceProvider epService)
        {
            TryMT(epService, 10, 3);
        }

        private void TryMT(EPServiceProvider epService, int numGroups, int numSeconds)
        {
            var eplCreateVariable = "create table vartotal (key string primary key, " +
                                    CollectionUtil.ToString(GetDeclareCols()) + ")";
            epService.EPAdministrator.CreateEPL(eplCreateVariable);

            var eplInto = "into table vartotal select " + CollectionUtil.ToString(GetIntoCols()) +
                          " from Local10ColEvent group by groupKey";
            epService.EPAdministrator.CreateEPL(eplInto);

            // initialize groups
            var groups = new string[numGroups];
            for (var i = 0; i < numGroups; i++)
            {
                groups[i] = "G" + i;
                epService.EPRuntime.SendEvent(new Local10ColEvent(groups[i], 0));
            }

            var writeRunnable = new WriteRunnable(epService, groups);
            var readRunnable = new ReadRunnable(epService, groups);

            // start
            var t1 = new Thread(writeRunnable.Run);
            var t2 = new Thread(readRunnable.Run);
            t1.Start();
            t2.Start();

            // wait
            Thread.Sleep(numSeconds * 1000);

            // shutdown
            writeRunnable.SetShutdown(true);
            readRunnable.SetShutdown(true);

            // join
            Log.Info("Waiting for completion");
            t1.Join();
            t2.Join();

            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
            Assert.IsTrue(writeRunnable.NumEvents > 100);
            Assert.IsTrue(readRunnable.NumQueries > 100);
            Log.Info("Send " + writeRunnable.NumEvents + " and performed " + readRunnable.NumQueries + " reads");
        }

        private ICollection<string> GetDeclareCols()
        {
            var cols = new List<string>();
            for (var i = 0; i < 10; i++)
            {
                // 10 columns, not configurable
                cols.Add("tc" + i + " sum(int)");
            }

            return cols;
        }

        private ICollection<string> GetIntoCols()
        {
            var cols = new List<string>();
            for (var i = 0; i < 10; i++)
            {
                // 10 columns, not configurable
                cols.Add("sum(c" + i + ") as tc" + i);
            }

            return cols;
        }

        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly string[] _groups;

            private int _numEvents;
            private bool _shutdown;

            public WriteRunnable(EPServiceProvider epService, string[] groups)
            {
                _epService = epService;
                _groups = groups;
            }

            public Exception Exception { get; private set; }

            public string[] Groups => _groups;

            public int NumEvents => _numEvents;

            public bool Shutdown => _shutdown;

            public void SetShutdown(bool shutdown)
            {
                _shutdown = shutdown;
            }

            public void Run()
            {
                Log.Info("Started event send for write");

                try
                {
                    while (!_shutdown)
                    {
                        var groupNum = _numEvents % _groups.Length;
                        _epService.EPRuntime.SendEvent(new Local10ColEvent(_groups[groupNum], _numEvents));
                        _numEvents++;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }

                Log.Info("Completed event send for write");
            }
        }

        public class ReadRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly string[] _groups;

            private int _numQueries;
            private bool _shutdown;

            public ReadRunnable(EPServiceProvider epService, string[] groups)
            {
                _epService = epService;
                _groups = groups;
            }

            public int NumQueries => _numQueries;

            public bool Shutdown => _shutdown;

            public string[] Groups => _groups;

            public Exception Exception { get; private set; }

            public void SetShutdown(bool shutdown)
            {
                _shutdown = shutdown;
            }

            public void Run()
            {
                Log.Info("Started event send for read");

                try
                {
                    var eplSelect = "select vartotal[TheString] as out from SupportBean";
                    var listener = new SupportUpdateListener();
                    _epService.EPAdministrator.CreateEPL(eplSelect).Events += listener.Update;

                    while (!_shutdown)
                    {
                        var groupNum = _numQueries % _groups.Length;
                        _epService.EPRuntime.SendEvent(new SupportBean(_groups[groupNum], 0));
                        var @event = listener.AssertOneGetNewAndReset();
                        AssertEvent((IDictionary<string, object>) @event.Get("out"));
                        _numQueries++;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }

                Log.Info("Completed event send for read");
            }

            private void AssertEvent(IDictionary<string, object> info)
            {
                object tc0 = info.Get("tc0");
                for (var i = 1; i < 10; i++)
                {
                    Assert.AreEqual(tc0, info.Get("tc" + i));
                }
            }
        }

        public sealed class Local10ColEvent
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

            public string GroupKey { get; }

            public int C0 { get; }

            public int C1 { get; }

            public int C2 { get; }

            public int C3 { get; }

            public int C4 { get; }

            public int C5 { get; }

            public int C6 { get; }

            public int C7 { get; }

            public int C8 { get; }

            public int C9 { get; }
        }
    }
} // end of namespace