///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
	/// <summary>
	/// Strategy for subselects with "=/!=/&gt;&lt; ALL".
	/// </summary>
	public class SubselectForgeNREqualsAllAnyWGroupBy : SubselectForgeNREqualsBase {

	    private readonly ExprForge havingEval;
	    private readonly bool isAll;

	    public SubselectForgeNREqualsAllAnyWGroupBy(ExprSubselectNode subselect, ExprForge valueEval, ExprForge selectEval, bool resultWhenNoMatchingEvents, bool isNot, SimpleNumberCoercer coercer, ExprForge havingEval, bool isAll) : base(subselect, valueEval, selectEval, resultWhenNoMatchingEvents, isNot, coercer)
	        {
	        this.havingEval = havingEval;
	        this.isAll = isAll;
	    }

	    protected override CodegenExpression CodegenEvaluateInternal(CodegenMethodScope parent, SubselectForgeNRSymbol symbols, CodegenClassScope classScope) {
	        CodegenExpression aggService = classScope.PackageScope.AddOrGetFieldWellKnown(new CodegenFieldNameSubqueryAgg(subselect.SubselectNumber), typeof(AggregationResultFuture));

	        CodegenMethod method = parent.MakeChild(subselect.EvaluationType, this.GetType(), classScope);
	        CodegenExpressionRef evalCtx = symbols.GetAddExprEvalCtx(method);
	        CodegenExpressionRef left = symbols.GetAddLeftResult(method);

	        method.Block
	                .DeclareVar(typeof(int), "cpid", ExprDotMethod(evalCtx, "getAgentInstanceId"))
	                .DeclareVar(typeof(AggregationService), "aggregationService", ExprDotMethod(aggService, "getContextPartitionAggregationService", @Ref("cpid")))
	                .DeclareVar(typeof(ICollection<object>), "groupKeys", ExprDotMethod(@Ref("aggregationService"), "getGroupKeys", evalCtx))
	                .DeclareVar(typeof(bool), "hasNullRow", ConstantFalse());

	        CodegenBlock forEach = method.Block.ForEach(typeof(object), "groupKey", @Ref("groupKeys"));
	        {
	            forEach.IfCondition(EqualsNull(left)).BlockReturn(ConstantNull())
	                    .ExprDotMethod(@Ref("aggregationService"), "setCurrentAccess", @Ref("groupKey"), @Ref("cpid"), ConstantNull());

	            if (havingEval != null) {
	                CodegenLegoBooleanExpression.CodegenContinueIfNullOrNotPass(forEach, havingEval.EvaluationType, havingEval.EvaluateCodegen(havingEval.EvaluationType, method, symbols, classScope));
	            }

	            Type valueRightType;
	            if (selectEval != null) {
	                valueRightType = Boxing.GetBoxedType(selectEval.EvaluationType);
	                forEach.DeclareVar(valueRightType, "valueRight", selectEval.EvaluateCodegen(valueRightType, method, symbols, classScope));
	            } else {
	                valueRightType = typeof(object);
	                forEach.DeclareVar(valueRightType, "valueRight", ExprDotUnderlying(ArrayAtIndex(symbols.GetAddEPS(method), Constant(0))));
	            }

	            CodegenBlock ifRightNotNull = forEach.IfCondition(EqualsNull(@Ref("valueRight")))
	                    .AssignRef("hasNullRow", ConstantTrue())
	                    .IfElse();
	            {
	                if (coercer == null) {
	                    ifRightNotNull.DeclareVar(typeof(bool), "eq", ExprDotMethod(left, "equals", @Ref("valueRight")));
	                } else {
	                    ifRightNotNull.DeclareVar(typeof(object), "left", coercer.CoerceCodegen(left, symbols.LeftResultType))
	                            .DeclareVar(typeof(object), "right", coercer.CoerceCodegen(@Ref("valueRight"), valueRightType))
	                            .DeclareVar(typeof(bool), "eq", ExprDotMethod(@Ref("left"), "equals", @Ref("right")));
	                }
	                if (isNot) {
	                    if (isAll) {
	                        ifRightNotNull.IfCondition(@Ref("eq")).BlockReturn(ConstantFalse());
	                    } else {
	                        ifRightNotNull.IfCondition(Not(@Ref("eq"))).BlockReturn(ConstantTrue());
	                    }
	                } else {
	                    if (isAll) {
	                        ifRightNotNull.IfCondition(Not(@Ref("eq"))).BlockReturn(ConstantFalse());
	                    } else {
	                        ifRightNotNull.IfCondition(@Ref("eq")).BlockReturn(ConstantTrue());
	                    }
	                }
	            }
	        }

	        method.Block
	                .IfCondition(@Ref("hasNullRow")).BlockReturn(ConstantNull())
	                .MethodReturn(isAll ? ConstantTrue() : ConstantFalse());

	        return LocalMethod(method);
	    }
	}
} // end of namespace