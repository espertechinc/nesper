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
    public class ReformatToDateTimeOffsetForge : ReformatForge,
        ReformatOp
    {
        private readonly TimeAbacus _timeAbacus;

        public ReformatToDateTimeOffsetForge(TimeAbacus timeAbacus)
        {
            _timeAbacus = timeAbacus;
        }

        public ReformatOp Op => this;

        public Type ReturnType => typeof(DateTimeOffset);

        public FilterExprAnalyzerAffector GetFilterDesc(
            EventType[] typesPerStream,
            DatetimeMethodDesc currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            return null;
        }

        public CodegenExpression CodegenLong(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var timeZoneField = codegenClassScope.AddOrGetDefaultFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var methodNode = codegenMethodScope
                .MakeChild(typeof(DateTimeOffset), typeof(ReformatToDateTimeOffsetForge), codegenClassScope)
                .AddParam<long>("ts");

            methodNode
                .Block
                .DeclareVar<DateTimeEx>("dtx", StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField))
                .Expression(_timeAbacus.DateTimeSetCodegen(Ref("ts"), Ref("dtx"), methodNode, codegenClassScope))
                .MethodReturn(GetProperty(Ref("dtx"), "DateTime"));

            return LocalMethod(methodNode, inner);
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression timeZoneField =
                codegenClassScope.AddOrGetDefaultFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var method = codegenMethodScope
                .MakeChild(typeof(DateTimeOffset), typeof(ReformatToDateTimeOffsetForge), codegenClassScope)
                .AddParam<DateTime>("input")
                .Block
                .DeclareVar<DateTimeEx>(
                    "dateTimeEx",
                    StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField))
                .ExprDotMethod(Ref("dateTimeEx"), "Set", Ref("input"))
                .MethodReturn(GetProperty(Ref("dateTimeEx"), "DateTime"));
            return LocalMethodBuild(method).Pass(inner).Call();
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return inner;
        }

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(DateTimeOffset), typeof(ReformatToDateTimeOffsetForge), codegenClassScope)
                .AddParam<DateTimeEx>("input")
                .Block
                .MethodReturn(GetProperty(Ref("input"), "DateTime"));
            return LocalMethodBuild(method).Pass(inner).Call();
        }

        public object Evaluate(
            long ts,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dateTimeEx = DateTimeEx.NowLocal();
            _timeAbacus.DateTimeSet(ts, dateTimeEx);
            return dateTimeEx.DateTime;
        }

        public object Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dateTimeEx = DateTimeEx.NowLocal();
            dateTimeEx.Set(dateTime);
            return dateTimeEx.DateTime;
        }

        public object Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return dateTimeOffset;
        }

        public object Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return dateTimeEx.DateTime;
        }
    }
}