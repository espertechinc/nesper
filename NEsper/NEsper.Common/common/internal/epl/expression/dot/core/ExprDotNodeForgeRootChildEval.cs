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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
	public class ExprDotNodeForgeRootChildEval : ExprEvaluator, ExprEnumerationEval {
	    private readonly ExprDotNodeForgeRootChild forge;
	    private readonly ExprDotEvalRootChildInnerEval innerEvaluator;
	    private readonly ExprDotEval[] evalIteratorEventBean;
	    private readonly ExprDotEval[] evalUnpacking;

	    public ExprDotNodeForgeRootChildEval(ExprDotNodeForgeRootChild forge, ExprDotEvalRootChildInnerEval innerEvaluator, ExprDotEval[] evalIteratorEventBean, ExprDotEval[] evalUnpacking) {
	        this.forge = forge;
	        this.innerEvaluator = innerEvaluator;
	        this.evalIteratorEventBean = evalIteratorEventBean;
	        this.evalUnpacking = evalUnpacking;
	    }

	    public ExprEnumerationEval ExprEvaluatorEnumeration {
	        get => this;
	    }

	    public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        object inner = innerEvaluator.Evaluate(eventsPerStream, isNewData, context);
	        if (inner != null) {
	            inner = ExprDotNodeUtility.EvaluateChain(forge.forgesUnpacking, evalUnpacking, inner, eventsPerStream, isNewData, context);
	        }
	        return inner;
	    }

	    public static CodegenExpression Codegen(ExprDotNodeForgeRootChild forge, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        Type innerType = EPTypeHelper.GetCodegenReturnType(forge.innerForge.TypeInfo);
	        Type evaluationType = forge.EvaluationType;
	        CodegenMethod methodNode = codegenMethodScope.MakeChild(evaluationType, typeof(ExprDotNodeForgeRootChildEval), codegenClassScope);

	        CodegenBlock block = methodNode.Block
	                .DeclareVar(innerType, "inner", forge.innerForge.CodegenEvaluate(methodNode, exprSymbol, codegenClassScope));
	        if (!innerType.IsPrimitive && evaluationType != typeof(void)) {
	            block.IfRefNullReturnNull("inner");
	        }

	        CodegenExpression typeInformation = ConstantNull();
	        if (codegenClassScope.IsInstrumented) {
	            typeInformation = codegenClassScope.AddOrGetFieldSharable(new EPTypeCodegenSharable(forge.innerForge.TypeInfo, codegenClassScope));
	        }

	        block.Apply(InstrumentationCode.Instblock(codegenClassScope, "qExprDotChain", typeInformation, @Ref("inner"), Constant(forge.forgesUnpacking.Length)));
	        CodegenExpression expression = ExprDotNodeUtility.EvaluateChainCodegen(methodNode, exprSymbol, codegenClassScope, @Ref("inner"), innerType, forge.forgesUnpacking, null);
	        if (evaluationType == typeof(void)) {
	            block.Expression(expression)
	                    .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
	                    .MethodEnd();
	        } else {
	            block.DeclareVar(evaluationType, "result", expression)
	                    .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
	                    .MethodReturn(@Ref("result"));
	        }
	        return LocalMethod(methodNode);
	    }

	    public ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        object inner = innerEvaluator.EvaluateGetROCollectionEvents(eventsPerStream, isNewData, context);
	        if (inner != null) {
	            inner = ExprDotNodeUtility.EvaluateChain(forge.forgesIteratorEventBean, evalIteratorEventBean, inner, eventsPerStream, isNewData, context);
	            if (inner is ICollection) {
	                return (ICollection<EventBean>) inner;
	            }
	        }
	        return null;
	    }

	    public static CodegenExpression CodegenEvaluateGetROCollectionEvents(ExprDotNodeForgeRootChild forge, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        CodegenMethod methodNode = codegenMethodScope.MakeChild(forge.EvaluationType, typeof(ExprDotNodeForgeRootChildEval), codegenClassScope);

	        CodegenExpression typeInformation = ConstantNull();
	        if (codegenClassScope.IsInstrumented) {
	            typeInformation = codegenClassScope.AddOrGetFieldSharable(new EPTypeCodegenSharable(forge.innerForge.TypeInfo, codegenClassScope));
	        }

	        methodNode.Block
	                .DeclareVar(typeof(ICollection<object>), "inner", forge.innerForge.EvaluateGetROCollectionEventsCodegen(methodNode, exprSymbol, codegenClassScope))
	                .Apply(InstrumentationCode.Instblock(codegenClassScope, "qExprDotChain", typeInformation, @Ref("inner"), Constant(forge.forgesUnpacking.Length)))
	                .IfRefNull("inner")
	                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
	                .BlockReturn(ConstantNull())
	                .DeclareVar(forge.EvaluationType, "result", ExprDotNodeUtility.EvaluateChainCodegen(methodNode, exprSymbol, codegenClassScope, @Ref("inner"), typeof(ICollection<object>), forge.forgesIteratorEventBean, null))
	                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
	                .MethodReturn(@Ref("result"));
	        return LocalMethod(methodNode);
	    }

	    public ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        object inner = innerEvaluator.EvaluateGetROCollectionScalar(eventsPerStream, isNewData, context);
	        if (inner != null) {
	            inner = ExprDotNodeUtility.EvaluateChain(forge.forgesIteratorEventBean, evalIteratorEventBean, inner, eventsPerStream, isNewData, context);
	            if (inner is ICollection) {
	                return (ICollection) inner;
	            }
	        }
	        return null;
	    }

	    public static CodegenExpression CodegenEvaluateGetROCollectionScalar(ExprDotNodeForgeRootChild forge, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        CodegenMethod methodNode = codegenMethodScope.MakeChild(forge.EvaluationType, typeof(ExprDotNodeForgeRootChildEval), codegenClassScope);

	        CodegenExpression typeInformation = ConstantNull();
	        if (codegenClassScope.IsInstrumented) {
	            typeInformation = codegenClassScope.AddOrGetFieldSharable(new EPTypeCodegenSharable(forge.innerForge.TypeInfo, codegenClassScope));
	        }

	        methodNode.Block.DeclareVar(typeof(ICollection<object>), "inner", forge.innerForge.EvaluateGetROCollectionScalarCodegen(methodNode, exprSymbol, codegenClassScope))
	                .Apply(InstrumentationCode.Instblock(codegenClassScope, "qExprDotChain", typeInformation, @Ref("inner"), Constant(forge.forgesUnpacking.Length)))
	                .IfRefNull("inner")
	                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
	                .BlockReturn(ConstantNull())
	                .DeclareVar(forge.EvaluationType, "result", ExprDotNodeUtility.EvaluateChainCodegen(methodNode, exprSymbol, codegenClassScope, @Ref("inner"), typeof(ICollection<object>), forge.forgesIteratorEventBean, null))
	                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
	                .MethodReturn(@Ref("result"));
	        return LocalMethod(methodNode);
	    }

	    public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        return null;
	    }

	}
} // end of namespace