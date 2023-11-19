///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.matchuntil
{
    /// <summary>
    /// This class represents a match-until observer in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalMatchUntilForgeNode : EvalForgeNodeBase
    {
        private ExprNode lowerBounds;
        private ExprNode upperBounds;
        private ExprNode singleBound;
        private MatchedEventConvertorForge convertor;
        private int[] tagsArrayed;

        public EvalMatchUntilForgeNode(
            bool attachPatternText,
            ExprNode lowerBounds,
            ExprNode upperBounds,
            ExprNode singleBound) : base(attachPatternText)
        {
            if (singleBound != null && (lowerBounds != null || upperBounds != null)) {
                throw new ArgumentException("Invalid bounds, specify either single bound or range bounds");
            }

            this.lowerBounds = lowerBounds;
            this.upperBounds = upperBounds;
            this.singleBound = singleBound;
        }

        protected override Type TypeOfFactory => typeof(EvalMatchUntilFactoryNode);

        protected override string NameOfFactory => "MatchUntil";

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
            if ((lowerBounds == null || lowerBounds.Forge.ForgeConstantType.IsCompileTimeConstant) &&
                (upperBounds == null || upperBounds.Forge.ForgeConstantType.IsCompileTimeConstant) &&
                (singleBound == null || singleBound.Forge.ForgeConstantType.IsCompileTimeConstant)) {
                converterExpression = ConstantNull();
            }
            else {
                converterExpression = convertor.MakeAnonymous(method, classScope);
            }

            method.Block
                .SetProperty(node, "Children", Ref("children"))
                .SetProperty(
                    node,
                    "LowerBounds",
                    lowerBounds == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            lowerBounds.Forge,
                            method,
                            GetType(),
                            classScope))
                .SetProperty(
                    node,
                    "UpperBounds",
                    upperBounds == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            upperBounds.Forge,
                            method,
                            GetType(),
                            classScope))
                .SetProperty(
                    node,
                    "SingleBound",
                    singleBound == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            singleBound.Forge,
                            method,
                            GetType(),
                            classScope))
                .SetProperty(node, "TagsArrayed", Constant(tagsArrayed))
                .SetProperty(node, "OptionalConvertor", converterExpression);
        }

        public override void CollectSelfFilterAndSchedule(
            Func<short, CallbackAttribution> callbackAttribution,
            IList<FilterSpecTracked> filters,
            IList<ScheduleHandleTracked> schedules)
        {
            // nothing for this node, children navigated elsewhere
        }

        /// <summary>
        /// Returns an array of tags for events, which is all tags used within the repeat-operator.
        /// </summary>
        /// <value>array of tags</value>
        public int[] TagsArrayed => tagsArrayed;

        public ExprNode LowerBounds {
            get => lowerBounds;
            set => lowerBounds = value;
        }

        public ExprNode UpperBounds {
            get => upperBounds;
            set => upperBounds = value;
        }

        public ExprNode SingleBound {
            get => singleBound;
            set => singleBound = value;
        }

        /// <summary>
        /// Sets the tags used within the repeat operator.
        /// </summary>
        /// <value>tags used within the repeat operator</value>
        public int[] TagsArrayedSet {
            set => tagsArrayed = value;
        }

        /// <summary>
        /// Sets the convertor for matching events to events-per-stream.
        /// </summary>
        /// <value>convertor</value>
        public MatchedEventConvertorForge Convertor {
            get => convertor;
            set => convertor = value;
        }

        public override string ToString()
        {
            return "EvalMatchUntilNode children=" + ChildNodes.Count;
        }

        public bool IsFilterChildNonQuitting => true;

        public bool IsStateful => true;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (singleBound != null) {
                writer.Write("[");
                writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(singleBound));
                writer.Write("] ");
            }
            else {
                if (lowerBounds != null || upperBounds != null) {
                    writer.Write("[");
                    if (lowerBounds != null) {
                        writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(lowerBounds));
                    }

                    writer.Write(":");
                    if (upperBounds != null) {
                        writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(upperBounds));
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

        public override PatternExpressionPrecedenceEnum Precedence => PatternExpressionPrecedenceEnum.REPEAT_UNTIL;

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.PATTERN_MATCHUNTIL;
        }
    }
} // end of namespace