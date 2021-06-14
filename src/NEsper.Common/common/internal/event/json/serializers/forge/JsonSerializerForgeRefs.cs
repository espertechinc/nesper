///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.@event.json.serializers.forge
{
    public class JsonSerializerForgeRefs
    {
        public JsonSerializerForgeRefs(
            CodegenExpression context,
            CodegenExpression field,
            CodegenExpression name)
        {
            Context = context;
            Field = field;
            Name = name;
        }

        /// <summary>
        /// Returns the expression that yields the JsonSerializationContext. 
        /// </summary>
        public CodegenExpression Context { get; }

        public CodegenExpression Field { get; }

        public CodegenExpression Name { get; }
    }
} // end of namespace