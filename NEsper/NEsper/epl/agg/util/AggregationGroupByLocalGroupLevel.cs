///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.util
{
    public class AggregationGroupByLocalGroupLevel
    {
        public AggregationGroupByLocalGroupLevel(
            IList<ExprNode> partitionExpr,
            IList<AggregationServiceAggExpressionDesc> expressions)
        {
            PartitionExpr = partitionExpr;
            Expressions = expressions;
        }

        public IList<ExprNode> PartitionExpr { get; private set; }

        public IList<AggregationServiceAggExpressionDesc> Expressions { get; private set; }
    }
} // end of namespace