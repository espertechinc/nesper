///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.map
{
    public class MapEventBeanPropertyWriterIndexedProp : MapEventBeanPropertyWriter
    {
        private readonly int _index;

        public MapEventBeanPropertyWriterIndexedProp(
            string propertyName,
            int index)
            : base(propertyName)
        {
            _index = index;
        }

        public override void Write(
            object value,
            IDictionary<string, object> map)
        {
            var arrayEntry = map.Get(propertyName) as Array;
            if (arrayEntry != null && arrayEntry.Length > _index) {
                arrayEntry.SetValue(value, _index);
            }
        }
        
        public override CodegenExpression WriteCodegen(
            CodegenExpression assigned, 
            CodegenExpression underlying,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return CodegenExpressionBuilder.StaticMethod(
                this.GetBoxedType(),
                "MapWriteSetArrayProp", 
                CodegenExpressionBuilder.Constant(propertyName),
                CodegenExpressionBuilder.Constant(_index),
                underlying,
                assigned);
        }
        
        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="index"></param>
        /// <param name="map"></param>
        /// <param name="value"></param>
        
        public static void MapWriteSetArrayProp(
            string propertyName,
            int index,
            IDictionary<string, object> map,
            object value)
        {
            var mapValue = map.Get(propertyName);
            if (mapValue is Array arrayValue && arrayValue.Length > index) {
                arrayValue.SetValue(value, index);
            }
        }
    }
}