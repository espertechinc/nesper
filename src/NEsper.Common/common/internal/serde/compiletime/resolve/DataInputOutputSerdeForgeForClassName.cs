///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    public class DataInputOutputSerdeForgeForClassName : DataInputOutputSerdeForge
    {
        public DataInputOutputSerdeForgeForClassName(string className)
        {
            ClassName = className;
        }

        public string ClassName { get; }

        public string ForgeClassName => ClassName;

        public CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression optionalEventTypeResolver)
        {
            return CodegenExpressionBuilder.NewInstanceInner(ClassName, optionalEventTypeResolver);
        }
    }
}