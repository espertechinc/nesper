///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.join.queryplan;

namespace com.espertech.esper.common.@internal.epl.join.support
{
    public class QueryPlanIndexDescOnExpr : QueryPlanIndexDescBase
    {
        private readonly string strategyName;
        private readonly string tableLookupStrategy;

        public QueryPlanIndexDescOnExpr(
            IndexNameAndDescPair[] tables,
            string strategyName,
            string tableLookupStrategy)
            : base(tables)
        {
            this.strategyName = strategyName;
            this.tableLookupStrategy = tableLookupStrategy;
        }

        public string StrategyName => strategyName;

        public string TableLookupStrategy => tableLookupStrategy;
    }
} // end of namespace