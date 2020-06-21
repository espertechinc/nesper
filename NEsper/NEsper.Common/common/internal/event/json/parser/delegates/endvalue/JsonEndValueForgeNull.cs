///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue
{
    public class JsonEndValueForgeNull : JsonEndValueForge
    {
        public static readonly JsonEndValueForgeNull INSTANCE = new JsonEndValueForgeNull();

        private JsonEndValueForgeNull()
        {
        }

        public CodegenExpression CaptureValue(
            JsonEndValueRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            throw new UnsupportedOperationException("Not supported for null-typed column");
        }
    }
} // end of namespace