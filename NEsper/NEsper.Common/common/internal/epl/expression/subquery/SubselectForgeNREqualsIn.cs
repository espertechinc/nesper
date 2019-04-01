///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
	/// <summary>
	/// Represents a in-subselect evaluation strategy.
	/// </summary>
	public class SubselectForgeNREqualsIn : SubselectForgeNREqualsInBase {
	    private readonly ExprForge filterEval;

	    public SubselectForgeNREqualsIn(ExprSubselectNode subselect, ExprForge valueEval, ExprForge selectEval, bool resultWhenNoMatchingEvents, bool isNotIn, SimpleNumberCoercer coercer, ExprForge filterEval) : base(subselect, valueEval, selectEval, resultWhenNoMatchingEvents, isNotIn, coercer)
	        {
	        this.filterEval = filterEval;
	    }

	    protected override CodegenExpression CodegenEvaluateInternal(CodegenMethodScope parent, SubselectForgeNRSymbol symbols, CodegenClassScope classScope) {
	        CodegenMethod method = parent.MakeChild(typeof(bool?), this.GetType(), classScope);
	        CodegenExpressionRef left = symbols.GetAddLeftResult(method);
	        method.Block.DeclareVar(typeof(bool), "hasNullRow", ConstantFalse());
	        CodegenBlock @foreach = method.Block.ForEach(typeof(EventBean), "theEvent", symbols.GetAddMatchingEvents(method));
	        {
	            @foreach.AssignArrayElement(NAME_EPS, Constant(0), @Ref("theEvent"));
	            if (filterEval != null) {
	                CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(@foreach, filterEval.EvaluationType, filterEval.EvaluateCodegen(typeof(bool?), method, symbols, classScope));
	            }
	            @foreach.IfRefNullReturnNull(left);

	            Type valueRightType;
	            if (selectEval != null) {
	                valueRightType = Boxing.GetBoxedType(selectEval.EvaluationType);
	                @foreach.DeclareVar(valueRightType, "valueRight", selectEval.EvaluateCodegen(valueRightType, method, symbols, classScope));
	            } else {
	                valueRightType = subselect.RawEventType.UnderlyingType;
	                @foreach.DeclareVar(valueRightType, "valueRight", Cast(valueRightType, ExprDotUnderlying(ArrayAtIndex(symbols.GetAddEPS(method), Constant(0)))));
	            }

	            CodegenBlock ifRight = @foreach.IfCondition(NotEqualsNull(@Ref("valueRight")));
	            {
	                if (coercer == null) {
	                    ifRight.IfCondition(ExprDotMethod(left, "equals", @Ref("valueRight"))).BlockReturn(Constant(!isNotIn));
	                } else {
	                    ifRight.DeclareVar(typeof(object), "left", coercer.CoerceCodegen(left, symbols.LeftResultType))
	                            .DeclareVar(typeof(object), "right", coercer.CoerceCodegen(@Ref("valueRight"), valueRightType))
	                            .DeclareVar(typeof(bool), "eq", ExprDotMethod(@Ref("left"), "equals", @Ref("right")))
	                            .IfCondition(@Ref("eq")).BlockReturn(Constant(!isNotIn));
	                }
	            }
	            ifRight.IfElse().AssignRef("hasNullRow", ConstantTrue());
	        }

	        method.Block
	                .IfCondition(@Ref("hasNullRow"))
	                .BlockReturn(ConstantNull())
	                .MethodReturn(Constant(isNotIn));
	        return LocalMethod(method);
	    }
	}
} // end of namespace