///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

using NEsper.Avro.Core;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterDynamicPoly : AvroEventPropertyGetter
    {
        private readonly AvroEventPropertyGetter[] _getters;

        public AvroEventBeanGetterDynamicPoly(AvroEventPropertyGetter[] getters)
        {
            _getters = getters;
        }

        public object GetAvroFieldValue(GenericRecord record)
        {
            return GetAvroFieldValuePoly(record, _getters);
        }

        public object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            return GetAvroFieldValue(record);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public object GetAvroFragment(GenericRecord record)
        {
            return null;
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            return GetAvroFieldValuePolyExists(record, _getters);
        }

        internal static bool GetAvroFieldValuePolyExists(GenericRecord record, AvroEventPropertyGetter[] getters)
        {
            if (record == null)
            {
                return false;
            }
            record = NavigatePoly(record, getters);
            return record != null && getters[getters.Length - 1].IsExistsPropertyAvro(record);
        }

        internal static object GetAvroFieldValuePoly(GenericRecord record, AvroEventPropertyGetter[] getters)
        {
            if (record == null)
            {
                return null;
            }
            record = NavigatePoly(record, getters);
            if (record == null)
            {
                return null;
            }
            return getters[getters.Length - 1].GetAvroFieldValue(record);
        }

        internal static object GetAvroFieldFragmentPoly(GenericRecord record, AvroEventPropertyGetter[] getters)
        {
            if (record == null)
            {
                return null;
            }
            record = NavigatePoly(record, getters);
            if (record == null)
            {
                return null;
            }
            return getters[getters.Length - 1].GetAvroFragment(record);
        }

        private static GenericRecord NavigatePoly(GenericRecord record, AvroEventPropertyGetter[] getters)
        {
            for (var i = 0; i < getters.Length - 1; i++)
            {
                var value = getters[i].GetAvroFieldValue(record);
                if (!(value is GenericRecord))
                {
                    return null;
                }
                record = (GenericRecord) value;
            }
            return record;
        }

        internal static string GetAvroFieldValuePolyExistsCodegen(ICodegenContext context, AvroEventPropertyGetter[] getters)
        {
            return context.AddMethod(typeof(bool), typeof(GenericRecord), "record", typeof(AvroEventBeanGetterDynamicPoly))
                .IfRefNullReturnFalse("record")
                .AssignRef("record", LocalMethod(NavigatePolyCodegen(context, getters), Ref("record")))
                .IfRefNullReturnFalse("record")
                .MethodReturn(getters[getters.Length - 1].CodegenUnderlyingExists(Ref("record"), context));
        }

        internal static string GetAvroFieldValuePolyCodegen(ICodegenContext context, AvroEventPropertyGetter[] getters)
        {
            return context.AddMethod(typeof(object), typeof(GenericRecord), "record", typeof(AvroEventBeanGetterDynamicPoly))
                .IfRefNullReturnNull("record")
                .AssignRef("record", LocalMethod(NavigatePolyCodegen(context, getters), Ref("record")))
                .IfRefNullReturnNull("record")
                .MethodReturn(getters[getters.Length - 1].CodegenUnderlyingGet(Ref("record"), context));
        }

        internal static string GetAvroFieldFragmentPolyCodegen(ICodegenContext context, AvroEventPropertyGetter[] getters)
        {
            return context.AddMethod(typeof(object), typeof(GenericRecord), "record", typeof(AvroEventBeanGetterDynamicPoly))
                .IfRefNullReturnNull("record")
                .AssignRef("record", LocalMethod(NavigatePolyCodegen(context, getters), Ref("record")))
                .IfRefNullReturnNull("record")
                .MethodReturn(getters[getters.Length - 1].CodegenUnderlyingFragment(Ref("record"), context));
        }

        private static string NavigatePolyCodegen(ICodegenContext context, AvroEventPropertyGetter[] getters)
        {
            var block = context.AddMethod(typeof(GenericRecord), typeof(GenericRecord), "record", typeof(AvroEventBeanGetterDynamicPoly));
            block.DeclareVar(typeof(object), "value", ConstantNull());
            for (int i = 0; i<getters.Length - 1; i++) {
                block.AssignRef("value", getters[i].CodegenUnderlyingGet(Ref("record"), context))
                    .IfRefNotTypeReturnConst("value", typeof(GenericRecord), null)
                    .AssignRef("record", Cast(typeof(GenericRecord), Ref("value")));
            }
            return block.MethodReturn(Ref("record"));
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(
                CastUnderlying(typeof(GenericRecord), beanExpression), context);
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
            return LocalMethod(GetAvroFieldValuePolyCodegen(context, _getters), underlyingExpression);
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
