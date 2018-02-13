///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.multithread
{
    public class TwoPatternRunnable
    {
        private readonly EPServiceProvider _engine;
        private readonly SupportStmtAwareUpdateListener _listener;
        private bool _isShutdown;
    
        public TwoPatternRunnable(EPServiceProvider engine) {
            _engine = engine;
            _listener = new SupportStmtAwareUpdateListener();
        }
    
        public void SetShutdown(bool shutdown) {
            _isShutdown = shutdown;
        }
    
        public void Run() {
            var stmtText = "every event1=SupportEvent(userId in ('100','101'),amount>=1000)";
            var statement = _engine.EPAdministrator.CreatePattern(stmtText);
            statement.Events += _listener.Update;
    
            var countLoops = 0;
            while (!_isShutdown) {
                countLoops++;
                var matches = new List<SupportTradeEvent>();
    
                for (var i = 0; i < 10000; i++) {
                    SupportTradeEvent bean;
                    if (i % 1000 == 1) {
                        bean = new SupportTradeEvent(i, "100", 1001);
                        matches.Add(bean);
                    } else {
                        bean = new SupportTradeEvent(i, "101", 10);
                    }
                    _engine.EPRuntime.SendEvent(bean);
                }
    
                // check results
                var received = _listener.GetNewDataListFlattened();
                Assert.AreEqual(matches.Count, received.Length);
                for (var i = 0; i < received.Length; i++) {
                    Assert.AreSame(matches[i], received[i].Get("event1"));
                }
    
                // Log.Info("Found " + received.Length + " matches in loop #" + countLoops);
                _listener.Reset();
            }
        }
    }
} // end of namespace
