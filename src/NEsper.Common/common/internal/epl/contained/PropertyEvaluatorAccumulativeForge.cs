///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using System.Collections.Generic;
using System.Linq;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.contained
{
    /// <summary>
    /// A property evaluator that returns a full row of events for each stream, i.e. flattened inner-join results for
    /// property-upon-property.
    /// </summary>
    public class PropertyEvaluatorAccumulativeForge
    {
        private readonly ContainedEventEvalForge[] containedEventEvals;
        private readonly bool[] fragmentEventTypeIsIndexed;
        private readonly ExprForge[] whereClauses;
        private readonly IList<string> propertyNames;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="containedEventEvals">property getters or other evaluators</param>
        /// <param name="fragmentEventTypeIsIndexed">property fragment types is indexed</param>
        /// <param name="whereClauses">filters, if any</param>
        /// <param name="propertyNames">the property names that are staggered</param>
        public PropertyEvaluatorAccumulativeForge(
            ContainedEventEvalForge[] containedEventEvals,
            bool[] fragmentEventTypeIsIndexed,
            ExprForge[] whereClauses,
            IList<string> propertyNames)
        {
            this.fragmentEventTypeIsIndexed = fragmentEventTypeIsIndexed;
            this.containedEventEvals = containedEventEvals;
            this.whereClauses = whereClauses;
            this.propertyNames = propertyNames;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(PropertyEvaluatorAccumulative), this.GetType(), classScope);
            method.Block
                .DeclareVar<PropertyEvaluatorAccumulative>("pe", NewInstance(typeof(PropertyEvaluatorAccumulative)))
                .SetProperty(
                    Ref("pe"),
                    "ContainedEventEvals",
                    MakeContained(containedEventEvals, method, symbols, classScope))
                .SetProperty(Ref("pe"), "WhereClauses", MakeWhere(whereClauses, method, symbols, classScope))
                .SetProperty(Ref("pe"), "PropertyNames", Constant(propertyNames.ToArray()))
                .SetProperty(Ref("pe"), "FragmentEventTypeIsIndexed", Constant(fragmentEventTypeIsIndexed))
                .MethodReturn(Ref("pe"));
            return LocalMethod(method);
        }

        protected internal static CodegenExpression MakeWhere(
            ExprForge[] whereClauses,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpression[] expressions = new CodegenExpression[whereClauses.Length];
            for (int i = 0; i < whereClauses.Length; i++) {
                expressions[i] = whereClauses[i] == null
                    ? ConstantNull()
                    : ExprNodeUtilityCodegen.CodegenEvaluator(
                        whereClauses[i],
                        method,
                        typeof(PropertyEvaluatorAccumulativeForge),
                        classScope);
            }

            return NewArrayWithInit(typeof(ExprEvaluator), expressions);
        }

        protected internal static CodegenExpression MakeContained(
            ContainedEventEvalForge[] evals,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpression[] expressions = new CodegenExpression[evals.Length];
            for (int i = 0; i < evals.Length; i++) {
                expressions[i] = evals[i].Make(parent, symbols, classScope);
            }

            return NewArrayWithInit(typeof(ContainedEventEval), expressions);
        }
    }
} // end of namespace