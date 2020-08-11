///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue
{
    public class JsonDeserializeForgeEnum : JsonDeserializeForge
    {
        private readonly Type _type;

        public JsonDeserializeForgeEnum(Type type)
        {
            this._type = type;
        }

        public CodegenExpression CodegenDeserialize(
            JsonDeserializeRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            // Generic types not yet implemented on codegen (TBD)
            return StaticMethod(
                typeof(JsonElementExtensions),
                "GetBoxedEnum",
                new Type[] {_type},
                refs.Element);
        }
    }
} // end of namespace