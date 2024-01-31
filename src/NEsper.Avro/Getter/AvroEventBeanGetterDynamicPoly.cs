///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using NEsper.Avro.Core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterDynamicPoly : AvroEventPropertyGetter
    {
        private readonly AvroEventPropertyGetter[] _getters;

        public AvroEventBeanGetterDynamicPoly(AvroEventPropertyGetter[] getters)
        {
            _getters = getters;
        }

        public object GetAvroFieldValue(GenericRecord record)
        {
            return GetAvroFieldValuePoly(record, _getters);
        }

        public object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            return GetAvroFieldValue(record);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsExistsPropertyAvro((GenericRecord) eventBean.Underlying);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public object GetAvroFragment(GenericRecord record)
        {
            return null;
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            return GetAvroFieldValuePolyExists(record, _getters);
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
                GetAvroFieldValuePolyCodegen(codegenMethodScope, codegenClassScope, _getters),
                underlyingExpression);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetAvroFieldValuePolyExistsCodegen(codegenMethodScope, codegenClassScope, _getters), underlyingExpression);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.ConstantNull();
        }

        internal static bool GetAvroFieldValuePolyExists(
            GenericRecord record,
            AvroEventPropertyGetter[] getters)
        {
            if (record == null) {
                return false;
            }

            record = NavigatePoly(record, getters);
            return record != null && getters[getters.Length - 1].IsExistsPropertyAvro(record);
        }

        internal static CodegenMethod GetAvroFieldValuePolyExistsCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            AvroEventPropertyGetter[] getters)
        {
            return codegenMethodScope.MakeChild(typeof(bool), typeof(AvroEventBeanGetterDynamicPoly), codegenClassScope)
                .AddParam<GenericRecord>("record")
                .Block
                .IfRefNullReturnFalse("record")
                .AssignRef(
                    "record",
                    CodegenExpressionBuilder.LocalMethod(
                        NavigatePolyCodegen(codegenMethodScope, codegenClassScope, getters),
                        CodegenExpressionBuilder.Ref("record")))
                .IfRefNullReturnFalse("record")
                .MethodReturn(
                    getters[getters.Length - 1]
                        .UnderlyingExistsCodegen(
                            CodegenExpressionBuilder.Ref("record"),
                            codegenMethodScope,
                            codegenClassScope));
        }

        internal static object GetAvroFieldValuePoly(
            GenericRecord record,
            AvroEventPropertyGetter[] getters)
        {
            if (record == null) {
                return null;
            }

            record = NavigatePoly(record, getters);
            if (record == null) {
                return null;
            }

            return getters[getters.Length - 1].GetAvroFieldValue(record);
        }

        internal static CodegenMethod GetAvroFieldValuePolyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            AvroEventPropertyGetter[] getters)
        {
            return codegenMethodScope.MakeChild(
                    typeof(object),
                    typeof(AvroEventBeanGetterDynamicPoly),
                    codegenClassScope)
                .AddParam<GenericRecord>("record")
                .Block
                .IfRefNullReturnNull("record")
                .AssignRef(
                    "record",
                    CodegenExpressionBuilder.LocalMethod(
                        NavigatePolyCodegen(codegenMethodScope, codegenClassScope, getters),
                        CodegenExpressionBuilder.Ref("record")))
                .IfRefNullReturnNull("record")
                .MethodReturn(
                    getters[getters.Length - 1]
                        .UnderlyingGetCodegen(
                            CodegenExpressionBuilder.Ref("record"),
                            codegenMethodScope,
                            codegenClassScope));
        }

        internal static object GetAvroFieldFragmentPoly(
            GenericRecord record,
            AvroEventPropertyGetter[] getters)
        {
            if (record == null) {
                return null;
            }

            record = NavigatePoly(record, getters);
            if (record == null) {
                return null;
            }

            return getters[getters.Length - 1].GetAvroFragment(record);
        }

        internal static CodegenMethod GetAvroFieldFragmentPolyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            AvroEventPropertyGetter[] getters)
        {
            return codegenMethodScope.MakeChild(
                    typeof(object),
                    typeof(AvroEventBeanGetterDynamicPoly),
                    codegenClassScope)
                .AddParam<GenericRecord>("record")
                .Block
                .IfRefNullReturnNull("record")
                .AssignRef(
                    "record",
                    CodegenExpressionBuilder.LocalMethod(
                        NavigatePolyCodegen(codegenMethodScope, codegenClassScope, getters),
                        CodegenExpressionBuilder.Ref("record")))
                .IfRefNullReturnNull("record")
                .MethodReturn(
                    getters[getters.Length - 1]
                        .UnderlyingFragmentCodegen(
                            CodegenExpressionBuilder.Ref("record"),
                            codegenMethodScope,
                            codegenClassScope));
        }

        internal static GenericRecord NavigatePoly(
            GenericRecord record,
            AvroEventPropertyGetter[] getters)
        {
            for (var i = 0; i < getters.Length - 1; i++) {
                var value = getters[i].GetAvroFieldValue(record);
                if (!(value is GenericRecord)) {
                    return null;
                }

                record = (GenericRecord) value;
            }

            return record;
        }

        internal static CodegenMethod NavigatePolyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            AvroEventPropertyGetter[] getters)
        {
            var block = codegenMethodScope.MakeChild(
                    typeof(GenericRecord),
                    typeof(AvroEventBeanGetterDynamicPoly),
                    codegenClassScope)
                .AddParam<GenericRecord>("record")
                .Block;
            block.DeclareVar<object>("value", CodegenExpressionBuilder.ConstantNull());
            for (var i = 0; i < getters.Length - 1; i++) {
                block.AssignRef(
                        "value",
                        getters[i]
                            .UnderlyingGetCodegen(
                                CodegenExpressionBuilder.Ref("record"),
                                codegenMethodScope,
                                codegenClassScope))
                    .IfRefNotTypeReturnConst("value", typeof(GenericRecord), null)
                    .AssignRef(
                        "record",
                        CodegenExpressionBuilder.Cast(typeof(GenericRecord), CodegenExpressionBuilder.Ref("value")));
            }

            return block.MethodReturn(CodegenExpressionBuilder.Ref("record"));
        }
    }
} // end of namespace