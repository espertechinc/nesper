///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

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
            var values = record.Get(_key);
            if (IsIndexableValue(values)) {
                return GetAvroIndexedValue(values, _index);
            }

            throw new ArgumentException("eventBean contains incorrect array", nameof(eventBean));
        }

        public object GetAvroFieldValue(GenericRecord record)
        {
            var values = record.Get(_key);
            if (IsIndexableValue(values)) {
                return GetAvroIndexedValue(values, _index);
            }

            throw new ArgumentException("eventBean contains incorrect array", nameof(record));
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
            if (_fragmentEventType == null) {
                return null;
            }

            var value = GetAvroFieldValue(record);
            if (value == null) {
                return null;
            }

            return _eventAdapterService.AdapterForTypedAvro(value, _fragmentEventType);
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
            return CodegenExpressionBuilder.ConstantTrue();
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(
                CodegenExpressionBuilder.CastUnderlying(typeof(GenericRecord), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var values = CodegenExpressionBuilder.Cast(
                typeof(ICollection<object>),
                CodegenExpressionBuilder.ExprDotMethod(
                    underlyingExpression,
                    "Get",
                    CodegenExpressionBuilder.Constant(_key)));
            return CodegenExpressionBuilder.StaticMethod(
                GetType(),
                "GetAvroIndexedValue",
                values,
                CodegenExpressionBuilder.Constant(_index));
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
            if (_fragmentEventType == null) {
                return CodegenExpressionBuilder.ConstantNull();
            }

            return CodegenExpressionBuilder.LocalMethod(
                GetAvroFragmentCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="values">coll</param>
        /// <param name="index">index</param>
        /// <returns>value</returns>
        public static object GetAvroIndexedValue(
            object values,
            int index)
        {
            switch (values) {
                case null:
                    return null;

                case Array valuesArray:
                    return valuesArray.Length > index ? valuesArray.GetValue(index) : null;
            }

            IList<object> list;

            if (values is IList<object> listRecast) {
                list = listRecast;
            }
            else if (values.GetType().IsGenericList()) {
                list = MagicMarker.SingletonInstance.GetList(values);
            }
            else {
                list = CompatExtensions.UnwrapIntoList<object>(values);
            }

            return list.Count > index ? list[index] : null;
        }

        private CodegenMethod GetAvroFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var factory = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(_fragmentEventType, EPStatementInitServicesConstants.REF));
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(GenericRecord), "record")
                .Block
                .DeclareVar<object>(
                    "value",
                    UnderlyingGetCodegen(CodegenExpressionBuilder.Ref("record"), codegenMethodScope, codegenClassScope))
                .MethodReturn(
                    CodegenExpressionBuilder.ExprDotMethod(
                        factory,
                        "AdapterForTypedAvro",
                        CodegenExpressionBuilder.Ref("value"),
                        eventType));
        }
        
        /// <summary>
        /// Returns true if the value being presented can be used as an indexable value.
        /// </summary>
        /// <param name="values">the value(s) being presented.</param>
        /// <returns></returns>
        public static bool IsIndexableValue(object values)
        {
            switch (values) {
                case null:
                case Array _:
                case IList<object> _:
                    return true;
            }

            if (values.GetType().IsGenericList()) {
                return true;
            }

            return false;
        }
    }
} // end of namespace