///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // newInstance

// staticMethod

namespace com.espertech.esper.common.@internal.@event.json.write
{
    public class JsonWriteForgeAppClass : JsonWriteForge
    {
        private readonly string delegateFactoryClassName;
        private readonly string writeMethodName;

        public JsonWriteForgeAppClass(
            string delegateFactoryClassName,
            string writeMethodName)
        {
            this.delegateFactoryClassName = delegateFactoryClassName;
            this.writeMethodName = writeMethodName;
        }

        public CodegenExpression CodegenWrite(
            JsonWriteForgeRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return StaticMethod(typeof(JsonWriteUtil), writeMethodName, refs.Writer, refs.Field, NewInstance(delegateFactoryClassName));
        }
    }
} // end of namespace