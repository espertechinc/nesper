///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedIndexed : EventPropertyGetterSPI
    {
        private readonly EventBeanTypedEventFactory _eventAdapterService;
        private readonly EventType _fragmentEventType;
        private readonly int _index;
        private readonly Field _pos;
        private readonly Field _top;

        public AvroEventBeanGetterNestedIndexed(
            Field top,
            Field pos,
            int index,
            EventType fragmentEventType,
            EventBeanTypedEventFactory eventAdapterService)
        {
            _top = top;
            _pos = pos;
            _index = index;
            _fragmentEventType = fragmentEventType;
            _eventAdapterService = eventAdapterService;
        }

        public object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_top);
            if (inner == null) {
                return null;
            }

            var collection = inner.Get(_pos);
            return AvroEventBeanGetterIndexed.GetAvroIndexedValue(collection, _index);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            if (_fragmentEventType == null) {
                return null;
            }

            var value = Get(eventBean);
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
            return CodegenExpressionBuilder.LocalMethod(
                GetCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression);
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
                GetFragmentCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression);
        }

        private CodegenMethod GetCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(GenericRecord), "record")
                .Block
                .DeclareVar<GenericRecord>(
                    "inner",
                    CodegenExpressionBuilder.Cast(
                        typeof(GenericRecord),
                        CodegenExpressionBuilder.ExprDotMethod(
                            CodegenExpressionBuilder.Ref("record"),
                            "Get",
                            CodegenExpressionBuilder.Constant(_top))))
                .IfRefNullReturnNull("inner")
                .DeclareVar<object>(
                    "collection",
                    CodegenExpressionBuilder.ExprDotMethod(
                        CodegenExpressionBuilder.Ref("inner"),
                        "Get",
                        CodegenExpressionBuilder.Constant(_pos)))
                .MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(AvroEventBeanGetterIndexed),
                        "GetAvroIndexedValue",
                        CodegenExpressionBuilder.Ref("collection"),
                        CodegenExpressionBuilder.Constant(_index)));
        }

        private CodegenMethod GetFragmentCodegen(
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
                .IfRefNullReturnNull("value")
                .MethodReturn(
                    CodegenExpressionBuilder.ExprDotMethod(
                        factory,
                        "AdapterForTypedAvro",
                        CodegenExpressionBuilder.Ref("value"),
                        eventType));
        }
    }
} // end of namespace