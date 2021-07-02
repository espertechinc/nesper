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
using System.Reflection;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.matchuntil
{
    /// <summary>
    ///     This class represents a match-until observer in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalMatchUntilForgeNode : EvalForgeNodeBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EvalMatchUntilForgeNode(
            bool attachPatternText,
            ExprNode lowerBounds,
            ExprNode upperBounds,
            ExprNode singleBound) : base(attachPatternText)
        {
            if (singleBound != null && (lowerBounds != null || upperBounds != null)) {
                throw new ArgumentException("Invalid bounds, specify either single bound or range bounds");
            }

            LowerBounds = lowerBounds;
            UpperBounds = upperBounds;
            SingleBound = singleBound;
        }

        /// <summary>
        ///     Returns an array of tags for events, which is all tags used within the repeat-operator.
        /// </summary>
        /// <value>array of tags</value>
        public int[] TagsArrayed { get; private set; }

        public ExprNode LowerBounds { get; set; }

        public ExprNode UpperBounds { get; set; }

        public ExprNode SingleBound { get; set; }

        /// <summary>
        ///     Sets the tags used within the repeat operator.
        /// </summary>
        /// <value>tags used within the repeat operator</value>
        public int[] TagsArrayedSet {
            set => TagsArrayed = value;
        }

        /// <summary>
        ///     Sets the convertor for matching events to events-per-stream.
        /// </summary>
        /// <value>convertor</value>
        public MatchedEventConvertorForge Convertor { get; set; }

        public bool IsFilterChildNonQuitting => true;

        public bool IsStateful => true;

        public override PatternExpressionPrecedenceEnum Precedence => PatternExpressionPrecedenceEnum.REPEAT_UNTIL;

        protected override Type TypeOfFactory()
        {
            return typeof(EvalMatchUntilFactoryNode);
        }

        protected override string NameOfFactory()
        {
            return "MatchUntil";
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

            var node = Ref("node");

            CodegenExpression converterExpression;
            if ((LowerBounds == null || LowerBounds.Forge.ForgeConstantType.IsCompileTimeConstant) &&
                (UpperBounds == null || UpperBounds.Forge.ForgeConstantType.IsCompileTimeConstant) &&
                (SingleBound == null || SingleBound.Forge.ForgeConstantType.IsCompileTimeConstant)) {
                converterExpression = ConstantNull();
            }
            else {
                converterExpression = Convertor.MakeAnonymous(method, classScope);
            }

            method.Block
                .SetProperty(node, "Children", Ref("children"))
                .SetProperty(
                    node,
                    "LowerBounds",
                    LowerBounds == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(LowerBounds.Forge, method, GetType(), classScope))
                .SetProperty(
                    node,
                    "UpperBounds",
                    UpperBounds == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(UpperBounds.Forge, method, GetType(), classScope))
                .SetProperty(
                    node,
                    "SingleBound",
                    SingleBound == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(SingleBound.Forge, method, GetType(), classScope))
                .SetProperty(node, "TagsArrayed", Constant(TagsArrayed))
                .SetProperty(node, "OptionalConvertor", converterExpression);
        }

        public override void CollectSelfFilterAndSchedule(
            IList<FilterSpecCompiled> filters,
            IList<ScheduleHandleCallbackProvider> schedules)
        {
            // nothing for this node, children navigated elsewhere
        }

        public override string ToString()
        {
            return "EvalMatchUntilNode children=" + ChildNodes.Count;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (SingleBound != null) {
                writer.Write("[");
                writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(SingleBound));
                writer.Write("] ");
            }
            else {
                if (LowerBounds != null || UpperBounds != null) {
                    writer.Write("[");
                    if (LowerBounds != null) {
                        writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(LowerBounds));
                    }

                    writer.Write(":");
                    if (UpperBounds != null) {
                        writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(UpperBounds));
                    }

                    writer.Write("] ");
                }
            }

            ChildNodes[0].ToEPL(writer, Precedence);
            if (ChildNodes.Count > 1) {
                writer.Write(" until ");
                ChildNodes[1].ToEPL(writer, Precedence);
            }
        }

        protected override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.PATTERN_MATCHUNTIL;
        }
    }
} // end of namespace