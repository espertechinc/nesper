///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.compile
{
    public class ContextMetaData
    {
        public ContextMetaData(
            string contextName,
            string contextModuleName,
            NameAccessModifier contextVisibility,
            EventType eventType,
            ContextControllerPortableInfo[] validationInfos)
        {
            ContextName = contextName;
            ContextModuleName = contextModuleName;
            ContextVisibility = contextVisibility;
            EventType = eventType;
            ValidationInfos = validationInfos;
        }

        public EventType EventType { get; }

        public ContextControllerPortableInfo[] ValidationInfos { get; }

        public string ContextName { get; }

        public string ContextModuleName { get; }

        public NameAccessModifier ContextVisibility { get; }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            var validationInfos = new CodegenExpression[ValidationInfos.Length];
            for (var i = 0; i < validationInfos.Length; i++) {
                validationInfos[i] = ValidationInfos[i].Make(addInitSvc);
            }

            return NewInstance<ContextMetaData>(
                Constant(ContextName),
                Constant(ContextModuleName),
                Constant(ContextVisibility),
                EventTypeUtility.ResolveTypeCodegen(EventType, addInitSvc),
                NewArrayWithInit(typeof(ContextControllerPortableInfo), validationInfos));
        }
    }
} // end of namespace