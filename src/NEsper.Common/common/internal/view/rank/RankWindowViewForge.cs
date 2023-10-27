///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.rank
{
    /// <summary>
    /// Factory for rank window views.
    /// </summary>
    public class RankWindowViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious
    {
        private IList<ExprNode> viewParameters;
        private ExprNode[] criteriaExpressions;
        private ExprNode[] sortCriteriaExpressions;
        private bool[] isDescendingValues;
        private ExprForge sizeForge;
        private bool useCollatorSort;
        private MultiKeyClassRef multiKeyClassNames;
        private DataInputOutputSerdeForge[] sortSerdes;

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            viewParameters = parameters;
            useCollatorSort = viewForgeEnv.Configuration.Compiler.Language.IsSortUsingCollator;
        }

        public override void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            eventType = parentEventType;
            var message =
                $"{ViewName} view requires a list of expressions providing unique keys, a numeric size parameter and a list of expressions providing sort keys";
            if (viewParameters.Count < 3) {
                throw new ViewParameterException(message);
            }

            // validate
            var validated = ViewForgeSupport.Validate(
                ViewName,
                parentEventType,
                viewParameters,
                true,
                viewForgeEnv);

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
                ViewForgeSupport.AssertReturnsNonConstant(ViewName, validated[i], i);
            }

            for (var i = indexNumericSize + 1; i < validated.Length; i++) {
                ViewForgeSupport.AssertReturnsNonConstant(ViewName, validated[i], i);
            }

            // get sort size
            ViewForgeSupport.ValidateNoProperties(ViewName, validated[indexNumericSize], indexNumericSize);
            sizeForge = ViewForgeSupport.ValidateSizeParam(ViewName, validated[indexNumericSize], indexNumericSize);

            // compile unique expressions
            criteriaExpressions = new ExprNode[indexNumericSize];
            Array.Copy(validated, 0, criteriaExpressions, 0, indexNumericSize);

            // compile sort expressions
            sortCriteriaExpressions = new ExprNode[validated.Length - indexNumericSize - 1];
            isDescendingValues = new bool[sortCriteriaExpressions.Length];

            var count = 0;
            for (var i = indexNumericSize + 1; i < validated.Length; i++) {
                if (validated[i] is ExprOrderedExpr) {
                    isDescendingValues[count] = ((ExprOrderedExpr)validated[i]).IsDescending;
                    sortCriteriaExpressions[count] = validated[i].ChildNodes[0];
                }
                else {
                    sortCriteriaExpressions[count] = validated[i];
                }

                count++;
            }

            sortSerdes = viewForgeEnv.SerdeResolver.SerdeForDataWindowSortCriteria(
                ExprNodeUtilityQuery.GetExprResultTypes(sortCriteriaExpressions),
                viewForgeEnv.StatementRawInfo);
        }

        public override IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv)
        {
            var desc = MultiKeyPlanner.PlanMultiKey(
                criteriaExpressions,
                false,
                viewForgeEnv.StatementRawInfo,
                viewForgeEnv.StatementCompileTimeServices.SerdeResolver);
            multiKeyClassNames = desc.ClassRef;
            return desc.MultiKeyForgeables;
        }

        internal override Type TypeOfFactory => typeof(RankWindowViewFactory);

        internal override string FactoryMethod => "Rank";

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(
                    factory,
                    "SizeEvaluator",
                    CodegenEvaluator(sizeForge, method, GetType(), classScope))
                .SetProperty(
                    factory,
                    "SortCriteriaEvaluators",
                    CodegenEvaluators(sortCriteriaExpressions, method, GetType(), classScope))
                .SetProperty(
                    factory,
                    "SortCriteriaTypes",
                    Constant(ExprNodeUtilityQuery.GetExprResultTypes(sortCriteriaExpressions)))
                .SetProperty(
                    factory,
                    "IsDescendingValues",
                    Constant(isDescendingValues))
                .SetProperty(
                    factory,
                    "UseCollatorSort",
                    Constant(useCollatorSort))
                .SetProperty(
                    factory,
                    "SortSerdes",
                    DataInputOutputSerdeForgeExtensions.CodegenArray(sortSerdes, method, classScope, null));
            ViewMultiKeyHelper.Assign(criteriaExpressions, multiKeyClassNames, method, factory, symbols, classScope);
        }

        public override string ViewName => ViewEnum.RANK_WINDOW.GetName();

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_RANK;
        }

        public MultiKeyClassRef MultiKeyClassNames => multiKeyClassNames;

        public DataInputOutputSerdeForge[] SortSerdes => sortSerdes;

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
} // end of namespace