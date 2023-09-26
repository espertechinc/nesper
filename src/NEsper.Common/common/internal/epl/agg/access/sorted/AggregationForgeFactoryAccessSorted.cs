///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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


namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationForgeFactoryAccessSorted : AggregationForgeFactoryAccessBase
    {
        private readonly ExprAggMultiFunctionSortedMinMaxByNode parent;
        private readonly AggregationAccessorForge accessor;
        private readonly Type accessorResultType;
        private readonly EventType containedEventType;
        private readonly AggregationMultiFunctionStateKey optionalStateKey;
        private readonly SortedAggregationStateDesc optionalSortedStateDesc;
        private readonly AggregationAgentForge optionalAgent;

        public AggregationForgeFactoryAccessSorted(
            ExprAggMultiFunctionSortedMinMaxByNode parent,
            AggregationAccessorForge accessor,
            Type accessorResultType,
            EventType containedEventType,
            AggregationMultiFunctionStateKey optionalStateKey,
            SortedAggregationStateDesc optionalSortedStateDesc,
            AggregationAgentForge optionalAgent)
        {
            this.parent = parent;
            this.accessor = accessor;
            this.accessorResultType = accessorResultType;
            this.containedEventType = containedEventType;
            this.optionalStateKey = optionalStateKey;
            this.optionalSortedStateDesc = optionalSortedStateDesc;
            this.optionalAgent = optionalAgent;
        }

        public override AggregationMultiFunctionStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            return optionalStateKey;
        }

        public override AggregationStateFactoryForge GetAggregationStateFactory(
            bool isMatchRecognize,
            bool join)
        {
            if (isMatchRecognize || optionalSortedStateDesc == null) {
                return null;
            }

            if (optionalSortedStateDesc.IsEver) {
                return new AggregationStateMinMaxByEverForge(this);
            }

            return new AggregationStateSortedForge(this, join);
        }

        public override AggregationAgentForge GetAggregationStateAgent(
            ImportService importService,
            string statementName)
        {
            return optionalAgent;
        }

        public override Type ResultType => accessorResultType;

        public override AggregationAccessorForge AccessorForge => accessor;

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation => new AggregationPortableValidationSorted(
            parent.AggregationFunctionName,
            containedEventType,
            optionalSortedStateDesc?.CriteriaTypes);

        public EventType ContainedEventType => containedEventType;

        public ExprAggMultiFunctionSortedMinMaxByNode Parent => parent;

        public SortedAggregationStateDesc OptionalSortedStateDesc => optionalSortedStateDesc;
    }
} // end of namespace