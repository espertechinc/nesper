///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.script.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class ExpressionScriptProvided
    {
        public ExpressionScriptProvided()
        {
        }

        public ExpressionScriptProvided(
            string name,
            string expression,
            string[] parameterNames,
            string optionalReturnTypeName,
            string optionalEventTypeName,
            string optionalDialect)
        {
            Name = name;
            Expression = expression;
            ParameterNames = parameterNames;
            OptionalReturnTypeName = optionalReturnTypeName;
            OptionalEventTypeName = optionalEventTypeName;
            OptionalDialect = optionalDialect;
            if (expression == null) {
                throw new ArgumentException("Invalid null expression received");
            }
        }

        public string Name { get; set; }

        public string Expression { get; set; }

        public string[] ParameterNames { get; set; }

        public string OptionalReturnTypeName { get; set; }

        public string OptionalEventTypeName { get; set; }

        public string OptionalDialect { get; set; }

        public string ModuleName { get; set; }

        public NameAccessModifier Visibility { get; set; } = NameAccessModifier.TRANSIENT;

        public ExpressionScriptCompiled CompiledBuf { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ExpressionScriptProvided), GetType(), classScope);
            method.Block
                .DeclareVarNewInstance(typeof(ExpressionScriptProvided), "sp")
                .SetProperty(Ref("sp"), "Name", Constant(Name))
                .SetProperty(Ref("sp"), "Expression", Constant(Expression))
                .SetProperty(Ref("sp"), "ParameterNames", Constant(ParameterNames))
                .SetProperty(Ref("sp"), "OptionalReturnTypeName", Constant(OptionalReturnTypeName))
                .SetProperty(Ref("sp"), "OptionalEventTypeName", Constant(OptionalEventTypeName))
                .SetProperty(Ref("sp"), "OptionalDialect", Constant(OptionalDialect))
                .SetProperty(Ref("sp"), "ModuleName", Constant(ModuleName))
                .SetProperty(Ref("sp"), "Visibility", Constant(Visibility))
                .MethodReturn(Ref("sp"));
            return LocalMethod(method);
        }
    }
} // end of namespace