///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    public class DataInputOutputSerdeForgeOfForges : DataInputOutputSerdeForge
    {
        private readonly string forgeClassName;
        private readonly DataInputOutputSerdeForge[] forges;

        public DataInputOutputSerdeForgeOfForges(
            string forgeClassName,
            DataInputOutputSerdeForge[] forges)
        {
            this.forgeClassName = forgeClassName;
            this.forges = forges;
        }

        public string ForgeClassName => forgeClassName;

        public CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression optionalEventTypeResolver)
        {
            return NewInstanceInner(forgeClassName, optionalEventTypeResolver);
        }

        public DataInputOutputSerdeForge[] Forges => forges;
    }
} // end of namespace