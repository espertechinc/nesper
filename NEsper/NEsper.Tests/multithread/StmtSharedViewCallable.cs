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
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;

namespace com.espertech.esper.multithread
{
    public class StmtSharedViewCallable : ICallable<bool>
    {
        private readonly int _numRepeats;
        private readonly EPServiceProvider _engine;
        private readonly IList<string> _symbols;
    
        public StmtSharedViewCallable(int numRepeats, EPServiceProvider engine, IList<string> symbols)
        {
            _numRepeats = numRepeats;
            _engine = engine;
            _symbols = symbols;
        }

        public bool Call()
        {
            try
            {
                for (int loop = 0; loop < _numRepeats; loop++)
                {
                    foreach (String symbol in _symbols)
                    {
                        Object theEvent = MakeEvent(symbol, loop);
                        _engine.EPRuntime.SendEvent(theEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            return true;
        }
    
        private SupportMarketDataBean MakeEvent(String symbol, double price)
        {
            return new SupportMarketDataBean(symbol, price, null, null);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
