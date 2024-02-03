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
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.serdeset.multikey;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.compile.multikey
{
    public class MultiKeyClassRefPredetermined : MultiKeyClassRef
    {
        private readonly Type clazzMK;
        private readonly Type[] mkTypes;
        private readonly DataInputOutputSerdeForge serdeForge;
        private readonly DIOMultiKeyArraySerde mkSerde;

        public MultiKeyClassRefPredetermined(
            Type clazzMK,
            Type[] mkTypes,
            DataInputOutputSerdeForge serdeForge,
            DIOMultiKeyArraySerde mkSerde)
        {
            this.clazzMK = clazzMK;
            this.mkTypes = mkTypes;
            this.serdeForge = serdeForge;
            this.mkSerde = mkSerde;
        }

        public CodegenExpression GetExprMKSerde(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return serdeForge.Codegen(method, classScope, null);
        }

        public T Accept<T>(MultiKeyClassRefVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public NameOrType ClassNameMK => new NameOrType(clazzMK);

        public Type[] MKTypes => mkTypes;

        public DIOMultiKeyArraySerde MkSerde => mkSerde;

        public DataInputOutputSerdeForge[] SerdeForges {
            get {
                return new DataInputOutputSerdeForge[] {
                    serdeForge
                };
            }
        }
    }
} // end of namespace