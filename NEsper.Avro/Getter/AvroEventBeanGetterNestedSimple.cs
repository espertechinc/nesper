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
    public class AvroEventBeanGetterNestedSimple : EventPropertyGetterSPI
    {
        private readonly Field _posTop;
        private readonly Field _posInner;
        private readonly EventType _fragmentType;
        private readonly EventAdapterService _eventAdapterService;

        public AvroEventBeanGetterNestedSimple(Field posTop, Field posInner, EventType fragmentType, EventAdapterService eventAdapterService)
        {
            _posTop = posTop;
            _posInner = posInner;
            _fragmentType = fragmentType;
            _eventAdapterService = eventAdapterService;
        }

        public Object Get(EventBean eventBean)
        {
            return Get((GenericRecord)eventBean.Underlying);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            if (_fragmentType == null)
            {
                return null;
            }
            var value = Get(eventBean);
            if (value == null)
            {
                return null;
            }
            return _eventAdapterService.AdapterForTypedAvro(value, _fragmentType);
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            var mSvc = context.MakeAddMember(typeof(EventAdapterService), _eventAdapterService);
            var mType = context.MakeAddMember(typeof(EventType), _fragmentType);
            return context.AddMethod(typeof(Object), typeof(GenericRecord), "record", GetType())
                    .DeclareVar(typeof(Object), "value", CodegenUnderlyingGet(Ref("record"), context))
                    .IfRefNullReturnNull("value")
                    .MethodReturn(ExprDotMethod(
                        Ref(mSvc.MemberName), "AdapterForTypedAvro",
                        Ref("value"),
                        Ref(mType.MemberName)));
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(GenericRecord), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(CastUnderlying(typeof(GenericRecord), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            if (_fragmentType == null)
            {
                return ConstantNull();
            }
            return LocalMethod(GetFragmentCodegen(context), underlyingExpression);
        }

        private Object Get(GenericRecord record)
        {
            var inner = (GenericRecord)record.Get(_posTop);
            return inner?.Get(_posInner);
        }

        private string GetCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(GenericRecord), "record", GetType())
                .DeclareVar(
                    typeof(GenericRecord), "inner",
                    Cast(
                        typeof(GenericRecord),
                        ExprDotMethod(
                            Ref("record"), "Get", Constant(_posTop))))
                .IfRefNullReturnNull("inner")
                .MethodReturn(
                    ExprDotMethod(
                        Ref("inner"), "Get", Constant(_posInner)));
        }
    }
} // end of namespace
