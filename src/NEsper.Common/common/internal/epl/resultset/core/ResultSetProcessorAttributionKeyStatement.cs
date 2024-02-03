///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public class ResultSetProcessorAttributionKeyStatement : ResultSetProcessorAttributionKey
    {
        public static readonly ResultSetProcessorAttributionKeyStatement INSTANCE =
            new ResultSetProcessorAttributionKeyStatement();

        private ResultSetProcessorAttributionKeyStatement()
        {
        }

        public AggregationAttributionKey AggregationAttributionKey => AggregationAttributionKeyStatement.INSTANCE;
    }
} // end of namespace