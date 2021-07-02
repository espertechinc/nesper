///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeStrategyRowUnfilteredUnselectedTable : SubselectForgeStrategyRowPlain
    {
        private readonly TableMetaData _table;

        public SubselectForgeStrategyRowUnfilteredUnselectedTable(
            ExprSubselectRowNode subselect,
            TableMetaData table)
            :
            base(subselect)
        {
            this._table = table;
        }

        public override CodegenExpression EvaluateCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (subselect.EvaluationType.IsNullType()) {
                return ConstantNull();
            }
            
            var eventToPublic = TableDeployTimeResolver.MakeTableEventToPublicField(_table, classScope, GetType());
            var method = parent.MakeChild(subselect.EvaluationType, GetType(), classScope);
            method.Block
                .IfCondition(
                    Relational(
                        ExprDotName(symbols.GetAddMatchingEvents(method), "Count"),
                        CodegenExpressionRelational.CodegenRelational.GT,
                        Constant(1)))
                .BlockReturn(ConstantNull())
                .DeclareVar<EventBean>(
                    "@event",
                    StaticMethod(
                        typeof(EventBeanUtility),
                        "GetNonemptyFirstEvent",
                        symbols.GetAddMatchingEvents(method)))
                .MethodReturn(
                    ExprDotMethod(
                        eventToPublic,
                        "ConvertToUnd",
                        Ref("@event"),
                        symbols.GetAddEPS(method),
                        symbols.GetAddIsNewData(method),
                        symbols.GetAddExprEvalCtx(method)));
            return LocalMethod(method);
        }
    }
} // end of namespace