///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.join.support
{
    public class QueryPlanIndexDescHistorical
    {
        public QueryPlanIndexDescHistorical(
            string strategyName,
            string indexName)
        {
            StrategyName = strategyName;
            IndexName = indexName;
        }

        public string StrategyName { get; private set; }

        public string IndexName { get; private set; }
    }
}