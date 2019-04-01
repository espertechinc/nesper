///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.multithread
{
    public class StmtIterateCallable : ICallable<bool>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EPServiceProvider _engine;
        private readonly int _numRepeats;
        private readonly EPStatement[] _stmt;
        private readonly int _threadNum;

        public StmtIterateCallable(int threadNum, EPServiceProvider engine, EPStatement[] stmt, int numRepeats)
        {
            _threadNum = threadNum;
            _engine = engine;
            _stmt = stmt;
            _numRepeats = numRepeats;
        }

        public bool Call()
        {
            try
            {
                for (var loop = 0; loop < _numRepeats; loop++)
                {
                    Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " sending event " + loop);
                    var id = Convert.ToString(_threadNum * 100000000 + loop);
                    var bean = new SupportBean(id, 0);
                    _engine.EPRuntime.SendEvent(bean);

                    for (var i = 0; i < _stmt.Length; i++)
                    {
                        Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " starting iterator " + loop);
                        var it = _stmt[i].GetSafeEnumerator();
                        var found = false;
                        for (; it.MoveNext();)
                        {
                            var theEvent = it.Current;
                            if (theEvent.Get("TheString").Equals(id))
                            {
                                found = true;
                            }
                        }

                        Assert.IsTrue(found);
                        Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " end iterator " + loop);
                    }
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
    }
} // end of namespace