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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;
using NUnit.Framework;

namespace com.espertech.esper.supportregression.multithread
{
    public class StmtDatabaseJoinCallable : ICallable<bool>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EPServiceProvider _engine;
        private readonly string[] _myvarcharValues = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J"};
        private readonly int _numRepeats;
        private readonly EPStatement _stmt;

        public StmtDatabaseJoinCallable(EPServiceProvider engine, EPStatement stmt, int numRepeats)
        {
            _engine = engine;
            _stmt = stmt;
            _numRepeats = numRepeats;
        }

        public bool Call()
        {
            try
            {
                // Add assertListener
                var assertListener = new SupportMTUpdateListener();
                ThreadLogUtil.Trace("adding listeners ", assertListener);
                _stmt.Events += assertListener.Update;

                for (var loop = 0; loop < _numRepeats; loop++)
                {
                    var intPrimitive = loop % 10 + 1;
                    object eventS0 = MakeEvent(intPrimitive);

                    _engine.EPRuntime.SendEvent(eventS0);

                    // Should have received one that's mine, possible multiple since the statement is used by other threads
                    var found = false;
                    var events = assertListener.GetNewDataListFlattened();
                    foreach (var theEvent in events)
                    {
                        var s0Received = theEvent.Get("s0");
                        var s1Received = (IDictionary<string, object>) theEvent.Get("s1");
                        if (s0Received == eventS0 ||
                            s1Received.Get("myvarchar").Equals(_myvarcharValues[intPrimitive - 1]))
                        {
                            found = true;
                        }
                    }

                    if (!found)
                    {
                    }

                    Assert.IsTrue(found);
                    assertListener.Reset();
                }
            }
            catch (AssertionException ex)
            {
                Log.Error("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }

            return true;
        }

        private SupportBean MakeEvent(int intPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            return theEvent;
        }
    }
} // end of namespace