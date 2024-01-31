///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    public class DataInputOutputSerdeForgeSkip : DataInputOutputSerdeForge
    {
        public static readonly DataInputOutputSerdeForgeSkip INSTANCE = new DataInputOutputSerdeForgeSkip();

        private DataInputOutputSerdeForgeSkip()
        {
        }

        public CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression optionalEventTypeResolver)
        {
            return PublicConstValue(typeof(DIOSkipSerde), "INSTANCE");
        }

        public string ForgeClassName => nameof(DIOSkipSerde);
    }
} // end of namespace