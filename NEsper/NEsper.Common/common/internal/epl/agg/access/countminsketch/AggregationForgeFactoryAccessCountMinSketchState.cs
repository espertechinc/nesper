///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
    public class AggregationForgeFactoryAccessCountMinSketchState : AggregationForgeFactoryAccessBase
    {
        private readonly ExprAggMultiFunctionCountMinSketchNode parent;
        private readonly AggregationStateCountMinSketchForge stateFactory;

        public AggregationForgeFactoryAccessCountMinSketchState(
            ExprAggMultiFunctionCountMinSketchNode parent,
            AggregationStateCountMinSketchForge stateFactory)
        {
            this.parent = parent;
            this.stateFactory = stateFactory;
        }

        public override Type ResultType => null;

        public override AggregationAccessorForge AccessorForge => new AggregationAccessorForgeCountMinSketch();

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation =>
            new AggregationPortableValidationCountMinSketch(stateFactory.specification.Agent.AcceptableValueTypes);

        public override AggregationMultiFunctionStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            throw new UnsupportedOperationException("State key not available as always used with tables");
        }

        public override AggregationStateFactoryForge GetAggregationStateFactory(bool isMatchRecognize)
        {
            // For match-recognize we don't allow
            if (isMatchRecognize) {
                throw new IllegalStateException("Count-min-sketch is not supported for match-recognize");
            }

            return stateFactory;
        }

        public override AggregationAgentForge GetAggregationStateAgent(
            ImportService importService,
            string statementName)
        {
            throw new UnsupportedOperationException("Agent not available for state-function");
        }
    }
} // end of namespace