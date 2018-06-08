///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.events;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedDynamicPoly : EventPropertyGetterSPI
    {
        private readonly string _fieldTop;
        private readonly AvroEventPropertyGetter _getter;

        public AvroEventBeanGetterNestedDynamicPoly(string fieldTop, AvroEventPropertyGetter getter)
        {
            _fieldTop = fieldTop;
            _getter = getter;
        }

        public Object Get(EventBean eventBean)
        {
            return Get((GenericRecord)eventBean.Underlying);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsExistsProperty((GenericRecord)eventBean.Underlying);
        }

        private Object Get(GenericRecord record)
        {
            GenericRecord inner = (GenericRecord)record.Get(_fieldTop);
            return inner == null ? null : _getter.GetAvroFieldValue(inner);
        }

        private string GetCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(GenericRecord), "record", GetType())
                .DeclareVar(typeof(GenericRecord), "inner", Cast(typeof(GenericRecord), ExprDotMethod(Ref("record"), "get", Constant(_fieldTop))))
                .MethodReturn(Conditional(EqualsNull(Ref("inner")), ConstantNull(), _getter.CodegenUnderlyingGet(Ref("inner"), context)));
        }

        private bool IsExistsProperty(GenericRecord record)
        {
            var field = record.Schema.GetField(_fieldTop);
            if (field == null)
            {
                return false;
            }
            var inner = record.Get(_fieldTop);
            if (!(inner is GenericRecord))
            {
                return false;
            }
            return _getter.IsExistsPropertyAvro((GenericRecord)inner);
        }

        private string IsExistsPropertyCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(bool), typeof(GenericRecord), "record", GetType())
                .DeclareVar(typeof(Field), "field", ExprDotMethodChain(Ref("record")).AddNoParam("GetSchema").AddWConst("GetField", _fieldTop))
                .IfRefNullReturnFalse("field")
                .DeclareVar(typeof(Object), "inner", ExprDotMethod(Ref("record"), "get", Constant(_fieldTop)))
                .IfRefNotTypeReturnConst("inner", typeof(GenericRecord), false)
                .MethodReturn(_getter.CodegenUnderlyingExists(Cast(typeof(GenericRecord), Ref("inner")), context));
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(GenericRecord), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(GenericRecord), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(IsExistsPropertyCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantNull();
        }
    }
} // end of namespace
