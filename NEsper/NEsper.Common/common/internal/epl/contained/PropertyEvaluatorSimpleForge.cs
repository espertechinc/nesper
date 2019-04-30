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
    ///     Property evaluator that considers only level one and considers a where-clause,
    ///     but does not consider a select clause or N-level.
    /// </summary>
    public class PropertyEvaluatorSimpleForge : PropertyEvaluatorForge
    {
        private readonly ContainedEventEvalForge containedEventEval;
        private readonly FragmentEventType fragmentEventType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="containedEventEval">property getter or other evaluator</param>
        /// <param name="fragmentEventType">property event type</param>
        /// <param name="filter">optional where-clause expression</param>
        /// <param name="expressionText">the property name</param>
        public PropertyEvaluatorSimpleForge(
            ContainedEventEvalForge containedEventEval,
            FragmentEventType fragmentEventType,
            ExprForge filter,
            string expressionText)
        {
            this.fragmentEventType = fragmentEventType;
            this.containedEventEval = containedEventEval;
            Filter = filter;
            ExpressionText = expressionText;
        }

        public ExprForge Filter { get; }

        public string ExpressionText { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(PropertyEvaluatorSimple), GetType(), classScope);
            method.Block
                .DeclareVar(typeof(PropertyEvaluatorSimple), "pe", NewInstance(typeof(PropertyEvaluatorSimple)))
                .SetProperty(Ref("pe"), "Filter",
                    Filter == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(Filter, method, GetType(), classScope))
                .SetProperty(Ref("pe"), "ContainedEventEval", containedEventEval.Make(method, symbols, classScope))
                .SetProperty(Ref("pe"), "FragmentIsIndexed", Constant(fragmentEventType.IsIndexed))
                .SetProperty(Ref("pe"), "ExpressionText", Constant(ExpressionText))
                .SetProperty(Ref("pe"), "EventType",
                    EventTypeUtility.ResolveTypeCodegen(fragmentEventType.FragmentType, symbols.GetAddInitSvc(method)))
                .MethodReturn(Ref("pe"));
            return LocalMethod(method);
        }

        public EventType FragmentEventType => fragmentEventType.FragmentType;

        public bool CompareTo(PropertyEvaluatorForge otherEval)
        {
            if (!(otherEval is PropertyEvaluatorSimpleForge)) {
                return false;
            }

            var other = (PropertyEvaluatorSimpleForge) otherEval;
            if (!other.ExpressionText.Equals(ExpressionText)) {
                return false;
            }

            if (other.Filter == null && Filter == null) {
                return true;
            }

            return false;
        }
    }
} // end of namespace