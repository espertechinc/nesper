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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.rank
{
    /// <summary>
    ///     Factory for rank window views.
    /// </summary>
    public class RankWindowViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious
    {
        private const string NAME = "Rank";

        /// <summary>
        ///     The flags defining the ascending or descending sort order.
        /// </summary>
        private bool[] isDescendingValues;

        /// <summary>
        ///     The sort window size.
        /// </summary>
        private ExprForge sizeForge;

        /// <summary>
        ///     The sort-by expressions.
        /// </summary>
        private ExprNode[] sortCriteriaExpressions;

        /// <summary>
        ///     The unique-by expressions.
        /// </summary>
        private ExprNode[] uniqueCriteriaExpressions;

        private bool useCollatorSort;

        private IList<ExprNode> viewParameters;

        public override string ViewName => NAME;

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            viewParameters = parameters;
            useCollatorSort = viewForgeEnv.Configuration.Compiler.Language.IsSortUsingCollator;
        }

        public override void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv)
        {
            eventType = parentEventType;
            var message =
                NAME +
                " view requires a list of expressions providing unique keys, a numeric size parameter and a list of expressions providing sort keys";
            if (viewParameters.Count < 3) {
                throw new ViewParameterException(message);
            }

            // validate
            var validated = ViewForgeSupport.Validate(
                NAME, parentEventType, viewParameters, true, viewForgeEnv, streamNumber);

            // find size-parameter index
            var indexNumericSize = -1;
            for (var i = 0; i < validated.Length; i++) {
                if (validated[i] is ExprConstantNode || validated[i] is ExprContextPropertyNode) {
                    indexNumericSize = i;
                    break;
                }
            }

            if (indexNumericSize == -1) {
                throw new ViewParameterException("Failed to find constant value for the numeric size parameter");
            }

            if (indexNumericSize == 0) {
                throw new ViewParameterException(
                    "Failed to find unique value expressions that are expected to occur before the numeric size parameter");
            }

            if (indexNumericSize == validated.Length - 1) {
                throw new ViewParameterException(
                    "Failed to find sort key expressions after the numeric size parameter");
            }

            // validate non-constant for unique-keys and sort-keys
            for (var i = 0; i < indexNumericSize; i++) {
                ViewForgeSupport.AssertReturnsNonConstant(NAME, validated[i], i);
            }

            for (var i = indexNumericSize + 1; i < validated.Length; i++) {
                ViewForgeSupport.AssertReturnsNonConstant(NAME, validated[i], i);
            }

            // get sort size
            ViewForgeSupport.ValidateNoProperties(ViewName, validated[indexNumericSize], indexNumericSize);
            sizeForge = ViewForgeSupport.ValidateSizeParam(ViewName, validated[indexNumericSize], indexNumericSize);

            // compile unique expressions
            uniqueCriteriaExpressions = new ExprNode[indexNumericSize];
            Array.Copy(validated, 0, uniqueCriteriaExpressions, 0, indexNumericSize);

            // compile sort expressions
            sortCriteriaExpressions = new ExprNode[validated.Length - indexNumericSize - 1];
            isDescendingValues = new bool[sortCriteriaExpressions.Length];

            var count = 0;
            for (var i = indexNumericSize + 1; i < validated.Length; i++) {
                if (validated[i] is ExprOrderedExpr) {
                    isDescendingValues[count] = ((ExprOrderedExpr) validated[i]).IsDescending;
                    sortCriteriaExpressions[count] = validated[i].ChildNodes[0];
                }
                else {
                    sortCriteriaExpressions[count] = validated[i];
                }

                count++;
            }
        }

        internal override Type TypeOfFactory()
        {
            return typeof(RankWindowViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "rank";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .ExprDotMethod(factory, "setSize", CodegenEvaluator(sizeForge, method, GetType(), classScope))
                .ExprDotMethod(
                    factory, "setSortCriteriaEvaluators",
                    CodegenEvaluators(sortCriteriaExpressions, method, GetType(), classScope))
                .ExprDotMethod(
                    factory, "setSortCriteriaTypes",
                    Constant(ExprNodeUtilityQuery.GetExprResultTypes(sortCriteriaExpressions)))
                .ExprDotMethod(factory, "setIsDescendingValues", Constant(isDescendingValues))
                .ExprDotMethod(factory, "setUseCollatorSort", Constant(useCollatorSort))
                .ExprDotMethod(
                    factory, "setUniqueEvaluators",
                    CodegenEvaluators(uniqueCriteriaExpressions, method, GetType(), classScope))
                .ExprDotMethod(
                    factory, "setUniqueTypes",
                    Constant(ExprNodeUtilityQuery.GetExprResultTypes(uniqueCriteriaExpressions)));
        }
    }
} // end of namespace