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
            if (value == null || !(value is IDictionary<string, object>)) {
                return null;
            }

            return AvroEventBeanGetterMapped.GetAvroMappedValueWNullCheck((IDictionary<string, object>) value, _key);
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
            return IsAvroFieldExists(record, _propertyName);
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
                "IsAvroFieldExists",
                underlyingExpression,
                CodegenExpressionBuilder.Constant(_propertyName));
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.ConstantNull();
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
            if (value == null || !(value is IDictionary<string, object>)) {
                return null;
            }

            return AvroEventBeanGetterMapped.GetAvroMappedValueWNullCheck((IDictionary<string, object>) value, key);
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
            return value == null || value is IDictionary<string, object>;
        }
    }
} // end of namespace