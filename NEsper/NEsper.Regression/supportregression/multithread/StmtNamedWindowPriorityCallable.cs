///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;

namespace com.espertech.esper.supportregression.multithread
{
    public class StmtNamedWindowPriorityCallable : ICallable<object>
    {
        private readonly int _threadNum;
        private readonly EPServiceProvider _engine;
        private readonly int _numRepeats;

        public StmtNamedWindowPriorityCallable(int threadNum, EPServiceProvider engine, int numRepeats)
        {
            _threadNum = threadNum;
            _engine = engine;
            _numRepeats = numRepeats;
        }

        public object Call()
        {
            try
            {
                int offset = _threadNum + 1000000;
                for (int i = 0; i < _numRepeats; i++)
                {
                    _engine.EPRuntime.SendEvent(
                        new SupportBean_S0(i + offset, "c0_" + i + offset, "p01_" + i + offset));
                    _engine.EPRuntime.SendEvent(new SupportBean_S1(i + offset, "c0_" + i + offset, "x", "y"));
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return null;
            }

            return null;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
