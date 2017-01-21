///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.core.service
{
    /// <summary>Query result. </summary>
    public class EPQueryResultImpl : EPOnDemandQueryResult
    {
        private readonly EPPreparedQueryResult _queryResult;
    
        /// <summary>Ctor. </summary>
        /// <param name="queryResult">is the prepared query</param>
        public EPQueryResultImpl(EPPreparedQueryResult queryResult)
        {
            _queryResult = queryResult;
        }
    
        public IEnumerator<EventBean> GetEnumerator()
        {
            return ((IEnumerable<EventBean>) _queryResult.Result).GetEnumerator();
        }

        public EventBean[] Array
        {
            get { return _queryResult.Result; }
        }

        public EventType EventType
        {
            get { return _queryResult.EventType; }
        }
    }
}
