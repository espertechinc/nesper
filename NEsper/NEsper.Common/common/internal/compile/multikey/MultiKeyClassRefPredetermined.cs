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
        private readonly Type _clazzMK;
        private readonly DataInputOutputSerdeForge _serdeForge;

        public MultiKeyClassRefPredetermined(
            Type clazzMK,
            Type[] mkTypes,
            DataInputOutputSerdeForge serdeForge)
        {
            _clazzMK = clazzMK;
            MKTypes = mkTypes;
            _serdeForge = serdeForge;
        }

        public NameOrType ClassNameMK => new NameOrType(_clazzMK);

        public CodegenExpression GetExprMKSerde(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return _serdeForge.Codegen(method, classScope, null);
        }

        public Type[] MKTypes { get; }
    }
} // end of namespace