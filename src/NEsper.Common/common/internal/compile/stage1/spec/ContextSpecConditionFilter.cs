///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.condition;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class ContextSpecConditionFilter : ContextSpecCondition
    {
        public ContextSpecConditionFilter(
            FilterSpecRaw filterSpecRaw,
            string optionalFilterAsName)
        {
            FilterSpecRaw = filterSpecRaw;
            OptionalFilterAsName = optionalFilterAsName;
        }

        public FilterSpecRaw FilterSpecRaw { get; }

        public string OptionalFilterAsName { get; }

        public FilterSpecCompiled FilterSpecCompiled { get; set; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContextConditionDescriptorFilter), GetType(), classScope);
            method.Block
                .DeclareVar<ContextConditionDescriptorFilter>(
                    "condition",
                    NewInstance(typeof(ContextConditionDescriptorFilter)))
                .SetProperty(
                    Ref("condition"),
                    "FilterSpecActivatable",
                    LocalMethod(FilterSpecCompiled.MakeCodegen(method, symbols, classScope)))
                .SetProperty(Ref("condition"), "OptionalFilterAsName", Constant(OptionalFilterAsName))
                .MethodReturn(Ref("condition"));
            return LocalMethod(method);
        }
    }
} // end of namespace