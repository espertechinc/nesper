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

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedDynamicSimple : EventPropertyGetterSPI
    {
        private readonly Field _posTop;
        private readonly string _propertyName;

        public AvroEventBeanGetterNestedDynamicSimple(
            Field posTop,
            string propertyName)
        {
            _posTop = posTop;
            _propertyName = propertyName;
        }

        public object Get(EventBean eventBean)
        {
            return Get((GenericRecord) eventBean.Underlying);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsExistsProperty((GenericRecord) eventBean.Underlying);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
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
            return CodegenExpressionBuilder.ConstantNull();
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
            return CodegenExpressionBuilder.LocalMethod(
                IsExistsPropertyCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.ConstantNull();
        }

        private object Get(GenericRecord record)
        {
            var inner = (GenericRecord) record.Get(_posTop);
            if (inner == null) {
                return null;
            }

            return inner.Get(_propertyName);
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
                    CodegenExpressionBuilder.Cast(
                        typeof(GenericRecord),
                        CodegenExpressionBuilder.StaticMethod(
                            typeof(GenericRecordExtensions),
                            "Get",
                            CodegenExpressionBuilder.Ref("record"),
                            CodegenExpressionBuilder.Constant(_posTop.Name))))
                .IfRefNullReturnNull("inner")
                .MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(GenericRecordExtensions),
                        "Get",
                        CodegenExpressionBuilder.Ref("inner"),
                        CodegenExpressionBuilder.Constant(_propertyName)));
        }

        private bool IsExistsProperty(GenericRecord record)
        {
            GenericRecord inner = (GenericRecord) record.Get(_posTop);
            if (inner == null) {
                return false;
            }

            return inner.Schema.GetField(_propertyName) != null;
        }

        private CodegenMethod IsExistsPropertyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam<GenericRecord>("record")
                .Block
                .DeclareVar<GenericRecord>(
                    "inner",
                    CodegenExpressionBuilder.Cast(
                        typeof(GenericRecord),
                        CodegenExpressionBuilder.StaticMethod(
                            typeof(GenericRecordExtensions),
                            "Get",
                            CodegenExpressionBuilder.Ref("record"),
                            CodegenExpressionBuilder.Constant(_posTop.Name))))
                .IfRefNullReturnFalse("inner")
                .MethodReturn(
                    CodegenExpressionBuilder.NotEqualsNull(
                        CodegenExpressionBuilder.StaticMethod(
                            typeof(SchemaExtensions),
                            "GetField",
                            CodegenExpressionBuilder.ExprDotMethodChain(CodegenExpressionBuilder.Ref("inner")).Get("Schema"),
                            CodegenExpressionBuilder.Constant(_propertyName))));
        }
    }
} // end of namespace