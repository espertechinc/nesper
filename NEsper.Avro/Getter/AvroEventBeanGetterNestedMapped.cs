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
using com.espertech.esper.common.@internal.@event.core;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedMapped : EventPropertyGetterSPI
    {
        private readonly string _key;
        private readonly Field _pos;
        private readonly Field _top;

        public AvroEventBeanGetterNestedMapped(
            Field top,
            Field pos,
            string key)
        {
            _top = top;
            _pos = pos;
            _key = key;
        }

        public object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_top);
            if (inner == null) {
                return null;
            }

            var map = (IDictionary<string, object>) inner.Get(_pos);
            return AvroEventBeanGetterMapped.GetAvroMappedValueWNullCheck(map, _key);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
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
            return CodegenExpressionBuilder.ConstantTrue();
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
            return CodegenExpressionBuilder.ConstantTrue();
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.ConstantNull();
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
                .DeclareVar<IDictionary<string, object>>(
                    "map",
                    CodegenExpressionBuilder.Cast(
                        typeof(IDictionary<string, object>),
                        CodegenExpressionBuilder.ExprDotMethod(
                            CodegenExpressionBuilder.Ref("inner"),
                            "Get",
                            CodegenExpressionBuilder.Constant(_pos))))
                .MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(AvroEventBeanGetterMapped),
                        "GetAvroMappedValueWNullCheck",
                        CodegenExpressionBuilder.Ref("map"),
                        CodegenExpressionBuilder.Constant(_key)));
        }
    }
} // end of namespace