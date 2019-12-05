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
using com.espertech.esper.common.@internal.@event.core;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedDynamicPoly : EventPropertyGetterSPI
    {
        private readonly string _fieldTop;
        private readonly AvroEventPropertyGetter _getter;

        public AvroEventBeanGetterNestedDynamicPoly(
            string fieldTop,
            AvroEventPropertyGetter getter)
        {
            _fieldTop = fieldTop;
            _getter = getter;
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
            var inner = (GenericRecord) record.Get(_fieldTop);
            return inner == null ? null : _getter.GetAvroFieldValue(inner);
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
                        CodegenExpressionBuilder.StaticMethod(
                            typeof(GenericRecordExtensions),
                            "Get",
                            CodegenExpressionBuilder.Ref("record"),
                            CodegenExpressionBuilder.Constant(_fieldTop))))
                .MethodReturn(
                    CodegenExpressionBuilder.Conditional(
                        CodegenExpressionBuilder.EqualsNull(CodegenExpressionBuilder.Ref("inner")),
                        CodegenExpressionBuilder.ConstantNull(),
                        _getter.UnderlyingGetCodegen(
                            CodegenExpressionBuilder.Ref("inner"),
                            codegenMethodScope,
                            codegenClassScope)));
        }

        private bool IsExistsProperty(GenericRecord record)
        {
            var field = record.Schema.GetField(_fieldTop);
            if (field == null) {
                return false;
            }

            var inner = record.Get(_fieldTop);
            if (!(inner is GenericRecord)) {
                return false;
            }

            return _getter.IsExistsPropertyAvro((GenericRecord) inner);
        }

        private CodegenMethod IsExistsPropertyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(GenericRecord), "record")
                .Block
                .DeclareVar<Field>(
                    "field",
                    CodegenExpressionBuilder.ExprDotMethodChain(CodegenExpressionBuilder.Ref("record"))
                        .Get("Schema")
                        .Add("GetField", CodegenExpressionBuilder.Constant(_fieldTop)))
                .IfRefNullReturnFalse("field")
                .DeclareVar<object>(
                    "inner",
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(GenericRecordExtensions),
                        "Get",
                        CodegenExpressionBuilder.Ref("record"),
                        CodegenExpressionBuilder.Constant(_fieldTop)))
                .IfRefNotTypeReturnConst("inner", typeof(GenericRecord), false)
                .MethodReturn(
                    _getter.UnderlyingExistsCodegen(
                        CodegenExpressionBuilder.Cast(typeof(GenericRecord), CodegenExpressionBuilder.Ref("inner")),
                        codegenMethodScope,
                        codegenClassScope));
        }
    }
} // end of namespace