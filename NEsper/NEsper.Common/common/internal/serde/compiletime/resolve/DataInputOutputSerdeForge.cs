///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; //.constantNull;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; //.newArrayWithInit;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    /// <summary>
    /// Encapsulates information on what serde should be used, for byte code production.
    /// Byte code production produces the equivalent <seealso cref="com.espertech.esper.common.client.serde.DataInputOutputSerde" />.
    /// </summary>
    public interface DataInputOutputSerdeForge
    {
        static CodegenExpression CodegenArray(
            DataInputOutputSerdeForge[] serdes,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression optionalEventTypeResolver)
        {
            if (serdes == null) {
                return ConstantNull();
            }

            CodegenExpression[] expressions = new CodegenExpression[serdes.Length];
            for (int i = 0; i < serdes.Length; i++) {
                expressions[i] = serdes[i].Codegen(method, classScope, optionalEventTypeResolver);
            }

            return NewArrayWithInit(typeof(DataInputOutputSerde), expressions);
        }

        String ForgeClassName();

        CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression optionalEventTypeResolver);
    }
}