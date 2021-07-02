///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    public class ObjectArrayEventBeanPropertyWriterIndexedProp : ObjectArrayEventBeanPropertyWriter
    {
        private readonly int _indexTarget;

        public ObjectArrayEventBeanPropertyWriterIndexedProp(
            int propertyIndex,
            int indexTarget)
            : base(propertyIndex)
        {
            this._indexTarget = indexTarget;
        }

        public override void Write(
            object value,
            object[] array)
        {
            ObjectArrayWriteIndexedProp(value, array, index, _indexTarget);
        }

        public override CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression underlying,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return StaticMethod(
                typeof(ObjectArrayEventBeanPropertyWriterIndexedProp),
                "ObjectArrayWriteIndexedProp",
                assigned,
                underlying,
                Constant(index),
                Constant(_indexTarget));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="array">underlying</param>
        /// <param name="index">from</param>
        /// <param name="indexTarget">to</param>
        public static void ObjectArrayWriteIndexedProp(
            object value,
            object[] array,
            int index,
            int indexTarget)
        {
            var arrayEntry = array[index];
            if (arrayEntry != null && arrayEntry is Array arrayEntryArray && arrayEntryArray.Length > indexTarget) {
                arrayEntryArray.SetValue(value, indexTarget);
            }
        }
    }
} // end of namespace