///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
	public class EnumToMapScalarLambdaForgeEval : EnumEval {

	    private readonly EnumToMapScalarLambdaForge forge;
	    private readonly ExprEvaluator innerExpression;
	    private readonly ExprEvaluator secondExpression;

	    public EnumToMapScalarLambdaForgeEval(EnumToMapScalarLambdaForge forge, ExprEvaluator innerExpression, ExprEvaluator secondExpression) {
	        this.forge = forge;
	        this.innerExpression = innerExpression;
	        this.secondExpression = secondExpression;
	    }

	    public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> enumcoll, bool isNewData, ExprEvaluatorContext context) {
	        if (enumcoll.IsEmpty()) {
	            return Collections.EmptyMap();
	        }

	        IDictionary<object, object> map = new Dictionary<object, object>();
	        ObjectArrayEventBean resultEvent = new ObjectArrayEventBean(new object[1], forge.resultEventType);
	        eventsLambda[forge.streamNumLambda] = resultEvent;
	        object[] props = resultEvent.Properties;

	        ICollection<object> values = (ICollection<object>) enumcoll;
	        foreach (object next in values) {

	            props[0] = next;

	            object key = innerExpression.Evaluate(eventsLambda, isNewData, context);
	            object value = secondExpression.Evaluate(eventsLambda, isNewData, context);
	            map.Put(key, value);
	        }

	        return map;
	    }

	    public static CodegenExpression Codegen(EnumToMapScalarLambdaForge forge, EnumForgeCodegenParams args, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        CodegenExpressionField resultTypeMember = codegenClassScope.AddFieldUnshared(true, typeof(ObjectArrayEventType), Cast(typeof(ObjectArrayEventType), EventTypeUtility.ResolveTypeCodegen(forge.resultEventType, EPStatementInitServicesConstants.REF)));

	        ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
	        CodegenMethod methodNode = codegenMethodScope.MakeChildWithScope(typeof(IDictionary<object, object>), typeof(EnumToMapScalarLambdaForgeEval), scope, codegenClassScope).AddParam(EnumForgeCodegenNames.PARAMS);

	        CodegenBlock block = methodNode.Block
	                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "isEmpty"))
	                .BlockReturn(StaticMethod(typeof(Collections), "emptyMap"));
	        block.DeclareVar(typeof(IDictionary<object, object>), "map", NewInstance(typeof(Dictionary<object, object>)))
	                .DeclareVar(typeof(ObjectArrayEventBean), "resultEvent", NewInstance(typeof(ObjectArrayEventBean), NewArrayByLength(typeof(object), Constant(1)), resultTypeMember))
	                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("resultEvent"))
	                .DeclareVar(typeof(object[]), "props", ExprDotMethod(@Ref("resultEvent"), "getProperties"));
	        CodegenBlock forEach = block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
	                .AssignArrayElement("props", Constant(0), @Ref("next"))
	                .DeclareVar(typeof(object), "key", forge.innerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
	                .DeclareVar(typeof(object), "value", forge.secondExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
	                .Expression(ExprDotMethod(@Ref("map"), "put", @Ref("key"), @Ref("value")));
	        block.MethodReturn(@Ref("map"));
	        return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
	    }
	}
} // end of namespace