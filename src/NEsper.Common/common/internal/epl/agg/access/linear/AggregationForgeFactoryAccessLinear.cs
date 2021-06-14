///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationForgeFactoryAccessLinear : AggregationForgeFactoryAccessBase
    {
        private readonly EventType containedEventType;
        private readonly AggregationAgentForge optionalAgent;
        private readonly AggregationStateFactoryForge optionalStateFactory;
        private readonly AggregationMultiFunctionStateKey optionalStateKey;

        private readonly ExprAggMultiFunctionLinearAccessNode parent;

        public AggregationForgeFactoryAccessLinear(
            ExprAggMultiFunctionLinearAccessNode parent,
            AggregationAccessorForge accessor,
            Type accessorResultType,
            AggregationMultiFunctionStateKey optionalStateKey,
            AggregationStateFactoryForge optionalStateFactory,
            AggregationAgentForge optionalAgent,
            EventType containedEventType)
        {
            this.parent = parent;
            AccessorForge = accessor;
            ResultType = accessorResultType;
            this.optionalStateKey = optionalStateKey;
            this.optionalStateFactory = optionalStateFactory;
            this.optionalAgent = optionalAgent;
            this.containedEventType = containedEventType;
        }

        public override Type ResultType { get; }

        public override AggregationAccessorForge AccessorForge { get; }

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation =>
            new AggregationPortableValidationLinear(containedEventType);

        public override AggregationMultiFunctionStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            return optionalStateKey;
        }

        public override AggregationStateFactoryForge GetAggregationStateFactory(bool isMatchRecognize)
        {
            return optionalStateFactory;
        }

        public override AggregationAgentForge GetAggregationStateAgent(
            ImportService importService,
            string statementName)
        {
            return optionalAgent;
        }
    }
} // end of namespace