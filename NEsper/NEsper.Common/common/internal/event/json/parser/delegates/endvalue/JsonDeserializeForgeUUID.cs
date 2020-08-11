///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // staticMethod

// handleParseException

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue
{
    public class JsonDeserializeForgeUUID : JsonDeserializeForge
    {
        public static readonly JsonDeserializeForgeUUID INSTANCE = new JsonDeserializeForgeUUID();

        private JsonDeserializeForgeUUID()
        {
        }

        public CodegenExpression CodegenDeserialize(
            JsonDeserializeRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return ExprDotMethod(refs.Element, "GetBoxedUuid");
        }
    }
} // end of namespace