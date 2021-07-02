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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationForgeFactoryAccessSorted : AggregationForgeFactoryAccessBase
    {
        private readonly AggregationAgentForge _optionalAgent;

        private readonly AggregationMultiFunctionStateKey _optionalStateKey;

        public AggregationForgeFactoryAccessSorted(
            ExprAggMultiFunctionSortedMinMaxByNode parent,
            AggregationAccessorForge accessor,
            Type accessorResultType,
            EventType containedEventType,
            AggregationMultiFunctionStateKey optionalStateKey,
            SortedAggregationStateDesc optionalSortedStateDesc,
            AggregationAgentForge optionalAgent)
        {
            Parent = parent;
            AccessorForge = accessor;
            ResultType = accessorResultType;
            ContainedEventType = containedEventType;
            this._optionalStateKey = optionalStateKey;
            OptionalSortedStateDesc = optionalSortedStateDesc;
            this._optionalAgent = optionalAgent;
        }

        public override Type ResultType { get; }

        public override AggregationAccessorForge AccessorForge { get; }

        public override ExprAggregateNodeBase AggregationExpression => Parent;

        public override AggregationPortableValidation AggregationPortableValidation =>
            new AggregationPortableValidationSorted(
                Parent.AggregationFunctionName,
                ContainedEventType,
                OptionalSortedStateDesc?.CriteriaTypes);

        public EventType ContainedEventType { get; }

        public ExprAggMultiFunctionSortedMinMaxByNode Parent { get; }

        public SortedAggregationStateDesc OptionalSortedStateDesc { get; }

        public override void InitMethodForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            throw new UnsupportedOperationException("Not supported");
        }

        public override AggregationMultiFunctionStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            return _optionalStateKey;
        }

        public override AggregationStateFactoryForge GetAggregationStateFactory(bool isMatchRecognize)
        {
            if (isMatchRecognize || OptionalSortedStateDesc == null) {
                return null;
            }

            if (OptionalSortedStateDesc.IsEver) {
                return new AggregationStateMinMaxByEverForge(this);
            }

            return new AggregationStateSortedForge(this);
        }

        public override AggregationAgentForge GetAggregationStateAgent(
            ImportService importService,
            string statementName)
        {
            return _optionalAgent;
        }
    }
} // end of namespace