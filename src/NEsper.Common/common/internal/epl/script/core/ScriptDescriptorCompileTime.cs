///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ScriptDescriptorCompileTime
    {
        private readonly string _defaultDialect;

        private readonly string optionalDialect;
        private readonly string scriptName;
        private readonly string expression;
        private readonly string[] parameterNames;
        private readonly ExprNode[] parameters;
        private readonly Type returnType;

        public ScriptDescriptorCompileTime(
            string optionalDialect,
            string scriptName,
            string expression,
            string[] parameterNames,
            ExprNode[] parameters,
            Type returnType,
            string defaultDialect)
        {
            this.optionalDialect = optionalDialect;
            this.scriptName = scriptName;
            this.expression = expression;
            this.parameterNames = parameterNames;
            this.parameters = parameters;
            this.returnType = returnType;
            _defaultDialect = defaultDialect;
        }

        public CodegenExpression Make(
            CodegenMethodScope parentInitMethod,
            CodegenClassScope classScope)
        {
            var method = parentInitMethod.MakeChild(typeof(ScriptDescriptorRuntime), GetType(), classScope)
                .AddParam<EPStatementInitServices>(EPStatementInitServicesConstants.REF.Ref);
            method.Block
                .DeclareVarNewInstance(typeof(ScriptDescriptorRuntime), "sd")
                .SetProperty(Ref("sd"), "OptionalDialect", Constant(optionalDialect))
                .SetProperty(Ref("sd"), "ScriptName", Constant(scriptName))
                .SetProperty(Ref("sd"), "Expression", Constant(expression))
                .SetProperty(Ref("sd"), "ParameterNames", Constant(parameterNames))
                .SetProperty(Ref("sd"), "EvaluationTypes", Constant(ExprNodeUtilityQuery.GetExprResultTypes(parameters)))
                .SetProperty(Ref("sd"), "Parameters", ExprNodeUtilityCodegen.CodegenEvaluators(parameters, method, GetType(), classScope))
                .SetProperty(Ref("sd"), "DefaultDialect", Constant(_defaultDialect))
                .SetProperty(Ref("sd"), "ImportService", 
                    ExprDotMethodChain(EPStatementInitServicesConstants.REF)
                        .Get(EPStatementInitServicesConstants.IMPORTSERVICERUNTIME))
                .SetProperty(Ref("sd"), "ScriptCompiler", 
                    ExprDotMethodChain(EPStatementInitServicesConstants.REF)
                        .Get(EPStatementInitServicesConstants.SCRIPTCOMPILER))
                .SetProperty(
                    Ref("sd"),
                    "Coercer",
                    returnType.IsTypeNumeric()
                        ? StaticMethod(
                            typeof(SimpleNumberCoercerFactory),
                            "GetCoercer",
                            Constant(typeof(object)),
                            Constant(returnType.GetBoxedType()))
                        : ConstantNull())
                .MethodReturn(Ref("sd"));
            return LocalMethod(method, EPStatementInitServicesConstants.REF);
        }

        public string OptionalDialect => optionalDialect;

        public string ScriptName => scriptName;

        public string Expression => expression;

        public string[] ParameterNames => parameterNames;

        public ExprNode[] Parameters => parameters;

        public Type ReturnType => returnType;
    }
} // end of namespace