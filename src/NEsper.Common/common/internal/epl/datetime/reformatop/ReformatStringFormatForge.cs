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
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public class ReformatStringFormatForge : ReformatForge,
        ReformatOp
    {
        public ReformatOp Op => this;

        public CodegenExpression CodegenLong(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression timeZoneField =
                codegenClassScope.AddOrGetDefaultFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var dateTimeExpr =
                StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField, inner);

            return ActionCodegen(dateTimeExpr);
        }

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotMethod(EnumValue(typeof(DateTimeFormat), "ISO_DATE_TIME"), "Format", inner);
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotMethod(EnumValue(typeof(DateTimeFormat), "ISO_DATE_TIME"), "Format", inner);
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotMethod(EnumValue(typeof(DateTimeFormat), "ISO_DATE_TIME"), "Format", inner);
        }

        public Type ReturnType => typeof(string);

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
            return Action(DateTimeEx.UtcInstance(ts));
        }

        public object Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return DateTimeFormat.ISO_DATE_TIME.Format(dateTimeEx);
        }

        public object Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return DateTimeFormat.ISO_DATE_TIME.Format(dateTimeOffset);
        }

        public object Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return DateTimeFormat.ISO_DATE_TIME.Format(dateTime);
        }

        private static string Action(DateTimeEx d)
        {
            var format = new SimpleDateFormat();
            return format.Format(d);
        }

        private CodegenExpression ActionCodegen(CodegenExpression date)
        {
            return ExprDotMethod(NewInstance<SimpleDateFormat>(), "Format", date);
        }
    }
} // end of namespace