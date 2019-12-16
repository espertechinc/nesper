///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.order;
using com.espertech.esper.common.@internal.epl.resultset.select.core;

namespace com.espertech.esper.common.@internal.epl.agg.rollup
{
    public class GroupByRollupPerLevelForge
    {
        public GroupByRollupPerLevelForge(
            SelectExprProcessorForge[] selectExprProcessorForges,
            ExprForge[] optionalHavingForges,
            OrderByElementForge[][] optionalOrderByElements)
        {
            SelectExprProcessorForges = selectExprProcessorForges;
            OptionalHavingForges = optionalHavingForges;
            OptionalOrderByElements = optionalOrderByElements;
        }

        public SelectExprProcessorForge[] SelectExprProcessorForges { get; }

        public ExprForge[] OptionalHavingForges { get; }

        public OrderByElementForge[][] OptionalOrderByElements { get; }
    }
} // end of namespace