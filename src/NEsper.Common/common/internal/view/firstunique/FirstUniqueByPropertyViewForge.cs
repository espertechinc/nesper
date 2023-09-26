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
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;

namespace com.espertech.esper.common.@internal.view.firstunique
{
    /// <summary>
    ///     Factory for <seealso cref="FirstUniqueByPropertyView" /> instances.
    /// </summary>
    public class FirstUniqueByPropertyViewForge : ViewFactoryForgeBase,
        AsymetricDataWindowViewForge,
        DataWindowViewForgeUniqueCandidate
    {
        public const string NAME = "First-Unique-By";

        private ExprNode[] criteriaExpressions;
        private IList<ExprNode> viewParameters;
        private MultiKeyClassRef multiKeyClassNames;

        public override string ViewName => NAME;

        public ISet<string> UniquenessCandidatePropertyNames =>
            ExprNodeUtilityQuery.GetPropertyNamesIfAllProps(criteriaExpressions);

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            viewParameters = parameters;
        }

        public override void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            criteriaExpressions = ViewForgeSupport.Validate(
                ViewName,
                parentEventType,
                viewParameters,
                false,
                viewForgeEnv);

            if (criteriaExpressions.Length == 0) {
                var errorMessage =
                    ViewName + " view requires a one or more expressions provinding unique values as parameters";
                throw new ViewParameterException(errorMessage);
            }

            eventType = parentEventType;
        }

        public override IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv)
        {
            var desc = MultiKeyPlanner.PlanMultiKey(
                criteriaExpressions,
                false,
                viewForgeEnv.StatementRawInfo,
                viewForgeEnv.SerdeResolver);
            multiKeyClassNames = desc.ClassRef;
            return desc.MultiKeyForgeables;
        }

        internal override Type TypeOfFactory => typeof(FirstUniqueByPropertyViewFactory);

        internal override string FactoryMethod => "Firstunique";

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            ViewMultiKeyHelper.Assign(criteriaExpressions, multiKeyClassNames, method, factory, symbols, classScope);
        }

        public MultiKeyClassRef MultiKeyClassNames => multiKeyClassNames;

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_FIRSTUNIQUE;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
} // end of namespace