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

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue
{
    public class JsonEndValueForgeCharacter : JsonEndValueForge
    {
        public static readonly JsonEndValueForgeCharacter INSTANCE = new JsonEndValueForgeCharacter();

        private JsonEndValueForgeCharacter()
        {
        }

        public CodegenExpression CaptureValue(
            JsonEndValueRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return StaticMethod(typeof(JsonEndValueForgeCharacter), "JsonToCharacter", refs.ValueString);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>char</returns>
        public static char? JsonToCharacter(string value)
        {
            if (value == null) {
                return null;
            }

            return value[0];
        }

        public static char JsonToCharacterNonNull(string stringValue)
        {
            return stringValue[0];
        }
    }
} // end of namespace