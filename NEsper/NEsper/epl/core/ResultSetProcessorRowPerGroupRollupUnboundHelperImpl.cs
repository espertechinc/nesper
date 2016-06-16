///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.core
{
    public class ResultSetProcessorRowPerGroupRollupUnboundHelperImpl : ResultSetProcessorRowPerGroupRollupUnboundHelper
    {
        private readonly LinkedHashMap<object, EventBean>[] eventPerGroupBuf;

        public ResultSetProcessorRowPerGroupRollupUnboundHelperImpl(int levelCount)
        {
            eventPerGroupBuf = new LinkedHashMap<object, EventBean>[levelCount];
            for (int i = 0; i < levelCount; i++)
            {
                eventPerGroupBuf[i] = new LinkedHashMap<object, EventBean>();
            }
        }

        public IDictionary<object, EventBean>[] Buffer
        {
            get { return eventPerGroupBuf; }
        }

        public void Destroy()
        {
            // no action required
        }
    }
} // end of namespace