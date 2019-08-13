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

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedIndexRootedMultilevel : EventPropertyGetterSPI
    {
        private readonly int _index;
        private readonly AvroEventPropertyGetter[] _nested;
        private readonly Field _posTop;

        public AvroEventBeanGetterNestedIndexRootedMultilevel(
            Field posTop,
            int index,
            AvroEventPropertyGetter[] nested)
        {
            _posTop = posTop;
            _index = index;
            _nested = nested;
        }

        public object Get(EventBean eventBean)
        {
            var value = Navigate((GenericRecord) eventBean.Underlying);
            if (value == null) {
                return null;
            }

            return _nested[_nested.Length - 1].GetAvroFieldValue(value);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            var value = Navigate((GenericRecord) eventBean.Underlying);
            if (value == null) {
                return null;
            }

            return _nested[_nested.Length - 1].GetAvroFragment(value);
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

        private CodegenMethod GetCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(GenericRecord), "record")
                .Block
                .DeclareVar<GenericRecord>(
                    "value",
                    CodegenExpressionBuilder.LocalMethod(
                        NavigateMethodCodegen(codegenMethodScope, codegenClassScope),
                        CodegenExpressionBuilder.Ref("record")))
                .IfRefNullReturnNull("value")
                .MethodReturn(
                    _nested[_nested.Length - 1]
                        .UnderlyingGetCodegen(
                            CodegenExpressionBuilder.Ref("value"),
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
                .DeclareVar<GenericRecord>(
                    "value",
                    CodegenExpressionBuilder.LocalMethod(
                        NavigateMethodCodegen(codegenMethodScope, codegenClassScope),
                        CodegenExpressionBuilder.Ref("record")))
                .IfRefNullReturnNull("value")
                .MethodReturn(
                    _nested[_nested.Length - 1]
                        .UnderlyingFragmentCodegen(
                            CodegenExpressionBuilder.Ref("value"),
                            codegenMethodScope,
                            codegenClassScope));
        }

        private GenericRecord Navigate(GenericRecord record)
        {
            object value = AvroEventBeanGetterNestedIndexRooted.GetAtIndex(record, _posTop, _index);
            if (value == null) {
                return null;
            }

            return NavigateRecord((GenericRecord) value);
        }

        private CodegenMethod NavigateMethodCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var navigateRecordMethod = NavigateRecordMethodCodegen(codegenMethodScope, codegenClassScope);
            return codegenMethodScope.MakeChild(typeof(GenericRecord), GetType(), codegenClassScope)
                .AddParam(typeof(GenericRecord), "record")
                .Block
                .DeclareVar<object>(
                    "value",
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(AvroEventBeanGetterNestedIndexRooted),
                        "GetAtIndex",
                        CodegenExpressionBuilder.Ref("record"),
                        CodegenExpressionBuilder.Constant(_posTop),
                        CodegenExpressionBuilder.Constant(_index)))
                .IfRefNullReturnNull("value")
                .MethodReturn(
                    CodegenExpressionBuilder.LocalMethod(
                        navigateRecordMethod,
                        CodegenExpressionBuilder.CastRef(typeof(GenericRecord), "value")));
        }

        private GenericRecord NavigateRecord(GenericRecord record)
        {
            var current = record;
            for (var i = 0; i < _nested.Length - 1; i++) {
                var value = _nested[i].GetAvroFieldValue(current);
                if (!(value is GenericRecord)) {
                    return null;
                }

                current = (GenericRecord) value;
            }

            return current;
        }

        private CodegenMethod NavigateRecordMethodCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope.MakeChild(typeof(GenericRecord), GetType(), codegenClassScope)
                .AddParam(typeof(GenericRecord), "record")
                .Block
                .DeclareVar<GenericRecord>("current", CodegenExpressionBuilder.Ref("record"))
                .DeclareVarNull(typeof(object), "value");
            for (var i = 0; i < _nested.Length - 1; i++) {
                block.AssignRef(
                        "value",
                        _nested[i]
                            .UnderlyingGetCodegen(
                                CodegenExpressionBuilder.Ref("current"),
                                codegenMethodScope,
                                codegenClassScope))
                    .IfRefNotTypeReturnConst("value", typeof(GenericRecord), null)
                    .AssignRef("current", CodegenExpressionBuilder.CastRef(typeof(GenericRecord), "value"));
            }

            return block.MethodReturn(CodegenExpressionBuilder.Ref("current"));
        }
    }
} // end of namespace