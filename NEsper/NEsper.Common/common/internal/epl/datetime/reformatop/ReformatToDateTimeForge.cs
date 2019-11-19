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

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public class ReformatToDateTimeForge : ReformatForge,
        ReformatOp
    {
        private readonly TimeAbacus timeAbacus;

        public ReformatToDateTimeForge(TimeAbacus timeAbacus)
        {
            this.timeAbacus = timeAbacus;
        }

        public ReformatOp Op => this;

        public Type ReturnType => typeof(DateTime);

        public FilterExprAnalyzerAffector GetFilterDesc(
            EventType[] typesPerStream,
            DateTimeMethodEnum currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            return null;
        }

        // Aaron you haven't done this yet - do it when you get back

        public CodegenExpression CodegenLong(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(DateTime), typeof(ReformatToDateTimeForge), codegenClassScope)
                .AddParam(typeof(long), "ts")
                .Block
                .DeclareVar<DateTime>(
                    "dateTime",
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(DateTimeHelper),
                        "UtcFromMillis",
                        CodegenExpressionBuilder.Ref("ts")))
                .MethodReturn(CodegenExpressionBuilder.Ref("dateTime"));
            return CodegenExpressionBuilder.LocalMethod(methodNode, inner);
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return inner;
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(DateTime), typeof(ReformatToDateTimeForge), codegenClassScope)
                .AddParam(typeof(DateTimeOffset), "input")
                .Block
                .MethodReturn(
                    CodegenExpressionBuilder.GetProperty(
                        CodegenExpressionBuilder.Ref("input"),
                        "DateTime"));
            return CodegenExpressionBuilder.LocalMethodBuild(method).Pass(inner).Call();
        }

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(DateTime), typeof(ReformatToDateTimeForge), codegenClassScope)
                .AddParam(typeof(DateTimeEx), "input")
                .Block
                .MethodReturn(
                    CodegenExpressionBuilder.GetProperty(
                        CodegenExpressionBuilder.GetProperty(
                            CodegenExpressionBuilder.Ref("input"),
                            "DateTime"),
                        "DateTime"));
            return CodegenExpressionBuilder.LocalMethodBuild(method).Pass(inner).Call();
        }

        public object Evaluate(
            long ts,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return DateTimeHelper.UtcFromMillis(ts);
        }

        public object Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return dateTime;
        }

        public object Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return dateTimeOffset.DateTime;
        }

        public object Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return dateTimeEx.DateTime.DateTime;
        }
    }
}