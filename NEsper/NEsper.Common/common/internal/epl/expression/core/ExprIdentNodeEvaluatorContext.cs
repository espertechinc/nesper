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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
	public class ExprIdentNodeEvaluatorContext : ExprIdentNodeEvaluator {

	    private readonly int streamNum;
	    private readonly Type resultType;
	    private readonly EventPropertyGetterSPI getter;

	    public ExprIdentNodeEvaluatorContext(int streamNum, Type resultType, EventPropertyGetterSPI getter) {
	        this.streamNum = streamNum;
	        this.resultType = resultType;
	        this.getter = getter;
	    }

	    public bool EvaluatePropertyExists(EventBean[] eventsPerStream, bool isNewData) {
	        return true;
	    }

	    public int StreamNum
	    {
	        get => streamNum;
	    }

	    public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        if (context.ContextProperties != null) {
	            return getter.Get(context.ContextProperties);
	        }
	        return null;
	    }

	    public CodegenExpression Codegen(Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        CodegenMethod methodNode = codegenMethodScope.MakeChild(resultType, this.GetType(), codegenClassScope);
	        CodegenExpressionRef refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);

	        methodNode.Block
	                .IfCondition(NotEqualsNull(refExprEvalCtx))
	                .BlockReturn(CodegenLegoCast.CastSafeFromObjectType(resultType, getter.EventBeanGetCodegen(ExprDotMethod(refExprEvalCtx, "getContextProperties"), methodNode, codegenClassScope)))
	                .MethodReturn(ConstantNull());
	        return LocalMethod(methodNode);
	    }

	    public Type EvaluationType
	    {
	        get => resultType;
	    }

	    public EventPropertyGetterSPI Getter
	    {
	        get => getter;
	    }

	    public bool IsContextEvaluated
	    {
	        get => true;
	    }

	    public bool OptionalEvent {
	        set { }
	    }
	}
} // end of namespace