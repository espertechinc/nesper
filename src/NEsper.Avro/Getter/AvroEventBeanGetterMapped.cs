///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterMapped : AvroEventPropertyGetter
    {
        private readonly string _key;
        private readonly Field _pos;

        public AvroEventBeanGetterMapped(
            Field pos,
            string key)
        {
            _pos = pos;
            _key = key;
        }

        public object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var values = record.Get(_pos).AsStringDictionary();
            return GetAvroMappedValueWNullCheck(values, _key);
        }

        public object GetAvroFieldValue(GenericRecord record)
        {
            var values = record.Get(_pos).AsStringDictionary();
            return GetAvroMappedValueWNullCheck(values, _key);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsExistsPropertyAvro((GenericRecord) eventBean.Underlying);
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            var values = record.Get(_pos).AsStringDictionary();
            return values?.ContainsKey(_key) ?? false;
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public object GetAvroFragment(GenericRecord record)
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
                GetAvroFieldValueCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.LocalMethod(
                GetAvroFieldExistsCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.ConstantNull();
        }

        private CodegenMethod GetAvroFieldValueCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope
                .MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam<GenericRecord>("record")
                .Block
                .DeclareVar<IDictionary<string, object>>(
                    "values",
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(CompatExtensions),
                        "AsStringDictionary",
                        CodegenExpressionBuilder.StaticMethod(
                            typeof(GenericRecordExtensions),
                            "Get",
                            CodegenExpressionBuilder.Ref("record"),
                            CodegenExpressionBuilder.Constant(_pos.Name))))
                .IfRefNullReturnNull("values")
                .MethodReturn(
                    CodegenExpressionBuilder.ExprDotMethod(
                        CodegenExpressionBuilder.Ref("values"),
                        "Get",
                        CodegenExpressionBuilder.Constant(_key)));
        }

        private CodegenMethod GetAvroFieldExistsCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope
                .MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam<GenericRecord>("record")
                .Block
                .DeclareVar<IDictionary<string, object>>(
                    "values",
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(CompatExtensions),
                        "AsStringDictionary",
                        CodegenExpressionBuilder.StaticMethod(
                            typeof(GenericRecordExtensions),
                            "Get",
                            CodegenExpressionBuilder.Ref("record"),
                            CodegenExpressionBuilder.Constant(_pos.Name))))
                .IfRefNullReturnFalse("values")
                .MethodReturn(
                    CodegenExpressionBuilder.ExprDotMethod(
                        CodegenExpressionBuilder.Ref("values"),
                        "ContainsKey",
                        CodegenExpressionBuilder.Constant(_key)));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="map">map</param>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        public static object GetAvroMappedValueWNullCheck(
            IDictionary<string, object> map,
            string key)
        {
            return map?.Get(key);
        }
    }
} // end of namespace