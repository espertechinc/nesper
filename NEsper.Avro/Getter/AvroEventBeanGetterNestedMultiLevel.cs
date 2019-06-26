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
    public class AvroEventBeanGetterNestedMultiLevel : EventPropertyGetterSPI
    {
        private readonly EventBeanTypedEventFactory _eventAdapterService;
        private readonly EventType _fragmentEventType;
        private readonly Field[] _path;
        private readonly Field _top;

        public AvroEventBeanGetterNestedMultiLevel(
            Field top,
            Field[] path,
            EventType fragmentEventType,
            EventBeanTypedEventFactory eventAdapterService)
        {
            _top = top;
            _path = path;
            _fragmentEventType = fragmentEventType;
            _eventAdapterService = eventAdapterService;
        }

        public object Get(EventBean eventBean)
        {
            return GetRecordValueTopWPath((GenericRecord) eventBean.Underlying, _top, _path);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            if (_fragmentEventType == null)
            {
                return null;
            }

            var value = Get(eventBean);
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
            return CodegenExpressionBuilder.StaticMethod(
                typeof(AvroEventBeanGetterNestedMultiLevel), "getRecordValueTopWPath", underlyingExpression, CodegenExpressionBuilder.Constant(_top), CodegenExpressionBuilder.Constant(_path));
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
            return CodegenExpressionBuilder.LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="record">record</param>
        /// <param name="top">top index</param>
        /// <param name="path">path of indexes</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException property access problem</throws>
        public static object GetRecordValueTopWPath(
            GenericRecord record,
            Field top,
            Field[] path)
        {
            var inner = (GenericRecord) record.Get(top);
            if (inner == null)
            {
                return null;
            }

            for (var i = 0; i < path.Length - 1; i++)
            {
                inner = (GenericRecord) inner.Get(path[i]);
                if (inner == null)
                {
                    return null;
                }
            }

            return inner.Get(path[path.Length - 1]);
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var factory = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddFieldUnshared(
                true, typeof(EventType), EventTypeUtility.ResolveTypeCodegen(_fragmentEventType, EPStatementInitServicesConstants.REF));
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope).AddParam(typeof(GenericRecord), "record").Block
                .DeclareVar(typeof(object), "value", UnderlyingGetCodegen(CodegenExpressionBuilder.Ref("record"), codegenMethodScope, codegenClassScope))
                .IfRefNullReturnNull("value")
                .MethodReturn(CodegenExpressionBuilder.ExprDotMethod(factory, "adapterForTypedAvro", CodegenExpressionBuilder.Ref("value"), eventType));
        }
    }
} // end of namespace