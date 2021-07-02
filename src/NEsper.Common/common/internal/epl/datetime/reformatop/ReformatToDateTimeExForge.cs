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
    public class ReformatToDateTimeExForge : ReformatForge,
        ReformatOp
    {
        private readonly TimeAbacus _timeAbacus;

        public ReformatToDateTimeExForge(TimeAbacus timeAbacus)
        {
            _timeAbacus = timeAbacus;
        }

        public ReformatOp Op => this;

        public Type ReturnType => typeof(DateTimeEx);

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
                .MakeChild(typeof(DateTimeEx), typeof(ReformatToDateTimeExForge), codegenClassScope)
                .AddParam(typeof(long), "ts");

            methodNode
                .Block
                .DeclareVar<DateTimeEx>("dtx", StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField))
                .Expression(_timeAbacus.DateTimeSetCodegen(Ref("ts"), Ref("dtx"), methodNode, codegenClassScope))
                .MethodReturn(Ref("dtx"));
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
                .MakeChild(typeof(DateTimeEx), typeof(ReformatToDateTimeExForge), codegenClassScope)
                .AddParam(typeof(DateTime), "d")
                .Block
                .DeclareVar<DateTimeEx>(
                    "dateTimeEx",
                    StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField))
                .ExprDotMethod(Ref("dateTimeEx"), "Set", Ref("d"))
                .MethodReturn(Ref("dateTimeEx"));
            return LocalMethodBuild(method).Pass(inner).Call();
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression timeZoneField =
                codegenClassScope.AddOrGetDefaultFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var method = codegenMethodScope
                .MakeChild(typeof(DateTimeEx), typeof(ReformatToDateTimeExForge), codegenClassScope)
                .AddParam(typeof(DateTimeOffset), "d")
                .Block
                .DeclareVar<DateTimeEx>(
                    "dateTimeEx",
                    StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField))
                .ExprDotMethod(Ref("dateTimeEx"), "Set", Ref("d"))
                .MethodReturn(Ref("dateTimeEx"));
            return LocalMethodBuild(method).Pass(inner).Call();
        }

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return inner;
        }

        public object Evaluate(
            long ts,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dateTimeEx = DateTimeEx.NowLocal();
            _timeAbacus.DateTimeSet(ts, dateTimeEx);
            return dateTimeEx;
        }

        public object Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dateTimeEx = DateTimeEx.NowLocal();
            dateTimeEx.Set(dateTime);
            return dateTimeEx;
        }

        public object Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dateTimeEx = DateTimeEx.NowLocal();
            dateTimeEx.Set(dateTimeOffset);
            return dateTimeEx;
        }

        public object Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return dateTimeEx;
        }
    }
} // end of namespace