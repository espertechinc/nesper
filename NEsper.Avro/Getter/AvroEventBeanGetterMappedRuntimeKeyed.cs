///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterMappedRuntimeKeyed : EventPropertyGetterMappedSPI
    {
        private readonly Field _pos;

        public AvroEventBeanGetterMappedRuntimeKeyed(Field pos)
        {
            _pos = pos;
        }

        public object Get(
            EventBean @event,
            string key)
        {
            var record = (GenericRecord) @event.Underlying;
            var values = (IDictionary<string, object>) record.Get(_pos);
            return AvroEventBeanGetterMapped.GetAvroMappedValueWNullCheck(values, key);
        }

        public CodegenExpression EventBeanGetMappedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            var method = codegenMethodScope.MakeChild(
                    typeof(object),
                    typeof(AvroEventBeanGetterMappedRuntimeKeyed),
                    codegenClassScope)
                .AddParam(typeof(EventBean), "event")
                .AddParam(typeof(string), "key")
                .Block
                .DeclareVar<GenericRecord>(
                    "record",
                    CodegenExpressionBuilder.CastUnderlying(
                        typeof(GenericRecord),
                        CodegenExpressionBuilder.Ref("event")))
                .DeclareVar<IDictionary<string, object>>(
                    "values",
                    CodegenExpressionBuilder.Cast(
                        typeof(IDictionary<string, object>),
                        CodegenExpressionBuilder.ExprDotMethod(
                            CodegenExpressionBuilder.Ref("record"),
                            "Get",
                            CodegenExpressionBuilder.Constant(_pos))))
                .MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(AvroEventBeanGetterMapped),
                        "GetAvroMappedValueWNullCheck",
                        CodegenExpressionBuilder.Ref("values"),
                        CodegenExpressionBuilder.Ref("key")));
            return CodegenExpressionBuilder.LocalMethodBuild(method).Pass(beanExpression).Pass(key).Call();
        }
    }
} // end of namespace