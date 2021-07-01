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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.map
{
    public class MapEventBeanPropertyWriterMapProp : MapEventBeanPropertyWriter
    {
        private readonly string _key;

        public MapEventBeanPropertyWriterMapProp(
            string propertyName,
            string key)
            : base(propertyName)
        {
            _key = key;
        }

        public override void Write(
            object value,
            IDictionary<string, object> map)
        {
            var mapEntry = (IDictionary<string, object>) map.Get(propertyName);
            mapEntry?.Put(_key, value);
        }
        
        public override CodegenExpression WriteCodegen(
            CodegenExpression assigned, 
            CodegenExpression underlying,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return CodegenExpressionBuilder.StaticMethod(
                this.GetBoxedType(), "MapWriteSetMapProp", 
                CodegenExpressionBuilder.Constant(propertyName),
                CodegenExpressionBuilder.Constant(_key),
                underlying,
                assigned);
        }
        
        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="key"></param>
        /// <param name="map"></param>
        /// <param name="value"></param>
        
        public static void MapWriteSetMapProp(
            string propertyName,
            string key,
            IDictionary<string, object> map,
            object value)
        {
            var mapEntry = (IDictionary<string, object>) map.Get(propertyName);
            if (mapEntry != null) {
                mapEntry[key] = value;
            }
        }
    }
}