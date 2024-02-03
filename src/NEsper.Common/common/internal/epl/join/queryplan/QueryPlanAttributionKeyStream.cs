///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    public class QueryPlanAttributionKeyStream : QueryPlanAttributionKey
    {
        private readonly int streamNum;

        public QueryPlanAttributionKeyStream(int streamNum)
        {
            this.streamNum = streamNum;
        }

        public T Accept<T>(QueryPlanAttributionKeyVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public int StreamNum => streamNum;
    }
} // end of namespace