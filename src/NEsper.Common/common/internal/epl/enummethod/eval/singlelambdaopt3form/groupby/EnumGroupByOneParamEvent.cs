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
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.groupby
{
	public class EnumGroupByOneParamEvent : ThreeFormEventPlain {

	    public EnumGroupByOneParamEvent(ExprDotEvalParamLambda lambda) : base(lambda)
	        {
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
					    if (enumcoll.IsEmpty()) {
						    return EmptyDictionary<object, ICollection<object>>.Instance;
					    }

					    var result = new Dictionary<object, ICollection<object>>();
					    var beans = (ICollection<EventBean>) enumcoll;
					    foreach (EventBean next in beans) {
						    eventsLambda[StreamNumLambda] = next;

						    var key = inner.Evaluate(eventsLambda, isNewData, context);
						    var value = result.Get(key);
						    if (value == null) {
							    value = new List<object>();
							    result.Put(key, value);
						    }

						    value.Add(next.Underlying);
					    }

					    return result;
				    },
			    };
		    }
	    }

	    public override Type ReturnType() {
	        return typeof(IDictionary<object,object>);
	    }

	    public override CodegenExpression ReturnIfEmptyOptional()
	    {
		    return EnumValue(typeof(EmptyDictionary<object, object>), "Instance");
	    }

	    public override void InitBlock(
		    CodegenBlock block,
		    CodegenMethod methodNode,
		    ExprForgeCodegenSymbol scope,
		    CodegenClassScope codegenClassScope)
	    {
		    block.DeclareVar<IDictionary<object, object>>("result", NewInstance(typeof(NullableDictionary<object, object>)));
	    }

	    public override void ForEachBlock(CodegenBlock block, CodegenMethod methodNode, ExprForgeCodegenSymbol scope, CodegenClassScope codegenClassScope) {
	        block.DeclareVar<object>("key", InnerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
	            .DeclareVar<ICollection<object>>("value", Cast(typeof(ICollection<object>), ExprDotMethod(Ref("result"), "Get", Ref("key"))))
	            .IfRefNull("value")
	            .AssignRef("value", NewInstance(typeof(List<object>)))
	            .Expression(ExprDotMethod(Ref("result"), "Put", Ref("key"), Ref("value")))
	            .BlockEnd()
	            .Expression(ExprDotMethod(Ref("value"), "Add", ExprDotUnderlying(Ref("next"))));
	    }

	    public override void ReturnResult(CodegenBlock block) {
	        block.MethodReturn(Ref("result"));
	    }
	}
} // end of namespace
