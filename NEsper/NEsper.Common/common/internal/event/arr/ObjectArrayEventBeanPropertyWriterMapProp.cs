///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    public class ObjectArrayEventBeanPropertyWriterMapProp : ObjectArrayEventBeanPropertyWriter
    {
        private readonly string key;

        public ObjectArrayEventBeanPropertyWriterMapProp(
            int propertyIndex,
            string key)
            : base(propertyIndex)
        {
            this.key = key;
        }

        public override void Write(
            object value,
            object[] array)
        {
            ObjectArrayWriteMapProp(value, array, index, key);
        }

        public override CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression und,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return StaticMethod(
                typeof(ObjectArrayEventBeanPropertyWriterMapProp),
                "ObjectArrayWriteMapProp",
                assigned,
                und,
                Constant(index),
                Constant(key));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="array">underlying</param>
        /// <param name="index">index</param>
        /// <param name="key">key</param>
        public static void ObjectArrayWriteMapProp(
            object value,
            object[] array,
            int index,
            string key)
        {
            var mapEntry = (IDictionary<string, object>) array[index];
            if (mapEntry != null) {
                mapEntry.Put(key, value);
            }
        }
    }
} // end of namespace