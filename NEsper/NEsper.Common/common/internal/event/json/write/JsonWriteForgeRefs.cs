///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.@event.json.write
{
    public class JsonWriteForgeRefs
    {
        public JsonWriteForgeRefs(
            CodegenExpression writer,
            CodegenExpression field,
            CodegenExpression name)
        {
            Writer = writer;
            Field = field;
            Name = name;
        }

        public CodegenExpression Writer { get; }

        public CodegenExpression Field { get; }

        public CodegenExpression Name { get; }
    }
} // end of namespace