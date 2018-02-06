///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterIndexed : AvroEventPropertyGetter
    {
        private readonly Field _pos;
        private readonly int _index;
        private readonly EventType _fragmentEventType;
        private readonly EventAdapterService _eventAdapterService;

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static object GetAvroIndexedValue(ICollection<object> values, int index)
        {
            if (values == null)
            {
                return null;
            }
            else if (values is IList<object> list)
            {
                return list.Count > index ? list[index] : null;
            }

            return values.Skip(index).FirstOrDefault();
        }

        public AvroEventBeanGetterIndexed(
            Field pos,
            int index,
            EventType fragmentEventType,
            EventAdapterService eventAdapterService)
        {
            _pos = pos;
            _index = index;
            _fragmentEventType = fragmentEventType;
            _eventAdapterService = eventAdapterService;
        }

        public object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var values = record.Get(_pos).Unwrap<object>(true);
            return GetAvroIndexedValue(values, _index);
        }

        public object GetAvroFieldValue(GenericRecord record)
        {
            var values = record.Get(_pos).Unwrap<object>(true);
            return GetAvroIndexedValue(values, _index);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            return GetAvroFragment(record);
        }

        public object GetAvroFragment(GenericRecord record)
        {
            if (_fragmentEventType == null)
            {
                return null;
            }
            var value = GetAvroFieldValue(record);
            if (value == null)
            {
                return null;
            }
            return _eventAdapterService.AdapterForTypedAvro(value, _fragmentEventType);
        }

        internal static object GetIndexedValue(Array values, int index)
        {
            if (values == null)
            {
                return null;
            }

            return values.Length > index ? values.GetValue(index) : null;
        }

        internal static object GetIndexedValue(IEnumerable<object> values, int index)
        {
            if (values == null)
            {
                return null;
            }
            if (values is IList<object>)
            {
                var list = (IList<object>)values;
                return list.Count > index ? list[index] : null;
            }

            return values.Skip(index).FirstOrDefault(null);
            //return values.ToArray()[index];
        }

        private String GetAvroFragmentCodegen(ICodegenContext context)
        {
            var mSvc = context.MakeAddMember(typeof(EventAdapterService), _eventAdapterService);
            var mType = context.MakeAddMember(typeof(EventType), _fragmentEventType);
            return context.AddMethod(typeof(object), typeof(GenericRecord), "record", this.GetType())
                .DeclareVar(typeof(object), "value", CodegenUnderlyingGet(Ref("record"), context))
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
            var values = Cast(typeof(ICollection<object>), ExprDotMethod(underlyingExpression, "get", Constant(_pos)));
            return StaticMethod(this.GetType(), "GetAvroIndexedValue", values, Constant(_index));
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
            return LocalMethod(GetAvroFragmentCodegen(context), underlyingExpression);
        }
    }
} // end of namespace
