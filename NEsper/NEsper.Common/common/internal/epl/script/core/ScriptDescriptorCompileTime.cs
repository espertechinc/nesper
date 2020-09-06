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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ScriptDescriptorCompileTime
    {
        private readonly string _defaultDialect;

        public ScriptDescriptorCompileTime(
            string optionalDialect,
            string scriptName,
            string expression,
            string[] parameterNames,
            ExprNode[] parameters,
            Type returnType,
            string defaultDialect)
        {
            OptionalDialect = optionalDialect;
            ScriptName = scriptName;
            Expression = expression;
            ParameterNames = parameterNames;
            Parameters = parameters;
            ReturnType = returnType;
            _defaultDialect = defaultDialect;
        }

        public string OptionalDialect { get; }

        public string ScriptName { get; }

        public string Expression { get; }

        public string[] ParameterNames { get; }

        public ExprNode[] Parameters { get; }

        public Type ReturnType { get; }

        public CodegenExpression Make(
            CodegenMethodScope parentInitMethod,
            CodegenClassScope classScope)
        {
            var method = parentInitMethod.MakeChild(typeof(ScriptDescriptorRuntime), GetType(), classScope)
                .AddParam(
                    typeof(EPStatementInitServices),
                    EPStatementInitServicesConstants.REF.Ref);
            method.Block
                .DeclareVar<ScriptDescriptorRuntime>("sd", NewInstance(typeof(ScriptDescriptorRuntime)))
                .SetProperty(Ref("sd"), "OptionalDialect", Constant(OptionalDialect))
                .SetProperty(Ref("sd"), "ScriptName", Constant(ScriptName))
                .SetProperty(Ref("sd"), "Expression", Constant(Expression))
                .SetProperty(Ref("sd"), "ParameterNames", Constant(ParameterNames))
                .SetProperty(
                    Ref("sd"),
                    "EvaluationTypes",
                    Constant(ExprNodeUtilityQuery.GetExprResultTypes(Parameters)))
                .SetProperty(
                    Ref("sd"),
                    "Parameters",
                    ExprNodeUtilityCodegen.CodegenEvaluators(Parameters, method, GetType(), classScope))
                .SetProperty(Ref("sd"), "DefaultDialect", Constant(_defaultDialect))
                .SetProperty(
                    Ref("sd"),
                    "ImportService",
                    ExprDotName(
                            EPStatementInitServicesConstants.REF,
                            EPStatementInitServicesConstants.IMPORTSERVICERUNTIME))
                .SetProperty(
                    Ref("sd"),
                    "ScriptCompiler",
                    ExprDotName(
                        EPStatementInitServicesConstants.REF,
                        EPStatementInitServicesConstants.SCRIPTCOMPILER))
                .SetProperty(
                    Ref("sd"),
                    "Coercer",
                    ReturnType.IsNumeric()
                        ? StaticMethod(
                            typeof(SimpleNumberCoercerFactory),
                            "GetCoercer",
                            Constant(typeof(object)),
                            Constant(ReturnType.GetBoxedType()))
                        : ConstantNull())
                .MethodReturn(Ref("sd"));
            return LocalMethod(method, EPStatementInitServicesConstants.REF);
        }
    }
} // end of namespace