///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.groupby
{
    public class AggGroupByDesc
    {
        public AggGroupByDesc(
            AggregationRowStateForgeDesc rowStateForgeDescs,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect,
            ExprNode[] groupByNodes,
            MultiKeyClassRef groupByMultiKey)
        {
            RowStateForgeDescs = rowStateForgeDescs;
            IsUnidirectional = isUnidirectional;
            IsFireAndForget = isFireAndForget;
            IsOnSelect = isOnSelect;
            GroupByNodes = groupByNodes;
            GroupByMultiKey = groupByMultiKey;
        }

        public AggregationRowStateForgeDesc RowStateForgeDescs { get; }

        public AggSvcGroupByReclaimAgedEvalFuncFactoryForge ReclaimEvaluationFunctionMaxAge { get; private set; }

        public AggSvcGroupByReclaimAgedEvalFuncFactoryForge ReclaimEvaluationFunctionFrequency { get; private set; }

        public object NumMethods => RowStateForgeDescs.NumMethods;

        public object NumAccess => RowStateForgeDescs.NumAccess;

        public bool IsUnidirectional { get; }

        public bool IsFireAndForget { get; }

        public bool IsOnSelect { get; }

        public bool IsRefcounted { get; set; }

        public bool IsReclaimAged { get; set; }

        public ExprNode[] GroupByNodes { get; }

        public MultiKeyClassRef GroupByMultiKey { get; }

        public void SetReclaimEvaluationFunctionMaxAge(
            AggSvcGroupByReclaimAgedEvalFuncFactoryForge reclaimEvaluationFunctionMaxAge)
        {
            ReclaimEvaluationFunctionMaxAge = reclaimEvaluationFunctionMaxAge;
        }

        public void SetReclaimEvaluationFunctionFrequency(
            AggSvcGroupByReclaimAgedEvalFuncFactoryForge reclaimEvaluationFunctionFrequency)
        {
            ReclaimEvaluationFunctionFrequency = reclaimEvaluationFunctionFrequency;
        }
    }
} // end of namespace