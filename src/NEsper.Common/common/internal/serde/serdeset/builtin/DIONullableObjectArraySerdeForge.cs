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
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
    public class DIONullableObjectArraySerdeForge : DataInputOutputSerdeForge
    {
        private readonly Type componentType;
        private readonly DataInputOutputSerdeForge componentSerde;

        public DIONullableObjectArraySerdeForge(
            Type componentType,
            DataInputOutputSerdeForge componentSerde)
        {
            this.componentType = componentType;
            this.componentSerde = componentSerde;
        }

        public string ForgeClassName => nameof(DIONullableObjectArraySerde);

        public CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression optionalEventTypeResolver)
        {
            return NewInstance(
                typeof(DIONullableObjectArraySerde),
                Constant(componentType),
                componentSerde.Codegen(method, classScope, optionalEventTypeResolver));
        }

        public Type ComponentType => componentType;

        public DataInputOutputSerdeForge ComponentSerde => componentSerde;
    }
} // end of namespace