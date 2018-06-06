///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NEsper.Examples.StockTicker.eventbean;

namespace NEsper.Examples.StockTicker.monitor
{
    public class StockTickerResultListener
    {
        private IList<Object> matchEvents = new List<Object>().AsSyncList();
    
        public void Emitted(LimitAlert @object)
        {
            log.Info(".Emitted Received emitted " + @object);
            matchEvents.Add(@object);
        }

        public int Count
        {
            get { return matchEvents.Count; }
        }

        public IList<object> MatchEvents
        {
            get { return matchEvents; }
        }

        public void ClearMatched()
        {
            matchEvents.Clear();
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
