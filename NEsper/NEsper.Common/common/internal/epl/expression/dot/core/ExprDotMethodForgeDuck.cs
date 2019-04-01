///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
	public class ExprDotMethodForgeDuck : ExprDotForge {
	    private readonly string statementName;
	    private readonly ImportService _importService;
	    private readonly string methodName;
	    private readonly Type[] parameterTypes;
	    private readonly ExprForge[] parameters;

	    public ExprDotMethodForgeDuck(string statementName, ImportService importService, string methodName, Type[] parameterTypes, ExprForge[] parameters) {
	        this.statementName = statementName;
	        this._importService = importService;
	        this.methodName = methodName;
	        this.parameterTypes = parameterTypes;
	        this.parameters = parameters;
	    }

	    public EPType TypeInfo
	    {
	        get => EPTypeHelper.SingleValue(typeof(object));
	    }

	    public void Visit(ExprDotEvalVisitor visitor) {
	        visitor.VisitMethod(methodName);
	    }

	    public ExprDotEval DotEvaluator
	    {
	        get => new ExprDotMethodForgeDuckEval(this, ExprNodeUtilityQuery.GetEvaluatorsNoCompile(parameters));
	    }

	    public CodegenExpression Codegen(CodegenExpression inner, Type innerType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return ExprDotMethodForgeDuckEval.Codegen(this, inner, innerType, codegenMethodScope, exprSymbol, codegenClassScope);
	    }

	    public string StatementName
	    {
	        get => statementName;
	    }

	    public ImportService ImportService
	    {
	        get => _importService;
	    }

	    public string MethodName
	    {
	        get => methodName;
	    }

	    public Type[] ParameterTypes
	    {
	        get => parameterTypes;
	    }

	    public ExprForge[] Parameters
	    {
	        get => parameters;
	    }
	}
} // end of namespace