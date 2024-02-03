///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // ref

namespace com.espertech.esper.common.@internal.@event.json.deserializers.forge
{
    public class JsonEndValueRefs
    {
        private static readonly CodegenExpression Stringvalue = Ref("stringValue");
        private static readonly CodegenExpression Objectvalue = Ref("objectValue");
        private static readonly CodegenExpression Isnumber = Ref("isNumber");
        private static readonly CodegenExpression Jsonfieldname = Ref("name");

        public static readonly JsonEndValueRefs INSTANCE = new JsonEndValueRefs(
            Stringvalue,
            Isnumber,
            Objectvalue,
            Jsonfieldname);

        private JsonEndValueRefs(
            CodegenExpression valueString,
            CodegenExpression isNumber,
            CodegenExpression valueObject,
            CodegenExpression name)
        {
            ValueString = valueString;
            IsNumber = isNumber;
            ValueObject = valueObject;
            Name = name;
        }

        public CodegenExpression ValueString { get; }

        public CodegenExpression IsNumber { get; }

        public CodegenExpression ValueObject { get; }

        public CodegenExpression Name { get; }
    }
} // end of namespace