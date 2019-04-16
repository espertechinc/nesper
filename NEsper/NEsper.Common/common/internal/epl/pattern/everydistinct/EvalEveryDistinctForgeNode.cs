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
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.everydistinct
{
    /// <summary>
    ///     This class represents an 'every-distinct' operator in the evaluation tree representing an event expression.
    /// </summary>
    public class EvalEveryDistinctForgeNode : EvalForgeNodeBase
    {
        [NonSerialized] private MatchedEventConvertorForge convertor;

        private ExprNode expiryTimeExp;
        private TimePeriodComputeForge timePeriodComputeForge;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="expressions">distinct-value expressions</param>
        public EvalEveryDistinctForgeNode(IList<ExprNode> expressions)
        {
            Expressions = expressions;
        }

        public MatchedEventConvertorForge Convertor {
            get => convertor;
            set => convertor = value;
        }

        /// <summary>
        ///     Returns all expressions.
        /// </summary>
        /// <returns>expressions</returns>
        public IList<ExprNode> Expressions { get; }

        /// <summary>
        ///     Returns distinct expressions.
        /// </summary>
        /// <returns>expressions</returns>
        public IList<ExprNode> DistinctExpressions { get; private set; }

        public bool IsFilterChildNonQuitting => true;

        public bool IsStateful => true;

        public override PatternExpressionPrecedenceEnum Precedence => PatternExpressionPrecedenceEnum.UNARY;

        protected override Type TypeOfFactory()
        {
            return typeof(EvalEveryDistinctFactoryNode);
        }

        protected override string NameOfFactory()
        {
            return "everyDistinct";
        }

        protected override void InlineCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(
                    Ref("node"), "ChildNode",
                    LocalMethod(ChildNodes[0].MakeCodegen(method, symbols, classScope)))
                .SetProperty(
                    Ref("node"), "DistinctExpression",
                    ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                        ExprNodeUtilityQuery.GetForges(
                            ExprNodeUtilityQuery.ToArray(DistinctExpressions)), null, method, GetType(),
                        classScope))
                .SetProperty(
                    Ref("node"), "DistinctTypes",
                    Constant(ExprNodeUtilityQuery.GetExprResultTypes(DistinctExpressions)))
                .SetProperty(
                    Ref("node"), "Convertor", 
                    convertor.MakeAnonymous(method, classScope))
                .SetProperty(
                    Ref("node"), "TimePeriodCompute",
                    timePeriodComputeForge == null
                        ? ConstantNull()
                        : timePeriodComputeForge.MakeEvaluator(method, classScope));
        }

        public override void CollectSelfFilterAndSchedule(
            IList<FilterSpecCompiled> filters,
            IList<ScheduleHandleCallbackProvider> schedules)
        {
            // nothing to collect for this node
        }

        public override string ToString()
        {
            return "EvalEveryNode children=" + ChildNodes.Count;
        }

        /// <summary>
        ///     Sets the convertor for matching events to events-per-stream.
        /// </summary>
        /// <param name="convertor">convertor</param>
        public void SetConvertor(MatchedEventConvertorForge convertor)
        {
            this.convertor = convertor;
        }

        public void SetDistinctExpressions(
            IList<ExprNode> distinctExpressions,
            TimePeriodComputeForge timePeriodComputeForge,
            ExprNode expiryTimeExp)
        {
            DistinctExpressions = distinctExpressions;
            this.timePeriodComputeForge = timePeriodComputeForge;
            this.expiryTimeExp = expiryTimeExp;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("every-distinct(");
            ExprNodeUtilityPrint.ToExpressionStringParameterList(DistinctExpressions, writer);
            if (expiryTimeExp != null) {
                writer.Write(",");
                writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(expiryTimeExp));
            }

            writer.Write(") ");
            ChildNodes[0].ToEPL(writer, Precedence);
        }
    }
} // end of namespace