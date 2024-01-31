///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.compiletime;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.core
{
    public class JsonEventTypeDetail
    {
        public JsonEventTypeDetail()
        {
        }

        public JsonEventTypeDetail(
            string underlyingClassName,
            Type optionalUnderlyingProvided,
            string delegateClassName,
            string deserializerClassName,
            string serializerClassName,
            string serdeClassName,
            IDictionary<string, JsonUnderlyingField> fieldDescriptors,
            bool dynamic,
            int numFieldsSupertype)
        {
            UnderlyingClassName = underlyingClassName;
            OptionalUnderlyingProvided = optionalUnderlyingProvided;
            DelegateClassName = delegateClassName;
            DeserializerClassName = deserializerClassName;
            SerializerClassName = serializerClassName;
            SerdeClassName = serdeClassName;
            FieldDescriptors = fieldDescriptors;
            IsDynamic = dynamic;
            NumFieldsSupertype = numFieldsSupertype;
        }

        public bool IsDynamic { get; set; }

        public string UnderlyingClassName { get; set; }

        public string DelegateClassName { get; set; }

        public string DeserializerClassName { get; set; }

        public string SerializerClassName { get; set; }

        public Type OptionalUnderlyingProvided { set; get; }

        public string SerdeClassName { get; set; }

        public IDictionary<string, JsonUnderlyingField> FieldDescriptors { get; set; }

        public int NumFieldsSupertype { get; set; }

        public CodegenExpression ToExpression(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(JsonEventTypeDetail), typeof(JsonEventTypeDetail), classScope);
            method.Block
                .DeclareVarNewInstance<JsonEventTypeDetail>("detail")
                .SetProperty(Ref("detail"), "UnderlyingClassName", Constant(UnderlyingClassName))
                .SetProperty(Ref("detail"), "OptionalUnderlyingProvided", Constant(OptionalUnderlyingProvided))
                .SetProperty(Ref("detail"), "DelegateClassName", Constant(DelegateClassName))
                .SetProperty(Ref("detail"), "DeserializerClassName", Constant(DeserializerClassName))
                .SetProperty(Ref("detail"), "SerializerClassName", Constant(SerializerClassName))
                .SetProperty(Ref("detail"), "SerdeClassName", Constant(SerdeClassName))
                .SetProperty(Ref("detail"), "FieldDescriptors", LocalMethod(MakeFieldDescCodegen(method, classScope)))
                .SetProperty(Ref("detail"), "IsDynamic", Constant(IsDynamic))
                .SetProperty(Ref("detail"), "NumFieldsSupertype", Constant(NumFieldsSupertype))
                .MethodReturn(Ref("detail"));
            return LocalMethod(method);
        }

        private CodegenMethod MakeFieldDescCodegen(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent
                .MakeChild(
                    typeof(IDictionary<string, JsonUnderlyingField>),
                    typeof(JsonEventTypeDetail),
                    classScope);

            method.Block.DeclareVar(
                typeof(IDictionary<string, JsonUnderlyingField>),
                "fields",
                NewInstance(typeof(Dictionary<string, JsonUnderlyingField>)));

            foreach (var entry in FieldDescriptors) {
                method.Block.ExprDotMethod(
                    Ref("fields"),
                    "Put",
                    Constant(entry.Key),
                    entry.Value.ToCodegenExpression());
            }

            method.Block.MethodReturn(Ref("fields"));
            return method;
        }
    }
} // end of namespace