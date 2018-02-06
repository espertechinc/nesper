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
using com.espertech.esper.compat;
using com.espertech.esper.events;

using NEsper.Avro.Extensions;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace NEsper.Avro.Getter
{
    using Map = IDictionary<string, object>;

    public class AvroEventBeanGetterNestedMapped : EventPropertyGetterSPI
    {
        private readonly Field _top;
        private readonly Field _pos;
        private readonly string _key;

        public AvroEventBeanGetterNestedMapped(Field top, Field pos, string key)
        {
            _top = top;
            _pos = pos;
            _key = key;
        }

        public Object Get(EventBean eventBean)
        {
            GenericRecord record = (GenericRecord)eventBean.Underlying;
            GenericRecord inner = (GenericRecord)record.Get(_top);
            if (inner == null)
            {
                return null;
            }

            var map = inner.Get(_pos).AsStringDictionary();
            return AvroEventBeanGetterMapped.GetAvroMappedValueWNullCheck(map, _key);
        }

        private string GetCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(GenericRecord), "record", GetType())
                .DeclareVar(
                    typeof(GenericRecord), "inner", Cast(
                        typeof(GenericRecord),
                        ExprDotMethod(Ref("record"), "Get", Constant(_top))))
                .IfRefNullReturnNull("inner")
                .DeclareVar(
                    typeof(Map), "map",
                    Cast(typeof(Map), ExprDotMethod(Ref("inner"), "Get", Constant(_pos))))
                .MethodReturn(
                    StaticMethod(
                        typeof(AvroEventBeanGetterMapped), "GetAvroMappedValueWNullCheck", Ref("map"), Constant(_key)));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
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
            return ConstantTrue();
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
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantNull();
        }
    }
} // end of namespace
