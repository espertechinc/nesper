///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.agg.@base;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationServiceForgeDesc
    {
        public AggregationServiceForgeDesc(
            AggregationServiceFactoryForge aggregationServiceFactoryForge,
            IList<AggregationServiceAggExpressionDesc> expressions,
            IList<ExprAggregateNodeGroupKey> groupKeyExpressions)
        {
            AggregationServiceFactoryForge = aggregationServiceFactoryForge;
            Expressions = expressions;
            GroupKeyExpressions = groupKeyExpressions;
        }

        public AggregationServiceFactoryForge AggregationServiceFactoryForge { get; }

        public IList<AggregationServiceAggExpressionDesc> Expressions { get; }

        public IList<ExprAggregateNodeGroupKey> GroupKeyExpressions { get; }
    }
} // end of namespace