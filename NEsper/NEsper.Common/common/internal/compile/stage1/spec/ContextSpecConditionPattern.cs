///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class ContextSpecConditionPattern : ContextSpecCondition
    {
        public ContextSpecConditionPattern(
            EvalForgeNode patternRaw,
            bool inclusive,
            bool immediate)
        {
            PatternRaw = patternRaw;
            IsInclusive = inclusive;
            IsImmediate = immediate;
        }

        public EvalForgeNode PatternRaw { get; }

        public PatternStreamSpecCompiled PatternCompiled { get; set; }

        public bool IsInclusive { get; }

        public bool IsImmediate { get; }

        public PatternContext PatternContext { get; set; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContextConditionDescriptorPattern), GetType(), classScope);
            method.Block
                .DeclareVar<ContextConditionDescriptorPattern>(
                    "condition",
                    NewInstance(typeof(ContextConditionDescriptorPattern)))
                .SetProperty(
                    Ref("condition"),
                    "Pattern",
                    LocalMethod(PatternCompiled.Root.MakeCodegen(method, symbols, classScope)))
                .SetProperty(Ref("condition"), "PatternContext", PatternContext.Make(method, symbols, classScope))
                .SetProperty(
                    Ref("condition"),
                    "TaggedEvents",
                    Constant(PatternCompiled.TaggedEventTypes.Keys.ToArray()))
                .SetProperty(Ref("condition"), "ArrayEvents", Constant(PatternCompiled.ArrayEventTypes.Keys.ToArray()))
                .SetProperty(Ref("condition"), "Inclusive", Constant(IsInclusive))
                .SetProperty(Ref("condition"), "IsImmediate", Constant(IsImmediate))
                .MethodReturn(Ref("condition"));
            return LocalMethod(method);
        }
    }
} // end of namespace