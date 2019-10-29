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

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedIndexRooted : EventPropertyGetterSPI
    {
        private readonly int _index;
        private readonly AvroEventPropertyGetter _nested;
        private readonly Field _posTop;

        public AvroEventBeanGetterNestedIndexRooted(
            Field posTop,
            int index,
            AvroEventPropertyGetter nested)
        {
            _posTop = posTop;
            _index = index;
            _nested = nested;
        }

        public object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = GetAtIndex(record, _posTop, _index);
            return inner == null ? null : _nested.GetAvroFieldValue(inner);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var values = record.Get(_posTop);
            var value = AvroEventBeanGetterIndexed.GetAvroIndexedValue(values, _index);
            if (value == null || !(value is GenericRecord)) {
                return null;
            }

            return _nested.GetAvroFragment((GenericRecord) value);
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
            return CodegenExpressionBuilder.LocalMethod(
                GetFragmentCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="record">record</param>
        /// <param name="posTop">postop</param>
        /// <param name="index">index</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException ex</throws>
        public static GenericRecord GetAtIndex(
            GenericRecord record,
            Field posTop,
            int index)
        {
            var values = record.Get(posTop);
            var value = AvroEventBeanGetterIndexed.GetAvroIndexedValue(values, index);
            if (value == null || !(value is GenericRecord)) {
                return null;
            }

            return (GenericRecord) value;
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
                    CodegenExpressionBuilder.StaticMethod(
                        GetType(),
                        "GetAtIndex",
                        CodegenExpressionBuilder.Ref("record"),
                        CodegenExpressionBuilder.Constant(_posTop),
                        CodegenExpressionBuilder.Constant(_index)))
                .IfRefNullReturnNull("inner")
                .MethodReturn(
                    _nested.UnderlyingGetCodegen(
                        CodegenExpressionBuilder.Ref("inner"),
                        codegenMethodScope,
                        codegenClassScope));
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(GenericRecord), "record")
                .Block
                .DeclareVar<object>(
                    "values",
                    CodegenExpressionBuilder.ExprDotMethod(
                        CodegenExpressionBuilder.Ref("record"),
                        "Get",
                        CodegenExpressionBuilder.Constant(_posTop)))
                .DeclareVar<object>(
                    "value",
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(AvroEventBeanGetterIndexed),
                        "GetAvroIndexedValue",
                        CodegenExpressionBuilder.Ref("values"),
                        CodegenExpressionBuilder.Constant(_index)))
                .IfRefNullReturnNull("value")
                .IfRefNotTypeReturnConst("value", typeof(GenericRecord), null)
                .MethodReturn(
                    _nested.UnderlyingFragmentCodegen(
                        CodegenExpressionBuilder.Cast(typeof(GenericRecord), CodegenExpressionBuilder.Ref("value")),
                        codegenMethodScope,
                        codegenClassScope));
        }
    }
} // end of namespace