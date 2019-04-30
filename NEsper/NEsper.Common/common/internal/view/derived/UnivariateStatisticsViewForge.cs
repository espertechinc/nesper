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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    ///     Factory for <seealso cref="UnivariateStatisticsView" /> instances.
    /// </summary>
    public class UnivariateStatisticsViewForge : ViewFactoryForgeBase
    {
        internal const string NAME = "Univariate statistics";
        internal StatViewAdditionalPropsForge additionalProps;
        internal ExprNode fieldExpression;

        private IList<ExprNode> viewParameters;

        public override string ViewName => NAME;

        private string ViewParamMessage =>
            ViewName + " view require a single expression returning a numeric value as a parameter";

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
            var validated = ViewForgeSupport.Validate(
                ViewName, parentEventType, viewParameters, true, viewForgeEnv, streamNumber);
            if (validated.Length < 1) {
                throw new ViewParameterException(ViewParamMessage);
            }

            if (!validated[0].Forge.EvaluationType.IsNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }

            fieldExpression = validated[0];

            additionalProps = StatViewAdditionalPropsForge.Make(validated, 1, parentEventType, streamNumber);
            eventType = UnivariateStatisticsView.CreateEventType(additionalProps, viewForgeEnv, streamNumber);
        }

        internal override Type TypeOfFactory()
        {
            return typeof(UnivariateStatisticsViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "uni";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (additionalProps != null) {
                method.Block.SetProperty(factory, "AdditionalProps", additionalProps.Codegen(method, classScope));
            }

            method.Block.SetProperty(factory, "FieldEval", CodegenEvaluator(fieldExpression.Forge, method, GetType(), classScope));
        }
    }
} // end of namespace