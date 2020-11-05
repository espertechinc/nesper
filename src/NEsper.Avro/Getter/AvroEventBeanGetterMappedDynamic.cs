///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterMappedDynamic : AvroEventPropertyGetter
    {
        private readonly string _key;
        private readonly string _propertyName;

        public AvroEventBeanGetterMappedDynamic(
            string propertyName,
            string key)
        {
            _propertyName = propertyName;
            _key = key;
        }

        public object GetAvroFieldValue(GenericRecord record)
        {
            var value = record.Get(_propertyName);
            if (value == null) {
                return null;
            }

            var stringDictionary = GetUnderlyingMap(value);
            if (stringDictionary != null) {
                return AvroEventBeanGetterMapped.GetAvroMappedValueWNullCheck(stringDictionary, _key);
            }

            return null;
        }

        public object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            return GetAvroFieldValue(record);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsExistsPropertyAvro((GenericRecord) eventBean.Underlying);
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            return IsExistsPropertyAvro(record, _propertyName, _key);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public object GetAvroFragment(GenericRecord record)
        {
            return null;
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CodegenExpressionBuilder.CastUnderlying(typeof(GenericRecord), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CodegenExpressionBuilder.CastUnderlying(typeof(GenericRecord), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.ConstantNull();
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.StaticMethod(
                GetType(),
                "GetAvroFieldValue",
                underlyingExpression,
                CodegenExpressionBuilder.Constant(_propertyName),
                CodegenExpressionBuilder.Constant(_key));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.StaticMethod(
                GetType(),
                "IsExistsPropertyAvro",
                underlyingExpression,
                CodegenExpressionBuilder.Constant(_propertyName),
                CodegenExpressionBuilder.Constant(_key));
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.ConstantNull();
        }

        public static IDictionary<string, object> GetUnderlyingMap(object value)
        {
            if (value == null) {
                return null;
            }

            if (value is IDictionary<string, object> valueMap) {
                return valueMap;
            }
            
            var valueType = value.GetType();
            if (valueType.IsGenericStringDictionary()) {
                var magicMap = MagicMarker.SingletonInstance.GetStringDictionaryFactory(valueType).Invoke(value);
                if (magicMap != null) {
                    return magicMap;
                }
            }

            return null;
        }
        
        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="record">record</param>
        /// <param name="propertyName">property</param>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        public static object GetAvroFieldValue(
            GenericRecord record,
            string propertyName,
            string key)
        {
            var value = record.Get(propertyName);
            var valueMap = GetUnderlyingMap(value);
            if (valueMap == null) {
                return null;
            }

            return AvroEventBeanGetterMapped.GetAvroMappedValueWNullCheck(valueMap, key);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="record">record</param>
        /// <param name="propertyName">property</param>
        /// <returns>value</returns>
        public static bool IsAvroFieldExists(
            GenericRecord record,
            string propertyName)
        {
            var field = record.Schema.GetField(propertyName);
            if (field == null) {
                return false;
            }

            var value = record.Get(propertyName);
            var valueMap = GetUnderlyingMap(value);
            return valueMap != null;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="record">the record (row) containing data</param>
        /// <param name="propertyName">property to evaluate</param>
        /// <param name="key">key within the map to test</param>
        /// <returns></returns>

        public static bool IsExistsPropertyAvro(
            GenericRecord record,
            string propertyName,
            string key)
        {
            var value = record.Get(propertyName);
            var valueMap = GetUnderlyingMap(value);
            if (valueMap == null) {
                return false;
            }

            return valueMap.ContainsKey(key);
        }
    }
} // end of namespace