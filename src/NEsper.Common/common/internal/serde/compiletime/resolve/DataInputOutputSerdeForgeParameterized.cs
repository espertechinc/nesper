///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static
    com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // newInstance;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    public class DataInputOutputSerdeForgeParameterized : DataInputOutputSerdeForge
    {
        private readonly string dioClassName;
        private readonly Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression>[] functions;

        public DataInputOutputSerdeForgeParameterized(
            string dioClassName,
            params Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression>[] functions)
        {
            this.dioClassName = dioClassName;
            this.functions = functions;
        }

        public string ForgeClassName => dioClassName;

        public CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression optionalEventTypeResolver)
        {
            var @params = new CodegenExpression[functions.Length];
            var vars = new DataInputOutputSerdeForgeParameterizedVars(method, classScope, optionalEventTypeResolver);
            for (var i = 0; i < @params.Length; i++) {
                @params[i] = functions[i].Invoke(vars);
            }

            return NewInstanceInner(dioClassName, @params);
        }
    }
} // end of namespace