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

namespace com.espertech.esper.common.@internal.epl.pattern.and
{
    /// <summary>
    ///     This class represents an 'and' operator in the evaluation tree representing an event expressions.
    /// </summary>
    public class EvalAndForgeNode : EvalForgeNodeBase
    {
        public bool IsFilterChildNonQuitting => false;

        public bool IsStateful => true;

        public override PatternExpressionPrecedenceEnum Precedence => PatternExpressionPrecedenceEnum.AND;

        public override string ToString()
        {
            return "EvalAndFactoryNode children=" + ChildNodes.Count;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            PatternExpressionUtil.ToPrecedenceFreeEPL(writer, "and", ChildNodes, Precedence);
        }

        protected override Type TypeOfFactory()
        {
            return typeof(EvalAndFactoryNode);
        }

        protected override string NameOfFactory()
        {
            return "And";
        }

        protected override void InlineCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.DeclareVar<EvalFactoryNode[]>(
                "children",
                NewArrayByLength(typeof(EvalFactoryNode), Constant(ChildNodes.Count)));
            for (var i = 0; i < ChildNodes.Count; i++) {
                method.Block.AssignArrayElement(
                    Ref("children"),
                    Constant(i),
                    LocalMethod(ChildNodes[i].MakeCodegen(method, symbols, classScope)));
            }

            method.Block.SetProperty(Ref("node"), "Children", Ref("children"));
        }

        public override void CollectSelfFilterAndSchedule(
            IList<FilterSpecCompiled> filters,
            IList<ScheduleHandleCallbackProvider> schedules)
        {
        }
    }
} // end of namespace