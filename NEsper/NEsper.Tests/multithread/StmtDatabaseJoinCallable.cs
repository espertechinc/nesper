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
using com.espertech.esper.support.util;
using com.espertech.esper.util;


using NUnit.Framework;

using DataMap = System.Collections.Generic.IDictionary<string, object>;

namespace com.espertech.esper.multithread
{
    public class StmtDatabaseJoinCallable : ICallable<bool>
    {
        private readonly EPServiceProvider _engine;
        private readonly String[] _myvarcharValues = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J"};
        private readonly int _numRepeats;
        private readonly EPStatement _stmt;

        public StmtDatabaseJoinCallable(EPServiceProvider engine, EPStatement stmt, int numRepeats)
        {
            _engine = engine;
            _stmt = stmt;
            _numRepeats = numRepeats;
        }

        #region ICallable Members

        public bool Call()
        {
            try {
                // Add assertListener
                var assertListener1 = new SupportMTUpdateListener(Thread.CurrentThread.Name + "#1");
                ThreadLogUtil.Trace("adding listeners ", assertListener1);
                //stmt.Events += assertListener1.Update;
                _stmt.Events += assertListener1.Update;

                for (int loop = 0; loop < _numRepeats; loop++) {
                    var intPrimitive = loop%10 + 1;
                    var eventS0 = MakeEvent(intPrimitive);
                    _engine.EPRuntime.SendEvent(eventS0);

                    // Should have received one that's mine, possible multiple since the statement is used by other threads
                    var found = false;
                    var events = assertListener1.GetNewDataListFlattened();

                    foreach (var theEvent in events) {
                        var s0Received = theEvent.Get("s0");
                        var s1Received = (DataMap) theEvent.Get("s1");
                        if ((s0Received == eventS0) ||
                            (s1Received.Get("myvarchar").Equals(_myvarcharValues[intPrimitive - 1]))) {
                            found = true;
                        }
                    }

                    Assert.IsTrue(found);
                    assertListener1.Reset();
                }
            }
            catch (AssertionException ex) {
                Console.WriteLine(ex.Message);
                Log.Fatal("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            catch (Exception ex) {
                Log.Fatal("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            return true;
        }

        #endregion

        private static SupportBean MakeEvent(int intPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            return theEvent;
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
