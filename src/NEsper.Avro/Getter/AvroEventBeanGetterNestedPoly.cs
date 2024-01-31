///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.@event.core;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedPoly : EventPropertyGetterSPI
    {
        private readonly AvroEventPropertyGetter[] _getters;
        private readonly Field _top;

        public AvroEventBeanGetterNestedPoly(
            Field top,
            AvroEventPropertyGetter[] getters)
        {
            _top = top;
            _getters = getters;
        }

        public object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_top);
            return AvroEventBeanGetterDynamicPoly.GetAvroFieldValuePoly(inner, _getters);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_top);
            return AvroEventBeanGetterDynamicPoly.GetAvroFieldValuePolyExists(inner, _getters);
        }

        public object GetFragment(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_top);
            return AvroEventBeanGetterDynamicPoly.GetAvroFieldFragmentPoly(inner, _getters);
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
            return CodegenExpressionBuilder.LocalMethod(
                AvroEventBeanGetterDynamicPoly.GetAvroFieldValuePolyCodegen(
                    codegenMethodScope,
                    codegenClassScope,
                    _getters),
                CodegenExpressionBuilder.Cast(
                    typeof(GenericRecord),
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(GenericRecordExtensions),
                        "Get",
                        underlyingExpression,
                        CodegenExpressionBuilder.Constant(_top.Name))));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.LocalMethod(
                AvroEventBeanGetterDynamicPoly.GetAvroFieldValuePolyExistsCodegen(
                    codegenMethodScope,
                    codegenClassScope,
                    _getters),
                CodegenExpressionBuilder.Cast(
                    typeof(GenericRecord),
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(GenericRecordExtensions),
                        "Get",
                        underlyingExpression,
                        CodegenExpressionBuilder.Constant(_top.Name))));
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.LocalMethod(
                AvroEventBeanGetterDynamicPoly.GetAvroFieldFragmentPolyCodegen(
                    codegenMethodScope,
                    codegenClassScope,
                    _getters),
                CodegenExpressionBuilder.Cast(
                    typeof(GenericRecord),
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(GenericRecordExtensions),
                        "Get",
                        underlyingExpression,
                        CodegenExpressionBuilder.Constant(_top.Name))));
        }
    }
} // end of namespace