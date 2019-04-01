///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.approx;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.agg.factory
{
    public interface AggregationFactoryFactory
    {
        AggregationMethodFactory MakeCount(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprCountNode exprCountNode, bool ignoreNulls, Type countedValueType);

        AggregationMethodFactory MakeSum(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprSumNode exprSumNode, Type childType);

        AggregationMethodFactory MakeAvedev(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprAvedevNode exprAvedevNode, Type childType, ExprNode[] positionalParams);

        AggregationMethodFactory MakeAvg(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprAvgNode exprAvgNode, Type childType, MathContext optionalMathContext);

        AggregationMethodFactory MakeCountEver(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprCountEverNode exprCountEverNode, bool ignoreNulls);

        AggregationMethodFactory MakeFirstEver(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprFirstEverNode exprFirstEverNode, Type type);

        AggregationMethodFactory MakeLastEver(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprLastEverNode exprLastEverNode, Type type);

        AggregationMethodFactory MakeLeaving(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprLeavingAggNode exprLeavingAggNode);

        AggregationMethodFactory MakeMedian(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprMedianNode exprMedianNode, Type childType);

        AggregationMethodFactory MakeMinMax(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprMinMaxAggrNode exprMinMaxAggrNode, Type type, bool hasDataWindows);

        AggregationMethodFactory MakeNth(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprNthAggNode exprNthAggNode, Type type, int size);

        AggregationMethodFactory MakePlugInMethod(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprPlugInAggNode expr, AggregationFunctionFactory factory, Type childType);

        AggregationMethodFactory MakeRate(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprRateAggNode exprRateAggNode, bool isEver, long intervalMsec, TimeProvider timeProvider,
            TimeAbacus timeAbacus);

        AggregationMethodFactory MakeStddev(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprStddevNode exprStddevNode, Type childType);

        AggregationMethodFactory MakeLinearUnbounded(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprAggMultiFunctionLinearAccessNode exprAggMultiFunctionLinearAccessNode, EventType containedType,
            Type accessorResultType, int streamNum, bool hasFilter);

        AggregationStateFactory MakeLinear(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprAggMultiFunctionLinearAccessNode expr, int streamNum, ExprEvaluator optionalFilter);

        AggregationStateFactoryCountMinSketch MakeCountMinSketch(
            StatementExtensionSvcContext statementExtensionSvcContext, ExprAggCountMinSketchNode expr,
            CountMinSketchSpec specification);

        AggregationStateFactory MakeMinMaxEver(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprAggMultiFunctionSortedMinMaxByNode expr, AggregationStateMinMaxByEverSpec spec);

        AggregationStateFactory MakePlugInAccess(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprPlugInAggMultiFunctionNodeFactory factory);

        AggregationStateFactory MakeSorted(StatementExtensionSvcContext statementExtensionSvcContext,
            ExprAggMultiFunctionSortedMinMaxByNode expr, AggregationStateSortedSpec spec);
    }
} // end of namespace