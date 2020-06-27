///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.distinctof
{
	public class EnumDistinctOfEvent : ThreeFormEventPlain {

	    private readonly Type innerType;

	    public EnumDistinctOfEvent(ExprDotEvalParamLambda lambda) : base(lambda)
	        {
	        innerType = InnerExpression.EvaluationType.GetBoxedType();
	    }

	    public override EnumEval EnumEvaluator {
		    get {
			    ExprEvaluator inner = InnerExpression.ExprEvaluator;
			    return new ProxyEnumEval() {
				    ProcEvaluateEnumMethod = (
					    eventsLambda,
					    enumcoll,
					    isNewData,
					    context) => {
					    ICollection<EventBean> beans = (ICollection<EventBean>) enumcoll;
					    if (beans.Count <= 1) {
						    return beans;
					    }

					    IDictionary<object, EventBean> distinct = new LinkedHashMap<object, EventBean>();
					    foreach (EventBean next in beans) {
						    eventsLambda[StreamNumLambda] = next;

						    object comparable = inner.Evaluate(eventsLambda, isNewData, context);
						    if (!distinct.ContainsKey(comparable)) {
							    distinct.Put(comparable, next);
						    }
					    }

					    return distinct.Values;
				    },
			    };
		    }
	    }

	    public override Type ReturnType() {
	        return typeof(ICollection<object>);
	    }

	    public override CodegenExpression ReturnIfEmptyOptional() {
	        return null;
	    }

	    public override void InitBlock(CodegenBlock block, CodegenMethod methodNode, ExprForgeCodegenSymbol scope, CodegenClassScope codegenClassScope) 
	    {
	        block.IfCondition(Relational(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), LE, Constant(1)))
	            .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
	            .DeclareVar(typeof(IDictionary<string, object>), "distinct", NewInstance(typeof(LinkedHashMap<string, object>)));
	    }

	    public override void ForEachBlock(CodegenBlock block, CodegenMethod methodNode, ExprForgeCodegenSymbol scope, CodegenClassScope codegenClassScope) {
	        CodegenExpression eval = InnerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope);
	        EnumDistinctOfHelper.ForEachBlock(block, eval, innerType);
	    }

	    public override void ReturnResult(CodegenBlock block) {
	        block.MethodReturn(ExprDotMethod(Ref("distinct"), "values"));
	    }
	}
} // end of namespace