///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.join.@base;

namespace com.espertech.esper.support.epl
{
    public class SupportJoinExecutionStrategy : JoinExecutionStrategy
    {
        private EventBean[][] _lastNewDataPerStream;
        private EventBean[][] _lastOldDataPerStream;
    
        public void Join(EventBean[][] newDataPerStream, EventBean[][] oldDataPerStream)
        {
            _lastNewDataPerStream = newDataPerStream;
            _lastOldDataPerStream = oldDataPerStream;
        }
    
        public EventBean[][] GetLastNewDataPerStream()
        {
            return _lastNewDataPerStream;
        }
    
        public EventBean[][] GetLastOldDataPerStream()
        {
            return _lastOldDataPerStream;
        }
    
        public ISet<MultiKey<EventBean>> StaticJoin()
        {
            return null;  //To change body of implemented methods use File | Settings | File Templates.
        }
    }
}
