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
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
	public class ExprDotForgeProperty : ExprDotEval, ExprDotForge {

	    private readonly EventPropertyGetterSPI getter;
	    private readonly EPType returnType;

	    public ExprDotForgeProperty(EventPropertyGetterSPI getter, EPType returnType) {
	        this.getter = getter;
	        this.returnType = returnType;
	    }

	    public object Evaluate(object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
	        if (!(target is EventBean)) {
	            return null;
	        }
	        return getter.Get((EventBean) target);
	    }

	    public EPType TypeInfo {
	        get => returnType;
	    }

	    public void Visit(ExprDotEvalVisitor visitor) {
	        visitor.VisitPropertySource();
	    }

	    public ExprDotEval DotEvaluator {
	        get => this;
	    }

	    public ExprDotForge DotForge {
	        get => this;
	    }

	    public CodegenExpression Codegen(CodegenExpression inner, Type innerType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        Type type = EPTypeHelper.GetCodegenReturnType(returnType);
	        if (innerType == typeof(EventBean)) {
	            return CodegenLegoCast.CastSafeFromObjectType(type, getter.EventBeanGetCodegen(inner, codegenMethodScope, codegenClassScope));
	        }
	        CodegenMethod methodNode = codegenMethodScope.MakeChild(type, typeof(ExprDotForgeProperty), codegenClassScope).AddParam(innerType, "target");

	        methodNode.Block
	                .IfInstanceOf("target", typeof(EventBean))
	                .BlockReturn(CodegenLegoCast.CastSafeFromObjectType(type, getter.EventBeanGetCodegen(Cast(typeof(EventBean), inner), methodNode, codegenClassScope)))
	                .MethodReturn(ConstantNull());
	        return LocalMethod(methodNode, inner);
	    }
	}
} // end of namespace