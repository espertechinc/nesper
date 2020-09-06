///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.countof
{
	public class EnumCountOfEvent : ThreeFormEventPlain {
	    public EnumCountOfEvent(ExprDotEvalParamLambda lambda) : base(lambda) {
	    }

	    public override EnumEval EnumEvaluator {
		    get {
			    ExprEvaluator inner = InnerExpression.ExprEvaluator;
			    return new ProxyEnumEval(
				    (
					    eventsLambda,
					    enumcoll,
					    isNewData,
					    context) => {
					    if (enumcoll.IsEmpty()) {
						    return 0;
					    }

					    int count = 0;
					    ICollection<EventBean> beans = (ICollection<EventBean>) enumcoll;
					    foreach (EventBean next in beans) {
						    eventsLambda[StreamNumLambda] = next;

						    object pass = inner.Evaluate(eventsLambda, isNewData, context);
						    if (pass == null || false.Equals(pass)) {
							    continue;
						    }

						    count++;
					    }

					    return count;
				    });
		    }
	    }

	    public override Type ReturnType() {
	        return typeof(int);
	    }

	    public override CodegenExpression ReturnIfEmptyOptional() {
	        return Constant(0);
	    }

	    public override void InitBlock(CodegenBlock block, CodegenMethod methodNode, ExprForgeCodegenSymbol scope, CodegenClassScope codegenClassScope) {
	        block.DeclareVar<int>("count", Constant(0));
	    }

	    public override void ForEachBlock(CodegenBlock block, CodegenMethod methodNode, ExprForgeCodegenSymbol scope, CodegenClassScope codegenClassScope) {
	        CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(block, InnerExpression.EvaluationType, InnerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope));
	        block.IncrementRef("count");
	    }

	    public override void ReturnResult(CodegenBlock block) {
	        block.MethodReturn(Ref("count"));
	    }
	}
} // end of namespace
