///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.forge
{
    public class JsonDeserializeRefs
    {
        private static readonly CodegenExpression JsonElement = Ref("jsonElement");

        public static readonly JsonDeserializeRefs INSTANCE = new JsonDeserializeRefs(JsonElement);

        private JsonDeserializeRefs(CodegenExpression jsonElement)
        {
            Element = jsonElement;
        }

        public string ElementName => "jsonElement";
        public CodegenExpression Element { get; }
    }
} // end of namespace