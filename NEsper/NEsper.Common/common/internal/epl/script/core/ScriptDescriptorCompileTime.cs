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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.script.core
{
	public class ScriptDescriptorCompileTime {
	    private readonly string optionalDialect;
	    private readonly string scriptName;
	    private readonly string expression;
	    private readonly string[] parameterNames;
	    private readonly ExprForge[] parameters;
	    private readonly Type returnType;
	    private readonly string defaultDialect;

	    public ScriptDescriptorCompileTime(string optionalDialect, string scriptName, string expression, string[] parameterNames, ExprForge[] parameters, Type returnType, string defaultDialect) {
	        this.optionalDialect = optionalDialect;
	        this.scriptName = scriptName;
	        this.expression = expression;
	        this.parameterNames = parameterNames;
	        this.parameters = parameters;
	        this.returnType = returnType;
	        this.defaultDialect = defaultDialect;
	    }

	    public CodegenExpression Make(CodegenMethodScope parentInitMethod, CodegenClassScope classScope) {
	        CodegenMethod method = parentInitMethod.MakeChild(typeof(ScriptDescriptorRuntime), this.GetType(), classScope).AddParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
	        method.Block
	                .DeclareVar(typeof(ScriptDescriptorRuntime), "sd", NewInstance(typeof(ScriptDescriptorRuntime)))
	                .ExprDotMethod(@Ref("sd"), "setOptionalDialect", Constant(optionalDialect))
	                .ExprDotMethod(@Ref("sd"), "setScriptName", Constant(scriptName))
	                .ExprDotMethod(@Ref("sd"), "setExpression", Constant(expression))
	                .ExprDotMethod(@Ref("sd"), "setParameterNames", Constant(parameterNames))
	                .ExprDotMethod(@Ref("sd"), "setEvaluationTypes", Constant(ExprNodeUtilityQuery.GetExprResultTypes(parameters)))
	                .ExprDotMethod(@Ref("sd"), "setParameters", ExprNodeUtilityCodegen.CodegenEvaluators(parameters, method, this.GetType(), classScope))
	                .ExprDotMethod(@Ref("sd"), "setDefaultDialect", Constant(defaultDialect))
	                .ExprDotMethod(@Ref("sd"), "setClasspathImportService", ExprDotMethodChain(EPStatementInitServicesConstants.REF).Add(EPStatementInitServicesConstants.GETCLASSPATHIMPORTSERVICERUNTIME))
	                .ExprDotMethod(@Ref("sd"), "setCoercer", TypeHelper.IsNumeric(returnType) ? StaticMethod(typeof(SimpleNumberCoercerFactory), "getCoercer", Constant(typeof(object)),
	                        Constant(Boxing.GetBoxedType(returnType))) : ConstantNull())
	                .MethodReturn(@Ref("sd"));
	        return LocalMethod(method, EPStatementInitServicesConstants.REF);
	    }

	    public string OptionalDialect {
	        get => optionalDialect;
	    }

	    public string ScriptName {
	        get => scriptName;
	    }

	    public string Expression {
	        get => expression;
	    }

	    public string[] GetParameterNames() {
	        return parameterNames;
	    }

	    public ExprForge[] GetParameters() {
	        return parameters;
	    }

	    public Type ReturnType {
	        get => returnType;
	    }
	}
} // end of namespace