///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
	public class MethodTargetStrategyVariableForge : MethodTargetStrategyForge {
	    private readonly VariableMetaData variableMetaData;
	    private readonly MethodInfo reflectionMethod;

	    public MethodTargetStrategyVariableForge(VariableMetaData variableMetaData, MethodInfo reflectionMethod) {
	        this.variableMetaData = variableMetaData;
	        this.reflectionMethod = reflectionMethod;
	    }

	    public CodegenExpression Make(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	        CodegenMethod method = parent.MakeChild(typeof(MethodTargetStrategyVariableFactory), this.GetType(), classScope);
	        method.Block
	                .DeclareVar(typeof(MethodTargetStrategyVariableFactory), "target", NewInstance(typeof(MethodTargetStrategyVariableFactory)))
	                .ExprDotMethod(@Ref("target"), "setVariable", VariableDeployTimeResolver.MakeResolveVariable(variableMetaData, symbols.GetAddInitSvc(method)))
	                .ExprDotMethod(@Ref("target"), "setMethodName", Constant(reflectionMethod.Name))
	                .ExprDotMethod(@Ref("target"), "setMethodParameters", Constant(reflectionMethod.ParameterTypes))
	                .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add("addReadyCallback", @Ref("target")))
	                .MethodReturn(@Ref("target"));
	        return LocalMethod(method);
	    }
	}
} // end of namespace