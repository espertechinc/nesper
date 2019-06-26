///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterIndexed : AvroEventPropertyGetter
    {
        private readonly EventBeanTypedEventFactory _eventAdapterService;
        private readonly EventType _fragmentEventType;
        private readonly int _index;
        private readonly Field _key;

        public AvroEventBeanGetterIndexed(
            Field key,
            int index,
            EventType fragmentEventType,
            EventBeanTypedEventFactory eventAdapterService)
        {
            _key = key;
            _index = index;
            _fragmentEventType = fragmentEventType;
            _eventAdapterService = eventAdapterService;
        }

        public object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var values = (ICollection<object>) record.Get(_key);
            return GetAvroIndexedValue(values, _index);
        }

        public object GetAvroFieldValue(GenericRecord record)
        {
            var values = (ICollection<object>) record.Get(_key);
            return GetAvroIndexedValue(values, _index);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            return GetAvroFragment(record);
        }

        public object GetAvroFragment(GenericRecord record)
        {
            if (_fragmentEventType == null)
            {
                return null;
            }

            var value = GetAvroFieldValue(record);
            if (value == null)
            {
                return null;
            }

            return _eventAdapterService.AdapterForTypedAvro(value, _fragmentEventType);
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(CodegenExpressionBuilder.CastUnderlying(typeof(GenericRecord), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.ConstantTrue();
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(CodegenExpressionBuilder.CastUnderlying(typeof(GenericRecord), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var values = CodegenExpressionBuilder.Cast(typeof(ICollection<object>), CodegenExpressionBuilder.ExprDotMethod(underlyingExpression, "get", CodegenExpressionBuilder.Constant(_key)));
            return CodegenExpressionBuilder.StaticMethod(GetType(), "getAvroIndexedValue", values, CodegenExpressionBuilder.Constant(_index));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.ConstantTrue();
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (_fragmentEventType == null)
            {
                return CodegenExpressionBuilder.ConstantNull();
            }

            return CodegenExpressionBuilder.LocalMethod(GetAvroFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="values">coll</param>
        /// <param name="index">index</param>
        /// <returns>value</returns>
        public static object GetAvroIndexedValue(
            ICollection<object> values,
            int index)
        {
            if (values == null)
            {
                return null;
            }

            if (values is IList<object>)
            {
                var list = (IList<object>) values;
                return list.Count > index ? list[index] : null;
            }

            return values.ToArray()[index];
        }

        private CodegenMethod GetAvroFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var factory = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddFieldUnshared(
                true, typeof(EventType), EventTypeUtility.ResolveTypeCodegen(_fragmentEventType, EPStatementInitServicesConstants.REF));
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope).AddParam(typeof(GenericRecord), "record").Block
                .DeclareVar(typeof(object), "value", UnderlyingGetCodegen(CodegenExpressionBuilder.Ref("record"), codegenMethodScope, codegenClassScope))
                .MethodReturn(CodegenExpressionBuilder.ExprDotMethod(factory, "adapterForTypedAvro", CodegenExpressionBuilder.Ref("value"), eventType));
        }
    }
} // end of namespace