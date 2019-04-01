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

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using static NEsper.Avro.Getter.AvroEventBeanGetterDynamicPoly;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedPoly : EventPropertyGetterSPI
    {
        private readonly Field _top;
        private readonly AvroEventPropertyGetter[] _getters;

        public AvroEventBeanGetterNestedPoly(Field top, AvroEventPropertyGetter[] getters)
        {
            _top = top;
            _getters = getters;
        }

        public Object Get(EventBean eventBean)
        {
            GenericRecord record = (GenericRecord) eventBean.Underlying;
            GenericRecord inner = (GenericRecord) record.Get(_top);
            return AvroEventBeanGetterDynamicPoly.GetAvroFieldValuePoly(inner, _getters);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            GenericRecord record = (GenericRecord) eventBean.Underlying;
            GenericRecord inner = (GenericRecord) record.Get(_top);
            return GetAvroFieldValuePolyExists(inner, _getters);
        }

        public Object GetFragment(EventBean eventBean)
        {
            GenericRecord record = (GenericRecord) eventBean.Underlying;
            GenericRecord inner = (GenericRecord) record.Get(_top);
            return GetAvroFieldFragmentPoly(inner, _getters);
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
            return LocalMethod(
                GetAvroFieldValuePolyCodegen(context, _getters), Cast(
                    typeof(GenericRecord),
                    ExprDotMethod(underlyingExpression, "Get", Constant(_top))));
        }

        public ICodegenExpression CodegenUnderlyingExists(
            ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(
                GetAvroFieldValuePolyExistsCodegen(context, _getters), Cast(
                    typeof(GenericRecord),
                    ExprDotMethod(underlyingExpression, "Get", Constant(_top))));
        }

        public ICodegenExpression CodegenUnderlyingFragment(
            ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(
                GetAvroFieldFragmentPolyCodegen(context, _getters), Cast(
                    typeof(GenericRecord),
                    ExprDotMethod(underlyingExpression, "Get", Constant(_top))));
        }
    }
} // end of namespace
