///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.every
{
    /// <summary>
    ///     This class represents an 'every' operator in the evaluation tree representing an event expression.
    /// </summary>
    public class EvalEveryForgeNode : EvalForgeNodeBase
    {
        public override PatternExpressionPrecedenceEnum Precedence => PatternExpressionPrecedenceEnum.UNARY;

        public override string ToString()
        {
            return "EvalEveryNode children=" + ChildNodes.Count;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("every ");
            ChildNodes[0].ToEPL(writer, Precedence);
        }

        protected override Type TypeOfFactory()
        {
            return typeof(EvalEveryFactoryNode);
        }

        protected override string NameOfFactory()
        {
            return "every";
        }

        protected override void InlineCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .ExprDotMethod(
                    Ref("node"), "setChildNode", LocalMethod(ChildNodes[0].MakeCodegen(method, symbols, classScope)));
        }

        public override void CollectSelfFilterAndSchedule(
            IList<FilterSpecCompiled> filters,
            IList<ScheduleHandleCallbackProvider> schedules)
        {
        }
    }
} // end of namespace