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

namespace com.espertech.esper.common.@internal.compile.multikey
{
    public class MultiKeyClassRefPredetermined : MultiKeyClassRef
    {
        private readonly Type clazzMK;
        private readonly DataInputOutputSerdeForge serdeForge;

        public MultiKeyClassRefPredetermined(
            Type clazzMK,
            Type[] mkTypes,
            DataInputOutputSerdeForge serdeForge)
        {
            this.clazzMK = clazzMK;
            MKTypes = mkTypes;
            this.serdeForge = serdeForge;
        }

        public string ClassNameMK => clazzMK.Name;

        public CodegenExpression GetExprMKSerde(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return serdeForge.Codegen(method, classScope, null);
        }

        public Type[] MKTypes { get; }
    }
} // end of namespace