///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.approx;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.agg.factory
{
	public class AggregationFactoryFactoryDefault : AggregationFactoryFactory
    {
	    public readonly static AggregationFactoryFactoryDefault INSTANCE = new AggregationFactoryFactoryDefault();

	    private AggregationFactoryFactoryDefault()
        {
	    }

	    public AggregationMethodFactory MakeCount(StatementExtensionSvcContext statementExtensionSvcContext, ExprCountNode exprCountNode, bool ignoreNulls, Type countedValueType)
        {
	        return new AggregationMethodFactoryCount(exprCountNode, ignoreNulls, countedValueType);
	    }

	    public AggregationMethodFactory MakeSum(StatementExtensionSvcContext statementExtensionSvcContext, ExprSumNode exprSumNode, Type childType)
        {
	        return new AggregationMethodFactorySum(exprSumNode, childType);
	    }

	    public AggregationMethodFactory MakeAvedev(StatementExtensionSvcContext statementExtensionSvcContext, ExprAvedevNode exprAvedevNode, Type childType, ExprNode[] positionalParams)
        {
	        return new AggregationMethodFactoryAvedev(exprAvedevNode, childType, positionalParams);
	    }

	    public AggregationMethodFactory MakeAvg(StatementExtensionSvcContext statementExtensionSvcContext, ExprAvgNode exprAvgNode, Type childType, MathContext optionalMathContext)
        {
	        return new AggregationMethodFactoryAvg(exprAvgNode, childType, optionalMathContext);
	    }

	    public AggregationMethodFactory MakeCountEver(StatementExtensionSvcContext statementExtensionSvcContext, ExprCountEverNode exprCountEverNode, bool ignoreNulls)
        {
	        return new AggregationMethodFactoryCountEver(exprCountEverNode, ignoreNulls);
	    }

	    public AggregationMethodFactory MakeFirstEver(StatementExtensionSvcContext statementExtensionSvcContext, ExprFirstEverNode exprFirstEverNode, Type type)
        {
	        return new AggregationMethodFactoryFirstEver(exprFirstEverNode, type);
	    }

	    public AggregationMethodFactory MakeLastEver(StatementExtensionSvcContext statementExtensionSvcContext, ExprLastEverNode exprLastEverNode, Type type)
        {
	        return new AggregationMethodFactoryLastEver(exprLastEverNode, type);
	    }

	    public AggregationMethodFactory MakeLeaving(StatementExtensionSvcContext statementExtensionSvcContext, ExprLeavingAggNode exprLeavingAggNode)
        {
	        return new AggregationMethodFactoryLeaving(exprLeavingAggNode);
	    }

	    public AggregationMethodFactory MakeMedian(StatementExtensionSvcContext statementExtensionSvcContext, ExprMedianNode exprMedianNode, Type childType)
        {
	        return new AggregationMethodFactoryMedian(exprMedianNode, childType);
	    }

	    public AggregationMethodFactory MakeMinMax(StatementExtensionSvcContext statementExtensionSvcContext, ExprMinMaxAggrNode exprMinMaxAggrNode, Type type, bool hasDataWindows)
        {
	        return new AggregationMethodFactoryMinMax(exprMinMaxAggrNode, type, hasDataWindows);
	    }

	    public AggregationMethodFactory MakeNth(StatementExtensionSvcContext statementExtensionSvcContext, ExprNthAggNode exprNthAggNode, Type type, int size)
        {
	        return new AggregationMethodFactoryNth(exprNthAggNode, type, size);
	    }

	    public AggregationMethodFactory MakePlugInMethod(StatementExtensionSvcContext statementExtensionSvcContext, ExprPlugInAggNode expr, AggregationFunctionFactory factory, Type childType)
        {
	        return new AggregationMethodFactoryPlugIn(expr, factory, childType);
	    }

	    public AggregationMethodFactory MakeRate(StatementExtensionSvcContext statementExtensionSvcContext, ExprRateAggNode exprRateAggNode, bool isEver, long intervalMsec, TimeProvider timeProvider)
        {
	        return new AggregationMethodFactoryRate(exprRateAggNode, isEver, intervalMsec, timeProvider);
	    }

	    public AggregationMethodFactory MakeStddev(StatementExtensionSvcContext statementExtensionSvcContext, ExprStddevNode exprStddevNode, Type childType)
        {
	        return new AggregationMethodFactoryStddev(exprStddevNode, childType);
	    }

	    public AggregationStateFactory MakeLinear(StatementExtensionSvcContext statementExtensionSvcContext, ExprAggMultiFunctionLinearAccessNode expr, int streamNum)
        {
	        return new AggregationStateFactoryLinear(expr, streamNum);
	    }

	    public AggregationStateFactoryCountMinSketch MakeCountMinSketch(StatementExtensionSvcContext statementExtensionSvcContext, ExprAggCountMinSketchNode expr, CountMinSketchSpec specification)
        {
	        return new AggregationStateFactoryCountMinSketch(expr, specification);
	    }

	    public AggregationStateFactory MakeMinMaxEver(StatementExtensionSvcContext statementExtensionSvcContext, ExprAggMultiFunctionSortedMinMaxByNode expr, AggregationStateMinMaxByEverSpec spec)
        {
	        return new AggregationStateFactoryMinMaxByEver(expr, spec);
	    }

	    public AggregationStateFactory MakePlugInAccess(StatementExtensionSvcContext statementExtensionSvcContext, ExprPlugInAggMultiFunctionNodeFactory factory)
        {
	        return new AggregationStateFactoryPlugin(factory);
	    }

	    public AggregationStateFactory MakeSorted(StatementExtensionSvcContext statementExtensionSvcContext, ExprAggMultiFunctionSortedMinMaxByNode expr, AggregationStateSortedSpec spec)
        {
	        return new AggregationStateFactorySorted(expr, spec);
	    }
	}
} // end of namespace
