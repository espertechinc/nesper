///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.variable.compiletime
{
    public class VariableMetaData
    {
        public VariableMetaData(
            string variableName,
            string variableModuleName,
            NameAccessModifier variableVisibility,
            string optionalContextName,
            NameAccessModifier? optionalContextVisibility,
            string optionalContextModule,
            Type type,
            EventType eventType,
            bool preconfigured,
            bool constant,
            bool compileTimeConstant,
            object valueWhenAvailable,
            bool createdByCurrentModule)
        {
            VariableName = variableName;
            VariableModuleName = variableModuleName;
            VariableVisibility = variableVisibility;
            OptionalContextName = optionalContextName;
            OptionalContextVisibility = optionalContextVisibility;
            OptionalContextModule = optionalContextModule;
            Type = type;
            EventType = eventType;
            IsPreconfigured = preconfigured;
            IsConstant = constant;
            IsCompileTimeConstant = compileTimeConstant;
            ValueWhenAvailable = valueWhenAvailable;
            IsCreatedByCurrentModule = createdByCurrentModule;
        }

        public string VariableName { get; }

        public string VariableModuleName { get; }

        public string OptionalContextName { get; }

        public Type Type { get; }

        public EventType EventType { get; }

        public bool IsConstant { get; }

        public object ValueWhenAvailable { get; }

        public bool IsPreconfigured { get; }

        public NameAccessModifier? OptionalContextVisibility { get; }

        public string OptionalContextModule { get; }

        public bool IsCompileTimeConstant { get; }

        public NameAccessModifier VariableVisibility { get; }

        public bool IsCreatedByCurrentModule { get; }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            return NewInstance<VariableMetaData>(
                Constant(VariableName),
                Constant(VariableModuleName),
                Constant(VariableVisibility),
                Constant(OptionalContextName),
                Constant(OptionalContextVisibility),
                Constant(OptionalContextModule),
                Typeof(Type),
                EventType == null ? ConstantNull() : EventTypeUtility.ResolveTypeCodegen(EventType, addInitSvc),
                Constant(IsPreconfigured),
                Constant(IsConstant),
                Constant(IsCompileTimeConstant),
                Constant(ValueWhenAvailable),
                Constant(false));
        }
    }
} // end of namespace