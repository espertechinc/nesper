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
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.inner
{
	public class InnerDotEnumerableScalarCollectionForge : ExprDotEvalRootChildInnerForge {

	    internal readonly ExprEnumerationForge rootLambdaForge;
	    internal readonly Type componentType;

	    public InnerDotEnumerableScalarCollectionForge(ExprEnumerationForge rootLambdaForge, Type componentType) {
	        this.rootLambdaForge = rootLambdaForge;
	        this.componentType = componentType;
	    }

	    public ExprDotEvalRootChildInnerEval InnerEvaluator
	    {
	        get => new InnerDotEnumerableScalarCollectionEval(rootLambdaForge.ExprEvaluatorEnumeration);
	    }

	    public CodegenExpression CodegenEvaluate(CodegenMethod parentMethod, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return rootLambdaForge.EvaluateGetROCollectionScalarCodegen(parentMethod, exprSymbol, codegenClassScope);
	    }

	    public CodegenExpression EvaluateGetROCollectionEventsCodegen(CodegenMethod parentMethod, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return rootLambdaForge.EvaluateGetROCollectionEventsCodegen(parentMethod, exprSymbol, codegenClassScope);
	    }

	    public CodegenExpression EvaluateGetROCollectionScalarCodegen(CodegenMethod parentMethod, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return rootLambdaForge.EvaluateGetROCollectionScalarCodegen(parentMethod, exprSymbol, codegenClassScope);
	    }

	    public CodegenExpression EvaluateGetEventBeanCodegen(CodegenMethod parentMethod, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return ConstantNull();
	    }

	    public EventType EventTypeCollection
	    {
	        get => null;
	    }

	    public Type ComponentTypeCollection
	    {
	        get => componentType;
	    }

	    public EventType EventTypeSingle
	    {
	        get => null;
	    }

	    public EPType TypeInfo
	    {
	        get => EPTypeHelper.CollectionOfSingleValue(componentType);
	    }
	}
} // end of namespace