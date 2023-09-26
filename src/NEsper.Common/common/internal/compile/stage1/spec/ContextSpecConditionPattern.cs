///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class ContextSpecConditionPattern : ContextSpecCondition
    {
        private readonly EvalForgeNode patternRaw;
        private readonly bool inclusive;
        private readonly bool immediate;
        private readonly string asName;
        private string[] patternTags;
        private EventType asNameEventType;
        private PatternStreamSpecCompiled patternCompiled;
        private PatternContext patternContext;

        public ContextSpecConditionPattern(
            EvalForgeNode patternRaw,
            bool inclusive,
            bool immediate,
            string asName)
        {
            this.patternRaw = patternRaw;
            this.inclusive = inclusive;
            this.immediate = immediate;
            this.asName = asName;
        }

        public bool IsInclusive => inclusive;

        public bool IsImmediate => immediate;

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContextConditionDescriptorPattern), GetType(), classScope);
            method.Block.DeclareVarNewInstance(typeof(ContextConditionDescriptorPattern), "condition")
                .SetProperty(
                    Ref("condition"),
                    "Pattern",
                    LocalMethod(patternCompiled.Root.MakeCodegen(method, symbols, classScope)))
                .SetProperty(
                    Ref("condition"),
                    "PatternContext",
                    patternContext.Make(method, symbols, classScope))
                .SetProperty(
                    Ref("condition"),
                    "TaggedEvents",
                    Constant(CollectionUtil.ToArray(patternCompiled.TaggedEventTypes.Keys)))
                .SetProperty(
                    Ref("condition"),
                    "ArrayEvents",
                    Constant(PatternCompiled.ArrayEventTypes.Keys.ToArray()))
                .SetProperty(
                    Ref("condition"),
                    "IsInclusive",
                    Constant(inclusive))
                .SetProperty(
                    Ref("condition"),
                    "IsImmediate",
                    Constant(immediate))
                .SetProperty(
                    Ref("condition"),
                    "AsName",
                    Constant(asName))
                .SetProperty(
                    Ref("condition"),
                    "PatternTags",
                    Constant(patternTags))
                .SetProperty(
                    Ref("condition"),
                    "AsNameEventType",
                    asNameEventType == null
                        ? ConstantNull()
                        : EventTypeUtility.ResolveTypeCodegen(asNameEventType, symbols.GetAddInitSvc(method)))
                .MethodReturn(Ref("condition"));
            return LocalMethod(method);
        }

        public T Accept<T>(ContextSpecConditionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public EvalForgeNode PatternRaw => patternRaw;

        public PatternStreamSpecCompiled PatternCompiled {
            get => patternCompiled;

            set => patternCompiled = value;
        }

        public PatternContext PatternContext {
            get => patternContext;

            set => patternContext = value;
        }

        public EventType AsNameEventType {
            get => asNameEventType;

            set => asNameEventType = value;
        }

        public string[] PatternTags {
            set => patternTags = value;
        }

        public string AsName => asName;
    }
} // end of namespace