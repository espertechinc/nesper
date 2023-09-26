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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

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
            var und = (GenericRecord) eventBean.Underlying;
            var inner = und.Get(_posTop);
            return inner is GenericRecord;
        }

        public object GetFragment(EventBean eventBean)
        {
            if (_fragmentType == null) {
                return null;
            }

            var value = Get(eventBean);
            if (value == null) {
                return null;
            }

            return _eventAdapterService.AdapterForTypedAvro(value, _fragmentType);
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(GenericRecord), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(GenericRecord), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(
                CastUnderlying(typeof(GenericRecord), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GetCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                ExistsCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (_fragmentType == null) {
                return ConstantNull();
            }

            return LocalMethod(
                GetFragmentCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression);
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var factory = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(_fragmentType, EPStatementInitServicesConstants.REF));
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam<GenericRecord>("record")
                .Block
                .DeclareVar<object>(
                    "value",
                    UnderlyingGetCodegen(Ref("record"), codegenMethodScope, codegenClassScope))
                .IfRefNullReturnNull("value")
                .MethodReturn(
                    ExprDotMethod(
                        factory,
                        "AdapterForTypedAvro",
                        Ref("value"),
                        eventType));
        }

        private object Get(GenericRecord record)
        {
            var inner = (GenericRecord) record.Get(_posTop);
            return inner?.Get(_posInner);
        }

        private CodegenMethod GetCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam<GenericRecord>("record")
                .Block
                .DeclareVar<GenericRecord>(
                    "inner",
                    Cast(
                        typeof(GenericRecord),
                        StaticMethod(
                            typeof(GenericRecordExtensions),
                            "Get",
                            Ref("record"),
                            Constant(_posTop.Name))))
                .IfRefNullReturnNull("inner")
                .MethodReturn(
                    StaticMethod(
                        typeof(GenericRecordExtensions),
                        "Get",
                        Ref("inner"),
                        Constant(_posInner.Name)));
        }
        
        
        private CodegenMethod ExistsCodegen(CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope
                .MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam<GenericRecord>("record")
                .Block
                .DeclareVar<GenericRecord>(
                    "inner",
                    Cast(
                        typeof(GenericRecord),
                        StaticMethod(
                            typeof(GenericRecordExtensions),
                            "Get",
                            Ref("record"),
                            Constant(_posTop.Name))))
                .IfRefNullReturnFalse("inner")
                .MethodReturn(ConstantTrue());
        }
    }
} // end of namespace