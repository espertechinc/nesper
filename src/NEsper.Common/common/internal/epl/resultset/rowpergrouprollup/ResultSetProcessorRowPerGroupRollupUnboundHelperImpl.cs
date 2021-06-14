///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup
{
    using DictionaryEventBean = IDictionary<object, EventBean>;

    public class ResultSetProcessorRowPerGroupRollupUnboundHelperImpl : ResultSetProcessorRowPerGroupRollupUnboundHelper
    {
        public ResultSetProcessorRowPerGroupRollupUnboundHelperImpl(int levelCount)
        {
            Buffer = new DictionaryEventBean[levelCount];
            for (var i = 0; i < levelCount; i++) {
                Buffer[i] = new LinkedHashMap<object, EventBean>();
            }
        }

        public DictionaryEventBean[] Buffer { get; }

        public void Destroy()
        {
            // no action required
        }
    }
} // end of namespace