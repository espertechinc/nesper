///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.util;

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
            string delegateFactoryClassName,
            string serdeClassName,
            IDictionary<string, JsonUnderlyingField> fieldDescriptors,
            bool dynamic,
            int numFieldsSupertype)
        {
            UnderlyingClassName = underlyingClassName;
            OptionalUnderlyingProvided = optionalUnderlyingProvided;
            DelegateClassName = delegateClassName;
            DelegateFactoryClassName = delegateFactoryClassName;
            SerdeClassName = serdeClassName;
            FieldDescriptors = fieldDescriptors;
            IsDynamic = dynamic;
            NumFieldsSupertype = numFieldsSupertype;
        }

        public bool IsDynamic { get; set; }

        public string UnderlyingClassName { get; set; }

        public string DelegateClassName { get; set; }

        public string DelegateFactoryClassName { get; set; }

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
                .DeclareVar(typeof(JsonEventTypeDetail), "detail", NewInstance(typeof(JsonEventTypeDetail)))
                .SetProperty(Ref("detail"), "UnderlyingClassName", Constant(UnderlyingClassName))
                .SetProperty(Ref("detail"), "OptionalUnderlyingProvided", Constant(OptionalUnderlyingProvided))
                .SetProperty(Ref("detail"), "DelegateClassName", Constant(DelegateClassName))
                .SetProperty(Ref("detail"), "DelegateFactoryClassName", Constant(DelegateFactoryClassName))
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

            foreach (KeyValuePair<string, JsonUnderlyingField> entry in FieldDescriptors) {
                method.Block.ExprDotMethod(Ref("fields"), "put", Constant(entry.Key), entry.Value.ToExpression());
            }

            method.Block.MethodReturn(Ref("fields"));
            return method;
        }
    }
} // end of namespace