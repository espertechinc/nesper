///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.serde.serdeset.additional;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    public class DataInputOutputSerdeForgeMap : DataInputOutputSerdeForge
    {
        private readonly string[] keys;
        private readonly DataInputOutputSerdeForge[] valueSerdes;

        public DataInputOutputSerdeForgeMap(
            string[] keys,
            DataInputOutputSerdeForge[] valueSerdes)
        {
            this.keys = keys;
            this.valueSerdes = valueSerdes;
        }

        public string ForgeClassName => nameof(DIOMapPropertySerde);

        public CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression optionalEventTypeResolver)
        {
            return NewInstance(
                typeof(DIOMapPropertySerde),
                Constant(keys),
                DataInputOutputSerdeForgeExtensions.CodegenArray(
                    valueSerdes,
                    method,
                    classScope,
                    optionalEventTypeResolver));
        }

        public string[] Keys => keys;

        public DataInputOutputSerdeForge[] ValueSerdes => valueSerdes;
    }
} // end of namespace