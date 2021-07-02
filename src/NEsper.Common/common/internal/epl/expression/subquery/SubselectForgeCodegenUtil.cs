///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeCodegenUtil
    {
        public const string EVENTS_SHIFTED = "shift";
        public static readonly CodegenExpressionRef REF_EVENTS_SHIFTED = Ref(EVENTS_SHIFTED);

        public static readonly TriConsumer<CodegenMethod, CodegenBlock, ExprSubselectEvalMatchSymbol>
            DECLARE_EVENTS_SHIFTED =
                new ProxyTriConsumer<CodegenMethod, CodegenBlock, ExprSubselectEvalMatchSymbol>(
                    (
                        method,
                        block,
                        symbols) => {
                        block.DeclareVar<EventBean[]>(
                            EVENTS_SHIFTED,
                            StaticMethod(
                                typeof(EventBeanUtility),
                                "AllocatePerStreamShift",
                                symbols.GetAddEPS(method)));
                    });

        public class ReturnIfNoMatch : TriConsumer<CodegenMethod, CodegenBlock, ExprSubselectEvalMatchSymbol>
        {
            private readonly CodegenExpression _valueIfNull;
            private readonly CodegenExpression _valueIfEmpty;

            public ReturnIfNoMatch(
                CodegenExpression valueIfNull,
                CodegenExpression valueIfEmpty)
            {
                this._valueIfNull = valueIfNull;
                this._valueIfEmpty = valueIfEmpty;
            }

            public void Accept(
                CodegenMethod method,
                CodegenBlock block,
                ExprSubselectEvalMatchSymbol symbols)
            {
                CodegenExpression matching = symbols.GetAddMatchingEvents(method);
                block.IfCondition(EqualsNull(matching))
                    .BlockReturn(_valueIfNull)
                    .IfCondition(ExprDotMethod(matching, "IsEmpty"))
                    .BlockReturn(_valueIfEmpty);
            }
        }
    }
} // end of namespace