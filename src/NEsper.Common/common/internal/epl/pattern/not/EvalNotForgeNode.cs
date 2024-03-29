///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.not
{
    /// <summary>
    ///     This class represents an 'not' operator in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalNotForgeNode : EvalForgeNodeBase
    {
        public override PatternExpressionPrecedenceEnum Precedence => PatternExpressionPrecedenceEnum.UNARY;

        public EvalNotForgeNode(bool attachPatternText) : base(attachPatternText)
        {
        }

        public override string ToString()
        {
            return "EvalNotNode children=" + ChildNodes.Count;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("not ");
            ChildNodes[0].ToEPL(writer, Precedence);
        }

        protected override Type TypeOfFactory => typeof(EvalNotFactoryNode);

        protected override string NameOfFactory => "Not";

        protected override void InlineCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(
                    Ref("node"),
                    "ChildNode",
                    LocalMethod(ChildNodes[0].MakeCodegen(method, symbols, classScope)));
        }

        public override void CollectSelfFilterAndSchedule(
            Func<short, CallbackAttribution> callbackAttribution,
            IList<FilterSpecTracked> filters,
            IList<ScheduleHandleTracked> schedules)
        {
        }

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.PATTERN_NOT;
        }
    }
} // end of namespace