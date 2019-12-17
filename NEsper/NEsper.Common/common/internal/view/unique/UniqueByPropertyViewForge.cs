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

namespace com.espertech.esper.common.@internal.view.unique
{
    /// <summary>
    ///     Factory for <seealso cref="UniqueByPropertyView" /> instances.
    /// </summary>
    public class UniqueByPropertyViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeUniqueCandidate
    {
        public const string NAME = "Unique-By";
        protected ExprNode[] criteriaExpressions;

        protected IList<ExprNode> viewParameters;

        public ISet<string> UniquenessCandidatePropertyNames =>
            ExprNodeUtilityQuery.GetPropertyNamesIfAllProps(criteriaExpressions);

        public override string ViewName => NAME;

        public ExprNode[] CriteriaExpressions => criteriaExpressions;

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            viewParameters = parameters;
        }

        public override void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv)
        {
            criteriaExpressions = ViewForgeSupport.Validate(
                ViewName,
                parentEventType,
                viewParameters,
                false,
                viewForgeEnv,
                streamNumber);

            if (criteriaExpressions.Length == 0) {
                var errorMessage =
                    ViewName + " view requires a one or more expressions providing unique values as parameters";
                throw new ViewParameterException(errorMessage);
            }

            this.eventType = parentEventType;
        }

        internal override Type TypeOfFactory()
        {
            return typeof(UniqueByPropertyViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "Unique";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(
                    factory,
                    "CriteriaEvals",
                    CodegenEvaluators(criteriaExpressions, method, GetType(), classScope))
                .SetProperty(
                    factory,
                    "CriteriaTypes",
                    Constant(ExprNodeUtilityQuery.GetExprResultTypes(criteriaExpressions)));
        }
    }
} // end of namespace