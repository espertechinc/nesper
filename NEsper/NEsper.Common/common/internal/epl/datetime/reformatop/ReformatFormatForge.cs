///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.@join.analyze;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public class ReformatFormatForge : ReformatForge,
        ReformatOp
    {
        private readonly ExprForge formatter;
        private readonly ReformatFormatForgeDesc formatterType;
        private readonly TimeAbacus timeAbacus;

        public ReformatFormatForge(
            ReformatFormatForgeDesc formatterType,
            ExprForge formatter,
            TimeAbacus timeAbacus)
        {
            this.formatterType = formatterType;
            this.formatter = formatter;
            this.timeAbacus = timeAbacus;
        }

        private DateFormat DateFormatFormatter => (DateFormat) formatter.ExprEvaluator.Evaluate(null, true, null);

        public ReformatOp Op => this;

        public Type ReturnType => typeof(string);

        public FilterExprAnalyzerAffector GetFilterDesc(
            EventType[] typesPerStream,
            DateTimeMethodEnum currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            return null;
        }

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var formatField = CodegenFormatFieldInit(codegenClassScope);
            var blockMethod = codegenMethodScope
                .MakeChild(typeof(string), typeof(ReformatFormatForge), codegenClassScope)
                .AddParam(typeof(DateTimeEx), "dtx")
                .Block
                .LockOn(formatField)
                .BlockReturn(ExprDotMethod(formatField, "format", ExprDotMethod(Ref("dtx"), "getTime")));
            return LocalMethodBuild(blockMethod.MethodEnd()).Pass(inner).Call();
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var formatField = CodegenFormatFieldInit(codegenClassScope);
            return ExprDotMethod(inner, "format", formatField);
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var formatField = CodegenFormatFieldInit(codegenClassScope);
            return ExprDotMethod(inner, "format", formatField);
        }

        public CodegenExpression CodegenLong(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope classScope)
        {
            var formatField = CodegenFormatFieldInit(classScope);
            var blockMethod = codegenMethodScope.MakeChild(typeof(string), typeof(ReformatFormatForge), classScope)
                .AddParam(typeof(long), "ts")
                .Block;
            var syncBlock = blockMethod.LockOn(formatField);
            if (timeAbacus.OneSecond == 1000L) {
                syncBlock.BlockReturn(ExprDotMethod(formatField, "format", Ref("ts")));
            }
            else {
                syncBlock.BlockReturn(ExprDotMethod(formatField, "format", timeAbacus.ToDateCodegen(Ref("ts"))));
            }

            return LocalMethodBuild(blockMethod.MethodEnd()).Pass(inner).Call();
        }

        public object Evaluate(
            long ts,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            lock (this) {
                return DateFormatFormatter.Format(timeAbacus.ToDateTimeEx(ts));
            }
        }

        public object Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return DateFormatFormatter.Format(dateTimeEx);
        }

        public object Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return DateFormatFormatter.Format(dateTimeOffset);
        }

        public object Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return DateFormatFormatter.Format(dateTime);
        }

        private CodegenExpressionField CodegenFormatFieldInit(CodegenClassScope classScope)
        {
            var formatEvalCall = CodegenLegoMethodExpression.CodegenExpression(
                formatter,
                classScope.NamespaceScope.InitMethod,
                classScope);
            var formatEval = LocalMethod(formatEvalCall, ConstantNull(), ConstantTrue(), ConstantNull());
            CodegenExpression init;
            if (formatterType.FormatterType != typeof(string)) {
                init = formatEval;
            }
            else {
                var parse = classScope.NamespaceScope.InitMethod.MakeChild(
                    typeof(DateFormat),
                    GetType(),
                    classScope);
                parse.Block.MethodReturn(NewInstance<SimpleDateFormat>(formatEval));
                init = LocalMethod(parse);
            }

            return classScope.AddFieldUnshared(
                true,
                typeof(DateFormat),
                init);
        }
    }
} // end of namespace