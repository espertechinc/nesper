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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.contained
{
    /// <summary>
    ///     A property evaluator that considers nested properties and that considers where-clauses
    ///     but does not consider select-clauses.
    /// </summary>
    public class PropertyEvaluatorNestedForge : PropertyEvaluatorForge
    {
        private readonly ContainedEventEvalForge[] containedEventEvals;
        private readonly string[] expressionTexts;
        private readonly FragmentEventType[] fragmentEventTypes;
        private readonly bool[] fragmentEventTypesIsIndexed;
        private readonly ExprForge[] whereClauses;

        public PropertyEvaluatorNestedForge(
            ContainedEventEvalForge[] containedEventEvals,
            FragmentEventType[] fragmentEventTypes,
            ExprForge[] whereClauses,
            string[] expressionTexts)
        {
            this.containedEventEvals = containedEventEvals;
            this.fragmentEventTypes = fragmentEventTypes;
            this.whereClauses = whereClauses;
            this.expressionTexts = expressionTexts;
            fragmentEventTypesIsIndexed = new bool[fragmentEventTypes.Length];
            for (var i = 0; i < fragmentEventTypesIsIndexed.Length; i++) {
                fragmentEventTypesIsIndexed[i] = fragmentEventTypes[i].IsIndexed;
            }
        }

        public EventType FragmentEventType => fragmentEventTypes[fragmentEventTypes.Length - 1].FragmentType;

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(PropertyEvaluatorNested), GetType(), classScope);
            method.Block
                .DeclareVarNewInstance<PropertyEvaluatorNested>("pe")
                .SetProperty(
                    Ref("pe"),
                    "ResultEventType",
                    EventTypeUtility.ResolveTypeCodegen(
                        fragmentEventTypes[fragmentEventTypes.Length - 1].FragmentType,
                        symbols.GetAddInitSvc(method)))
                .SetProperty(Ref("pe"), "ExpressionTexts", Constant(expressionTexts))
                .SetProperty(
                    Ref("pe"),
                    "WhereClauses",
                    PropertyEvaluatorAccumulativeForge.MakeWhere(whereClauses, method, symbols, classScope))
                .SetProperty(
                    Ref("pe"),
                    "ContainedEventEvals",
                    PropertyEvaluatorAccumulativeForge.MakeContained(containedEventEvals, method, symbols, classScope))
                .SetProperty(Ref("pe"), "FragmentEventTypeIsIndexed", Constant(fragmentEventTypesIsIndexed))
                .MethodReturn(Ref("pe"));
            return LocalMethod(method);
        }

        public bool CompareTo(PropertyEvaluatorForge other)
        {
            return false;
        }
    }
} // end of namespace