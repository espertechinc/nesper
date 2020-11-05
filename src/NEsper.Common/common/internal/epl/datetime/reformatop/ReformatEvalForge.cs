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
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public class ReformatEvalForge : ReformatForge,
        ReformatOp
    {
        private readonly DateTimeExEval _dateTimeExEval;
        private readonly TimeAbacus _timeAbacus;

        public ReformatEvalForge(
            DateTimeExEval dateTimeExEval,
            TimeAbacus timeAbacus)
        {
            this._dateTimeExEval = dateTimeExEval;
            this._timeAbacus = timeAbacus;
        }

        public ReformatOp Op => this;

        public CodegenExpression CodegenLong(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression timeZoneField =
                codegenClassScope.AddOrGetDefaultFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var method = codegenMethodScope.MakeChild(typeof(int), typeof(ReformatEvalForge), codegenClassScope)
                .AddParam(typeof(long), "ts");
            method.Block
                .DeclareVar<DateTimeEx>("dtx", StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField))
                .Expression(_timeAbacus.DateTimeSetCodegen(Ref("ts"), Ref("dtx"), method, codegenClassScope))
                .MethodReturn(_dateTimeExEval.Codegen(Ref("dtx")));
            return LocalMethodBuild(method).Pass(inner).Call();
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var timeZoneField =
                codegenClassScope.AddOrGetDefaultFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var methodNode = codegenMethodScope
                .MakeChild(typeof(int), typeof(ReformatEvalForge), codegenClassScope)
                .AddParam(typeof(DateTime), "d");

            methodNode.Block
                .DeclareVar<DateTimeEx>("dtx", StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField))
                .Expression(ExprDotMethod(Ref("dtx"), "Set", Ref("d")))
                .MethodReturn(_dateTimeExEval.Codegen(Ref("dtx")));
            
            return LocalMethod(methodNode, inner);
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression timeZoneField =
                codegenClassScope.AddOrGetDefaultFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var methodNode = codegenMethodScope
                .MakeChild(typeof(int), typeof(ReformatEvalForge), codegenClassScope)
                .AddParam(typeof(DateTimeOffset), "d");

            methodNode.Block
                .DeclareVar<DateTimeEx>("dtx", StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField))
                .Expression(ExprDotMethod(Ref("dtx"), "Set", Ref("d")))
                .MethodReturn(_dateTimeExEval.Codegen(Ref("dtx")));
            return LocalMethod(methodNode, inner);
        }

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return _dateTimeExEval.Codegen(inner);
        }

        public Type ReturnType => typeof(int?);

        public FilterExprAnalyzerAffector GetFilterDesc(
            EventType[] typesPerStream,
            DatetimeMethodDesc currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            return null;
        }

        public object Evaluate(
            long ts,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dateTimeEx = DateTimeEx.NowUtc();
            _timeAbacus.DateTimeSet(ts, dateTimeEx);
            return _dateTimeExEval.EvaluateInternal(dateTimeEx);
        }

        public object Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return _dateTimeExEval.EvaluateInternal(
                DateTimeEx.UtcInstance(dateTimeOffset));
        }

        public object Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return _dateTimeExEval.EvaluateInternal(
                DateTimeEx.UtcInstance(dateTime));
        }

        public object Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return _dateTimeExEval.EvaluateInternal(dateTimeEx);
        }
    }
} // end of namespace