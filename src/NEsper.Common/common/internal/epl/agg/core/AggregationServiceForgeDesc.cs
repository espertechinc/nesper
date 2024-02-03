///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationServiceForgeDesc
    {
        public AggregationServiceForgeDesc(
            AggregationServiceFactoryForge aggregationServiceFactoryForge,
            IList<AggregationServiceAggExpressionDesc> expressions,
            IList<ExprAggregateNodeGroupKey> groupKeyExpressions,
            IList<StmtClassForgeableFactory> additionalForgeables,
            FabricCharge fabricCharge)
        {
            AggregationServiceFactoryForge = aggregationServiceFactoryForge;
            Expressions = expressions;
            GroupKeyExpressions = groupKeyExpressions;
            AdditionalForgeables = additionalForgeables;
            FabricCharge = fabricCharge;
        }

        public AggregationServiceFactoryForge AggregationServiceFactoryForge { get; }

        public IList<AggregationServiceAggExpressionDesc> Expressions { get; }

        public IList<ExprAggregateNodeGroupKey> GroupKeyExpressions { get; }

        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }

        public FabricCharge FabricCharge { get; }
    }
} // end of namespace