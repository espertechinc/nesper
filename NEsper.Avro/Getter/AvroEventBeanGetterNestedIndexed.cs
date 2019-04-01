///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

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
    public class AvroEventBeanGetterNestedIndexed : EventPropertyGetterSPI
    {
        private readonly Field _top;
        private readonly Field _pos;
        private readonly int _index;
        private readonly EventType _fragmentEventType;
        private readonly EventAdapterService _eventAdapterService;

        public AvroEventBeanGetterNestedIndexed(
            Field top, 
            Field pos, 
            int index, 
            EventType fragmentEventType, 
            EventAdapterService eventAdapterService)
        {
            _top = top;
            _pos = pos;
            _index = index;
            _fragmentEventType = fragmentEventType;
            _eventAdapterService = eventAdapterService;
        }

        public Object Get(EventBean eventBean)
        {
            var record = (GenericRecord)eventBean.Underlying;
            var inner = (GenericRecord)record.Get(_top);
            if (inner == null)
            {
                return null;
            }
            var collection = (ICollection<object>)inner.Get(_pos);
            return AvroEventBeanGetterIndexed.GetAvroIndexedValue(collection, _index);
        }

        private string GetCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(GenericRecord), "record", this.GetType())
                .DeclareVar(typeof(GenericRecord), "inner", Cast(typeof(GenericRecord), ExprDotMethod(Ref("record"), "Get", Constant(_top))))
                .IfRefNullReturnNull("inner")
                .DeclareVar(typeof(ICollection<object>), "collection", Cast(typeof(ICollection<object>), ExprDotMethod(Ref("inner"), "Get", Constant(_pos))))
                .MethodReturn(StaticMethod(typeof(AvroEventBeanGetterIndexed), "GetAvroIndexedValue", Ref("collection"), Constant(_index)));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            if (_fragmentEventType == null)
            {
                return null;
            }
            var value = Get(eventBean);
            if (value == null)
            {
                return null;
            }
            return _eventAdapterService.AdapterForTypedAvro(value, _fragmentEventType);
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            var mSvc = context.MakeAddMember(typeof(EventAdapterService), _eventAdapterService);
            var mType = context.MakeAddMember(typeof(EventType), _fragmentEventType);

            return context.AddMethod(typeof(Object), typeof(GenericRecord), "record", GetType())
                .DeclareVar(typeof(Object), "value", CodegenUnderlyingGet(Ref("record"), context))
                .IfRefNullReturnNull("value")
                .MethodReturn(ExprDotMethod(Ref(mSvc.MemberName), "AdapterForTypedAvro", Ref("value"), Ref(mType.MemberName)));
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
            if (_fragmentEventType == null)
            {
                return ConstantNull();
            }
            return LocalMethod(GetFragmentCodegen(context), underlyingExpression);
        }
    }
} // end of namespace
