///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Avro.Generic;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterIndexedDynamic : AvroEventPropertyGetter
    {
        private readonly string _propertyName;
        private readonly int _index;

        public static Object GetAvroFieldValue(GenericRecord record, String propertyName, int index)
        {
            var value = record.Get(propertyName);
            if (value is Array valueArray)
            {
                return AvroEventBeanGetterIndexed.GetIndexedValue(valueArray, index);
            }

            if (value is IEnumerable<object> valueGenericEnum)
            {
                return AvroEventBeanGetterIndexed.GetIndexedValue(valueGenericEnum, index);
            }

            if (value is IEnumerable valueEnum)
            {
                return AvroEventBeanGetterIndexed.GetIndexedValue(valueEnum.Cast<object>(), index);
            }

            return null;
        }

        public static bool IsAvroFieldExists(GenericRecord record, string propertyName)
        {
            var field = record.Schema.GetField(propertyName);
            if (field == null)
            {
                return false;
            }
            var value = record.Get(propertyName);
            return (value == null)
                   || (value is Array)
                   || (value is IEnumerable<object>)
                   || (value is IEnumerable);
        }

        public AvroEventBeanGetterIndexedDynamic(string propertyName, int index)
        {
            _propertyName = propertyName;
            _index = index;
        }

        public Object GetAvroFieldValue(GenericRecord record)
        {
            return GetAvroFieldValue(record, _propertyName, _index);
        }

        public Object Get(EventBean eventBean)
        {
            GenericRecord record = (GenericRecord) eventBean.Underlying;
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
            return StaticMethodTakingExprAndConst(GetType(), "GetAvroFieldValue", underlyingExpression, _propertyName, _index);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethodTakingExprAndConst(GetType(), "IsAvroFieldExists", underlyingExpression, _propertyName);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

    }
} // end of namespace
