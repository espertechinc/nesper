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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

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

        private CodegenMethod ExistsCodegen(CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope
                .MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(GenericRecord), "record")
                .Block
                .DeclareVar<GenericRecord>(
                    "inner",
                    StaticMethod(
                        GetType(),
                        "GetAtIndex",
                        Ref("record"),
                        Constant(_posTop.Name),
                        Constant(_index)))
                .IfRefNullReturnFalse("inner")
                .MethodReturn(
                    _nested.UnderlyingExistsCodegen(
                        Ref("inner"),
                        codegenMethodScope,
                        codegenClassScope));
        }

        public bool IsExistsProperty(EventBean eventBean) {
            var record = (GenericRecord) eventBean.Underlying;
            var values = record.Get(_posTop);
            return AvroEventBeanGetterIndexed.GetAvroIndexedExists(values, _index);
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
                ExistsCodegen(
                    codegenMethodScope,
                    codegenClassScope),
                underlyingExpression);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
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
        
        public static GenericRecord GetAtIndex(
            GenericRecord record,
            string posTop,
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
                    StaticMethod(
                        GetType(),
                        "GetAtIndex",
                        Ref("record"),
                        Constant(_posTop.Name),
                        Constant(_index)))
                .IfRefNullReturnNull("inner")
                .MethodReturn(
                    _nested.UnderlyingGetCodegen(
                        Ref("inner"),
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
                    StaticMethod(
                        typeof(GenericRecordExtensions),
                        "Get",
                        Ref("record"),
                        Constant(_posTop.Name)))
                .DeclareVar<object>(
                    "value",
                    StaticMethod(
                        typeof(AvroEventBeanGetterIndexed),
                        "GetAvroIndexedValue",
                        Ref("values"),
                        Constant(_index)))
                .IfRefNullReturnNull("value")
                .IfRefNotTypeReturnConst("value", typeof(GenericRecord), null)
                .MethodReturn(
                    _nested.UnderlyingFragmentCodegen(
                        Cast(typeof(GenericRecord), Ref("value")),
                        codegenMethodScope,
                        codegenClassScope));
        }
    }
} // end of namespace