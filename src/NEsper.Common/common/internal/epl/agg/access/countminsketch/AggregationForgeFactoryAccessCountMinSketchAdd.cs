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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
    public class AggregationForgeFactoryAccessCountMinSketchAdd : AggregationForgeFactoryAccessBase
    {
        public AggregationForgeFactoryAccessCountMinSketchAdd(
            ExprAggMultiFunctionCountMinSketchNode parent,
            ExprForge addOrFrequencyEvaluator,
            Type addOrFrequencyEvaluatorReturnType)
        {
            Parent = parent;
            AddOrFrequencyEvaluator = addOrFrequencyEvaluator;
            AddOrFrequencyEvaluatorReturnType = addOrFrequencyEvaluatorReturnType;
        }

        public override Type ResultType => null;

        public override AggregationAccessorForge AccessorForge => new AggregationAccessorForgeCountMinSketch();

        public override ExprAggregateNodeBase AggregationExpression => Parent;

        public override AggregationPortableValidation AggregationPortableValidation =>
            new AggregationPortableValidationCountMinSketch();

        public ExprAggMultiFunctionCountMinSketchNode Parent { get; }

        public ExprForge AddOrFrequencyEvaluator { get; }

        public Type AddOrFrequencyEvaluatorReturnType { get; }

        public override AggregationMultiFunctionStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            throw new UnsupportedOperationException("State key not available as always used with tables");
        }

        public override AggregationStateFactoryForge GetAggregationStateFactory(
            bool isMatchRecognize,
            bool isJoin)
        {
            throw new UnsupportedOperationException("State factory not available for 'add' operation");
        }

        public override AggregationAgentForge GetAggregationStateAgent(
            ImportService importService,
            string statementName)
        {
            return new AggregationAgentCountMinSketchForge(
                AddOrFrequencyEvaluator,
                Parent.OptionalFilter?.Forge);
        }
    }
} // end of namespace