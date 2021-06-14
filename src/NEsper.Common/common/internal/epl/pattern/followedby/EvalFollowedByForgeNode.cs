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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.followedby
{
    /// <summary>
    ///     This class represents a followed-by operator in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalFollowedByForgeNode : EvalForgeNodeBase
    {
        public EvalFollowedByForgeNode(bool attachPatternText, IList<ExprNode> optionalMaxExpressions)
            : base(attachPatternText)
        {
            OptionalMaxExpressions = optionalMaxExpressions;
        }

        public IList<ExprNode> OptionalMaxExpressions { get; set; }

        public override PatternExpressionPrecedenceEnum Precedence => PatternExpressionPrecedenceEnum.FOLLOWEDBY;

        public override string ToString()
        {
            return "EvalFollowedByNode children=" + ChildNodes.Count;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (OptionalMaxExpressions == null || OptionalMaxExpressions.IsEmpty()) {
                PatternExpressionUtil.ToPrecedenceFreeEPL(writer, "->", ChildNodes, Precedence);
            }
            else {
                ChildNodes[0].ToEPL(writer, PatternExpressionPrecedenceEnum.MINIMUM);
                for (var i = 1; i < ChildNodes.Count; i++) {
                    ExprNode optionalMaxExpression = null;
                    if (OptionalMaxExpressions.Count > i - 1) {
                        optionalMaxExpression = OptionalMaxExpressions[i - 1];
                    }

                    if (optionalMaxExpression == null) {
                        writer.Write(" -> ");
                    }
                    else {
                        writer.Write(" -[");
                        writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(optionalMaxExpression));
                        writer.Write("]> ");
                    }

                    ChildNodes[i].ToEPL(writer, PatternExpressionPrecedenceEnum.MINIMUM);
                }
            }
        }

        protected override Type TypeOfFactory()
        {
            return typeof(EvalFollowedByFactoryNode);
        }

        protected override string NameOfFactory()
        {
            return "Followedby";
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

            method.Block
                .SetProperty(Ref("node"), "Children", Ref("children"))
                .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add("AddReadyCallback", Ref("node")));

            if (OptionalMaxExpressions != null && !OptionalMaxExpressions.IsEmpty()) {
                method.Block.DeclareVar<ExprEvaluator[]>(
                    "evals",
                    NewArrayByLength(typeof(ExprEvaluator), Constant(ChildNodes.Count - 1)));

                for (var i = 0; i < ChildNodes.Count - 1; i++) {
                    if (OptionalMaxExpressions.Count <= i) {
                        continue;
                    }

                    var optionalMaxExpression = OptionalMaxExpressions[i];
                    if (optionalMaxExpression == null) {
                        continue;
                    }

                    method.Block.AssignArrayElement(
                        "evals",
                        Constant(i),
                        ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(
                            optionalMaxExpression.Forge,
                            method,
                            GetType(),
                            classScope));
                }

                method.Block.SetProperty(Ref("node"), "MaxPerChildEvals", Ref("evals"));
            }
        }

        public override void CollectSelfFilterAndSchedule(
            IList<FilterSpecCompiled> filters,
            IList<ScheduleHandleCallbackProvider> schedules)
        {
        }
    }
} // end of namespace