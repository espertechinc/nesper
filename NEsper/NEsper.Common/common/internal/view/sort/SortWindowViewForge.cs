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

namespace com.espertech.esper.common.@internal.view.sort
{
    /// <summary>
    ///     Factory for sort window views.
    /// </summary>
    public class SortWindowViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious
    {
        private const string NAME = "Sort";
        internal bool[] isDescendingValues;
        private ExprForge sizeForge;
        internal ExprNode[] sortCriteriaExpressions;
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
                NAME + " window requires a numeric size parameter and a list of expressions providing sort keys";
            if (viewParameters.Count < 2) {
                throw new ViewParameterException(message);
            }

            var validated = ViewForgeSupport.Validate(
                NAME + " window", parentEventType, viewParameters, true, viewForgeEnv, streamNumber);
            for (var i = 1; i < validated.Length; i++) {
                ViewForgeSupport.AssertReturnsNonConstant(NAME + " window", validated[i], i);
            }

            ViewForgeSupport.ValidateNoProperties(ViewName, validated[0], 0);
            sizeForge = ViewForgeSupport.ValidateSizeParam(ViewName, validated[0], 0);

            sortCriteriaExpressions = new ExprNode[validated.Length - 1];
            isDescendingValues = new bool[sortCriteriaExpressions.Length];

            for (var i = 1; i < validated.Length; i++) {
                if (validated[i] is ExprOrderedExpr) {
                    isDescendingValues[i - 1] = ((ExprOrderedExpr) validated[i]).IsDescending;
                    sortCriteriaExpressions[i - 1] = validated[i].ChildNodes[0];
                }
                else {
                    sortCriteriaExpressions[i - 1] = validated[i];
                }
            }
        }

        internal override Type TypeOfFactory()
        {
            return typeof(SortWindowViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "sort";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(factory, "Size", CodegenEvaluator(sizeForge, method, GetType(), classScope))
                .SetProperty(factory, "SortCriteriaEvaluators",
                    CodegenEvaluators(sortCriteriaExpressions, method, GetType(), classScope))
                .SetProperty(factory, "SortCriteriaTypes",
                    Constant(ExprNodeUtilityQuery.GetExprResultTypes(sortCriteriaExpressions)))
                .SetProperty(factory, "IsDescendingValues", Constant(isDescendingValues))
                .SetProperty(factory, "UseCollatorSort", Constant(useCollatorSort));
        }
    }
} // end of namespace