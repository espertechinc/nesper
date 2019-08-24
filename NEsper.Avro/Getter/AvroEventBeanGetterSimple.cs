///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

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
    public class AvroEventBeanGetterSimple : AvroEventPropertyGetter
    {
        private readonly EventBeanTypedEventFactory _eventAdapterService;
        private readonly EventType _fragmentType;
        private readonly Field _propertyIndex;
        private readonly Type _propertyType;

        public AvroEventBeanGetterSimple(
            Field propertyIndex,
            EventType fragmentType,
            EventBeanTypedEventFactory eventAdapterService,
            Type propertyType)
        {
            _propertyIndex = propertyIndex;
            _fragmentType = fragmentType;
            _eventAdapterService = eventAdapterService;
            _propertyType = propertyType;
        }

        public object GetAvroFieldValue(GenericRecord record)
        {
            return record.Get(_propertyIndex);
        }

        public object Get(EventBean theEvent)
        {
            return GetAvroFieldValue((GenericRecord) theEvent.Underlying);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            return true;
        }

        public object GetFragment(EventBean obj)
        {
            var value = Get(obj);
            return GetFragmentAvro(value, _eventAdapterService, _fragmentType);
        }

        public object GetAvroFragment(GenericRecord record)
        {
            var value = GetAvroFieldValue(record);
            return GetFragmentAvro(value, _eventAdapterService, _fragmentType);
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
            return CodegenExpressionBuilder.Cast(
                _propertyType,
                CodegenExpressionBuilder.StaticMethod(
                    typeof(GenericRecordExtensions),
                    "Get",
                    underlyingExpression,
                    CodegenExpressionBuilder.Constant(_propertyIndex.Name)));
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
            if (_fragmentType == null) {
                return CodegenExpressionBuilder.ConstantNull();
            }

            return CodegenExpressionBuilder.LocalMethod(
                GetAvroFragmentCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="eventAdapterService">svc</param>
        /// <param name="fragmentType">type</param>
        /// <returns>fragment</returns>
        public static object GetFragmentAvro(
            object value,
            EventBeanTypedEventFactory eventAdapterService,
            EventType fragmentType)
        {
            if (fragmentType == null) {
                return null;
            }

            if (value is GenericRecord) {
                return eventAdapterService.AdapterForTypedAvro(value, fragmentType);
            }

            if (value is ICollection<object>) {
                var coll = (ICollection<object>) value;
                var events = new EventBean[coll.Count];
                var index = 0;
                foreach (var item in coll) {
                    events[index++] = eventAdapterService.AdapterForTypedAvro(item, fragmentType);
                }

                return events;
            }

            return null;
        }

        private CodegenMethod GetAvroFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var factory = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var type = codegenClassScope.AddFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(_fragmentType, EPStatementInitServicesConstants.REF));
            return codegenMethodScope
                .MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(GenericRecord), "record")
                .Block
                .DeclareVar<object>(
                    "value",
                    UnderlyingGetCodegen(CodegenExpressionBuilder.Ref("record"), codegenMethodScope, codegenClassScope))
                .MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        GetType(),
                        "GetFragmentAvro",
                        CodegenExpressionBuilder.Ref("value"),
                        factory,
                        type));
        }
    }
} // end of namespace