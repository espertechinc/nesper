///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
	public class EnumTakeWhileIndexScalarForgeEval : EnumEval {

	    private readonly EnumTakeWhileIndexScalarForge forge;
	    private readonly ExprEvaluator innerExpression;

	    public EnumTakeWhileIndexScalarForgeEval(EnumTakeWhileIndexScalarForge forge, ExprEvaluator innerExpression) {
	        this.forge = forge;
	        this.innerExpression = innerExpression;
	    }

	    public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> enumcoll, bool isNewData, ExprEvaluatorContext context) {
	        if (enumcoll.IsEmpty()) {
	            return enumcoll;
	        }

	        ObjectArrayEventBean evalEvent = new ObjectArrayEventBean(new object[1], forge.evalEventType);
	        eventsLambda[forge.streamNumLambda] = evalEvent;
	        object[] evalProps = evalEvent.Properties;
	        ObjectArrayEventBean indexEvent = new ObjectArrayEventBean(new object[1], forge.indexEventType);
	        eventsLambda[forge.streamNumLambda + 1] = indexEvent;
	        object[] indexProps = indexEvent.Properties;

	        if (enumcoll.Count == 1) {
	            object item = enumcoll.First();
	            evalProps[0] = item;
	            indexProps[0] = 0;

	            object pass = innerExpression.Evaluate(eventsLambda, isNewData, context);
	            if (pass == null || false.Equals(pass)) {
	                return Collections.GetEmptyList<object>();
	            }
	            return Collections.SingletonList(item);
	        }

	        ArrayDeque<object> result = new ArrayDeque<object>();
	        int count = -1;

	        foreach (object next in enumcoll) {

	            count++;
	            evalProps[0] = next;
	            indexProps[0] = count;

	            object pass = innerExpression.Evaluate(eventsLambda, isNewData, context);
	            if (pass == null || false.Equals(pass)) {
	                break;
	            }

	            result.Add(next);
	        }

	        return result;
	    }

	    public static CodegenExpression Codegen(EnumTakeWhileIndexScalarForge forge, EnumForgeCodegenParams args, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        CodegenExpressionField evalTypeMember = codegenClassScope.AddFieldUnshared(true, typeof(ObjectArrayEventType), Cast(typeof(ObjectArrayEventType), EventTypeUtility.ResolveTypeCodegen(forge.evalEventType, EPStatementInitServicesConstants.REF)));
	        CodegenExpressionField indexTypeMember = codegenClassScope.AddFieldUnshared(true, typeof(ObjectArrayEventType), Cast(typeof(ObjectArrayEventType), EventTypeUtility.ResolveTypeCodegen(forge.indexEventType, EPStatementInitServicesConstants.REF)));

	        ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
	        CodegenMethod methodNode = codegenMethodScope.MakeChildWithScope(typeof(ICollection<object>), typeof(EnumTakeWhileIndexScalarForgeEval), scope, codegenClassScope).AddParam(EnumForgeCodegenNames.PARAMS);
	        CodegenExpression innerValue = forge.innerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope);

	        CodegenBlock block = methodNode.Block
	                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "isEmpty"))
	                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL);
	        block.DeclareVar(typeof(ObjectArrayEventBean), "evalEvent", NewInstance(typeof(ObjectArrayEventBean), NewArrayByLength(typeof(object), Constant(1)), evalTypeMember))
	                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("evalEvent"))
	                .DeclareVar(typeof(object[]), "evalProps", ExprDotMethod(@Ref("evalEvent"), "getProperties"))
	                .DeclareVar(typeof(ObjectArrayEventBean), "indexEvent", NewInstance(typeof(ObjectArrayEventBean), NewArrayByLength(typeof(object), Constant(1)), indexTypeMember))
	                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda + 1), @Ref("indexEvent"))
	                .DeclareVar(typeof(object[]), "indexProps", ExprDotMethod(@Ref("indexEvent"), "getProperties"));

	        CodegenBlock blockSingle = block.IfCondition(EqualsIdentity(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "size"), Constant(1)))
	                .DeclareVar(typeof(object), "item", ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL).Add("iterator").Add("next"))
	                .AssignArrayElement("evalProps", Constant(0), @Ref("item"))
	                .AssignArrayElement("indexProps", Constant(0), Constant(0));
	        CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(blockSingle, forge.innerExpression.EvaluationType, innerValue, StaticMethod(typeof(Collections), "emptyList"));
	        blockSingle.BlockReturn(StaticMethod(typeof(Collections), "singletonList", @Ref("item")));

	        block.DeclareVar(typeof(ArrayDeque<object>), "result", NewInstance(typeof(ArrayDeque<object>)))
	                .DeclareVar(typeof(int), "count", Constant(-1));

	        CodegenBlock forEach = block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
	                .Increment("count")
	                .AssignArrayElement("evalProps", Constant(0), @Ref("next"))
	                .AssignArrayElement("indexProps", Constant(0), @Ref("count"));
	        CodegenLegoBooleanExpression.CodegenBreakIfNotNullAndNotPass(forEach, forge.innerExpression.EvaluationType, innerValue);
	        forEach.Expression(ExprDotMethod(@Ref("result"), "add", @Ref("next")));
	        block.MethodReturn(@Ref("result"));
	        return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
	    }
	}
} // end of namespace