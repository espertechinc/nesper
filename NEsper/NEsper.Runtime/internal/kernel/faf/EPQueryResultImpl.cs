///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;

namespace com.espertech.esper.runtime.@internal.kernel.faf
{
    /// <summary>
    ///     Query result.
    /// </summary>
    public class EPQueryResultImpl : EPFireAndForgetQueryResult
    {
        private readonly EPPreparedQueryResult queryResult;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="queryResult">is the prepared query</param>
        public EPQueryResultImpl(EPPreparedQueryResult queryResult)
        {
            this.queryResult = queryResult;
        }

        public EventBean[] Array => queryResult.Result;

        public EventType EventType => queryResult.EventType;

        public IEnumerator<EventBean> GetEnumerator()
        {
            return new ArrayEventEnumerator(queryResult.Result);
        }
    }
} // end of namespace