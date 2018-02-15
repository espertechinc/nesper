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
    public class StmtUpdateSendCallable : ICallable<bool> {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EPServiceProvider _engine;
        private readonly int _numRepeats;
        private readonly int _threadNum;

        public StmtUpdateSendCallable(int threadNum, EPServiceProvider engine, int numRepeats)
        {
            this._threadNum = threadNum;
            this._engine = engine;
            this._numRepeats = numRepeats;
        }

        public bool Call()
        {
            try
            {
                Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " sending " + _numRepeats + " events");
                for (var loop = 0; loop < _numRepeats; loop++)
                {
                    var id = Convert.ToString(_threadNum * 100000000 + loop);
                    var bean = new SupportBean(id, 0);
                    _engine.EPRuntime.SendEvent(bean);
                }

                Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " completed.");
            }
            catch (AssertionException ex)
            {
                Log.Error("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            catch (Exception t)
            {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, t);
                return false;
            }

            return true;
        }
    }
} // end of namespace