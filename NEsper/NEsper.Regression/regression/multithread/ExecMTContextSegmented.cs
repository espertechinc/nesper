///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class ExecMTContextSegmented : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            string[] choices = "A,B,C,D".Split(',');
            TrySend(epService, 4, 1000, choices);
        }
    
        private void TrySend(EPServiceProvider epService, int numThreads, int numEvents, string[] choices) {
            if (numEvents < choices.Length) {
                throw new ArgumentException("Number of events must at least match number of choices");
            }
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.CreateEPL("create variable bool myvar = false");
            epService.EPAdministrator.CreateEPL("create context SegmentedByString as partition by TheString from SupportBean");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context SegmentedByString select TheString, count(*) - 1 as cnt from SupportBean output snapshot when myvar = true");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // preload - since concurrently sending same-category events an event can be dropped
            for (int i = 0; i < choices.Length; i++) {
                epService.EPRuntime.SendEvent(new SupportBean(choices[i], 0));
            }
    
            var runnables = new EventRunnable[numThreads];
            for (int i = 0; i < runnables.Length; i++) {
                runnables[i] = new EventRunnable(epService, numEvents, choices);
            }
    
            // start
            var threads = new Thread[runnables.Length];
            for (int i = 0; i < runnables.Length; i++) {
                threads[i] = new Thread(runnables[i].Run);
                threads[i].Start();
            }
    
            // join
            Log.Info("Waiting for completion");
            for (int i = 0; i < runnables.Length; i++) {
                threads[i].Join();
            }
    
            var totals = new Dictionary<string, long>();
            foreach (string choice in choices) {
                totals.Put(choice, 0L);
            }
    
            // verify
            int sum = 0;
            for (int i = 0; i < runnables.Length; i++) {
                Assert.IsNull(runnables[i].Exception);
                foreach (var entry in runnables[i].Totals) {
                    long current = totals.Get(entry.Key);
                    current += entry.Value;
                    sum += entry.Value;
                    totals.Put(entry.Key, current);
                    //Log.Info("Thread " + i + " key " + entry.Key + " count " + entry.Value);
                }
            }
    
            Assert.AreEqual(numThreads * numEvents, sum);
    
            epService.EPRuntime.SetVariableValue("myvar", true);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
            EventBean[] result = listener.LastNewData;
            Assert.AreEqual(choices.Length, result.Length);
            foreach (EventBean item in result) {
                string theString = (string) item.Get("TheString");
                long count = (long) item.Get("cnt");
                //Log.Info("string " + string + " count " + count);
                Assert.AreEqual(count, totals.Get(theString));
            }
        }
    
        public class EventRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly int _numEvents;
            private readonly string[] _choices;
            private readonly IDictionary<string, int> _totals = new Dictionary<string, int>();
    
            private Exception _exception;

            public int NumEvents => _numEvents;

            public string[] Choices => _choices;

            public IDictionary<string, int> Totals => _totals;

            public Exception Exception => _exception;

            public EventRunnable(EPServiceProvider epService, int numEvents, string[] choices) {
                _epService = epService;
                _numEvents = numEvents;
                _choices = choices;
            }
    
            public void Run() {
                Log.Info("Started event send");
    
                try {
                    for (int i = 0; i < _numEvents; i++) {
                        string chosen = _choices[i % _choices.Length];
                        _epService.EPRuntime.SendEvent(new SupportBean(chosen, 1));

                        if (!_totals.TryGetValue(chosen, out var current))
                        {
                            current = 0;
                        }

                        current += 1;
                        _totals.Put(chosen, current);
                    }
                } catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                Log.Info("Completed event send");
            }
        }
    }
} // end of namespace
