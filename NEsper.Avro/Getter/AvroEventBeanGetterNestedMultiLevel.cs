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
    public class AvroEventBeanGetterNestedMultiLevel : EventPropertyGetterSPI
    {
        private readonly Field _top;
        private readonly Field[] _path;
        private readonly EventType _fragmentEventType;
        private readonly EventAdapterService _eventAdapterService;

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="record">record</param>
        /// <param name="top">top index</param>
        /// <param name="path">path of indexes</param>
        /// <exception cref="PropertyAccessException">property access problem</exception>
        /// <returns>value</returns>
        public static Object GetRecordValueTopWPath(GenericRecord record, Field top, Field[] path)
        {
            GenericRecord inner = (GenericRecord)record.Get(top);
            if (inner == null)
            {
                return null;
            }
            for (int i = 0; i < path.Length - 1; i++)
            {
                inner = (GenericRecord)inner.Get(path[i]);
                if (inner == null)
                {
                    return null;
                }
            }
            return inner.Get(path[path.Length - 1]);
        }

        public AvroEventBeanGetterNestedMultiLevel(Field top, Field[] path, EventType fragmentEventType, EventAdapterService eventAdapterService)
        {
            _top = top;
            _path = path;
            _fragmentEventType = fragmentEventType;
            _eventAdapterService = eventAdapterService;
        }

        public Object Get(EventBean eventBean)
        {
            return GetRecordValueTopWPath((GenericRecord)eventBean.Underlying, _top, _path);
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
            Object value = Get(eventBean);
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
                .MethodReturn(
                    ExprDotMethod(
                        Ref(mSvc.MemberName), "AdapterForTypedAvro", Ref("value"),
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
            return StaticMethod(
                typeof(AvroEventBeanGetterNestedMultiLevel), "GetRecordValueTopWPath", 
                underlyingExpression, 
                Constant(_top),
                Constant(_path));
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetFragmentCodegen(context), underlyingExpression);
        }
    }
} // end of namespace
