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

using NEsper.Avro.Extensions;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedDynamicSimple : EventPropertyGetterSPI
    {
        private readonly Field _posTop;
        private readonly string _propertyName;

        public AvroEventBeanGetterNestedDynamicSimple(Field posTop, string propertyName)
        {
            _posTop = posTop;
            _propertyName = propertyName;
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
            var inner = (GenericRecord) record.Get(_posTop);
            if (inner == null)
            {
                return null;
            }
            return inner.Get(_propertyName);
        }

        private string GetCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(GenericRecord), "record", GetType())
                    .DeclareVar(typeof(GenericRecord), "inner", Cast(typeof(GenericRecord), ExprDotMethod(Ref("record"), "Get", Constant(_posTop))))
                    .IfRefNullReturnNull("inner")
                    .MethodReturn(ExprDotMethod(Ref("inner"), "Get", Constant(_propertyName)));
        }

        private bool IsExistsProperty(GenericRecord record)
        {
            var inner = (GenericRecord)record.Get(_posTop);
            if (inner == null)
            {
                return false;
            }
            return inner.Schema.GetField(_propertyName) != null;
        }

        private string IsExistsPropertyCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(bool), typeof(GenericRecord), "record", GetType())
                    .DeclareVar(typeof(GenericRecord), "inner", Cast(typeof(GenericRecord), ExprDotMethod(Ref("record"), "Get", Constant(_posTop))))
                    .IfRefNullReturnFalse("inner")
                    .MethodReturn(NotEqualsNull(ExprDotMethodChain(Ref("inner")).AddNoParam("GetSchema").AddWConst("GetField", _propertyName)));
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
