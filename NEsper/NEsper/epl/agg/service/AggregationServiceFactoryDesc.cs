///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.baseagg;

namespace com.espertech.esper.epl.agg.service
{
    public class AggregationServiceFactoryDesc
    {
        public AggregationServiceFactoryDesc(
            AggregationServiceFactory aggregationServiceFactory,
            IList<AggregationServiceAggExpressionDesc> expressions,
            IList<ExprAggregateNodeGroupKey> groupKeyExpressions)
        {
            AggregationServiceFactory = aggregationServiceFactory;
            Expressions = expressions;
            GroupKeyExpressions = groupKeyExpressions;
        }

        public AggregationServiceFactory AggregationServiceFactory { get; private set; }

        public IList<AggregationServiceAggExpressionDesc> Expressions { get; private set; }

        public IList<ExprAggregateNodeGroupKey> GroupKeyExpressions { get; private set; }
    }
}