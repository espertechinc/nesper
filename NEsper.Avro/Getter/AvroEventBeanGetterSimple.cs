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

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterSimple : AvroEventPropertyGetter
    {
        private readonly Field _propertyIndex;
        private readonly EventType _fragmentType;
        private readonly EventAdapterService _eventAdapterService;

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="eventAdapterService">svc</param>
        /// <param name="fragmentType">type</param>
        /// <returns>fragment</returns>
        public static Object GetFragmentAvro(Object value, EventAdapterService eventAdapterService, EventType fragmentType)
        {
            if (fragmentType == null)
            {
                return null;
            }
            if (value is GenericRecord)
            {
                return eventAdapterService.AdapterForTypedAvro(value, fragmentType);
            }
            if (value is ICollection<object> coll)
            {
                var events = new EventBean[coll.Count];
                int index = 0;
                foreach (Object item in coll)
                {
                    events[index++] = eventAdapterService.AdapterForTypedAvro(item, fragmentType);
                }
                return events;
            }
            return null;
        }

        public AvroEventBeanGetterSimple(Field propertyIndex, EventType fragmentType, EventAdapterService eventAdapterService)
        {
            _propertyIndex = propertyIndex;
            _fragmentType = fragmentType;
            _eventAdapterService = eventAdapterService;
        }

        public Object GetAvroFieldValue(GenericRecord record)
        {
            return record.Get(_propertyIndex);
        }

        public Object Get(EventBean theEvent)
        {
            return GetAvroFieldValue((GenericRecord)theEvent.Underlying);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            return true;
        }

        public Object GetFragment(EventBean obj)
        {
            Object value = Get(obj);
            return GetFragmentAvro(value, _eventAdapterService, _fragmentType);
        }

        public Object GetAvroFragment(GenericRecord record)
        {
            Object value = GetAvroFieldValue(record);
            return GetFragmentAvro(value, _eventAdapterService, _fragmentType);
        }

        private string GetAvroFragmentCodegen(ICodegenContext context)
        {
            var mSvc = context.MakeAddMember(typeof(EventAdapterService), _eventAdapterService);
            var mType = context.MakeAddMember(typeof(EventType), _fragmentType);
            return context.AddMethod(typeof(Object), typeof(GenericRecord), "record", GetType())
                .DeclareVar(typeof(Object), "value", CodegenUnderlyingGet( Ref("record"), context))
                .MethodReturn(StaticMethod(GetType(), "GetFragmentAvro", Ref("value"), Ref(mSvc.MemberName), Ref(mType.MemberName)));
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
            return CodegenUnderlyingFragment(CastUnderlying(typeof(GenericRecord), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ExprDotMethod(underlyingExpression, "Get", Constant(_propertyIndex));
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
            return LocalMethod(GetAvroFragmentCodegen(context), underlyingExpression);
        }
    }

} // end of namespace
