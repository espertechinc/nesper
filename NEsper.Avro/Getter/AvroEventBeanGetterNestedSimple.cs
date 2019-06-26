///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
    public class AvroEventBeanGetterNestedSimple : EventPropertyGetterSPI
    {
        private readonly EventBeanTypedEventFactory _eventAdapterService;
        private readonly EventType _fragmentType;
        private readonly Field _posInner;
        private readonly Field _posTop;

        public AvroEventBeanGetterNestedSimple(
            Field posTop,
            Field posInner,
            EventType fragmentType,
            EventBeanTypedEventFactory eventAdapterService)
        {
            _posTop = posTop;
            _posInner = posInner;
            _fragmentType = fragmentType;
            _eventAdapterService = eventAdapterService;
        }

        public object Get(EventBean eventBean)
        {
            return Get((GenericRecord) eventBean.Underlying);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            if (_fragmentType == null)
            {
                return null;
            }

            var value = Get(eventBean);
            if (value == null)
            {
                return null;
            }

            return _eventAdapterService.AdapterForTypedAvro(value, _fragmentType);
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
            return CodegenExpressionBuilder.LocalMethod(GetCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
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
            if (_fragmentType == null)
            {
                return CodegenExpressionBuilder.ConstantNull();
            }

            return CodegenExpressionBuilder.LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var factory = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddFieldUnshared(
                true, typeof(EventType), EventTypeUtility.ResolveTypeCodegen(_fragmentType, EPStatementInitServicesConstants.REF));
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope).AddParam(typeof(GenericRecord), "record").Block
                .DeclareVar(typeof(object), "value", UnderlyingGetCodegen(CodegenExpressionBuilder.Ref("record"), codegenMethodScope, codegenClassScope))
                .IfRefNullReturnNull("value")
                .MethodReturn(CodegenExpressionBuilder.ExprDotMethod(factory, "adapterForTypedAvro", CodegenExpressionBuilder.Ref("value"), eventType));
        }

        private object Get(GenericRecord record)
        {
            var inner = (GenericRecord) record.Get(_posTop);
            if (inner == null)
            {
                return null;
            }

            return inner.Get(_posInner);
        }

        private CodegenMethod GetCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope).AddParam(typeof(GenericRecord), "record").Block
                .DeclareVar(typeof(GenericRecord), "inner", CodegenExpressionBuilder.Cast(typeof(GenericRecord), CodegenExpressionBuilder.ExprDotMethod(CodegenExpressionBuilder.Ref("record"), "get", CodegenExpressionBuilder.Constant(_posTop))))
                .IfRefNullReturnNull("inner")
                .MethodReturn(CodegenExpressionBuilder.ExprDotMethod(CodegenExpressionBuilder.Ref("inner"), "get", CodegenExpressionBuilder.Constant(_posInner)));
        }
    }
} // end of namespace