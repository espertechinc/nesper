///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace NEsper.Avro.Getter
{
    using Map = IDictionary<string, object>;

    public class AvroEventBeanGetterMappedDynamic : AvroEventPropertyGetter
    {
        private readonly string _key;
        private readonly string _propertyName;

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="record">record</param>
        /// <param name="propertyName">property</param>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        public static Object GetAvroFieldValue(GenericRecord record, string propertyName, string key)
        {
            var value = record.Get(propertyName);
            if (!(value is Map valueAsMap))
            {
                return null;
            }
            return AvroEventBeanGetterMapped.GetAvroMappedValueWNullCheck(valueAsMap, key);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="record">record</param>
        /// <param name="propertyName">property</param>
        /// <returns>value</returns>
        public static bool IsAvroFieldExists(GenericRecord record, string propertyName)
        {
            var field = record.Schema.GetField(propertyName);
            if (field == null)
            {
                return false;
            }
            Object value = record.Get(propertyName);
            return value == null || value is Map;
        }

        public AvroEventBeanGetterMappedDynamic(string propertyName, string key)
        {
            _propertyName = propertyName;
            _key = key;
        }

        public Object GetAvroFieldValue(GenericRecord record)
        {
            Object value = record.Get(_propertyName);
            if (value == null || !(value is Map))
            {
                return null;
            }
            return AvroEventBeanGetterMapped.GetAvroMappedValueWNullCheck((Map) value, _key);
        }

        public Object Get(EventBean eventBean)
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

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public Object GetAvroFragment(GenericRecord record)
        {
            return null;
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(GenericRecord), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(GenericRecord), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(GetType(), "getAvroFieldValue", underlyingExpression, _propertyName, _key);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(GetType(), "isAvroFieldExists", underlyingExpression, _propertyName);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantNull();
        }
    }
} // end of namespace